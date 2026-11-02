using ZakiSAASERP.Interface.Transaction.Purchase;
using ZakiSAASERP.Models.Transactions.Purchase;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;
using AshproStringExtension;
using Microsoft.Extensions.Logging;
namespace ZakiSAASERP.Repo.Transaction.Purchase;

using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Transactions.Accounts;
using ZakiSAASERP.Repo.Transaction.Inventory;

public class PurchaseRepo : IPurchaseRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly ILogger<PurchaseRepo> _logger;
    private readonly StockService _stockService;

    public PurchaseRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, ILogger<PurchaseRepo> logger, StockService stockService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _logger = logger;
        _stockService = stockService;
    }

    public async Task<string> GetNextPurchaseNoAsync()
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.Purchase,
            async (code) => await db.Set<PurchaseHdr>().AnyAsync(x => x.PurchaseNo == code)
        );
    }

    public async Task<OutputResponse> CreateAsync(PurchaseDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.Hdr;
            var dtls = entity.Dtls;

            bool isAuto = await _codeService.IsAutoCodeEnabledAsync(CodeKeys.Purchase, db);
            if (!isAuto)
            {
                if (string.IsNullOrEmpty(hdr.PurchaseNo) || hdr.PurchaseNo == "0")
                {
                    var next = await GetNextVchrNo(hdr.FK_BranchId);
                    hdr.PurchaseNo = next.ToString();
                }
                else
                {
                    if (await db.Set<PurchaseHdr>().AnyAsync(x => x.PurchaseNo == hdr.PurchaseNo && x.FK_BranchId == hdr.FK_BranchId))
                    {
                        return new OutputResponse(0, "NoAlreadyExists");
                    }
                }
            }
            else if (string.IsNullOrEmpty(hdr.PurchaseNo) || hdr.PurchaseNo == "0")
            {
                // Fallback: If Auto but frontend didn't send a code, generate it here
                hdr.PurchaseNo = await _codeService.GetNextUniqueCodeAsync(
                    CodeKeys.Purchase,
                    async (code) => await db.Set<PurchaseHdr>().AnyAsync(x => x.PurchaseNo == code)
                );
            }

            await db.Set<PurchaseHdr>().AddAsync(hdr);
            if (hdr.FK_ProjectId == 0) hdr.FK_ProjectId = null;
            if (hdr.FK_PurchaserId == 0) hdr.FK_PurchaserId = null;
            if (hdr.FK_StoreId == 0) hdr.FK_StoreId = null;
            if (hdr.FK_CurrencyId == 0) hdr.FK_CurrencyId = null;
            if (hdr.FK_AccountId == 0) hdr.FK_AccountId = null;

            await db.SaveChangesAsync();

            if (isAuto)
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.Purchase, db);
            }

            int headerId = hdr.PurchaseId;

            // Process Stock Movements (Following Sales Invoice Pattern)
            var storeId = hdr.FK_StoreId ?? (await db.StoreMasters.FirstOrDefaultAsync())?.StoreId ?? 1;

            var details = new List<PurchaseDtls>();
            foreach (var item in dtls)
            {
                var dtl = item.CopyTo<PurchaseDtls>();
                dtl.FK_PurchaseHdrId = headerId;
                dtl.FK_StoreId = item.FK_StoreId == 0 ? storeId : item.FK_StoreId;
                details.Add(dtl);
            }

            await db.Set<PurchaseDtls>().AddRangeAsync(details);

            foreach (var item in dtls)
            {
                item.FK_ItemBaseUnit = (await db.ItemMasters.FirstOrDefaultAsync(x => x.ItemId == item.FK_ItemId))?.FK_UnitId ?? 0;
            }


            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = headerId;
            tHdr.HdrNo = hdr.PurchaseNo;
            tHdr.InvoiceDate = (hdr.PurchaseDate ?? DateTime.Now).ToString("dd-MM-yyyy");
            tHdr.FK_StoreId = storeId;
            tHdr.FK_BranchId = hdr.FK_BranchId;
            tHdr.FK_UserId = hdr.FK_UserId;
            tHdr.PartyId = hdr.FK_SupplierId;
            tHdr.VoucherType = "Purchase";
            tHdr.Remarks = $"By Purchase No: {hdr.PurchaseNo}";

            await _stockService.StockMovementForTransaction(db, dtls, tHdr, hdr.PurchaseDate.ToDateTime(), true); // true = Inward


            // Accounting Entries
            await PostToLedger(db, hdr, details);

            // Update Item Master Last Purchase Rate
            foreach (var item in details)
            {
                var itemMaster = await db.ItemMasters.FindAsync(item.FK_ItemId);
                if (itemMaster != null)
                {
                    // Update Purchase Rate
                    itemMaster.PurchaseRate = item.InRate > 0 ? item.InRate : item.ExRate;
                }
            }

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



    public async Task<(List<ExMasters> PTransactions, int TotalCount)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.Set<PurchaseHdr>().AsNoTracking().Include(x => x.Supplier).Where(x => !x.IsDeleted).AsQueryable();

        // Filter by branch FIRST
        query = query.Where(x => x.FK_BranchId == pagination.BranchId);

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => (x.PurchaseNo ?? string.Empty).ToLower().Contains(term) ||
                                    (x.RefNo ?? string.Empty).ToLower().Contains(term) ||
                                    (x.InvoiceNo ?? string.Empty).ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.PurchaseNo) : query.OrderBy(x => x.PurchaseNo),
                "Name" => desc ? query.OrderByDescending(x => x.Supplier!.Name) : query.OrderBy(x => x.Supplier!.Name),
                "Description" => desc ? query.OrderByDescending(x => x.PurchaseDate) : query.OrderBy(x => x.PurchaseDate),
                _ => desc ? query.OrderByDescending(x => x.PurchaseId) : query.OrderBy(x => x.PurchaseId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.PurchaseId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();

        var finalList = list.Select(e => new ExMasters
        {
            Id = e.PurchaseId,
            Name = e.SupplierName,
            Code = e.PurchaseNo ?? string.Empty,
            IsSelected = true,
            Amount = e.GrandTotal,
            ExtraField = e.RefNo,
            Description = e.PurchaseDate.HasValue ? e.PurchaseDate.Value.ToString("dd-MM-yyyy") : ""
        }).ToList();

        return (finalList, count);
    }

    public async Task<PurchaseDTO> GetAsync(int Id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.Set<PurchaseHdr>().AsNoTracking().FirstOrDefaultAsync(x => x.PurchaseId == Id);
        if (hdr == null) return null!;

        var dtls = await db.Set<PurchaseDtls>()
            .AsNoTracking()
            .Where(x => x.FK_PurchaseHdrId == Id)
            .ToListAsync();

        var mapped = dtls.Select(d => new TransactionDtls
        {
            DtlsId = d.PurchaseDtlsId,
            HdrId = d.FK_PurchaseHdrId,
            FK_ItemId = d.FK_ItemId,
            ItemName = d.ItemName,
            Barcode = d.Barcode,
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
            FK_StoreId = d.FK_StoreId ?? 0
        }).ToList();

        return new PurchaseDTO(hdr, mapped);
    }

    public async Task<int> GetNextVchrNo(int BranchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.Set<PurchaseHdr>()
                .AsNoTracking()
                .Where(x => x.FK_BranchId == BranchId)
                .OrderByDescending(x => x.PurchaseId)
                .Select(x => x.PurchaseNo)
                .Take(20)
                .ToListAsync();

            if (codes.Count == 0) return 1;

            int maxVal = 0;
            foreach (var code in codes)
            {
                if (int.TryParse(code, out int val))
                {
                    if (val > maxVal) maxVal = val;
                }
            }

            return maxVal + 1;
        }
        catch
        {
            return 1;
        }
    }

    public async Task<OutputResponse> UpdateAsync(PurchaseDTO entity)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.Hdr;
            var existing = await db.Set<PurchaseHdr>().FindAsync(hdr.PurchaseId);
            if (existing is null) return new OutputResponse(0, "Record not found.");

            if (hdr.IsDeleted) // Cancellation Logic
            {
                existing.IsDeleted = true;

                // Mark Details as Deleted
                await db.Set<PurchaseDtls>().Where(x => x.FK_PurchaseHdrId == existing.PurchaseId)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));

                // Reverse Stock
                await _stockService.ReverseStockMovements(existing.PurchaseNo, "Purchase", existing.FK_BranchId, db);

                // Remove Accounting Entries
                await db.TblAccountings.Where(x => x.VoucherId == existing.PurchaseId && x.Type == "Purchase").ExecuteDeleteAsync();

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.PurchaseId, "Purchase Cancelled Successfully", null);
            }

            // Check for returns before update
            if (await db.PurchaseReturnHdrs.AnyAsync(x => x.FK_PurchaseId == existing.PurchaseId))
            {
                return new OutputResponse(0, "Cannot edit Purchase because it has associated Returns.");
            }

            string oldNo = existing.PurchaseNo;

            if (hdr.PurchaseNo != existing.PurchaseNo)
            {
                if (await db.Set<PurchaseHdr>().AnyAsync(x => x.PurchaseNo == hdr.PurchaseNo && x.FK_BranchId == hdr.FK_BranchId && x.PurchaseId != hdr.PurchaseId))
                {
                    return new OutputResponse(0, "NoAlreadyExists");
                }
            }

            // Update header properties
            existing.PurchaseNo = hdr.PurchaseNo;
            existing.SupplierName = hdr.SupplierName;
            existing.RefNo = hdr.RefNo;
            existing.InvoiceNo = hdr.InvoiceNo;
            existing.PONo = hdr.PONo;
            existing.PODate = hdr.PODate;
            existing.GoodsRecNo = hdr.GoodsRecNo;
            existing.RFQNo = hdr.RFQNo;
            existing.GrossAmount = hdr.GrossAmount;
            existing.LineDsicount = hdr.LineDsicount;
            existing.TaxableAmount = hdr.TaxableAmount;
            existing.TaxTotal = hdr.TaxTotal;
            existing.NetAmount = hdr.NetAmount;
            existing.AdditionalAmount = hdr.AdditionalAmount;
            existing.AdditionalDiscPertge = hdr.AdditionalDiscPertge;
            existing.AdditionalDiscAmnt = hdr.AdditionalDiscAmnt;
            existing.AdditionalDiscountType = hdr.AdditionalDiscountType;
            existing.GrandTotal = hdr.GrandTotal;
            existing.BillType = hdr.BillType;
            existing.ReceivedAmount = hdr.ReceivedAmount;
            existing.PaymentAmount = hdr.PaymentAmount;
            existing.SupplierVatNo = hdr.SupplierVatNo;
            existing.Remarks = hdr.Remarks;
            existing.Narration = hdr.Narration;
            existing.Purchaser = hdr.Purchaser;
            existing.IsDeleted = hdr.IsDeleted;
            existing.FK_BranchId = hdr.FK_BranchId;
            existing.FK_SupplierId = hdr.FK_SupplierId;
            existing.FK_UserId = hdr.FK_UserId;
            existing.FK_FinancialYearId = hdr.FK_FinancialYearId;
            existing.FK_PurchaserId = (hdr.FK_PurchaserId == 0) ? null : hdr.FK_PurchaserId;
            existing.FK_ProjectId = (hdr.FK_ProjectId == 0) ? null : hdr.FK_ProjectId;
            existing.FK_StoreId = (hdr.FK_StoreId == 0) ? null : hdr.FK_StoreId;
            existing.FK_CurrencyId = (hdr.FK_CurrencyId == 0) ? null : hdr.FK_CurrencyId;
            existing.FK_AccountId = (hdr.FK_AccountId == 0) ? null : hdr.FK_AccountId;
            existing.PurchaseDate = hdr.PurchaseDate;
            existing.EntryDate = hdr.EntryDate;

            // Remove old details
            await db.Set<PurchaseDtls>().Where(x => x.FK_PurchaseHdrId == hdr.PurchaseId).ExecuteDeleteAsync();

            // Add new stock movements (Following Sales Invoice Pattern)
            var storeId = hdr.FK_StoreId ?? (await db.StoreMasters.FirstOrDefaultAsync())?.StoreId ?? 1;

            var details = entity.Dtls?.Select(d => new PurchaseDtls
            {
                FK_PurchaseHdrId = hdr.PurchaseId,
                FK_ItemId = d.FK_ItemId,
                ItemName = d.ItemName,
                Barcode = d.Barcode,
                FK_UnitId = d.FK_UnitId,
                Quantity = d.Quantity,
                QtyPerUnit = d.QtyPerUnit,
                ExRate = d.ExRate,
                InRate = d.InRate,
                DiscountPerc = d.DiscountPerc,
                TaxPerc = d.TaxPerc,
                IsDeleted = d.IsDeleted,
                FK_StoreId = d.FK_StoreId == 0 ? storeId : d.FK_StoreId
            }).ToList() ?? new List<PurchaseDtls>();

            if (details.Count > 0)
            {
                await db.Set<PurchaseDtls>().AddRangeAsync(details);
            }

            // Reverse old stock movements
            await _stockService.ReverseStockMovements(oldNo, "Purchase", existing.FK_BranchId, db);

            // Add new stock movements (Following Sales Invoice Pattern)
            foreach (var item in entity.Dtls)
            {
                item.FK_ItemBaseUnit = (await db.ItemMasters.FirstOrDefaultAsync(x => x.ItemId == item.FK_ItemId))?.FK_UnitId ?? 0;
            }


            var tHdr = hdr.CopyTo<TransactionHdr>();
            tHdr.HdrId = hdr.PurchaseId;
            tHdr.HdrNo = hdr.PurchaseNo;
            tHdr.InvoiceDate = (hdr.PurchaseDate ?? DateTime.Now).ToString("dd-MM-yyyy");
            tHdr.FK_StoreId = storeId;
            tHdr.FK_BranchId = hdr.FK_BranchId;
            tHdr.FK_UserId = hdr.FK_UserId;
            tHdr.PartyId = hdr.FK_SupplierId;
            tHdr.VoucherType = "Purchase";
            tHdr.Remarks = $"By Purchase No: {hdr.PurchaseNo} (Updated)";

            await _stockService.StockMovementForTransaction(db, entity.Dtls, tHdr, hdr.PurchaseDate.ToDateTime(), true); // true = Inward


            // Update Accounting: Delete old and create new
            await db.TblAccountings.Where(x => x.VoucherId == existing.PurchaseId && x.Type == "Purchase").ExecuteDeleteAsync();
            await PostToLedger(db, existing, details);

            // Update Item Master Last Purchase Rate
            foreach (var item in details)
            {
                var itemMaster = await db.ItemMasters.FindAsync(item.FK_ItemId);
                if (itemMaster != null)
                {
                    itemMaster.PurchaseRate = item.InRate > 0 ? item.InRate : item.ExRate;
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.PurchaseId, null);
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

            var hdr = await db.Set<PurchaseHdr>().FindAsync(id);
            if (hdr != null)
            {
                // Check for returns before delete
                if (await db.PurchaseReturnHdrs.AnyAsync(x => x.FK_PurchaseId == hdr.PurchaseId))
                {
                    return new OutputResponse(0, "Cannot delete Purchase because it has associated Returns.");
                }

                // Reverse Stock
                await _stockService.ReverseStockMovements(hdr.PurchaseNo, "Purchase", hdr.FK_BranchId, db);

                // Remove Accounting Entries
                await db.TblAccountings.Where(x => x.VoucherId == hdr.PurchaseId && x.Type == "Purchase").ExecuteDeleteAsync();

                // Soft Delete - Mark as deleted instead of hard delete
                hdr.IsDeleted = true;

                // Mark Details as Deleted
                await db.Set<PurchaseDtls>().Where(x => x.FK_PurchaseHdrId == id)
                      .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true));
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }

    private async Task PostToLedger(TenantDBContext db, PurchaseHdr hdr, List<PurchaseDtls> details)
    {
        var accountingList = new List<TblAccounting>();

        // 1. Find Accounts FROM AccountMaster
        // Purchase Account (Debit) - Code: 420001010001
        var purchaseAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "420001010001");
        if (purchaseAccount == null)
            throw new Exception("Default Account 'Purchase Account' (Code: 420001010001) not found. Please ensure database scripts are executed.");
        int? purchaseAccId = purchaseAccount.AccountId;

        // VAT Input Account (Debit) - Code: 110003010001
        var vatAccount = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "110003010001");
        if (vatAccount == null)
            throw new Exception("Default Account 'VAT Input' (Code: 110003010001) not found. Please ensure database scripts are executed.");
        int? vatAccId = vatAccount.AccountId;

        // Credit Account (Based on BillType)
        int? creditAccId = null;
        if (hdr.BillType == "Cash")
        {
            var cashAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name == "Cash in Hand");
            creditAccId = cashAcc?.AccountId;
        }
        else if (hdr.BillType == "Credit") // Supplier
        {
            // Look up Account by FK_AccountId in SupplierMaster (Correct Linkage)
            var supplier = await db.SupplierMasters.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierId == hdr.FK_SupplierId);
            if (supplier != null && supplier.FK_AccountId.HasValue)
            {
                creditAccId = supplier.FK_AccountId;
            }
            // Fallback to Code matching only if Linkage is missing (Legacy Support)
            else if (supplier != null)
            {
                var supAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == supplier.Code);
                creditAccId = supAcc?.AccountId;
            }
        }
        else // Bank / Card / Transfer
        {
            var bankAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Bank"));
            creditAccId = bankAcc?.AccountId;
        }

        if (creditAccId == null)
            throw new Exception($"Could not determine Credit Account for Bill Type '{hdr.BillType}'. Please check Supplier Account Linkage or Bank/Cash Accounts.");

        // 2. Create Entries

        // A. Debit Purchase Account (Taxable Amount)
        decimal purchaseValue = hdr.TaxableAmount;
        if (purchaseAccId.HasValue && purchaseValue > 0)
        {
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.PurchaseDate ?? DateTime.Now,
                VoucherId = hdr.PurchaseId,
                VoucherNo = hdr.PurchaseNo,
                FK_AccountId = purchaseAccId,
                Amount = purchaseValue, // Debit (+ve)
                Narration = $"Purchase - {hdr.SupplierName}",
                Type = "Purchase",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_SupplierId = hdr.FK_SupplierId
            });
        }

        // B. Debit VAT Account
        if (vatAccId.HasValue && hdr.TaxTotal > 0)
        {
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.PurchaseDate ?? DateTime.Now,
                VoucherId = hdr.PurchaseId,
                VoucherNo = hdr.PurchaseNo,
                FK_AccountId = vatAccId,
                Amount = hdr.TaxTotal, // Debit (+ve)
                Narration = $"VAT Input - {hdr.PurchaseNo}",
                Type = "Purchase",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_SupplierId = hdr.FK_SupplierId
            });
        }

        // C. Debit Freight/Expense Account (Additional Amount)
        if (hdr.AdditionalAmount > 0)
        {
            // Priority 1: Use Built-in "Delivery Expense" (Code: 410003010001) as proxy for Freight/Additional Charges
            var freightAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "410003010001");

            // Priority 2: Search by Name "Freight" or "Expense"
            if (freightAcc == null)
            {
                freightAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Freight") || x.Name.Contains("Expense"));
            }

            // Fallback: Purchase Account
            int? expenseAccId = freightAcc?.AccountId ?? purchaseAccId;

            if (expenseAccId.HasValue)
            {
                accountingList.Add(new TblAccounting
                {
                    EntryDate = hdr.PurchaseDate ?? DateTime.Now,
                    VoucherId = hdr.PurchaseId,
                    VoucherNo = hdr.PurchaseNo,
                    FK_AccountId = expenseAccId,
                    Amount = hdr.AdditionalAmount.Value, // Debit (+ve)
                    Narration = $"Add. Charges - {hdr.PurchaseNo}",
                    Type = "Purchase",
                    BillType = hdr.BillType,
                    FK_BranchId = hdr.FK_BranchId,
                    FK_UserId = hdr.FK_UserId,
                    FK_SupplierId = hdr.FK_SupplierId
                });
            }
        }

        // D. Credit Discount Received (Additional Discount)
        if (hdr.AdditionalDiscAmnt > 0)
        {
            // Priority 1: Use Built-in "Discount Received" (Code: 310002010001)
            var discAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "310002010001");

            // Priority 2: Search by Name "Discount Received"
            if (discAcc == null)
            {
                discAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Discount Received"));
            }

            // Fallback: Reduce from Purchase Account (Credit Purchase Account)
            // We prefer using 'Discount Received' (Income) => Credit. 
            // If missing, Crediting Purchase Account effectively reduces the Expense, which is also mathematically correct for the ledger but semantically different.
            int? discAccId = discAcc?.AccountId;

            if (discAccId.HasValue)
            {
                accountingList.Add(new TblAccounting
                {
                    EntryDate = hdr.PurchaseDate ?? DateTime.Now,
                    VoucherId = hdr.PurchaseId,
                    VoucherNo = hdr.PurchaseNo,
                    FK_AccountId = discAccId,
                    Amount = -hdr.AdditionalDiscAmnt.Value, // Credit (-ve)
                    Narration = $"Discount Received - {hdr.PurchaseNo}",
                    Type = "Purchase",
                    BillType = hdr.BillType,
                    FK_BranchId = hdr.FK_BranchId,
                    FK_UserId = hdr.FK_UserId,
                    FK_SupplierId = hdr.FK_SupplierId
                });
            }
            else
            {
                // If no specific discount account, treating as reduction in Purchase Cost (Credit Purchase Account) is acceptable fallback
                accountingList.Add(new TblAccounting
                {
                    EntryDate = hdr.PurchaseDate ?? DateTime.Now,
                    VoucherId = hdr.PurchaseId,
                    VoucherNo = hdr.PurchaseNo,
                    FK_AccountId = purchaseAccId, // Reduce Purchase Cost
                    Amount = -hdr.AdditionalDiscAmnt.Value, // Credit (-ve)
                    Narration = $"Discount - {hdr.PurchaseNo}",
                    Type = "Purchase",
                    BillType = hdr.BillType,
                    FK_BranchId = hdr.FK_BranchId,
                    FK_UserId = hdr.FK_UserId,
                    FK_SupplierId = hdr.FK_SupplierId
                });
            }
        }

        // E. Credit Supplier (Grand Total) - ALWAYS Credit Supplier first to record history
        // Ensure Balance: Total Debit = Total Credit
        // Debits = purchaseValue (Taxable) + TaxTotal + AdditionalAmount
        // Credits = Discount + SupplierAmount
        // Therefore: SupplierAmount = (Debits) - Discount

        decimal totalDebits = 0;
        foreach (var entry in accountingList.Where(x => (x.Amount ?? 0) > 0))
        {
            totalDebits += entry.Amount ?? 0;
        }

        decimal otherCredits = 0; // e.g. Discount
        foreach (var entry in accountingList.Where(x => (x.Amount ?? 0) < 0))
        {
            otherCredits += Math.Abs(entry.Amount ?? 0); // Sum absolute values of credits
        }

        // Expected Supplier Credit (Liability)
        decimal supplierCreditAmount = totalDebits - otherCredits;

        // Find Supplier Account ID
        int? supplierAccId = null;
        var supplierMaster = await db.SupplierMasters.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierId == hdr.FK_SupplierId);
        if (supplierMaster != null)
        {
            if (supplierMaster.FK_AccountId.HasValue)
                supplierAccId = supplierMaster.FK_AccountId;
            else
            {
                var supAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == supplierMaster.Code);
                supplierAccId = supAcc?.AccountId;
            }
        }

        if (supplierAccId.HasValue && supplierCreditAmount > 0)
        {
            // 1. Credit Supplier (Liability Creation)
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.PurchaseDate ?? DateTime.Now,
                VoucherId = hdr.PurchaseId,
                VoucherNo = hdr.PurchaseNo,
                FK_AccountId = supplierAccId,
                Amount = -supplierCreditAmount, // Credit (-ve)
                Narration = $"Purchase Invoice ({hdr.BillType}) - {hdr.PurchaseNo}",
                Type = "Purchase",
                BillType = hdr.BillType,
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId,
                FK_SupplierId = hdr.FK_SupplierId
            });

            // 2. If Cash/Bank, Settle Liability Immediately
            if (hdr.BillType != "Credit")
            {
                // Find Payment Account (Cash/Bank)
                int? paymentAccId = null;
                if (hdr.BillType == "Cash")
                {
                    var cashAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Cash"));
                    paymentAccId = cashAcc?.AccountId;
                }
                else // Bank / Card / Transfer
                {
                    var bankAcc = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Bank"));
                    paymentAccId = bankAcc?.AccountId;
                }

                if (paymentAccId.HasValue)
                {
                    // A. Debit Supplier (Reduce Liability)
                    accountingList.Add(new TblAccounting
                    {
                        EntryDate = hdr.PurchaseDate ?? DateTime.Now,
                        VoucherId = hdr.PurchaseId,
                        VoucherNo = hdr.PurchaseNo,
                        FK_AccountId = supplierAccId,
                        Amount = supplierCreditAmount, // Debit (+ve)
                        Narration = $"Payment ({hdr.BillType}) - {hdr.PurchaseNo}",
                        Type = "Purchase",
                        BillType = hdr.BillType,
                        FK_BranchId = hdr.FK_BranchId,
                        FK_UserId = hdr.FK_UserId
                    });

                    // B. Credit Cash/Bank (Reduce Asset)
                    accountingList.Add(new TblAccounting
                    {
                        EntryDate = hdr.PurchaseDate ?? DateTime.Now,
                        VoucherId = hdr.PurchaseId,
                        VoucherNo = hdr.PurchaseNo,
                        FK_AccountId = paymentAccId,
                        Amount = -supplierCreditAmount, // Credit (-ve)
                        Narration = $"Paid to Supplier - {hdr.SupplierName}",
                        Type = "Purchase",
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

}



