using AshproSAASERP.Interface.Transaction.Purchase;
using AshproSAASERP.Models.Transactions.Accounts;
using AshproSAASERP.Models.Transactions.Purchase;
using AshproSAASERP.Repo.CodeGenerationServices;
using Microsoft.Extensions.Logging;

namespace AshproSAASERP.Repo.Accounts;

public class SupplierPaymentRepo : ISupplierPaymentRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly ILogger<SupplierPaymentRepo> _logger;

    public SupplierPaymentRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, ILogger<SupplierPaymentRepo> logger)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _logger = logger;
    }

    public async Task<string> GetNextVoucherNoAsync(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.SupplierPayment,
            async (code) => await db.SupplierPaymentHdrs.AnyAsync(x => x.VoucherNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(SupplierPaymentDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.Hdr;
            var dtls = entity.Dtls;

            // Code Generation
            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.SupplierPayment, db);
            if (isAuto || string.IsNullOrEmpty(hdr.VoucherNo) || hdr.VoucherNo == "0")
            {
                hdr.VoucherNo = await GetNextVoucherNoAsync(hdr.FK_BranchId);
            }
            else
            {
                if (await db.SupplierPaymentHdrs.AnyAsync(x => x.VoucherNo == hdr.VoucherNo && x.FK_BranchId == hdr.FK_BranchId))
                {
                    return new OutputResponse(0, "NoAlreadyExists");
                }
            }

            // Validate total amount matches allocated + advance
            decimal totalAllocated = dtls.Sum(x => x.PayingAmount);
            if (hdr.Amount < totalAllocated)
            {
                return new OutputResponse(0, "Total allocated amount cannot exceed the Paid Amount.");
            }
            // Note: If hdr.Amount > totalAllocated, the difference is Advance.

            // Calculate Advance
            hdr.AdvanceAmount = hdr.Amount - totalAllocated;
            hdr.AllocatedAmount = totalAllocated;

            // Save Header
            await db.SupplierPaymentHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            // Save Details
            foreach (var item in dtls)
            {
                item.FK_PaymentId = hdr.PaymentId;

                if (item.IsOpeningEntry || (item.FK_PurchaseId == null && item.PurchaseNo != null && item.PurchaseNo.EndsWith("- OPENING")))
                {
                    item.FK_PurchaseId = null;
                    // Store the Identifier in Narration since PurchaseNo is not mapped
                    item.Narration = (item.SupplierInvoice ?? item.PurchaseNo) + " - OPENING";

                }
                else if (item.FK_PurchaseId > 0)
                {
                    var purchase = await db.PurchaseHdrs.FindAsync(item.FK_PurchaseId);
                    if (purchase != null)
                    {
                        decimal pending = purchase.GrandTotal - (purchase.PaymentAmount ?? 0);
                        item.InvoiceAmount = purchase.GrandTotal;
                        item.OutstandingAmount = pending;
                        item.PaidAmount = purchase.PaymentAmount ?? 0;
                        item.RemainingBalance = item.OutstandingAmount - item.PayingAmount - item.Discount;

                        purchase.PaymentAmount = (purchase.PaymentAmount ?? 0) + item.PayingAmount + item.Discount;
                    }
                }
            }
            await db.SupplierPaymentDtls.AddRangeAsync(dtls);
            await db.SaveChangesAsync();

            if (isAuto && hdr.VoucherNo != "0")
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.SupplierPayment, db);
            }

            // Accounting
            await PostToLedger(db, hdr, dtls);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OutputResponse(hdr.PaymentId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating Supplier Payment");
            return new OutputResponse(0, $"{ex.Message} {(ex.InnerException != null ? ex.InnerException.Message : "")}");
        }
    }

    public async Task<OutputResponse> UpdateAsync(SupplierPaymentDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.Hdr;
            var existing = await db.SupplierPaymentHdrs.FindAsync(hdr.PaymentId);
            if (existing == null) return new OutputResponse(0, "Record not found");

            // Reverse old Purchase Payment Updates
            var oldDtls = await db.SupplierPaymentDtls.Where(x => x.FK_PaymentId == hdr.PaymentId).ToListAsync();
            foreach (var oldItem in oldDtls)
            {
                if (oldItem.FK_PurchaseId > 0)
                {
                    var purchase = await db.PurchaseHdrs.FindAsync(oldItem.FK_PurchaseId);
                    if (purchase != null)
                    {
                        purchase.PaymentAmount = (purchase.PaymentAmount ?? 0) - (oldItem.PayingAmount + oldItem.Discount);
                    }
                }
            }
            db.SupplierPaymentDtls.RemoveRange(oldDtls);

            // Update Header
            existing.VoucherNo = hdr.VoucherNo;
            existing.PaidDate = hdr.PaidDate;
            existing.FK_SupplierId = hdr.FK_SupplierId;
            existing.RVNo = hdr.RVNo;
            existing.FK_CashBankId = hdr.FK_CashBankId;
            existing.PayMode = hdr.PayMode;
            existing.ChequeNo = hdr.ChequeNo;
            existing.ChequeDate = hdr.ChequeDate;
            existing.Bank = hdr.Bank;
            existing.FK_SalesmanId = hdr.FK_SalesmanId;
            existing.Amount = hdr.Amount;
            existing.FK_ProjectId = hdr.FK_ProjectId;
            existing.Narration = hdr.Narration;
            existing.FK_BranchId = hdr.FK_BranchId;
            existing.FK_UserId = hdr.FK_UserId;
            existing.FK_FinancialYearId = hdr.FK_FinancialYearId;

            // Add New Details and Update Purchase

            // Validate Totals
            decimal totalAllocated = entity.Dtls.Sum(x => x.PayingAmount);
            if (existing.Amount < totalAllocated)
            {
                return new OutputResponse(0, "Total allocated amount cannot exceed the Paid Amount.");
            }
            existing.AdvanceAmount = existing.Amount - totalAllocated;
            existing.AllocatedAmount = totalAllocated;

            foreach (var item in entity.Dtls)
            {
                item.FK_PaymentId = existing.PaymentId;

                if (item.IsOpeningEntry || (item.FK_PurchaseId == null && item.PurchaseNo != null && item.PurchaseNo.EndsWith("- OPENING")))
                {
                    item.FK_PurchaseId = null;
                    item.Narration = (item.SupplierInvoice ?? item.PurchaseNo) + " - OPENING";
                }
                else if (item.FK_PurchaseId > 0)
                {
                    var purchase = await db.PurchaseHdrs.FindAsync(item.FK_PurchaseId);
                    if (purchase != null)
                    {
                        decimal pending = purchase.GrandTotal - (purchase.PaymentAmount ?? 0);
                        item.InvoiceAmount = purchase.GrandTotal;
                        item.OutstandingAmount = pending;
                        item.PaidAmount = purchase.PaymentAmount ?? 0;
                        item.RemainingBalance = item.OutstandingAmount - item.PayingAmount - item.Discount;

                        purchase.PaymentAmount = (purchase.PaymentAmount ?? 0) + item.PayingAmount + item.Discount;
                    }
                }
            }
            await db.SupplierPaymentDtls.AddRangeAsync(entity.Dtls);

            // Accounting: Delete old, add new
            await db.TblAccountings.Where(x => x.VoucherId == existing.PaymentId && x.Type == "SupplierPayment").ExecuteDeleteAsync(); // Adjust type string
            await PostToLedger(db, existing, entity.Dtls);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OutputResponse(existing.PaymentId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, $"{ex.Message} {(ex.InnerException != null ? ex.InnerException.Message : "")}");
        }
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = await db.SupplierPaymentHdrs.FindAsync(id);
            if (hdr == null) return new OutputResponse(0, "Record not found");

            // Reverse Purchase Updates
            var dtls = await db.SupplierPaymentDtls.Where(x => x.FK_PaymentId == id).ToListAsync();
            foreach (var item in dtls)
            {
                if (item.FK_PurchaseId > 0)
                {
                    var purchase = await db.PurchaseHdrs.FindAsync(item.FK_PurchaseId);
                    if (purchase != null)
                    {
                        purchase.PaymentAmount = (purchase.PaymentAmount ?? 0) - (item.PayingAmount + item.Discount);
                    }
                }
            }

            // Remove Accounting
            await db.TblAccountings.Where(x => x.VoucherId == hdr.PaymentId && x.Type == "SupplierPayment").ExecuteDeleteAsync();

            db.SupplierPaymentDtls.RemoveRange(dtls);
            db.SupplierPaymentHdrs.Remove(hdr);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, "Deleted Successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, $"{ex.Message} {(ex.InnerException != null ? ex.InnerException.Message : "")}");
        }
    }

    public async Task<SupplierPaymentDTO> GetAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.SupplierPaymentHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.PaymentId == id);
        if (hdr == null) return null!;

        // Fill non-mapped names
        var supplier = await db.SupplierMasters.FindAsync(hdr.FK_SupplierId);
        hdr.SupplierName = supplier?.Name ?? "";
        hdr.SupplierCode = supplier?.Code;

        var cashBank = await db.AccountHeadMasters.FindAsync(hdr.FK_CashBankId);
        hdr.CashBankName = cashBank?.Name ?? "";

        if (hdr.FK_SalesmanId > 0)
        {
            var salesman = await db.SalesManMasters.FindAsync(hdr.FK_SalesmanId);
            hdr.SalesmanName = salesman?.Name ?? "";
            hdr.SalesmanCode = salesman?.Code ?? "";
        }

        var dtls = await db.SupplierPaymentDtls.AsNoTracking().Where(x => x.FK_PaymentId == id).ToListAsync();
        foreach (var d in dtls)
        {
            if (d.FK_PurchaseId > 0)
            {
                var pur = await db.PurchaseHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.PurchaseId == d.FK_PurchaseId);
                if (pur != null)
                {
                    d.PurchaseNo = pur.PurchaseNo;
                    d.PurchaseDate = pur.PurchaseDate;
                    d.PurchaseAmount = pur.GrandTotal;
                    d.PaidAmt = pur.PaymentAmount ?? 0;
                    d.PendingAmt = pur.GrandTotal - (pur.PaymentAmount ?? 0);
                }
            }
            else if (d.FK_PurchaseId == null && !string.IsNullOrEmpty(d.Narration) && d.Narration.EndsWith("- OPENING"))
            {
                // Reconstruct opening entry info from Narration
                // Format: "INVOICENO - OPENING"
                string identifier = d.Narration;
                d.PurchaseNo = identifier;
                d.SupplierInvoice = identifier.Replace(" - OPENING", "").Trim();
                d.IsOpeningEntry = true;
            }
        }

        return new SupplierPaymentDTO(hdr, dtls);
    }

    public async Task<(List<SupplierPaymentHdr> PTransactions, int TotalCount)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.SupplierPaymentHdrs.AsNoTracking().Where(x => !x.IsDeleted || x.IsDeleted == null).AsQueryable();

        if (pagination.BranchId > 0)
            query = query.Where(x => x.FK_BranchId == pagination.BranchId);

        if (!string.IsNullOrEmpty(pagination.search))
        {
            query = query.Where(x => x.VoucherNo.Contains(pagination.search) || x.SupplierName.Contains(pagination.search));
        }

        query = query.OrderByDescending(x => x.PaymentId); // Default sort

        int count = await query.CountAsync();
        var list = await query.Skip((pagination.page - 1) * pagination.pageSize).ToListAsync();

        // Populate Names
        var supplierIds = list.Select(x => x.FK_SupplierId).Distinct().ToList();
        var suppliers = await db.SupplierMasters.AsNoTracking()
            .Where(x => supplierIds.Contains(x.SupplierId))
            .ToDictionaryAsync(x => x.SupplierId, x => new { x.Name, x.Code });

        foreach (var item in list)
        {
            if (suppliers.TryGetValue(item.FK_SupplierId, out var sup))
            {
                item.SupplierName = sup.Name;
                item.SupplierCode = sup.Code;
            }
        }
        return (list, count);
    }

    public async Task<List<SupplierPaymentDtls>> GetPendingPurchaseInvoices(int supplierId)
    {
        await using var db = _contextProvider.CreateDbContext();
        var purchases = await db.PurchaseHdrs.AsNoTracking()
            .Where(x => x.FK_SupplierId == supplierId && x.BillType == "Credit" && (x.GrandTotal - (x.PaymentAmount ?? 0)) > 0)
            .OrderBy(x => x.PurchaseDate)
            .ToListAsync();

        // ... previous Purchase Fetching logic ...

        // 1. Fetch Invoices
        var pendingInvoices = new List<SupplierPaymentDtls>();

        // Fetch All Returns for this supplier first
        var returns = await db.PurchaseReturnHdrs.AsNoTracking()
            .Where(x => x.FK_SupplierId == supplierId && x.BillType == "Credit" && x.FK_PurchaseId != null && !x.IsDeleted)
            .Select(x => new { x.FK_PurchaseId, x.GrandTotal })
            .ToListAsync();

        // Group returns by Purchase ID
        var returnDict = returns.GroupBy(x => x.FK_PurchaseId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.GrandTotal));

        foreach (var p in purchases)
        {
            decimal returnedAmt = 0;
            if (returnDict.ContainsKey(p.PurchaseId))
            {
                returnedAmt = returnDict[p.PurchaseId];
            }

            // Pending = Invoice - Paid - Returned
            decimal pending = p.GrandTotal - (p.PaymentAmount ?? 0) - returnedAmt;
            if (pending > 0.00m)
            {
                pendingInvoices.Add(new SupplierPaymentDtls
                {
                    FK_PurchaseId = p.PurchaseId,
                    PurchaseNo = p.PurchaseNo,
                    PurchaseDate = p.PurchaseDate,
                    PurchaseAmount = p.GrandTotal,
                    InvoiceAmount = p.GrandTotal,
                    PaidAmount = p.PaymentAmount ?? 0,
                    ReturnedAmount = returnedAmt, // Set New Field
                    OutstandingAmount = pending,
                    RemainingBalance = pending,
                    PaidAmt = p.PaymentAmount ?? 0,
                    PendingAmt = pending,
                    SupplierInvoice = p.InvoiceNo,
                    Balance = pending
                });
            }
        }

        // 2. Fetch Opening Entries
        var pendingOpening = new List<SupplierPaymentDtls>();
        var openingEntries = await db.SupplierOPEntryUpdated.AsNoTracking()
            .Where(x => x.FK_SupplierId == supplierId)
            .ToListAsync();

        if (openingEntries.Any())
        {
            var opIdentities = openingEntries.Select(x => (x.PurchaseNo ?? "OPENING") + " - OPENING").ToList();

            var existingPayments = await db.SupplierPaymentDtls.AsNoTracking()
                .Where(x => x.FK_PurchaseId == null && opIdentities.Contains(x.Narration))
                .GroupBy(x => x.Narration)
                .Select(g => new { Identifier = g.Key, TotalPaid = g.Sum(x => x.PayingAmount + x.Discount) })
                .ToDictionaryAsync(x => x.Identifier, x => x.TotalPaid);

            foreach (var op in openingEntries)
            {
                string identifier = (op.PurchaseNo ?? "OPENING") + " - OPENING";
                decimal paid = existingPayments.ContainsKey(identifier) ? existingPayments[identifier] : 0;
                decimal balance = (op.PurchaseAmt ?? 0) - paid;

                if (balance > 0)
                {
                    pendingOpening.Add(new SupplierPaymentDtls
                    {
                        FK_PurchaseId = null,
                        PurchaseNo = identifier,
                        PurchaseDate = op.PurchaseDate,
                        PurchaseAmount = op.PurchaseAmt ?? 0,
                        InvoiceAmount = op.PurchaseAmt ?? 0, // Using InvoiceAmount for Grid Display consistency
                        PaidAmount = paid,
                        OutstandingAmount = balance,
                        RemainingBalance = balance,
                        PaidAmt = paid,
                        PendingAmt = balance,
                        SupplierInvoice = op.PurchaseNo ?? "OPENING",
                        Balance = balance,
                        IsOpeningEntry = true,
                        OpeningInvoiceNo = op.PurchaseNo
                    });
                }
            }
        }

        // 3. Merge and Sort
        var finalResult = pendingInvoices.Concat(pendingOpening).OrderBy(x => x.PurchaseDate).ToList();
        return finalResult;
    }

    public async Task<decimal> GetSupplierBalanceAsync(int supplierId, int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        try
        {
            var totalPurchases = await db.PurchaseHdrs
                .Where(p => p.FK_SupplierId == supplierId && p.BillType.ToLower() == "credit")
                .SumAsync(p => (decimal?)p.GrandTotal) ?? 0m;

            var openingBalance = await db.SupplierOPEntryUpdated
                .Where(op => op.FK_SupplierId == supplierId)
                .SumAsync(op => (decimal?)op.PurchaseAmt) ?? 0m;

            var totalPayments = await db.SupplierPaymentHdrs
                .Where(r => r.FK_SupplierId == supplierId)
                .SumAsync(r => (decimal?)r.Amount) ?? 0m;

            var totalDiscounts = await db.SupplierPaymentDtls
                .Join(db.SupplierPaymentHdrs,
                      dtl => dtl.FK_PaymentId,
                      hdr => hdr.PaymentId,
                      (dtl, hdr) => new { dtl, hdr })
                .Where(x => x.hdr.FK_SupplierId == supplierId && x.dtl.Discount > 0)
                .SumAsync(x => (decimal?)x.dtl.Discount) ?? 0m;

            var totalReturns = await db.PurchaseReturnHdrs
                .Where(r => r.FK_SupplierId == supplierId && r.BillType == "Credit")
                .SumAsync(r => (decimal?)r.GrandTotal) ?? 0m;

            return totalPurchases + openingBalance - (totalPayments + totalDiscounts + totalReturns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating supplier balance");
            return 0m;
        }
    }

    private async Task PostToLedger(TenantDBContext db, SupplierPaymentHdr hdr, List<SupplierPaymentDtls> dtls)
    {
        var list = new List<TblAccounting>();

        // 1. Identify Accounts (AccountMaster IDs)

        // A. Supplier Account
        int? supplierAccId = null;
        var supplier = await db.SupplierMasters.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierId == hdr.FK_SupplierId);
        if (supplier != null)
        {
            if (supplier.FK_AccountId.HasValue) supplierAccId = supplier.FK_AccountId;
            else
            {
                // Fallback: Match by Code
                var acc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == supplier.Code);
                supplierAccId = acc?.AccountId;
            }
        }

        // B. Discount Received Account (Income -> Credit)
        // Code: 310002010001
        int? discountAccId = null;
        decimal totalDiscount = dtls.Sum(x => x.Discount);

        if (totalDiscount > 0)
        {
            var discAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "310002010001");

            if (discAcc == null)
                discAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Discount Received"));

            discountAccId = discAcc?.AccountId;

            // Fallback to Purchase Account if Discount Account not found
            if (discountAccId == null)
            {
                var purAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "420001010001");
                discountAccId = purAcc?.AccountId;
            }
        }

        // C. Cash/Bank Account (Asset -> Credit)
        int? paymentAccId = null;
        if (hdr.FK_CashBankId > 0)
        {
            // hdr.FK_CashBankId is AccountHeadId from UI selection
            // Try to find AccountMaster with this HeadId
            var cashBankAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.FK_AccountHeadId == hdr.FK_CashBankId);

            // Fallback: If not found by Link, try by Name
            if (cashBankAcc == null)
            {
                var head = await db.AccountHeadMasters.FindAsync(hdr.FK_CashBankId);
                if (head != null)
                {
                    cashBankAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name == head.Name);
                }
            }
            paymentAccId = cashBankAcc?.AccountId;
        }

        // 2. Create Entries

        // Entry 1: Debit Supplier (Reduce Liability) with TOTAL Amount (Paid + Discount)
        if (supplierAccId.HasValue && (hdr.Amount > 0 || totalDiscount > 0))
        {
            decimal totalDebit = hdr.Amount + totalDiscount;

            list.Add(new TblAccounting
            {
                EntryDate = hdr.PaidDate ?? DateTime.Now,
                VoucherId = hdr.PaymentId,
                VoucherNo = hdr.VoucherNo,
                FK_AccountId = supplierAccId,
                Amount = totalDebit, // Debit (+ve)
                Narration = $"Payment to {hdr.SupplierName} (Disc: {totalDiscount}) - {hdr.Narration}",
                Type = "SupplierPayment",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                BillType = hdr.PayMode,
                FK_SupplierId = hdr.FK_SupplierId // Store Supplier ID for Sub-ledger link
            });
        }

        // Entry 2: Credit Cash/Bank (Paid Amount)
        if (paymentAccId.HasValue && hdr.Amount > 0)
        {
            list.Add(new TblAccounting
            {
                EntryDate = hdr.PaidDate ?? DateTime.Now,
                VoucherId = hdr.PaymentId,
                VoucherNo = hdr.VoucherNo,
                FK_AccountId = paymentAccId,
                Amount = -hdr.Amount, // Credit (-ve)
                Narration = $"Payment to {hdr.SupplierName} - {hdr.Narration}",
                Type = "SupplierPayment",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                BillType = hdr.PayMode,
                FK_SupplierId = hdr.FK_SupplierId
            });
        }

        // Entry 3: Credit Discount Received (Discount Amount)
        if (discountAccId.HasValue && totalDiscount > 0)
        {
            list.Add(new TblAccounting
            {
                EntryDate = hdr.PaidDate ?? DateTime.Now,
                VoucherId = hdr.PaymentId,
                VoucherNo = hdr.VoucherNo,
                FK_AccountId = discountAccId,
                Amount = -totalDiscount, // Credit (-ve)
                Narration = $"Discount Received from {hdr.SupplierName}",
                Type = "SupplierPayment",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                BillType = hdr.PayMode,
                FK_SupplierId = hdr.FK_SupplierId
            });
        }

        if (list.Any())
        {
            await db.TblAccountings.AddRangeAsync(list);
        }
    }
}



