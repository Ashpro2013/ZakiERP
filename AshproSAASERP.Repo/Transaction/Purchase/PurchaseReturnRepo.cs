using AshproSAASERP.Interface.Transaction.Purchase;
using AshproSAASERP.Models.Transactions.Accounts;
using AshproSAASERP.Models.Transactions.Purchase;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Repo.CodeGenerationServices;
using AshproSAASERP.Repo.Transaction.Inventory;
using AshproStringExtension;

namespace AshproSAASERP.Repo.Transaction.Purchase;

public class PurchaseReturnRepo : IPurchaseReturnRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly StockService _stockService;

    public PurchaseReturnRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, StockService stockService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _stockService = stockService;
    }

    public async Task<string> GetNextPurchaseReturnNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.PurchaseReturn,
            async (code) => await db.PurchaseReturnHdrs.AnyAsync(x => x.PurchaseReturnNo == code)
        );
    }
    public async Task<OutputResponse> CreateAsync(PurchaseReturnDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();

        // Guard: Block if payment is already recorded
        if (entity.PurchaseReturnHdr.FK_PurchaseId > 0 && await HasPaymentPaidAsync(db, entity.PurchaseReturnHdr.FK_PurchaseId ?? 0))
        {
            return new OutputResponse(0, "PurchaseReturnBlockedByPayment");
        }

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.PurchaseReturnHdr;
            var dtls = entity.PurchaseReturnDtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.PurchaseReturn, db);
            if (!isAuto)
            {
                var next = await GetNextVchrNo(hdr.FK_BranchId);
                hdr.PurchaseReturnNo = next.ToString();
            }

            await db.PurchaseReturnHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.PurchaseReturn, db);
            }
            int headerId = hdr.PurchaseReturnId;
            var details = new List<PurchaseReturnDtls>();
            var storeId = (await db.StoreMasters.FirstOrDefaultAsync())!.StoreId;
            foreach (var item in dtls)
            {
                item.FK_ItemBaseUnit = (await db.ItemMasters.FirstOrDefaultAsync())!.FK_UnitId ?? 0;
                var dtl = item.CopyTo<PurchaseReturnDtls>();
                dtl.FK_PurchaseReturnHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? storeId : item.FK_StoreId;
                details.Add(dtl);
            }
            await db.PurchaseReturnDtls.AddRangeAsync(details);

            // Stock Movement
            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = headerId;
            tHdr.HdrNo = hdr.PurchaseReturnNo;
            tHdr.FK_StoreId = storeId;
            tHdr.PartyId = hdr.FK_SupplierId ?? 0;
            tHdr.VoucherType = "Purchase Return";
            tHdr.Remarks = $"By Purchase Return No: {hdr.PurchaseReturnNo}";
            // Purchase Return reduces stock, so isPurchase=false? 
            // Purchase adds stock. Sales reduces stock.
            // Sales Return adds stock (isPurchase=true in SalesInvoiceRtnRepo stock call? Let's check).
            // Purchase Return reduces stock -> isPurchase=false.

            await _stockService.StockMovementForTransaction(db, dtls, tHdr, hdr.PurchaseReturnDate.ToDateTime(), false);

            // Accounting Entries
            await PostToLedger(db, hdr, details);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(headerId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            if (id == 0) return new OutputResponse(0, "Invalid identifier.");

            var hdr = await db.PurchaseReturnHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.PurchaseReturnId == id);
            if (hdr != null)
            {
                // Reverse Stock (Add back to stock) -> isPurchase=true (Inward)
                await _stockService.ReverseStockMovements(hdr.PurchaseReturnNo, "Purchase Return", hdr.FK_BranchId, db);

                // Delete Accounting
                await db.TblAccountings.Where(x => x.VoucherId == id && x.Type == "Purchase Return").ExecuteDeleteAsync();
            }

            await db.PurchaseReturnDtls.Where(x => x.FK_PurchaseReturnHdrId == id).ExecuteDeleteAsync();
            await db.PurchaseReturnHdrs.Where(x => x.PurchaseReturnId == id).ExecuteDeleteAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.PurchaseReturnHdrs.AsNoTracking().Include(x => x.Supplier).Where(x => !x.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.PurchaseReturnNo.ToLower().Contains(term) || (x.RefNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.PurchaseReturnNo) : query.OrderBy(x => x.PurchaseReturnNo),
                "Name" => desc ? query.OrderByDescending(x => x.Supplier!.Name) : query.OrderBy(x => x.Supplier!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.PurchaseReturnDate) : query.OrderBy(x => x.PurchaseReturnDate),
                _ => desc ? query.OrderByDescending(x => x.PurchaseReturnId) : query.OrderBy(x => x.PurchaseReturnId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.PurchaseReturnId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.PurchaseReturnId,
            Name = e.SupplierName,
            Code = e.PurchaseReturnNo,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.PurchaseReturnDate.HasValue ? e.PurchaseReturnDate.Value.ToString("dd-MM-yyyy") : ""
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }
    public async Task<PurchaseReturnDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.PurchaseReturnHdrs.AsNoTracking().FirstOrDefaultAsync(x => x.PurchaseReturnId == Id);
        if (hdr == null) return null!;
        var dtls = await db.PurchaseReturnDtls
            .AsNoTracking()
            .Where(x => x.FK_PurchaseReturnHdrId == Id)
            .ToListAsync();
        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.PurchaseReturnDtlsId,
            HdrId = d.FK_PurchaseReturnHdrId,
            FK_ItemId = d.FK_ItemId,
            ItemName = d.ItemName,
            Barcode = d.Barcode,
            FK_UnitId = d.FK_UnitId ?? 0,
            Quantity = d.Quantity,
            QtyPerUnit = d.QtyPerUnit,
            ExRate = d.ExRate,
            InRate = d.InRate,
            GrossAmount = d.GrossAmount,
            DiscountPerc = d.DiscountPerc,
            DiscountAmnt = d.DiscountAmnt,
            TaxableAmnt = d.TaxableAmnt,
            TaxPerc = d.TaxPerc,
            TaxAmount = d.TaxAmount,
            LineTotal = d.LineTotal,
            IsDeleted = d.IsDeleted,
            FK_StoreId = d.FK_StoreId ?? 0
        }).ToList();
        return new PurchaseReturnDTO(hdr, mapped);
    }
    public async Task<int> GetNextVchrNo(int BranchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var maxNo = await db.PurchaseReturnHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.PurchaseReturnId)
                .Select(x => x.PurchaseReturnNo)
                .FirstOrDefaultAsync();

            if (int.TryParse(maxNo, out int val)) return val + 1;
            return 1;
        }
        catch
        {
            return 1;
        }
    }
    public async Task<OutputResponse> UpdateAsync(PurchaseReturnDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.PurchaseReturnHdr;
            var existing = await db.PurchaseReturnHdrs.FindAsync(hdr.PurchaseReturnId);
            if (existing is null) return new OutputResponse(0, "Record not found.");

            // Guard: Block if payment is already recorded (only if not cancelling)
            if (!hdr.IsDeleted && hdr.FK_PurchaseId > 0 && await HasPaymentPaidAsync(db, hdr.FK_PurchaseId ?? 0))
            {
                return new OutputResponse(0, "PurchaseReturnBlockedByPayment");
            }

            // Soft Delete / Cancellation Logic
            if (hdr.IsDeleted)
            {
                existing.IsDeleted = true;

                // Reverse Stock Effects
                await _stockService.ReverseStockMovements(existing.PurchaseReturnNo, "Purchase Return", existing.FK_BranchId, db);

                // Remove Accounting Entries
                await db.TblAccountings.Where(x => x.VoucherId == existing.PurchaseReturnId && x.Type == "Purchase Return").ExecuteDeleteAsync();

                // Mark Details as Deleted
                await db.PurchaseReturnDtls.Where(x => x.FK_PurchaseReturnHdrId == existing.PurchaseReturnId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.PurchaseReturnId, "Purchase Return Cancelled Successfully.");
            }

            // Map properties
            existing.SupplierName = hdr.SupplierName;
            existing.RefNo = hdr.RefNo;
            existing.FK_PurchaseId = hdr.FK_PurchaseId;
            existing.InvoiceNo = hdr.InvoiceNo;
            existing.InvoiceDate = hdr.InvoiceDate;
            existing.BillType = hdr.BillType;
            existing.AccountNo = hdr.AccountNo;
            existing.CurrencyId = hdr.CurrencyId;
            existing.CurrencyRate = hdr.CurrencyRate;
            existing.Remarks = hdr.Remarks;

            existing.GrossAmount = hdr.GrossAmount;
            existing.LineDsicount = hdr.LineDsicount;
            existing.TaxableAmount = hdr.TaxableAmount;
            existing.TaxTotal = hdr.TaxTotal;
            existing.NetAmount = hdr.NetAmount;
            existing.AdditionalAmount = hdr.AdditionalAmount;
            existing.Freight = hdr.Freight;
            existing.AdditionalDiscPertge = hdr.AdditionalDiscPertge;
            existing.AdditionalDiscAmnt = hdr.AdditionalDiscAmnt;
            existing.AdditionalDiscountType = hdr.AdditionalDiscountType;
            existing.GrandTotal = hdr.GrandTotal;

            existing.FK_SupplierId = hdr.FK_SupplierId;
            existing.FK_PurchaserId = hdr.FK_PurchaserId;
            existing.FK_ProjectId = hdr.FK_ProjectId;
            existing.PurchaseReturnDate = hdr.PurchaseReturnDate;
            existing.EntryDate = hdr.EntryDate;
            existing.IsDeleted = hdr.IsDeleted;

            // remove old details
            await db.PurchaseReturnDtls.Where(x => x.FK_PurchaseReturnHdrId == hdr.PurchaseReturnId).ExecuteDeleteAsync();

            // Validate and Fix Units (Aligned with PurchaseRepo)
            foreach (var item in entity.PurchaseReturnDtls)
            {
                var itemMaster = await db.ItemMasters.AsNoTracking().FirstOrDefaultAsync(x => x.ItemId == item.FK_ItemId);

                // Fix Base Unit
                item.FK_ItemBaseUnit = itemMaster?.FK_UnitId ?? 0;

                // Fix Transaction Unit if missing
                if (item.FK_UnitId == 0 || item.FK_UnitId == null)
                {
                    item.FK_UnitId = itemMaster?.FK_UnitId ?? 1;
                }
            }

            var details = entity.PurchaseReturnDtls?.Select(d => new PurchaseReturnDtls
            {
                FK_PurchaseReturnHdrId = hdr.PurchaseReturnId,
                FK_ItemId = d.FK_ItemId,
                ItemName = d.ItemName,
                FK_UnitId = d.FK_UnitId,
                Quantity = d.Quantity,
                QtyPerUnit = d.QtyPerUnit,
                ExRate = d.ExRate,
                InRate = d.InRate,
                GrossAmount = d.GrossAmount,
                DiscountPerc = d.DiscountPerc,
                DiscountAmnt = d.DiscountAmnt,
                TaxableAmnt = d.TaxableAmnt,
                TaxPerc = d.TaxPerc,
                TaxAmount = d.TaxAmount,
                LineTotal = d.LineTotal,
                IsDeleted = d.IsDeleted,
                FK_StoreId = d.FK_StoreId
            }).ToList() ?? new List<PurchaseReturnDtls>();

            if (details.Count > 0)
            {
                await db.PurchaseReturnDtls.AddRangeAsync(details);
            }

            // Reverse Old Stock
            await _stockService.ReverseStockMovements(existing.PurchaseReturnNo, "Purchase Return", existing.FK_BranchId, db);

            // New Stock Movement
            var storeId = (await db.StoreMasters.FirstOrDefaultAsync())!.StoreId;
            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = hdr.PurchaseReturnId;
            tHdr.HdrNo = hdr.PurchaseReturnNo;
            tHdr.FK_StoreId = storeId;
            tHdr.PartyId = hdr.FK_SupplierId ?? 0;
            tHdr.VoucherType = "Purchase Return";
            tHdr.Remarks = $"By Purchase Return No: {hdr.PurchaseReturnNo} (Updated)";

            await _stockService.StockMovementForTransaction(db, entity.PurchaseReturnDtls, tHdr, hdr.PurchaseReturnDate.ToDateTime(), false);

            // Update Accounting
            await db.TblAccountings.Where(x => x.VoucherId == existing.PurchaseReturnId && x.Type == "Purchase Return").ExecuteDeleteAsync();
            await PostToLedger(db, existing, details);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.PurchaseReturnId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }


    private async Task PostToLedger(TenantDBContext db, PurchaseReturnHdr hdr, List<PurchaseReturnDtls> details)
    {
        var accountingList = new List<TblAccounting>();

        var purRetAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "420001010001");
        int? purRetAccId = purRetAccount?.AccountId;

        if (purRetAccId == null) throw new Exception("Purchase Account (Code: 420001010001) not found. Please ensure database scripts are executed.");

        // VAT Input Account (Credit - Reversal) - Code: 110003010001
        var vatAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "110003010001");
        int? vatAccId = vatAccount?.AccountId;
        if (vatAccId == null) throw new Exception("VAT Input Account not found (Code: 110003010001).");

        // Debit Account (Based on BillType)
        int? debitAccId = null;
        if (hdr.BillType == "Cash")
        {
            var cashAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Cash in Hand");
            debitAccId = cashAcc?.AccountId;
        }
        else if (hdr.BillType == "Credit")
        {
            var supplier = await db.SupplierMasters.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierId == hdr.FK_SupplierId);
            if (supplier != null)
            {
                debitAccId = supplier.FK_AccountId;
                if (debitAccId == null) // Linkage Fallback
                {
                    var supAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == supplier.Code);
                    debitAccId = supAcc?.AccountId;
                }
            }
        }
        else // Bank, Transfer, Cheque
        {
            var bankAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Bank"));
            debitAccId = bankAcc?.AccountId;
        }

        if (debitAccId == null) throw new Exception($"Could not determine Debit Account for Bill Type '{hdr.BillType}'.");

        decimal netPurchaseReturnValue = hdr.TaxableAmount + (hdr.AdditionalAmount ?? 0) - (hdr.AdditionalDiscAmnt ?? 0);

        if (netPurchaseReturnValue != 0)
        {
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.PurchaseReturnDate ?? DateTime.Now,
                VoucherId = hdr.PurchaseReturnId,
                VoucherNo = hdr.PurchaseReturnNo,
                FK_AccountId = purRetAccId,
                Amount = -netPurchaseReturnValue, // Credit (-ve)
                Narration = $"Purchase Return - {hdr.SupplierName}",
                Type = "Purchase Return",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_SupplierId = hdr.FK_SupplierId
            });
        }

        // B. Credit VAT Input (Tax Amount)
        if (hdr.TaxTotal > 0)
        {
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.PurchaseReturnDate ?? DateTime.Now,
                VoucherId = hdr.PurchaseReturnId,
                VoucherNo = hdr.PurchaseReturnNo,
                FK_AccountId = vatAccId,
                Amount = -hdr.TaxTotal, // Credit (-ve)
                Narration = $"VAT Reversal - {hdr.PurchaseReturnNo}",
                Type = "Purchase Return",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_SupplierId = hdr.FK_SupplierId
            });
        }

        // C. Debit Supplier (Grand Total) - ALWAYS Debit Supplier to record history
        // Use Actual Grand Total
        decimal debitAmount = hdr.GrandTotal;

        // Find Supplier Account
        int? supplierAccId = null;
        var supplierMaster = await db.SupplierMasters.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierId == hdr.FK_SupplierId);
        if (supplierMaster != null)
        {
            supplierAccId = supplierMaster.FK_AccountId;
            if (supplierAccId == null)
            {
                var supAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == supplierMaster.Code);
                supplierAccId = supAcc?.AccountId;
            }
        }

        if (supplierAccId.HasValue && debitAmount > 0)
        {
            // 1. Debit Supplier (Reduce Liability / Create Asset)
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.PurchaseReturnDate ?? DateTime.Now,
                VoucherId = hdr.PurchaseReturnId,
                VoucherNo = hdr.PurchaseReturnNo,
                FK_AccountId = supplierAccId,
                Amount = debitAmount, // Debit (+ve)
                Narration = $"Purchase Return ({hdr.BillType}) - {hdr.PurchaseReturnNo}",
                Type = "Purchase Return",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_SupplierId = hdr.FK_SupplierId
            });

            // 2. If Cash/Bank, Receive Payment Immediately
            if (hdr.BillType != "Credit")
            {
                int? receiptAccId = null;
                if (hdr.BillType == "Cash")
                {
                    var cashAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Cash"));
                    receiptAccId = cashAcc?.AccountId;
                }
                else
                {
                    var bankAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Bank"));
                    receiptAccId = bankAcc?.AccountId;
                }

                if (receiptAccId.HasValue)
                {
                    // A. Credit Supplier (Settle Debit)
                    accountingList.Add(new TblAccounting
                    {
                        EntryDate = hdr.PurchaseReturnDate ?? DateTime.Now,
                        VoucherId = hdr.PurchaseReturnId,
                        VoucherNo = hdr.PurchaseReturnNo,
                        FK_AccountId = supplierAccId,
                        Amount = -debitAmount, // Credit (-ve)
                        Narration = $"Refund Received ({hdr.BillType}) - {hdr.PurchaseReturnNo}",
                        Type = "Purchase Return",
                        BillType = hdr.BillType,
                        FK_BranchId = hdr.FK_BranchId,
                        FK_UserId = hdr.FK_UserId,
                        FK_SupplierId = hdr.FK_SupplierId
                    });

                    // B. Debit Cash/Bank (Increase Asset)
                    accountingList.Add(new TblAccounting
                    {
                        EntryDate = hdr.PurchaseReturnDate ?? DateTime.Now,
                        VoucherId = hdr.PurchaseReturnId,
                        VoucherNo = hdr.PurchaseReturnNo,
                        FK_AccountId = receiptAccId,
                        Amount = debitAmount, // Debit (+ve)
                        Narration = $"Refund from Supplier - {hdr.SupplierName}",
                        Type = "Purchase Return",
                        BillType = hdr.BillType,
                        FK_BranchId = hdr.FK_BranchId,
                        FK_UserId = hdr.FK_UserId,
                        FK_SupplierId = hdr.FK_SupplierId
                    });
                }
            }
        }

        if (accountingList.Count > 0)
        {
            await db.TblAccountings.AddRangeAsync(accountingList);
        }
    }


    public async Task<List<TransactionDtls>> GetPurchaseReturnBalance(int purchaseId)
    {
        await using var db = _contextProvider.CreateDbContext();
        // 1. Get Original Purchase
        var purchaseHdr = await db.Set<PurchaseHdr>().AsNoTracking().FirstOrDefaultAsync(x => x.PurchaseId == purchaseId);
        if (purchaseHdr == null) return new List<TransactionDtls>();

        var purchaseDtls = await db.Set<PurchaseDtls>().AsNoTracking().Where(x => x.FK_PurchaseHdrId == purchaseHdr.PurchaseId).ToListAsync();

        // 2. Get Previous Returns
        // We link by FK_PurchaseId
        var returnDtls = await db.PurchaseReturnDtls.AsNoTracking()
            .Where(x => x.PurchaseReturnHdr.FK_PurchaseId == purchaseId && !x.IsDeleted)
            .ToListAsync();

        var balancerList = new List<TransactionDtls>();

        foreach (var item in purchaseDtls)
        {
            var returnedQty = returnDtls.Where(x => x.FK_ItemId == item.FK_ItemId).Sum(x => x.Quantity);

            decimal balance = item.Quantity - returnedQty;

            if (balance > 0)
            {
                var trans = item.CopyTo<TransactionDtls>();
                trans.HdrId = item.FK_PurchaseHdrId;
                trans.Quantity = balance; // Set Qty to Balance
                // We might want to store OriginalQty somewhere for UI reference, but usually the 'Max' is effectively this balance.
                trans.DtlsId = 0; // New Detail
                trans.FK_StoreId = item.FK_StoreId ?? 0;
                balancerList.Add(trans);
            }
        }

        return balancerList;
    }

    private async Task<bool> HasPaymentPaidAsync(TenantDBContext db, int purchaseId)
    {
        return await db.SupplierPaymentDtls
            .Join(db.SupplierPaymentHdrs,
                dtl => dtl.FK_PaymentId,
                hdr => hdr.PaymentId,
                (dtl, hdr) => new { dtl, hdr })
            .AnyAsync(x => x.dtl.FK_PurchaseId == purchaseId && !x.hdr.IsDeleted && x.dtl.PayingAmount > 0);
    }
}



