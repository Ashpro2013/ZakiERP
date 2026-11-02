using System.Linq.Expressions;
namespace AshproSAASERP.Repo.Masters;

public class BranchRepo(IDbContextProvider contextProvider) : IBranchRepo
{
    public async Task<OutputResponse> CreateAsync(BranchMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var exists = await db.BranchMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
            if (exists) return new OutputResponse(0, "Branch Master already exists.");

            await db.BranchMasters.AddAsync(entity);
            await db.SaveChangesAsync();
            return new OutputResponse(entity.BranchId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.BranchMasters.FindAsync(id);
            if (existing is null) { return new OutputResponse(0, "Branch not found."); }

            db.BranchMasters.Remove(existing);
            await db.SaveChangesAsync();
            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<List<BranchMaster>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.BranchMasters.AsNoTracking().ToListAsync();
    }
    public async Task<(List<BranchMaster>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        IQueryable<BranchMaster> query;
        var sortExpr = pagination.sortColumn switch
        {
            "Code" => (Expression<Func<BranchMaster, object>>)(x => x.Code),
            "Name" => x => x.Name,
            _ => x => x.BranchId
        };
        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            query = pagination.sortDirection == "desc" ? db.BranchMasters.AsNoTracking().Where(x => x.Name.Contains(pagination.search) || x.Code.Contains(pagination.search))
             .OrderByDescending(sortExpr)
           : db.BranchMasters.AsNoTracking().Where(x => x.Name.Contains(pagination.search) || x.Code.Contains(pagination.search)).OrderBy(sortExpr);
        }
        else
        {
            query = pagination.sortDirection == "desc" ? db.BranchMasters.AsNoTracking().OrderByDescending(sortExpr) :
                db.BranchMasters.AsNoTracking().OrderBy(sortExpr);
        }
        int count = await query.CountAsync();
        var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
        return (list, count);
    }
    public async Task<BranchMaster> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        var model = await db.BranchMasters.AsNoTracking().FirstOrDefaultAsync(x => x.BranchId == id);
        return model!;
    }
    public async Task<OutputResponse> UpdateAsync(BranchMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.BranchMasters.FirstOrDefaultAsync(x => x.BranchId == entity.BranchId);
            if (existing is null) { return new OutputResponse(0, "Branch not found."); }
            existing.Code = entity.Code;
            existing.Name = entity.Name;
            existing.Remarks = entity.Remarks;
            existing.AccountId = entity.AccountId;
            existing.ShowInLogin = entity.ShowInLogin;
            await db.SaveChangesAsync();
            return new OutputResponse(existing.BranchId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
}



