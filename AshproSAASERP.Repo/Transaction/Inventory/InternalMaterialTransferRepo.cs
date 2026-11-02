using AshproSAASERP.Interface.Transaction.Inventory;
using AshproSAASERP.Models.Transactions.Inventory;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Repo.CodeGenerationServices;

namespace AshproSAASERP.Repo.Transaction.Inventory;

public class InternalMaterialTransferRepo : IInternalMaterialTransferRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly StockService _stockService;

    public InternalMaterialTransferRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, StockService stockService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _stockService = stockService;
    }

    public async Task<string> GetVoucherNo(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.InternalMaterialTransfer,
            async (code) => await db.InternalMaterialTransferHdrs.AnyAsync(x => x.VoucherNo == code && x.FK_BranchId == branchId)
        );
    }

    public async Task<int> GetNextVchrNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.InternalMaterialTransferHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == branchId)
                .OrderByDescending(x => x.IntMatTransId)
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

    public async Task<OutputResponse> CreateAsync(InternalMaterialTransferHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Clean navigation properties
            if (hdr.InternalMaterialTransferDetails != null)
            {
                foreach (var d in hdr.InternalMaterialTransferDetails)
                {
                    d.Item = null;
                    d.Unit = null;
                    d.Header = null;
                }
            }

            // Recalculate Totals
            hdr.TotalAmount = hdr.InternalMaterialTransferDetails.Sum(x => x.Amount);
            hdr.TotalQty = hdr.InternalMaterialTransferDetails.Sum(x => x.Quantity);

            await db.InternalMaterialTransferHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();

            // Stock Movement: OUT from FromStore and IN to ToStore (Same Branch)
            var movements = new List<StockMovementDto>();

            foreach (var item in hdr.InternalMaterialTransferDetails)
            {
                var itemMaster = await db.ItemMasters.FindAsync(item.FK_ItemId);
                int baseUnitId = itemMaster?.FK_UnitId ?? item.FK_UnitId;

                // Priority: User Input > MultiUnit Config > Default 1
                decimal qtyPerUnit = item.QtyPerUnit > 0 ? item.QtyPerUnit : 1;

                if (qtyPerUnit == 1 && item.FK_UnitId != baseUnitId && item.QtyPerUnit <= 0)
                {
                    var multiUnit = await db.ItemMultiUnits
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.FK_ItemId == item.FK_ItemId && x.FK_UnitId == item.FK_UnitId);
                    qtyPerUnit = multiUnit?.QTYPerUnit ?? 1;
                }

                decimal baseQty = item.Quantity * qtyPerUnit;
                decimal baseRate = item.Rate / qtyPerUnit;

                // 1. Stock OUT from FromStore
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = hdr.FK_BranchId,
                    StoreId = hdr.FK_FromStoreId, // Use Item Store
                    VoucherId = hdr.IntMatTransId,
                    VoucherNo = hdr.VoucherNo,
                    VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                    VoucherType = "Internal Material Transfer",
                    FormType = "IMT-OUT",
                    IsInward = false,
                    Quantity = item.Quantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Internal Transfer OUT [{hdr.ReferenceNo}] - {hdr.Remarks}",
                    UserId = hdr.FK_UserId
                });

                // 2. Stock IN to ToStore
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = hdr.FK_BranchId,
                    StoreId = hdr.FK_ToStoreId, // Use Item Store
                    VoucherId = hdr.IntMatTransId,
                    VoucherNo = hdr.VoucherNo,
                    VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                    VoucherType = "Internal Material Transfer",
                    FormType = "IMT-IN",
                    IsInward = true,
                    Quantity = item.Quantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Internal Transfer IN [{hdr.ReferenceNo}] - {hdr.Remarks}",
                    UserId = hdr.FK_UserId
                });
            }

            await _stockService.ProcessStockMovements(movements, db);

            // Auto-code consumption
            if (await _codeService.IsAutoCodeEnabledAsync(CodeKeys.InternalMaterialTransfer, db))
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.InternalMaterialTransfer, db);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(hdr.IntMatTransId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<OutputResponse> UpdateAsync(InternalMaterialTransferHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.InternalMaterialTransferHdrs
                .Include(x => x.InternalMaterialTransferDetails)
                .FirstOrDefaultAsync(x => x.IntMatTransId == hdr.IntMatTransId);

            if (existing == null)
                return new OutputResponse(0, "Transaction not found", null);

            // Reverse old stock movements
            await _stockService.ReverseStockMovements(existing.VoucherNo, "IMT-OUT", existing.FK_BranchId, db);
            await _stockService.ReverseStockMovements(existing.VoucherNo, "IMT-IN", existing.FK_BranchId, db);

            // Remove old details
            db.InternalMaterialTransferDtls.RemoveRange(existing.InternalMaterialTransferDetails);

            // Update header
            existing.VoucherNo = hdr.VoucherNo;
            existing.VoucherDate = hdr.VoucherDate;
            existing.ReferenceNo = hdr.ReferenceNo;
            existing.Remarks = hdr.Remarks;
            existing.EnteredBy = hdr.EnteredBy;
            existing.FK_BranchId = hdr.FK_BranchId;
            existing.FK_FromStoreId = hdr.FK_FromStoreId;
            existing.FK_ToStoreId = hdr.FK_ToStoreId;
            existing.TotalAmount = hdr.InternalMaterialTransferDetails.Sum(x => x.Amount);
            existing.TotalQty = hdr.InternalMaterialTransferDetails.Sum(x => x.Quantity);

            // Add new details
            foreach (var detail in hdr.InternalMaterialTransferDetails)
            {
                detail.FK_IntMatTransId = existing.IntMatTransId;
                detail.Item = null;
                detail.Unit = null;
                detail.Header = null;

                existing.InternalMaterialTransferDetails.Add(detail);
            }

            await db.SaveChangesAsync();

            // Apply new stock movements
            var movements = new List<StockMovementDto>();

            foreach (var item in existing.InternalMaterialTransferDetails)
            {
                var itemMaster = await db.ItemMasters.FindAsync(item.FK_ItemId);
                int baseUnitId = itemMaster?.FK_UnitId ?? item.FK_UnitId;

                decimal qtyPerUnit = item.QtyPerUnit > 0 ? item.QtyPerUnit : 1;

                if (qtyPerUnit == 1 && item.FK_UnitId != baseUnitId && item.QtyPerUnit <= 0)
                {
                    var multiUnit = await db.ItemMultiUnits
                       .AsNoTracking()
                       .FirstOrDefaultAsync(x => x.FK_ItemId == item.FK_ItemId && x.FK_UnitId == item.FK_UnitId);
                    qtyPerUnit = multiUnit?.QTYPerUnit ?? 1;
                }

                decimal baseQty = item.Quantity * qtyPerUnit;
                decimal baseRate = item.Rate / qtyPerUnit;

                // OUT from FromStore
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = existing.FK_BranchId,
                    StoreId = hdr.FK_FromStoreId, // Use Item Store
                    VoucherId = existing.IntMatTransId,
                    VoucherNo = existing.VoucherNo,
                    VoucherDate = existing.VoucherDate ?? DateTime.Now,
                    VoucherType = "Internal Material Transfer",
                    FormType = "IMT-OUT",
                    IsInward = false,
                    Quantity = item.Quantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Internal Transfer OUT [{existing.ReferenceNo}] - {existing.Remarks}",
                    UserId = existing.FK_UserId
                });

                // IN to ToStore
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = existing.FK_BranchId,
                    StoreId = hdr.FK_ToStoreId, // Use Item Store

                    VoucherId = existing.IntMatTransId,
                    VoucherNo = existing.VoucherNo,
                    VoucherDate = existing.VoucherDate ?? DateTime.Now,
                    VoucherType = "Internal Material Transfer",
                    FormType = "IMT-IN",
                    IsInward = true,
                    Quantity = item.Quantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Internal Transfer IN [{existing.ReferenceNo}] - {existing.Remarks}",
                    UserId = existing.FK_UserId
                });
            }

            await _stockService.ProcessStockMovements(movements, db);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.IntMatTransId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<InternalMaterialTransferHdr?> GetByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.InternalMaterialTransferHdrs
            .Include(x => x.InternalMaterialTransferDetails)
            .ThenInclude(d => d.Item)
            .ThenInclude(i => i.MultiUnits)
            .Include(x => x.InternalMaterialTransferDetails)
            .ThenInclude(d => d.Unit)
            .Include(x => x.FromStore)
            .Include(x => x.ToStore)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.IntMatTransId == id && !x.IsDeleted);

        if (hdr != null)
        {
            if (string.IsNullOrEmpty(hdr.EnteredBy) && hdr.User != null)
            {
                hdr.EnteredBy = hdr.User.UserName;
            }
        }

        if (hdr != null && hdr.InternalMaterialTransferDetails != null)
        {
            int sl = 1;
            foreach (var detail in hdr.InternalMaterialTransferDetails)
            {
                detail.SL = sl++;

                if (detail.Item != null)
                {
                    if (string.IsNullOrEmpty(detail.ItemName)) detail.ItemName = detail.Item.ItemName;
                    if (string.IsNullOrEmpty(detail.Barcode)) detail.Barcode = detail.Item.Barcode;

                    if (detail.Item.MultiUnits != null && detail.QtyPerUnit <= 0)
                    {
                        var conversion = detail.Item.MultiUnits.FirstOrDefault(x => x.FK_UnitId == detail.FK_UnitId);
                        if (conversion != null)
                        {
                            detail.QtyPerUnit = conversion.QTYPerUnit ?? 1;
                        }
                    }
                }

                if (detail.Unit != null)
                {
                    detail.UnitName = detail.Unit.Name;
                }

                if (detail.QtyPerUnit == 0) detail.QtyPerUnit = 1;
            }
        }

        return hdr;
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = await db.InternalMaterialTransferHdrs
                .Include(x => x.InternalMaterialTransferDetails)
                .FirstOrDefaultAsync(x => x.IntMatTransId == id);

            if (hdr == null)
                return new OutputResponse(0, "Transaction not found", null);

            await _stockService.ReverseStockMovements(hdr.VoucherNo, "IMT-OUT", hdr.FK_BranchId, db);
            await _stockService.ReverseStockMovements(hdr.VoucherNo, "IMT-IN", hdr.FK_BranchId, db);

            hdr.IsDeleted = true;
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
        var query = db.InternalMaterialTransferHdrs
            .Where(x => x.FK_BranchId == pagination.BranchId && !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(pagination.search))
        {
            query = query.Where(x => x.VoucherNo.Contains(pagination.search)
                || (x.ReferenceNo != null && x.ReferenceNo.Contains(pagination.search)));
        }

        var totalCount = await query.CountAsync();

        var list = await query
            .Include(x => x.ToStore)
            //.OrderByDescending(x => x.VoucherDate) // Try sorting by ID if date can be same
            .OrderByDescending(x => x.IntMatTransId)
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .Select(x => new ExMasters
            {
                Id = x.IntMatTransId,
                Code = x.VoucherNo,
                Description = x.VoucherDate.HasValue ? x.VoucherDate.Value.ToString("dd-MM-yyyy") : "",
                Name = x.FromStore.Name,
                ExtraField = x.ToStore.Name,
                ReferenceNo = x.ReferenceNo,
                Amount = x.TotalAmount
            })
            .ToListAsync();

        return new TransactionPDTO(list, totalCount);
    }
}
