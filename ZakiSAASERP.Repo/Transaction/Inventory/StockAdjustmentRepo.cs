using ZakiSAASERP.Interface.Transaction.Inventory;
using ZakiSAASERP.Models.Transactions.Accounts;
using ZakiSAASERP.Models.Transactions.Inventory;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Transaction.Inventory;

public class StockAdjustmentRepo : IStockAdjustmentRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly StockService _stockService;
    private readonly CodeSequenceService _codeService;

    public StockAdjustmentRepo(IDbContextProvider contextProvider, StockService stockService, CodeSequenceService codeService)
    {
        _contextProvider = contextProvider;
        _stockService = stockService;
        _codeService = codeService;
    }

    public async Task<string> GetVoucherNo(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.StockAdjustment,
            async (code) => await db.StockAdjustmentHdrs.AnyAsync(x => x.VoucherNo == code && x.FK_BranchId == branchId)
        );
    }

    public async Task<int> GetNextVchrNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.StockAdjustmentHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == branchId)
                .OrderByDescending(x => x.StockAdjId)
                .Select(x => x.VoucherNo)
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

    public async Task<OutputResponse> CreateAsync(StockAdjustmentHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Clean navigation properties to prevent "IDENTITY_INSERT" errors
            if (hdr.StockAdjustmentDetails != null)
            {
                foreach (var d in hdr.StockAdjustmentDetails)
                {
                    d.Item = null;
                    d.Unit = null;
                    d.Header = null; // Important: break circular reference if any
                }
            }

            // Calculate Header Total
            hdr.TotalAmount = hdr.StockAdjustmentDetails.Sum(x => x.Quantity * x.Rate);

            await db.StockAdjustmentHdrs.AddAsync(hdr); // both hdr and details saving

            await db.SaveChangesAsync();

            // Process Stock
            await PostStock(db, hdr);

            // Standard Accounting Post
            // await PostToLedger(hdr);
            await db.SaveChangesAsync();
            // Consume the code sequence if auto-code is enabled
            if (await _codeService.IsAutoCodeEnabledAsync(CodeKeys.StockAdjustment, db))
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.StockAdjustment, db);
            }

            await transaction.CommitAsync();
            return new OutputResponse(hdr.StockAdjId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<OutputResponse> UpdateAsync(StockAdjustmentHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.StockAdjustmentHdrs
                .Include(x => x.StockAdjustmentDetails)
                .FirstOrDefaultAsync(x => x.StockAdjId == hdr.StockAdjId);

            if (existing != null)
            {
                // Reverse usage of old stock
                await _stockService.ReverseStockMovements(existing.VoucherNo, "Stock Adjustment", existing.FK_BranchId, db);

                // Update Header
                existing.VoucherDate = hdr.VoucherDate;
                existing.ReferenceNo = hdr.ReferenceNo;
                existing.Remarks = hdr.Remarks;
                existing.TotalAmount = hdr.StockAdjustmentDetails.Sum(x => x.Quantity * x.Rate);

                // Remove old details
                db.StockAdjustmentDtls.RemoveRange(existing.StockAdjustmentDetails);

                // Add new details
                foreach (var d in hdr.StockAdjustmentDetails)
                {
                    // Clean navigation properties
                    d.Item = null;
                    d.Unit = null;
                    d.Header = null;

                    d.FK_StockAdjId = existing.StockAdjId;
                    db.StockAdjustmentDtls.Add(d);
                }
                await db.SaveChangesAsync();

                // Process Stock (New inputs)
                await PostStock(db, hdr);

                // Accounting (Delete Old, Post New)
                // await db.TblAccountings.Where(x => x.VoucherId == existing.StockAdjId && x.Type == "Stock Adjustment").ExecuteDeleteAsync();
                // await PostToLedger(hdr);
                await db.SaveChangesAsync(); // Ensure StockTranLog and Accounting saved

                await transaction.CommitAsync();
                return new OutputResponse(hdr.StockAdjId, null, null);
            }
            return new OutputResponse(0, "Record Not Found", null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    private async Task<int> GetAdjustmentAccountId(TenantDBContext db)
    {
        // Based on 10001_105_Data_BuiltInAccounts.sql, there is no specific 'Stock Adjustment' account.
        // We use 'Purchase Account' (Cost of Sales) - Code: 420001010001
        // Adjustments typically affect Cost of Goods Sold.
        var adjHead = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "420001010001");

        if (adjHead != null) return adjHead.AccountId;

        // Fallback checks (e.g. if script changed)
        var general = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Name.Contains("Purchase Account"));
        if (general == null) throw new Exception("Default Account 'Purchase Account' (Code: 420001010001) not found. Please ensure database scripts are executed.");
        return general.AccountId;
    }

    private async Task PostStock(TenantDBContext db, StockAdjustmentHdr hdr)
    {
        var details = hdr.StockAdjustmentDetails;
        var movements = new List<StockMovementDto>();

        // Fetch Base Units for items involved
        var itemIds = details.Select(x => x.FK_ItemId).Distinct().ToList();
        var itemBaseUnits = await db.ItemMasters
            .Where(x => itemIds.Contains(x.ItemId))
            .ToDictionaryAsync(x => x.ItemId, x => x.FK_UnitId);

        int adjAccountId = await GetAdjustmentAccountId(db);

        foreach (var d in details)
        {
            decimal qty = d.Quantity;
            bool isInward = qty >= 0;

            // Determine Base Unit and Conversion
            int baseUnitId = d.FK_UnitId; // Default to Trans Unit if not found
            if (itemBaseUnits.TryGetValue(d.FK_ItemId, out var bUnitId))
            {
                baseUnitId = bUnitId ?? d.FK_UnitId;
            }

            decimal conversion = d.QtyPerUnit > 0 ? d.QtyPerUnit : 1;
            decimal baseQty = Math.Abs(qty) * conversion;
            decimal baseRate = conversion > 0 ? d.Rate / conversion : d.Rate;

            movements.Add(new StockMovementDto
            {
                ItemId = d.FK_ItemId,
                BranchId = hdr.FK_BranchId,
                StoreId = d.FK_StoreId,
                VoucherId = hdr.StockAdjId,
                VoucherNo = hdr.VoucherNo,
                VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                VoucherType = "Stock Adjustment",
                FormType = "Stock Adjustment",
                IsInward = isInward,
                Quantity = Math.Abs(qty), // Transaction Qty
                UnitId = d.FK_UnitId, // Transaction Unit (Following OpeningStockEntryRepo pattern)
                ActualUnitId = baseUnitId, // Base Unit
                ActualQuantity = baseQty, // Base Qty (Critical for Stock Balance)
                Rate = d.Rate,
                ActualRate = baseRate,
                Narration = !string.IsNullOrWhiteSpace(hdr.Remarks) ? hdr.Remarks : $"Stock Adjustment - {hdr.VoucherNo}",
                UserId = hdr.FK_UserId,
                AccountId = adjAccountId
            });
        }

        await _stockService.ProcessStockMovements(movements, db);
    }

    public async Task<StockAdjustmentHdr?> GetByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.StockAdjustmentHdrs
            .Include(x => x.StockAdjustmentDetails)
            .Include(x => x.StockAdjustmentDetails)
            .ThenInclude(x => x.Item)
            .ThenInclude(x => x.MultiUnits) // Include MultiUnits for conversion lookup
            .Include(x => x.StockAdjustmentDetails)
            .ThenInclude(x => x.Unit)
            .FirstOrDefaultAsync(x => x.StockAdjId == id);

        if (hdr != null && hdr.StockAdjustmentDetails != null)
        {
            foreach (var d in hdr.StockAdjustmentDetails)
            {
                if (d.Item != null)
                {
                    d.ItemName = d.Item.ItemName;
                    d.Barcode = d.Item.Barcode;
                }
                if (d.Unit != null)
                {
                    d.UnitName = d.Unit.Name;
                }

                // Resolve QtyPerUnit: Use persisted value if > 0, else lookup Master
                if (d.QtyPerUnit <= 0)
                {
                    decimal factor = 1;
                    if (d.Item != null && d.Item.MultiUnits != null)
                    {
                        var conversion = d.Item.MultiUnits.FirstOrDefault(x => x.FK_UnitId == d.FK_UnitId);
                        if (conversion != null)
                        {
                            factor = conversion.QTYPerUnit ?? 1;
                        }
                    }
                    d.QtyPerUnit = factor;
                }
            }
        }

        return hdr;
    }

    public async Task<OutputResponse> DeleteStockAdjustment(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.StockAdjustmentHdrs.FindAsync(id);
            if (existing == null) return new OutputResponse(0, "Not Found", null);

            existing.IsDeleted = true;

            // Reverse Stock
            await _stockService.ReverseStockMovements(existing.VoucherNo, "Stock Adjustment", existing.FK_BranchId, db);
            // await db.TblAccountings.Where(x => x.VoucherId == existing.StockAdjId && x.Type == "Stock Adjustment").ExecuteDeleteAsync();

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<TransactionPDTO> GetAllAsync(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.StockAdjustmentHdrs
            .Where(x => x.FK_BranchId == pagination.BranchId && !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(pagination.search))
        {
            query = query.Where(x => x.VoucherNo.Contains(pagination.search) ||
                                     (x.ReferenceNo != null && x.ReferenceNo.Contains(pagination.search)));
        }

        var totalCount = await query.CountAsync();

        var list = await query
            .OrderByDescending(x => x.VoucherDate)
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .Select(x => new ExMasters
            {
                Id = x.StockAdjId,
                Code = x.VoucherNo,
                Description = x.VoucherDate.HasValue ? x.VoucherDate.Value.ToString("dd-MM-yyyy") : "",
                Name = "", // Store removed from Header
                Amount = x.TotalAmount
            })
            .ToListAsync();

        return new TransactionPDTO(list, totalCount);
    }
    public async Task<decimal> GetItemCurrentStock(int itemId, int? storeId, int? excludeVoucherId = 0)
    {
        await using var db = _contextProvider.CreateDbContext();
        // Ensure ItemStockBalance is in sync with StockTranLogs
        await _stockService.RecalculateStockBalance(db, itemId);

        var query = db.StockTranLogs
            .Where(x => x.FK_ItemId == itemId);

        if (storeId.HasValue && storeId > 0)
        {
            query = query.Where(x => x.FK_StoreId == storeId);
        }

        if (excludeVoucherId.HasValue && excludeVoucherId > 0)
        {
            query = query.Where(x => !(x.VchrId == excludeVoucherId && (x.VchrType == "Stock Adjustment" || x.VchrFormType == "Stock Adjustment")));
        }

        var stock = await query.SumAsync(x => (x.InQuantity ?? 0) - (x.OutQuantity ?? 0));
        return stock;
    }

    private async Task PostToLedger(TenantDBContext db, StockAdjustmentHdr hdr)
    {
        var accountingList = new List<TblAccounting>();

        // Inventory Account (Asset) - Code: 110004010001
        var stockHead = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.Code == "110004010001");
        if (stockHead == null) throw new Exception("Default Account 'Inventory Asset' (Code: 110004010001) not found.");
        int stockHeadId = stockHead.AccountId;

        // Stock Adjustment Account (Income/Expense)
        int adjHeadId = await GetAdjustmentAccountId(db);

        decimal totalGain = 0;
        decimal totalLoss = 0;

        foreach (var d in hdr.StockAdjustmentDetails)
        {
            decimal val = d.Quantity * d.Rate;
            if (val > 0) totalGain += val;
            else totalLoss += Math.Abs(val);
        }

        string narration = !string.IsNullOrWhiteSpace(hdr.Remarks) ? hdr.Remarks : $"Stock Adjustment - {hdr.VoucherNo}";

        // 1. Handle GAIN (Stock Increase)
        // Debit Inventory, Credit Adjustment
        if (totalGain > 0)
        {
            // Debit Inventory
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.VoucherDate ?? DateTime.Now,
                VoucherId = hdr.StockAdjId,
                VoucherNo = hdr.VoucherNo,
                FK_AccountId = stockHeadId,
                Amount = totalGain, // Debit
                Narration = narration,
                Type = "Stock Adjustment",
                BillType = "Debit",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId
            });

            // Credit Adjustment
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.VoucherDate ?? DateTime.Now,
                VoucherId = hdr.StockAdjId,
                VoucherNo = hdr.VoucherNo,
                FK_AccountId = adjHeadId,
                Amount = -totalGain, // Credit
                Narration = narration,
                Type = "Stock Adjustment",
                BillType = "Credit",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId
            });
        }

        // 2. Handle LOSS (Stock Decrease)
        // Debit Adjustment, Credit Inventory
        if (totalLoss > 0)
        {
            // Debit Adjustment
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.VoucherDate ?? DateTime.Now,
                VoucherId = hdr.StockAdjId,
                VoucherNo = hdr.VoucherNo,
                FK_AccountId = adjHeadId,
                Amount = totalLoss, // Debit
                Narration = narration,
                Type = "Stock Adjustment",
                BillType = "Debit",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId
            });

            // Credit Inventory
            accountingList.Add(new TblAccounting
            {
                EntryDate = hdr.VoucherDate ?? DateTime.Now,
                VoucherId = hdr.StockAdjId,
                VoucherNo = hdr.VoucherNo,
                FK_AccountId = stockHeadId,
                Amount = -totalLoss, // Credit
                Narration = narration,
                Type = "Stock Adjustment",
                BillType = "Credit",
                FK_BranchId = hdr.FK_BranchId,
                FK_UserId = hdr.FK_UserId
            });
        }

        if (accountingList.Count > 0)
        {
            await db.TblAccountings.AddRangeAsync(accountingList);
        }
    }
}
