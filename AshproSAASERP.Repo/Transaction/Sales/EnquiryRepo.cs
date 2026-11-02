using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.Models.Transactions.Sales;

namespace AshproSAASERP.Repo.Transaction.Sales;

public class EnquiryRepo(IDbContextProvider _contextProvider) : IEnquiryRepo
{
    public async Task<string> SaveAsync(RFQHdr rfqHdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            if (string.IsNullOrWhiteSpace(rfqHdr.RFQNo))
            {
                rfqHdr.RFQNo = await GenerateNextRFQNoAsync(db, rfqHdr.FK_BranchId);
            }

            await db.RFQHdrs.AddAsync(rfqHdr);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return "Saved Successfully";
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> UpdateAsync(RFQHdr rfqHdr)
    {
        await using var db = _contextProvider.CreateDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var existing = await db.RFQHdrs.FirstOrDefaultAsync(x => x.RFQId == rfqHdr.RFQId);
            if (existing is null)
            {
                throw new InvalidOperationException("Record not found.");
            }

            existing.RFQNo = rfqHdr.RFQNo;
            existing.RefNo = rfqHdr.RefNo;
            existing.CustomerRefNo = rfqHdr.CustomerRefNo;
            existing.CustomerRFQDate = rfqHdr.CustomerRFQDate;
            existing.FK_CustomerId = rfqHdr.FK_CustomerId;
            existing.FK_SalesManId = rfqHdr.FK_SalesManId;
            existing.Remarks = rfqHdr.Remarks;
            existing.IsDeleted = rfqHdr.IsDeleted;
            existing.FK_BranchId = rfqHdr.FK_BranchId;
            existing.FK_UserId = rfqHdr.FK_UserId;
            existing.FK_FinancialYearId = rfqHdr.FK_FinancialYearId;
            existing.EntryDate = rfqHdr.EntryDate;
            existing.Remarks = rfqHdr.Remarks;
            await db.RFQDtls.Where(x => x.FK_RFQId == rfqHdr.RFQId).ExecuteDeleteAsync();

            if (rfqHdr.RFQDetails?.Count > 0)
            {
                foreach (var detail in rfqHdr.RFQDetails)
                {
                    detail.RFQDtlsId = 0;
                    detail.FK_RFQId = rfqHdr.RFQId;
                    detail.Store = null;
                    detail.Unit = null;
                    detail.Item = null;
                }

                await db.RFQDtls.AddRangeAsync(rfqHdr.RFQDetails);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return "Updated Successfully";
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        var affected = await db.RFQHdrs
            .Where(x => x.RFQId == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsDeleted, true));

        return affected > 0;
    }

    public async Task<RFQHdr> GetByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();

        var rfq = await db.RFQHdrs
            .AsNoTracking()
            .Include(h => h.Customer)
            .Include(h => h.RFQDetails)!.ThenInclude(d => d.Item)
            .Include(h => h.RFQDetails)!.ThenInclude(d => d.Unit)
            .Include(h => h.RFQDetails)!.ThenInclude(d => d.Store)
            .FirstOrDefaultAsync(x => x.RFQId == id);

        if (rfq != null)
        {
            rfq.CustomerName = rfq.Customer?.Name;

            foreach (var detail in rfq.RFQDetails)
            {
                detail.ItemName = detail.Item?.ItemName;
                detail.ItemCode = detail.Item?.ItemCode;
                detail.ItemArabicName = detail.Item?.ItemNameArabic;
            }
        }

        return rfq!;
    }

    public async Task<List<RFQHdr>> GetAllAsync(string conn)
    {
        await using var db = _contextProvider.CreateDbContext();

        var rfqs = await db.RFQHdrs
            .AsNoTracking()
            .Include(h => h.Customer)
            .Where(h => !h.IsDeleted)
            .OrderByDescending(h => h.RFQId)
            .ToListAsync();

        foreach (var rfq in rfqs)
        {
            rfq.CustomerName = rfq.Customer?.Name;
        }

        return rfqs;
    }

    public async Task<string> GetNextEnquiryNoAsync(string conn, int branchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await GenerateNextRFQNoAsync(db, branchId);
    }

    public async Task<CustomerMaster> GetCustomerByIdAsync(int id)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await db.CustomerMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerId == id)!;
    }

    public async Task<List<StoreMaster>> GetStoresAsync(string conn)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await db.StoreMasters
            .AsNoTracking()
            .ToListAsync();
    }



    private static async Task<string> GenerateNextRFQNoAsync(TenantDBContext db, int? branchId)
    {
        var query = db.RFQHdrs.AsNoTracking();

        if (branchId.HasValue && branchId > 0)
        {
            query = query.Where(x => x.FK_BranchId == branchId);
        }

        var lastNo = await query
            .Where(x => x.RFQNo != null && x.RFQNo.StartsWith("RFQ-"))
            .OrderByDescending(x => x.RFQId)
            .Select(x => x.RFQNo)
            .FirstOrDefaultAsync();

        int nextNum = 1;
        if (!string.IsNullOrEmpty(lastNo))
        {
            var parts = lastNo.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out var lastSeq))
            {
                nextNum = lastSeq + 1;
            }
        }

        return $"RFQ-{nextNum:D4}";
    }
}



