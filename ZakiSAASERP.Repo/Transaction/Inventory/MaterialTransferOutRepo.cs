using ZakiSAASERP.Interface.Transaction.Inventory;
using ZakiSAASERP.Models.Transactions.Inventory;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Transaction.Inventory;

public class MaterialTransferOutRepo : IMaterialTransferOutRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly StockService _stockService;

    public MaterialTransferOutRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, StockService stockService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _stockService = stockService;
    }

    public async Task<string> GetVoucherNo(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.MaterialTransferOut,
            async (code) => await db.MaterialTransferOutHdrs.AnyAsync(x => x.VoucherNo == code && x.FK_FromBranchId == branchId)
        );
    }

    public async Task<int> GetNextVchrNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.MaterialTransferOutHdrs
                .AsNoTracking()
                .Where(x => x.FK_FromBranchId == branchId)
                .OrderByDescending(x => x.MatTransOutId)
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

    public async Task<OutputResponse> CreateAsync(MaterialTransferOutHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Clean navigation properties
            if (hdr.MaterialTransferOutDetails != null)
            {
                foreach (var d in hdr.MaterialTransferOutDetails)
                {
                    d.Item = null;
                    d.Unit = null;
                    d.FromStore = null;
                    d.ToStore = null;
                    d.Header = null;
                }
            }

            hdr.TotalAmount = hdr.MaterialTransferOutDetails.Sum(x => x.Amount);
            hdr.TotalQty = hdr.MaterialTransferOutDetails.Sum(x => x.Quantity);

            await db.MaterialTransferOutHdrs.AddAsync(hdr);
            await db.SaveChangesAsync(); // Generates MatTransOutId

            // Stock Movement: OUT from FromBranch and IN to ToBranch
            var movements = new List<StockMovementDto>();

            foreach (var item in hdr.MaterialTransferOutDetails)
            {
                // Get base unit for stock logging
                var itemMaster = await db.ItemMasters.FindAsync(item.FK_ItemId);
                int baseUnitId = itemMaster?.FK_UnitId ?? item.FK_UnitId;

                // Get QtyPerUnit conversion
                // Get QtyPerUnit conversion - Prioritize User Input
                decimal qtyPerUnit = item.QtyPerUnit > 0 ? item.QtyPerUnit : 1;

                // Fallback to Master Data if not provided and unit is not base unit
                if (qtyPerUnit == 1 && item.FK_UnitId != baseUnitId && item.QtyPerUnit <= 0)
                {
                    var multiUnit = await db.ItemMultiUnits
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.FK_ItemId == item.FK_ItemId && x.FK_UnitId == item.FK_UnitId);
                    qtyPerUnit = multiUnit?.QTYPerUnit ?? 1;
                }

                decimal baseQty = item.Quantity * qtyPerUnit;
                decimal baseRate = item.Rate / qtyPerUnit;

                // 1. Stock OUT from FromBranch/FromStore
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = hdr.FK_FromBranchId,
                    StoreId = item.FK_FromStoreId,
                    VoucherId = hdr.MatTransOutId,
                    VoucherNo = hdr.VoucherNo,
                    VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer OUT",
                    FormType = "MTO-OUT",
                    IsInward = false,
                    Quantity = item.Quantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Material Transfer OUT to Branch {hdr.FK_ToBranchId} - {hdr.Remarks}",
                    UserId = hdr.FK_UserId
                });

                // 2. Stock IN to ToBranch/ToStore
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = hdr.FK_ToBranchId,
                    StoreId = item.FK_ToStoreId,
                    VoucherId = hdr.MatTransOutId,
                    VoucherNo = hdr.VoucherNo,
                    VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer OUT",
                    FormType = "MTO-IN",
                    IsInward = true,
                    Quantity = item.Quantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Material Transfer IN from Branch {hdr.FK_FromBranchId} - {hdr.Remarks}",
                    UserId = hdr.FK_UserId
                });
            }

            await _stockService.ProcessStockMovements(movements, db);

            // Auto-code consumption if enabled
            if (await _codeService.IsAutoCodeEnabledAsync(CodeKeys.MaterialTransferOut, db))
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.MaterialTransferOut, db);
            }

            await db.SaveChangesAsync(); // Update Stock Balance
            await transaction.CommitAsync();
            return new OutputResponse(hdr.MatTransOutId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<OutputResponse> UpdateAsync(MaterialTransferOutHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.MaterialTransferOutHdrs
                .Include(x => x.MaterialTransferOutDetails)
                .FirstOrDefaultAsync(x => x.MatTransOutId == hdr.MatTransOutId);

            if (existing == null)
                return new OutputResponse(0, "Material Transfer OUT not found", null);

            // Reverse old stock movements
            await _stockService.ReverseStockMovements(existing.VoucherNo, "MTO-OUT", existing.FK_FromBranchId, db);
            await _stockService.ReverseStockMovements(existing.VoucherNo, "MTO-IN", existing.FK_ToBranchId, db);

            // Remove old details
            db.MaterialTransferOutDtls.RemoveRange(existing.MaterialTransferOutDetails);

            // Update header
            existing.VoucherNo = hdr.VoucherNo;
            existing.VoucherDate = hdr.VoucherDate;
            existing.ReferenceNo = hdr.ReferenceNo;
            existing.Remarks = hdr.Remarks;
            existing.EnteredBy = hdr.EnteredBy;
            existing.FK_FromBranchId = hdr.FK_FromBranchId;
            existing.FK_ToBranchId = hdr.FK_ToBranchId;
            existing.TotalAmount = hdr.MaterialTransferOutDetails.Sum(x => x.Amount);
            existing.TotalQty = hdr.MaterialTransferOutDetails.Sum(x => x.Quantity);

            // Add new details
            foreach (var detail in hdr.MaterialTransferOutDetails)
            {
                detail.FK_MatTransOutId = existing.MatTransOutId;
                detail.Item = null;
                detail.Unit = null;
                detail.FromStore = null;
                detail.ToStore = null;
                detail.Header = null;
                existing.MaterialTransferOutDetails.Add(detail);
            }

            await db.SaveChangesAsync();

            // Apply new stock movements
            var movements = new List<StockMovementDto>();

            foreach (var item in existing.MaterialTransferOutDetails)
            {
                var itemMaster = await db.ItemMasters.FindAsync(item.FK_ItemId);
                int baseUnitId = itemMaster?.FK_UnitId ?? item.FK_UnitId;

                decimal qtyPerUnit = item.QtyPerUnit > 0 ? item.QtyPerUnit : 1;

                // Fallback only if QtyPerUnit was not valid in the input (<= 0)
                if (qtyPerUnit == 1 && item.FK_UnitId != baseUnitId && item.QtyPerUnit <= 0)
                {
                    // Fallback for legacy data or if QtyPerUnit was not saved
                    var multiUnit = await db.ItemMultiUnits
                       .AsNoTracking()
                       .FirstOrDefaultAsync(x => x.FK_ItemId == item.FK_ItemId && x.FK_UnitId == item.FK_UnitId);
                    qtyPerUnit = multiUnit?.QTYPerUnit ?? 1;
                }

                decimal baseQty = item.Quantity * qtyPerUnit;
                decimal baseRate = item.Rate / qtyPerUnit;

                // Stock OUT from FromBranch
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = existing.FK_FromBranchId,
                    StoreId = item.FK_FromStoreId,
                    VoucherId = existing.MatTransOutId,
                    VoucherNo = existing.VoucherNo,
                    VoucherDate = existing.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer OUT",
                    FormType = "MTO-OUT",
                    IsInward = false,
                    Quantity = item.Quantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Material Transfer OUT to Branch {existing.FK_ToBranchId} - {existing.Remarks}",
                    UserId = existing.FK_UserId
                });

                // Stock IN to ToBranch
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = existing.FK_ToBranchId,
                    StoreId = item.FK_ToStoreId,
                    VoucherId = existing.MatTransOutId,
                    VoucherNo = existing.VoucherNo,
                    VoucherDate = existing.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer OUT",
                    FormType = "MTO-IN",
                    IsInward = true,
                    Quantity = item.Quantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Material Transfer IN from Branch {existing.FK_FromBranchId} - {existing.Remarks}",
                    UserId = existing.FK_UserId
                });
            }

            await _stockService.ProcessStockMovements(movements, db);

            await db.SaveChangesAsync(); // Update Stock Balance
            await transaction.CommitAsync();
            return new OutputResponse(existing.MatTransOutId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<MaterialTransferOutHdr?> GetByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.MaterialTransferOutHdrs
            .Include(x => x.MaterialTransferOutDetails)
            .ThenInclude(d => d.Item)
            .ThenInclude(i => i.MultiUnits)
            .Include(x => x.MaterialTransferOutDetails)
            .ThenInclude(d => d.Unit)
            .Include(x => x.MaterialTransferOutDetails)
            .ThenInclude(d => d.FromStore)
            .Include(x => x.MaterialTransferOutDetails)
            .ThenInclude(d => d.ToStore)
            .Include(x => x.FromBranch)
            .Include(x => x.ToBranch)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.MatTransOutId == id && !x.IsDeleted);

        if (hdr != null)
        {
            if (string.IsNullOrEmpty(hdr.EnteredBy))
            {
                hdr.EnteredBy = hdr.User?.UserName ?? "";
            }
        }

        if (hdr != null && hdr.MaterialTransferOutDetails != null)
        {
            int sl = 1;
            foreach (var detail in hdr.MaterialTransferOutDetails)
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

                if (detail.QtyPerUnit == 0)
                {
                    detail.QtyPerUnit = 1;
                }
            }
        }

        return hdr;
    }

    public async Task<OutputResponse> DeleteMaterialTransferOut(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = await db.MaterialTransferOutHdrs
                .Include(x => x.MaterialTransferOutDetails)
                .FirstOrDefaultAsync(x => x.MatTransOutId == id);

            if (hdr == null)
                return new OutputResponse(0, "Material Transfer OUT not found", null);

            // Reverse stock movements
            await _stockService.ReverseStockMovements(hdr.VoucherNo, "MTO-OUT", hdr.FK_FromBranchId, db);
            await _stockService.ReverseStockMovements(hdr.VoucherNo, "MTO-IN", hdr.FK_ToBranchId, db);

            // Soft delete
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
        var query = db.MaterialTransferOutHdrs
            .Where(x => (x.FK_FromBranchId == pagination.BranchId || x.FK_ToBranchId == pagination.BranchId)
                        && !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrEmpty(pagination.search))
        {
            query = query.Where(x => x.VoucherNo.Contains(pagination.search)
                || (x.ReferenceNo != null && x.ReferenceNo.Contains(pagination.search)));
        }

        var totalCount = await query.CountAsync();

        var list = await query
            .Include(x => x.FromBranch)
            .Include(x => x.ToBranch)
            .OrderByDescending(x => x.VoucherDate)
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .Select(x => new ExMasters
            {
                Id = x.MatTransOutId,
                Code = x.VoucherNo,
                Description = x.VoucherDate.HasValue ? x.VoucherDate.Value.ToString("dd-MM-yyyy") : "",
                Name = x.FromBranch.Name,
                ExtraField = x.ToBranch.Name,
                ReferenceNo = x.ReferenceNo,
                Amount = x.TotalAmount
            })
            .ToListAsync();

        return new TransactionPDTO(list, totalCount);
    }
}
