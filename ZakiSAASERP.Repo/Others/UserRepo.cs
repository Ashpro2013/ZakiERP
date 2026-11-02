using ZakiSAASERP.Models.User;
namespace ZakiSAASERP.Repo.Others;

public class UserRepo(IDbContextProvider contextProvider) : IUserRepo
{
    public async Task<OutputResponse> CreateAsync(UserDetails entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var exists = await db.UserDetails.AnyAsync(x => x.UserName == entity.UserName);
            if (exists) return new OutputResponse(0, "User already exists.");
            await db.UserDetails.AddAsync(entity);
            await db.SaveChangesAsync();
            return new OutputResponse(entity.UserId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            if (id is 0) return new OutputResponse(0, "Invalid identifier.");
            var existing = await db.UserDetails.FindAsync(id);
            if (existing is null) return new OutputResponse(0, "User not found.");

            // Delete associated UserBranches first
            var userBranches = db.UserBranches.Where(x => x.FK_UserId == id);
            db.UserBranches.RemoveRange(userBranches);

            db.UserDetails.Remove(existing);
            await db.SaveChangesAsync();
            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<List<UserDetails>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.UserDetails.AsNoTracking().ToListAsync();
    }
    public async Task<(List<UserDetails>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        int count = await db.UserDetails.CountAsync();
        var list = await db.UserDetails
            .AsNoTracking()
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        return (list, count);
    }
    public async Task<UserDetails> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        if (id is 0) return null!;
        var model = await db.UserDetails.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id);
        return model!;
    }
    public async Task<UserDetails> GetForLogin(string userName, string password)
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.UserDetails.FirstOrDefaultAsync(x => x.UserName == userName && x.PassWord == password)!;
    }
    public async Task<OutputResponse> UpdateAsync(UserDetails entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.UserDetails.FirstOrDefaultAsync(x => x.UserId == entity.UserId);
            if (existing is null) return new OutputResponse(0, "User not found.");
            existing.PrevilageId = entity.PrevilageId;
            existing.UserName = entity.UserName;
            existing.PassWord = entity.PassWord;
            existing.FK_BranchId = entity.FK_BranchId;
            await db.SaveChangesAsync();
            return new OutputResponse(existing.UserId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<List<ZakiSAASERP.Models.Masters.BranchMaster>> GetBranchesForUser(string userName)
    {
        await using var db = contextProvider.CreateDbContext();
        return await (from ub in db.UserBranches
                      join u in db.UserDetails on ub.FK_UserId equals u.UserId
                      join b in db.BranchMasters on ub.FK_BranchId equals b.BranchId
                      where u.UserName == userName && b.ShowInLogin
                      select b).AsNoTracking().ToListAsync();
    }

    public async Task<OutputResponse> SaveUserBranches(List<UserBranch> userBranches)
    {
        await using var db = contextProvider.CreateDbContext();
        if (userBranches == null || !userBranches.Any()) return new OutputResponse(0, "No branches to save.");
        try
        {
            var userId = userBranches.First().FK_UserId;
            var existing = await db.UserBranches.Where(x => x.FK_UserId == userId).ToListAsync();
            db.UserBranches.RemoveRange(existing);
            await db.SaveChangesAsync();

            await db.UserBranches.AddRangeAsync(userBranches);
            await db.SaveChangesAsync();
            return new OutputResponse(userId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
}



