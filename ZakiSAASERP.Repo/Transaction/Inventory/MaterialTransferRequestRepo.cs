using ZakiSAASERP.Interface.Transaction.Inventory;
using ZakiSAASERP.Models.Transactions.Inventory;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Transaction.Inventory;

public class MaterialTransferRequestRepo : IMaterialTransferRequestRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;
    private readonly StockService _stockService;

    public MaterialTransferRequestRepo(IDbContextProvider contextProvider, CodeSequenceService codeService, StockService stockService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
        _stockService = stockService;
    }

    public async Task<string> GetVoucherNo(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(
            CodeKeys.MaterialTransferRequest,
            async (code) => await db.MaterialTransferRequestHdrs.AnyAsync(x => x.VoucherNo == code && x.FK_FromBranchId == branchId)
        );
    }

    public async Task<int> GetNextVchrNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.MaterialTransferRequestHdrs
                .AsNoTracking()
                .Where(x => x.FK_FromBranchId == branchId)
                .OrderByDescending(x => x.MatTransReqId)
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

    public async Task<OutputResponse> CreateAsync(MaterialTransferRequestHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Clean navigation properties
            if (hdr.MaterialTransferRequestDetails != null)
            {
                foreach (var d in hdr.MaterialTransferRequestDetails)
                {
                    d.Item = null;
                    d.Unit = null;
                    d.FromStore = null;
                    d.Header = null;
                }
            }

            hdr.TotalAmount = hdr.MaterialTransferRequestDetails.Sum(x => x.Quantity * x.Rate);
            hdr.TotalQty = hdr.MaterialTransferRequestDetails.Sum(x => x.Quantity);

            await db.MaterialTransferRequestHdrs.AddAsync(hdr);
            await db.SaveChangesAsync(); // Generates MatTransReqId

            // No stock movement or accounting for just a REQUEST usually, 
            // unless we want to reserve stock (not specified)

            // Auto-code consumption if enabled
            if (await _codeService.IsAutoCodeEnabledAsync(CodeKeys.MaterialTransferRequest, db))
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.MaterialTransferRequest, db);
            }

            await transaction.CommitAsync();
            return new OutputResponse(hdr.MatTransReqId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<OutputResponse> UpdateAsync(MaterialTransferRequestHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.MaterialTransferRequestHdrs
                .Include(x => x.MaterialTransferRequestDetails)
                .FirstOrDefaultAsync(x => x.MatTransReqId == hdr.MatTransReqId);

            if (existing != null)
            {
                if (existing.Status == "Approved" || existing.Status == "Rejected")
                {
                    return new OutputResponse(0, "Cannot update processed request", null);
                }

                existing.VoucherDate = hdr.VoucherDate;
                existing.ReferenceNo = hdr.ReferenceNo;
                existing.Remarks = hdr.Remarks;
                existing.FK_ToBranchId = hdr.FK_ToBranchId; // Allow changing destination
                // existing.FK_FromBranchId = hdr.FK_FromBranchId; // Usually fixed?

                existing.TotalAmount = hdr.MaterialTransferRequestDetails.Sum(x => x.Quantity * x.Rate);
                existing.TotalQty = hdr.MaterialTransferRequestDetails.Sum(x => x.Quantity);

                // Remove old details
                db.MaterialTransferRequestDtls.RemoveRange(existing.MaterialTransferRequestDetails);

                // Add new details
                foreach (var d in hdr.MaterialTransferRequestDetails)
                {
                    d.Item = null;
                    d.Unit = null;
                    d.FromStore = null;
                    d.Header = null;

                    d.FK_MatTransReqId = existing.MatTransReqId;
                    db.MaterialTransferRequestDtls.Add(d);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OutputResponse(hdr.MatTransReqId, null, null);
            }
            return new OutputResponse(0, "Record Not Found", null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<MaterialTransferRequestHdr?> GetByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.MaterialTransferRequestHdrs
            .Include(x => x.MaterialTransferRequestDetails)
            .ThenInclude(x => x.Item)
            .ThenInclude(x => x.MultiUnits)
            .Include(x => x.MaterialTransferRequestDetails)
            .ThenInclude(x => x.Unit)
            .Include(x => x.User)
            .Include(x => x.ApproverUser)
            .Include(x => x.RejectorUser)
            .FirstOrDefaultAsync(x => x.MatTransReqId == id);

        if (hdr != null && hdr.MaterialTransferRequestDetails != null)
        {
            foreach (var d in hdr.MaterialTransferRequestDetails)
            {
                decimal factor = 1;

                if (d.Item != null)
                {
                    // Default to Item Master details first
                    d.ItemName = d.Item.ItemName;
                    d.Barcode = d.Item.Barcode;

                    if (d.Item.MultiUnits != null)
                    {
                        var conversion = d.Item.MultiUnits.FirstOrDefault(x => x.FK_UnitId == d.FK_UnitId);
                        if (conversion != null)
                        {
                            // Populate QtyPerUnit
                            factor = conversion.QTYPerUnit ?? 1;
                        }
                    }
                }

                if (d.Unit != null)
                {
                    d.UnitName = d.Unit.Name;
                }

                d.QtyPerUnit = factor;
            }
        }
        return hdr;
    }

    public async Task<OutputResponse> DeleteMaterialTransferRequest(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var existing = await db.MaterialTransferRequestHdrs.FindAsync(id);
        if (existing == null) return new OutputResponse(0, "Not Found", null);

        if (existing.Status == "Approved" || existing.Status == "Rejected")
        {
            return new OutputResponse(0, "Cannot delete processed request", null);
        }

        existing.IsDeleted = true;
        await db.SaveChangesAsync();
        return new OutputResponse(id, null, null);
    }

    public async Task<TransactionPDTO> GetAllAsync(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.MaterialTransferRequestHdrs
            .Where(x => x.FK_FromBranchId == pagination.BranchId && !x.IsDeleted)
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
                Id = x.MatTransReqId,
                Code = x.VoucherNo,
                Description = x.VoucherDate.HasValue ? x.VoucherDate.Value.ToString("dd-MM-yyyy") : "",
                Name = x.Status,
                Amount = x.TotalAmount,
                ReferenceNo = x.ReferenceNo
            })
            .ToListAsync();

        return new TransactionPDTO(list, totalCount);
    }

    public async Task<List<MaterialTransferRequestHdr>> GetApprovalList(MTRApprovalFilter filter)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.MaterialTransferRequestHdrs
            .Include(x => x.MaterialTransferRequestDetails)
            .ThenInclude(x => x.Item)
            .Include(x => x.MaterialTransferRequestDetails)
            .ThenInclude(x => x.Unit)
            .Include(x => x.ApproverUser) // FIX: ApproverUser
            .Include(x => x.RejectorUser) // FIX: RejectorUser
            .Include(x => x.User)         // Include Creator User
            .Include(x => x.FromBranch)
            .Include(x => x.ToBranch)
            .AsNoTracking()
            .AsQueryable();

        // Filter by Branch (Source Branch)
        if (filter.BranchId.HasValue)
        {
            query = query.Where(x => x.FK_FromBranchId == filter.BranchId.Value);
        }

        // Filter by Status
        if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "All")
        {
            // Pending = "Opened"
            // Approved = "Approved"
            // Rejected = "Rejected"
            var dbStatus = filter.Status == "Pending" ? "Opened" : filter.Status;
            query = query.Where(x => x.Status == dbStatus);
        }

        // Filter by Date Range (optional)
        if (filter.FromDate.HasValue)
        {
            query = query.Where(x => x.VoucherDate >= filter.FromDate.Value);
        }
        if (filter.ToDate.HasValue)
        {
            query = query.Where(x => x.VoucherDate <= filter.ToDate.Value);
        }

        // Filter by Search Term
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(x => x.VoucherNo.Contains(filter.SearchTerm) ||
                                     (x.ReferenceNo != null && x.ReferenceNo.Contains(filter.SearchTerm)) ||
                                     x.TotalAmount.ToString().Contains(filter.SearchTerm));
            // Note: Searching by User name requires joining UserDetails, which might be complex here. 
            // Stick to simpler fields first or add Include(x => x.User) and search x.User.UserName
        }

        return await query.OrderByDescending(x => x.VoucherDate).Take(500).ToListAsync();
    }

    public async Task<OutputResponse> ApproveMaterialTransferRequest(int id, int userId, List<MaterialTransferRequestDtls> updatedDetails)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = await db.MaterialTransferRequestHdrs
                .Include(x => x.MaterialTransferRequestDetails)
                .FirstOrDefaultAsync(x => x.MatTransReqId == id);

            if (hdr == null) return new OutputResponse(0, "Material Transfer Request not found", null);
            if (hdr.Status != "Opened") return new OutputResponse(0, "Only opened requests can be approved", null);

            // Update header status
            hdr.Status = "Approved";
            hdr.FK_ApprovedBy = userId;
            hdr.ApprovedDate = DateTime.Now;

            // Update ToStore assignments from approval dialog
            // Update ToStore and ApprovedQuantity from approval dialog/details page
            foreach (var detail in hdr.MaterialTransferRequestDetails)
            {
                var updated = updatedDetails.FirstOrDefault(x => x.MatTransReqDtlsId == detail.MatTransReqDtlsId);
                if (updated != null)
                {
                    if (updated.FK_ToStoreId.HasValue)
                        detail.FK_ToStoreId = updated.FK_ToStoreId;

                    // Update ApprovedQuantity (default to requested Quantity if not set)
                    // If 0 is passed, it might mean 0 approved, but usually we fallback to requested. 
                    // Let's assume passed value is correct if not negative.
                    detail.ApprovedQuantity = updated.ApprovedQuantity >= 0 ? updated.ApprovedQuantity : detail.Quantity;

                    // Update field: ApproverComment
                    detail.ApproverComment = updated.ApproverComment;

                    // Calculate and save ApprovedAmount
                    detail.ApprovedAmount = detail.ApprovedQuantity * detail.Rate;
                }
                else
                {
                    // If not in updated list (shouldn't happen), approve full amount
                    detail.ApprovedQuantity = detail.Quantity;
                    detail.ApprovedAmount = detail.Quantity * detail.Rate;
                }
            }

            // Calculate and store TotalApprovedAmount in header
            hdr.TotalApprovedAmount = hdr.MaterialTransferRequestDetails.Sum(x => x.ApprovedAmount);

            await db.SaveChangesAsync();

            // Stock Movement: OUT from FromStore and IN to ToStore
            var movements = new List<StockMovementDto>();

            foreach (var item in hdr.MaterialTransferRequestDetails)
            {
                // Get base unit for stock logging
                var itemMaster = await db.ItemMasters.FindAsync(item.FK_ItemId);
                int baseUnitId = itemMaster?.FK_UnitId ?? item.FK_UnitId;

                // Get QtyPerUnit conversion
                decimal qtyPerUnit = 1;
                if (item.FK_UnitId != baseUnitId)
                {
                    var multiUnit = await db.ItemMultiUnits
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.FK_ItemId == item.FK_ItemId && x.FK_UnitId == item.FK_UnitId);
                    qtyPerUnit = multiUnit?.QTYPerUnit ?? 1;
                }

                decimal baseQty = item.ApprovedQuantity * qtyPerUnit;
                decimal baseRate = item.Rate / qtyPerUnit;

                // 1. Stock IN to FromStore (Requester / Source of Request)
                movements.Add(new StockMovementDto
                {
                    ItemId = item.FK_ItemId,
                    BranchId = hdr.FK_FromBranchId,
                    StoreId = item.FK_FromStoreId,
                    VoucherId = hdr.MatTransReqId,
                    VoucherNo = hdr.VoucherNo,
                    VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                    VoucherType = "Material Transfer",
                    FormType = "MTR-IN",
                    IsInward = true,
                    Quantity = item.ApprovedQuantity,
                    UnitId = item.FK_UnitId,
                    ActualUnitId = baseUnitId,
                    ActualQuantity = baseQty,
                    Rate = item.Rate,
                    ActualRate = baseRate,
                    Narration = $"Material Transfer Received from {hdr.FK_ToBranchId} - {hdr.Remarks}",
                    UserId = userId
                });

                // 2. Stock OUT from ToStore (Supplier / Destination of Request)
                if (item.FK_ToStoreId.HasValue)
                {
                    movements.Add(new StockMovementDto
                    {
                        ItemId = item.FK_ItemId,
                        BranchId = hdr.FK_ToBranchId,
                        StoreId = item.FK_ToStoreId.Value,
                        VoucherId = hdr.MatTransReqId,
                        VoucherNo = hdr.VoucherNo,
                        VoucherDate = hdr.VoucherDate ?? DateTime.Now,
                        VoucherType = "Material Transfer",
                        FormType = "MTR-OUT",
                        IsInward = false,
                        Quantity = item.ApprovedQuantity,
                        UnitId = item.FK_UnitId,
                        ActualUnitId = baseUnitId,
                        ActualQuantity = baseQty,
                        Rate = item.Rate,
                        ActualRate = baseRate,
                        Narration = $"Material Transfer issued to {hdr.FK_FromBranchId} - {hdr.Remarks}",
                        UserId = userId
                    });
                }
            }

            if (movements.Any())
            {
                await _stockService.ProcessStockMovements(movements, db);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OutputResponse(hdr.MatTransReqId, null, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<OutputResponse> RejectMaterialTransferRequest(int id, int userId, string? rejectionReason)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var hdr = await db.MaterialTransferRequestHdrs.FindAsync(id);
            if (hdr == null) return new OutputResponse(0, "Material Transfer Request not found", null);
            if (hdr.Status != "Opened") return new OutputResponse(0, "Only opened requests can be rejected", null);

            hdr.Status = "Rejected";
            hdr.FK_RejectedBy = userId;
            hdr.RejectedDate = DateTime.Now;
            hdr.RejectionReason = rejectionReason;

            await db.SaveChangesAsync();
            return new OutputResponse(hdr.MatTransReqId, null, null);
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<List<MaterialTransferRequestHdr>> GetPendingApprovals(int? branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.MaterialTransferRequestHdrs
            .Include(x => x.MaterialTransferRequestDetails)
            .ThenInclude(x => x.Item)
            .Include(x => x.MaterialTransferRequestDetails)
            .ThenInclude(x => x.FromStore)
            .Include(x => x.FromBranch)
            .Include(x => x.ToBranch)
            .Where(x => x.Status == "Opened" && !x.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(x => x.FK_ToBranchId == branchId);

        return await query.OrderByDescending(x => x.VoucherDate).ToListAsync();
    }
}
