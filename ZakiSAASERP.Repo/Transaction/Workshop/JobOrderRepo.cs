using ZakiSAASERP.Interface.Transaction.Workshop;
using ZakiSAASERP.Models.Transactions.Workshop;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Transaction.Workshop;

public class JobOrderRepo : IJobOrderRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;

    public JobOrderRepo(IDbContextProvider contextProvider, CodeSequenceService codeService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
    }

    public async Task<string> GetVoucherNo(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(CodeKeys.JobOrder,
            async (code) => await db.JobOrderHdrs.AnyAsync(x => x.VoucherNo == code && x.FK_BranchId == branchId));
    }

    public async Task<int> GetNextVchrNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.JobOrderHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == branchId)
                .OrderByDescending(x => x.JobOrderId)
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

    public async Task<OutputResponse> CreateAsync(JobOrderHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Clean navigation properties to avoid duplicate inserts
            hdr.Vehicle = null;
            hdr.Customer = null;
            hdr.SalesMan = null;
            if (hdr.JobOrderDetails != null)
            {
                foreach (var d in hdr.JobOrderDetails)
                {
                    d.JobOrderHdr = null;
                    d.Repair = null;
                    d.Technician = null;
                }
            }

            // Generate Code if AutoCode matches logic
            if (await _codeService.IsAutoCodeEnabledAsync(CodeKeys.JobOrder, db))
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.JobOrder, db);
            }

            await db.JobOrderHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(hdr.JobOrderId, "Saved Successfully", null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<OutputResponse> UpdateAsync(JobOrderHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.JobOrderHdrs
                .Include(x => x.JobOrderDetails)
                .FirstOrDefaultAsync(x => x.JobOrderId == hdr.JobOrderId);

            if (existing == null) return new OutputResponse(0, "Not Found", null);

            if (hdr.IsDeleted) // Cancellation Logic
            {
                existing.IsDeleted = true;
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.JobOrderId, "Job Order Cancelled Successfully", null);
            }

            existing.VoucherDate = hdr.VoucherDate;
            existing.PromisedDate = hdr.PromisedDate;
            existing.FK_VehicleId = hdr.FK_VehicleId;
            existing.Kilometer = hdr.Kilometer;
            existing.FK_CustomerId = hdr.FK_CustomerId;
            existing.DeliveredBy = hdr.DeliveredBy;
            existing.TimeIn = hdr.TimeIn;
            existing.ContactNo = hdr.ContactNo; // Added mapping
            existing.ReferenceNo = hdr.ReferenceNo;
            existing.Status = hdr.Status;
            existing.FK_SalesManId = hdr.FK_SalesManId;
            existing.VehicleInfo = hdr.VehicleInfo;
            existing.Comments = hdr.Comments;
            existing.IstimaraPath = hdr.IstimaraPath;
            existing.TotalAmount = hdr.TotalAmount;
            existing.TotalVat = hdr.TotalVat;
            existing.GrandTotal = hdr.GrandTotal;

            // Update Grid
            db.JobOrderDtls.RemoveRange(existing.JobOrderDetails);

            if (hdr.JobOrderDetails != null)
            {
                foreach (var d in hdr.JobOrderDetails)
                {
                    d.JobOrderDtlsId = 0; // Reset ID for new insert
                    d.FK_JobOrderId = existing.JobOrderId;
                    d.JobOrderHdr = null;
                    d.Repair = null;
                    d.Technician = null;
                    db.JobOrderDtls.Add(d);
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.JobOrderId, "Updated Successfully", null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<JobOrderHdr?> GetByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.JobOrderHdrs
            .Include(x => x.JobOrderDetails)
            .ThenInclude(x => x.Repair)
            .Include(x => x.JobOrderDetails)
            .ThenInclude(x => x.Technician)
            .Include(x => x.Vehicle)
            .Include(x => x.Customer)
            .Include(x => x.SalesMan)
            .FirstOrDefaultAsync(x => x.JobOrderId == id);

        if (hdr != null)
        {
            // Populate NotMapped fields from Relations
            if (hdr.Vehicle != null)
            {
                hdr.PlateNo = hdr.Vehicle.PlateNo;
                hdr.ChassisNo = hdr.Vehicle.ChassisNo;
                hdr.Model = hdr.Vehicle.Model;
                hdr.Year = hdr.Vehicle.Year;
            }
            if (hdr.Customer != null)
            {
                hdr.CustomerName = hdr.Customer.Name;
                hdr.CustomerCode = hdr.Customer.Code;
                // Only overwrite ContactNo if it's null/empty? 
                // Currently it overrides whatever is in DB with Customer Mobile.
                // If ContactNo is now a real DB column, we should use that.
                // If DB is empty, maybe fallback to Customer.Mobile?
                if (string.IsNullOrEmpty(hdr.ContactNo))
                {
                    hdr.ContactNo = hdr.Customer.Mobile;
                }
            }

            if (hdr.JobOrderDetails != null)
            {
                foreach (var d in hdr.JobOrderDetails)
                {
                    if (d.Repair != null)
                    {
                        d.RepairName = d.Repair.Name;
                        d.RepairCode = d.Repair.Code;
                    }
                    if (d.Technician != null)
                    {
                        d.TechnicianName = d.Technician.Name;
                    }
                }
            }
        }
        return hdr;
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var existing = await db.JobOrderHdrs.FindAsync(id);
            if (existing == null) return new OutputResponse(0, "Not Found", null);

            existing.IsDeleted = true;
            await db.SaveChangesAsync();
            return new OutputResponse(id, "Deleted Successfully", null);
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.JobOrderHdrs
            .Include(x => x.Vehicle)
            .Include(x => x.Customer)
            .Where(x => x.FK_BranchId == pagination.BranchId && !x.IsDeleted)
            .AsNoTracking();

        if (pagination.FilterId.HasValue && pagination.FilterId > 0)
        {
            query = query.Where(x => x.FK_VehicleId == pagination.FilterId);
        }

        if (!string.IsNullOrEmpty(pagination.search))
        {
            query = query.Where(x => x.VoucherNo.Contains(pagination.search) ||
                                     (x.Vehicle != null && x.Vehicle.PlateNo.Contains(pagination.search)) ||
                                     (x.Customer != null && x.Customer.Name.Contains(pagination.search)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.VoucherDate)
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .Select(x => new ExMasters
            {
                Id = x.JobOrderId,
                Code = x.VoucherNo,
                Name = x.Vehicle != null ? x.Vehicle.PlateNo : "",
                Description = x.Customer != null ? x.Customer.Name : "",
                Amount = x.GrandTotal,
                ReferenceNo = x.Status
            })
            .ToListAsync();

        return new TransactionPDTO(items, totalCount);
    }
}
