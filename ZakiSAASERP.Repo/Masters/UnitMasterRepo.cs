namespace ZakiSAASERP.Repo.Masters;

public class UnitMasterRepo(IDbContextProvider contextProvider) : IUnitMasterRepo
{
    public async Task<OutputResponse> CreateAsync(UnitMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var exists = await db.UnitMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
            if (exists) return new OutputResponse(0, "Unit Master already exists.");

            await db.UnitMasters.AddAsync(entity);
            await db.SaveChangesAsync();
            return new OutputResponse(entity.UnitId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }

    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.UnitMasters.FindAsync(id);
            if (existing is null) { return new OutputResponse(0, "Unit not found."); }

            db.UnitMasters.Remove(existing);
            await db.SaveChangesAsync();
            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }

    public async Task<List<UnitMaster>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.UnitMasters.AsNoTracking().ToListAsync();
    }

    public async Task<(List<UnitMaster>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.UnitMasters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term) || x.Code.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            if (pagination.sortDirection == "desc")
            {
                query = pagination.sortColumn switch
                {
                    "Code" => query.OrderByDescending(x => x.Code),
                    "Name" => query.OrderByDescending(x => x.Name),
                    _ => query.OrderByDescending(x => x.UnitId)
                };
            }
            else
            {
                query = pagination.sortColumn switch
                {
                    "Code" => query.OrderBy(x => x.Code),
                    "Name" => query.OrderBy(x => x.Name),
                    _ => query.OrderBy(x => x.UnitId)
                };
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.UnitId);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        return (list, count);
    }

    public async Task<UnitMaster> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        var model = await db.UnitMasters.AsNoTracking().FirstOrDefaultAsync(x => x.UnitId == id);
        return model!;
    }

    public async Task<OutputResponse> UpdateAsync(UnitMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.UnitMasters.FirstOrDefaultAsync(x => x.UnitId == entity.UnitId);
            if (existing is null) { return new OutputResponse(0, "Unit not found."); }

            existing.Code = entity.Code;
            existing.Name = entity.Name;
            existing.Remarks = entity.Remarks;

            await db.SaveChangesAsync();
            return new OutputResponse(existing.UnitId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
}



