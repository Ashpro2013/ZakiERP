using AshproSAASERP.Models.Others;
using AshproSAASERP.Models.ViewModel;

namespace AshproSAASERP.Repo.Others;

public class BarcodeDesignRepo(IDbContextProvider contextProvider) : IBarcodeDesignRepo
{
    public async Task<OutputResponse> CreateAsync(BarcodeDesignDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.hdr;
            var dtls = entity.dtls;
            if (hdr.IsDefault)
            {
                await db.BarcodeDesignHdrs.Where(x => x.FK_BranchId == hdr.FK_BranchId).
                     ExecuteUpdateAsync(x => x.SetProperty(e => e.IsDefault, false));
            }
            var exists = await db.BarcodeDesignHdrs.AnyAsync(x => x.DesignName == hdr.DesignName);
            if (exists) return new OutputResponse(0, "Barcode Designer already exists.");
            await db.BarcodeDesignHdrs.AddAsync(hdr);
            await db.SaveChangesAsync();
            dtls.ForEach(x => x.FK_BarcodeDesignHdrId = hdr.BarcodeDesignHdrId);
            await db.BarcodeDesignDtls.AddRangeAsync(dtls);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OutputResponse(hdr.BarcodeDesignHdrId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            if (id == 0) return new OutputResponse(0, "Invalid identifier.");
            await db.BarcodeDesignDtls.Where(x => x.FK_BarcodeDesignHdrId == id).ExecuteDeleteAsync();
            await db.BarcodeDesignHdrs.Where(x => x.BarcodeDesignHdrId == id).ExecuteDeleteAsync();
            await transaction.CommitAsync();
            return new OutputResponse(id, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
    public async Task<BarcodeDesignDTO> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        var hdr = await db.BarcodeDesignHdrs.FirstOrDefaultAsync(x => x.BarcodeDesignHdrId == id);
        if (hdr == null) { return null; }
        var dtls = await db.BarcodeDesignDtls.Where(x => x.FK_BarcodeDesignHdrId == hdr.BarcodeDesignHdrId).ToListAsync();
        return new BarcodeDesignDTO(hdr, dtls);
    }
    public async Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.BarcodeDesignHdrs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.DesignName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            bool desc = pagination.sortDirection == "desc";
            query = pagination.sortColumn switch
            {
                "Code" => desc ? query.OrderByDescending(x => x.DesignName) : query.OrderBy(x => x.DesignName),
                "Name" => desc ? query.OrderByDescending(x => x.DesignName) : query.OrderBy(x => x.DesignName),
                _ => desc ? query.OrderByDescending(x => x.BarcodeDesignHdrId) : query.OrderBy(x => x.BarcodeDesignHdrId)
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.BarcodeDesignHdrId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        var finalList = list.Where(x => x.FK_BranchId == pagination.BranchId).Select(e => new ExMasters
        {
            Id = e.BarcodeDesignHdrId,
            Name = e.DesignName,
            Code = e.DesignName,
            IsSelected = true,
            Amount = 0M,
            ExtraField = e.IsDefault ? "Default" : "",
            Description = $"{e.Width}x{e.Height}"
        }).ToList();
        return new TransactionPDTO(finalList, count);
    }

    public async Task<OutputResponse> UpdateAsync(BarcodeDesignDTO entity)
    {
        await using var db = contextProvider.CreateDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var hdr = entity.hdr;
            if (hdr.IsDefault)
            {
                await db.BarcodeDesignHdrs.Where(x => x.FK_BranchId == hdr.FK_BranchId).
                     ExecuteUpdateAsync(x => x.SetProperty(e => e.IsDefault, false));
            }
            // 1. Ensure header exists (important)
            db.BarcodeDesignHdrs.Attach(entity.hdr);
            db.Entry(entity.hdr).State = EntityState.Modified;

            // 2. Delete existing details
            await db.BarcodeDesignDtls
                .Where(x => x.FK_BarcodeDesignHdrId == entity.hdr.BarcodeDesignHdrId)
                .ExecuteDeleteAsync();

            // 3. Ensure FK is correctly assigned in each detail
            foreach (var d in entity.dtls)
            {
                d.BarcodeDesignDtlsId = 0;
                d.FK_BarcodeDesignHdrId = entity.hdr.BarcodeDesignHdrId;
            }
            // 4. Insert details
            await db.BarcodeDesignDtls.AddRangeAsync(entity.dtls);

            // 5. Save
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OutputResponse(entity.hdr.BarcodeDesignHdrId, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new OutputResponse(0, ex.Message);
        }
    }
    public async Task<BarcodeDesignDTO> GetAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        var hdr = await db.BarcodeDesignHdrs.FirstOrDefaultAsync(x => x.IsDefault == true);
        if (hdr == null) { hdr = await db.BarcodeDesignHdrs.FirstOrDefaultAsync(); }
        if (hdr == null) { return null; }
        var dtls = await db.BarcodeDesignDtls.Where(x => x.FK_BarcodeDesignHdrId == hdr.BarcodeDesignHdrId).ToListAsync();
        return new BarcodeDesignDTO(hdr, dtls);
    }

    public async Task<List<BarcodeDesignHdr>> GetAllDesignsAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.BarcodeDesignHdrs.AsNoTracking().ToListAsync();
    }
}
