using AshproSAASERP.Interface.Transaction.Inventory;
using AshproSAASERP.Models.Transactions.Inventory;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Repo.CodeGenerationServices;

namespace AshproSAASERP.Repo.Transaction.Inventory;

public class MaterialTransferInRepo : IMaterialTransferInRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly StockService _stockService;

    public MaterialTransferInRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, StockService stockService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _stockService = stockService;
    }

    public async Task<string> GetVoucherNo(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.MaterialTransferIn,
            async (code) => await db.MaterialTransferInHdrs.AnyAsync(x => x.VoucherNo == code && x.FK_ToBranchId == branchId)
        );
    }

    public async Task<int> GetNextVchrNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.MaterialTransferInHdrs
                .AsNoTracking()
                .Where(x => x.FK_ToBranchId == branchId)
                .OrderByDescending(x => x.MatTransInId)
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

    public async Task<OutputResponse> CreateAsync(MaterialTransferInHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Clean navigation properties
            if (hdr.MaterialTransferInDetails != null)
            {
                foreach (var d in hdr.MaterialTransferInDetails)
                {
                    d.Item = null;
                    d.Unit = null;
                    d.FromStore = null;
                    d.ToStore = null;
                    d.Header = null;
                }
            }

            hdr.TotalAmount = hdr.MaterialTransferInDetails.Sum(x => x.Amount);
            hdr.TotalQty = hdr.MaterialTransferInDetails.Sum(x => x.Quantity);

            await db.MaterialTransferInHdrs.AddAsync(hdr);
            await db.SaveChangesAsync(); // Generates MatTransInId

            // Stock Movement: OUT from FromBranch and IN to ToBranch
            var movements = new List<StockMovementDto>();

            foreach (var item in hdr.MaterialTransferInDetails)
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
                    VoucherId = hdr.MatTransInId,
                    VoucherNo = hdr.VoucherNo,
                    VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer IN",
                    FormType = "MTI-OUT",
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
                    VoucherId = hdr.MatTransInId,
                    VoucherNo = hdr.VoucherNo,
                    VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer IN",
                    FormType = "MTI-IN",
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
            if (await _codeService.IsAutoCodeEnabledAsync(CodeKeys.MaterialTransferIn, db))
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.MaterialTransferIn, db);
            }

            await db.SaveChangesAsync(); // Update Stock Balance
            await transaction.CommitAsync();
            return new OutputResponse(hdr.MatTransInId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<OutputResponse> UpdateAsync(MaterialTransferInHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.MaterialTransferInHdrs
                .Include(x => x.MaterialTransferInDetails)
                .FirstOrDefaultAsync(x => x.MatTransInId == hdr.MatTransInId);

            if (existing == null)
                return new OutputResponse(0, "Material Transfer IN not found", null);

            // Reverse old stock movements
            await _stockService.ReverseStockMovements(existing.VoucherNo, "MTI-OUT", existing.FK_FromBranchId, db);
            await _stockService.ReverseStockMovements(existing.VoucherNo, "MTI-IN", existing.FK_ToBranchId, db);

            // Remove old details
            db.MaterialTransferInDtls.RemoveRange(existing.MaterialTransferInDetails);

            // Update header
            existing.VoucherNo = hdr.VoucherNo;
            existing.VoucherDate = hdr.VoucherDate;
            existing.ReferenceNo = hdr.ReferenceNo;
            existing.Remarks = hdr.Remarks;
            existing.EnteredBy = hdr.EnteredBy;
            existing.FK_FromBranchId = hdr.FK_FromBranchId;
            existing.FK_ToBranchId = hdr.FK_ToBranchId;
            existing.TotalAmount = hdr.MaterialTransferInDetails.Sum(x => x.Amount);
            existing.TotalQty = hdr.MaterialTransferInDetails.Sum(x => x.Quantity);

            // Add new details
            foreach (var detail in hdr.MaterialTransferInDetails)
            {
                detail.FK_MatTransInId = existing.MatTransInId;
                detail.Item = null;
                detail.Unit = null;
                detail.FromStore = null;
                detail.ToStore = null;
                detail.Header = null;
                existing.MaterialTransferInDetails.Add(detail);
            }

            await db.SaveChangesAsync();

            // Apply new stock movements
            var movements = new List<StockMovementDto>();

            foreach (var item in existing.MaterialTransferInDetails)
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
                    VoucherId = existing.MatTransInId,
                    VoucherNo = existing.VoucherNo,
                    VoucherDate = existing.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer IN",
                    FormType = "MTI-OUT",
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
                    VoucherId = existing.MatTransInId,
                    VoucherNo = existing.VoucherNo,
                    VoucherDate = existing.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer IN",
                    FormType = "MTI-IN",
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
            return new OutputResponse(existing.MatTransInId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<MaterialTransferInHdr?> GetByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.MaterialTransferInHdrs
            .Include(x => x.MaterialTransferInDetails)
            .ThenInclude(d => d.Item)
            .ThenInclude(i => i.MultiUnits)
            .Include(x => x.MaterialTransferInDetails)
            .ThenInclude(d => d.Unit)
            .Include(x => x.MaterialTransferInDetails)
            .ThenInclude(d => d.FromStore)
            .Include(x => x.MaterialTransferInDetails)
            .ThenInclude(d => d.ToStore)
            .Include(x => x.FromBranch)
            .Include(x => x.ToBranch)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.MatTransInId == id && !x.IsDeleted);

        if (hdr != null)
        {
            if (string.IsNullOrEmpty(hdr.EnteredBy) && hdr.User != null)
            {
                hdr.EnteredBy = hdr.User.UserName;
            }
        }

        if (hdr != null && hdr.MaterialTransferInDetails != null)
        {
            int sl = 1;
            foreach (var detail in hdr.MaterialTransferInDetails)
            {
                detail.SL = sl++;

                if (detail.Item != null)
                {
                    // Do not unconditionaly overwrite ItemName/Barcode from Master, 
                    // as it might be a MultiUnit Saved Variant (e.g. Cola-Box).
                    // Logic will be handled in View or here only if null.

                    if (string.IsNullOrEmpty(detail.ItemName)) detail.ItemName = detail.Item.ItemName;
                    if (string.IsNullOrEmpty(detail.Barcode)) detail.Barcode = detail.Item.Barcode;

                    // Apply ItemSearchMode logic for multi-unit display
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

                // Default QtyPerUnit if not set
                if (detail.QtyPerUnit == 0)
                {
                    detail.QtyPerUnit = 1;
                }
            }
        }

        return hdr;
    }

    public async Task<OutputResponse> DeleteMaterialTransferIn(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = await db.MaterialTransferInHdrs
                .Include(x => x.MaterialTransferInDetails)
                .FirstOrDefaultAsync(x => x.MatTransInId == id);

            if (hdr == null)
                return new OutputResponse(0, "Material Transfer IN not found", null);

            // Reverse stock movements
            await _stockService.ReverseStockMovements(hdr.VoucherNo, "MTI-OUT", hdr.FK_FromBranchId, db);
            await _stockService.ReverseStockMovements(hdr.VoucherNo, "MTI-IN", hdr.FK_ToBranchId, db);

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

    // Pagination support for search dialog
    public async Task<TransactionPDTO> GetAllAsync(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.MaterialTransferInHdrs
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
                Id = x.MatTransInId,
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
