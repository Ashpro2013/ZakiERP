using ZakiSAASERP.Interface.Transaction.Workshop;
using ZakiSAASERP.Models.Transactions.Workshop;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Transaction.Workshop;

public class JobCardRepo : IJobCardRepo
{
    private readonly IDbContextProvider _contextProvider;
    private readonly CodeSequenceService _codeService;

    public JobCardRepo(IDbContextProvider contextProvider, CodeSequenceService codeService)
    {
        _contextProvider = contextProvider;
        _codeService = codeService;
    }

    public async Task<string> GetVoucherNo(int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await _codeService.GetNextUniqueCodeAsync(CodeKeys.JobCard,
            async (code) => await db.JobCardHdrs.AnyAsync(x => x.VoucherNo == code && x.FK_BranchId == branchId));
    }

    public async Task<int> GetNextVchrNo(int branchId)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var codes = await db.JobCardHdrs
                .AsNoTracking()
                .Where(x => x.FK_BranchId == branchId)
                .OrderByDescending(x => x.JobCardId)
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

    public async Task<OutputResponse> CreateAsync(JobCardHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            // Clean navigation properties to avoid duplicate inserts
            hdr.JobOrder = null;
            hdr.SalesMan = null; // Added SalesMan cleanup
            if (hdr.JobCardDetails != null)
            {
                foreach (var d in hdr.JobCardDetails)
                {
                    d.JobCardHdr = null;
                    d.Item = null;
                    d.Unit = null;
                }
            }

            // Auto Code Logic
            if (await _codeService.IsAutoCodeEnabledAsync(CodeKeys.JobCard, db))
            {
                await _codeService.ConsumeCodeAsync(CodeKeys.JobCard, db);
            }

            await db.JobCardHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(hdr.JobCardId, "Saved Successfully", null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<OutputResponse> UpdateAsync(JobCardHdr hdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.JobCardHdrs
                .Include(x => x.JobCardDetails)
                .FirstOrDefaultAsync(x => x.JobCardId == hdr.JobCardId);

            if (existing == null) return new OutputResponse(0, "Not Found", null);

            if (hdr.IsDeleted) // Cancellation Logic
            {
                existing.IsDeleted = true;
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OutputResponse(existing.JobCardId, "Job Card Cancelled Successfully", null);
            }

            existing.VoucherDate = hdr.VoucherDate;
            existing.PromisedDate = hdr.PromisedDate;
            existing.FK_JobOrderId = hdr.FK_JobOrderId;
            existing.DeliveredBy = hdr.DeliveredBy;
            existing.TimeIn = hdr.TimeIn;
            existing.ContactNo = hdr.ContactNo;
            existing.FK_VehicleId = hdr.FK_VehicleId;
            existing.PlateNo = hdr.PlateNo;
            existing.ChassisNo = hdr.ChassisNo;
            existing.Model = hdr.Model;
            existing.Year = hdr.Year;
            existing.Kilometer = hdr.Kilometer;
            existing.VehicleInfo = hdr.VehicleInfo;
            existing.FK_CustomerId = hdr.FK_CustomerId;
            existing.CustomerName = hdr.CustomerName;
            existing.QuotationNo = hdr.QuotationNo;
            existing.Status = hdr.Status;

            existing.TotalAmount = hdr.TotalAmount;
            existing.DiscountTotal = hdr.DiscountTotal;
            existing.TotalVat = hdr.TotalVat;
            existing.GrandTotal = hdr.GrandTotal;

            // Update Grid
            db.JobCardDtls.RemoveRange(existing.JobCardDetails);

            if (hdr.JobCardDetails != null)
            {
                foreach (var d in hdr.JobCardDetails)
                {
                    d.JobCardDtlsId = 0; // Reset ID for new insert
                    d.FK_JobCardId = existing.JobCardId;
                    d.JobCardHdr = null;
                    d.Item = null;
                    d.Unit = null;
                    db.JobCardDtls.Add(d);
                }
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(existing.JobCardId, "Updated Successfully", null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message, null);
        }
    }

    public async Task<JobCardHdr?> GetByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var hdr = await db.JobCardHdrs
            .Include(x => x.JobCardDetails)
            .ThenInclude(x => x.Item)
            .Include(x => x.JobCardDetails)
            .ThenInclude(x => x.Unit)
            .Include(x => x.JobOrder)
            .ThenInclude(x => x.Vehicle)
            .Include(x => x.JobOrder)
            .ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.JobCardId == id);

        if (hdr != null)
        {
            // Populate NotMapped fields from Relations
            if (hdr.JobOrder != null)
            {
                hdr.JobOrderNo = hdr.JobOrder.VoucherNo;
                hdr.FK_VehicleId = hdr.JobOrder.FK_VehicleId;
                hdr.FK_CustomerId = hdr.JobOrder.FK_CustomerId;

                if (hdr.JobOrder.Vehicle != null)
                {
                    hdr.PlateNo = hdr.JobOrder.Vehicle.PlateNo;
                    hdr.ChassisNo = hdr.JobOrder.Vehicle.ChassisNo;
                    hdr.Model = hdr.JobOrder.Vehicle.Model;
                    hdr.Year = hdr.JobOrder.Vehicle.Year;
                    hdr.Kilometer = hdr.JobOrder.Kilometer; // Get KM from Job Order or should it be editable? Taking from JO for now.
                }
                if (hdr.JobOrder.Customer != null)
                {
                    hdr.CustomerName = hdr.JobOrder.Customer.Name;
                    hdr.CustomerCode = hdr.JobOrder.Customer.Code;
                    if (string.IsNullOrEmpty(hdr.ContactNo))
                    {
                        hdr.ContactNo = hdr.JobOrder.Customer.Mobile;
                    }
                }
            }

            if (hdr.JobCardDetails != null)
            {
                foreach (var d in hdr.JobCardDetails)
                {
                    if (d.Item != null)
                    {
                        d.ItemName = d.Item.ItemName;
                        d.ItemCode = d.Item.ItemCode;
                        d.ItemArabicName = d.Item.ItemNameArabic;
                        d.Barcode = d.Item.Barcode;
                    }
                    if (d.Unit != null)
                    {
                        d.UnitName = d.Unit.Name;
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
            var existing = await db.JobCardHdrs.FindAsync(id);
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
        var query = db.JobCardHdrs
            .Include(x => x.JobOrder)
            .ThenInclude(x => x.Vehicle)
             .Include(x => x.JobOrder)
            .ThenInclude(x => x.Customer)
            .Where(x => x.FK_BranchId == pagination.BranchId && !x.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(pagination.search))
        {
            query = query.Where(x => x.VoucherNo.Contains(pagination.search) ||
                                     (x.PlateNo != null && x.PlateNo.Contains(pagination.search)) ||
                                     (x.CustomerName != null && x.CustomerName.Contains(pagination.search)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.VoucherDate)
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .Select(x => new ExMasters
            {
                Id = x.JobCardId,
                Code = x.VoucherNo,
                Name = x.PlateNo ?? "",
                Description = x.CustomerName ?? "",
                Amount = x.GrandTotal,
                ReferenceNo = x.Status
            })
            .ToListAsync();

        return new TransactionPDTO(items, totalCount);
    }

    public async Task<List<JobCardHistoryDTO>> GetJobCardHistory(string plateNo)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await db.JobCardHdrs
            .AsNoTracking()
            .Where(x => x.PlateNo == plateNo && !x.IsDeleted)
            .OrderByDescending(x => x.VoucherDate)
            .Select(x => new JobCardHistoryDTO
            {
                JobCardNo = x.VoucherNo,
                JobCardDate = x.VoucherDate,
                PromisedDate = x.PromisedDate,
                PlateNo = x.PlateNo
            })
            .ToListAsync();
    }
}
