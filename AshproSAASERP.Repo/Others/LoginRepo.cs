using AshproSAASERP.Models.User;

namespace AshproSAASERP.Repo.Others;

public class LoginRepo(IDbContextProvider contextProvider) : ILoginRepo
{
    public async Task<List<BranchMaster>> GetBranchesForUserAsync(string username, string connectionString)
    {
        await using var db = contextProvider.CreateDbContext(connectionString);
        var branches = await (from b in db.BranchMasters
                              join ub in db.UserBranches on b.BranchId equals ub.FK_BranchId into ubGroup
                              from ub in ubGroup.DefaultIfEmpty()
                              join ud in db.UserDetails on ub.FK_UserId equals ud.UserId into udGroup
                              from ud in udGroup.DefaultIfEmpty()
                              where b.ShowInLogin && (ud.UserName == username || ud.UserName == "Admin")
                              select b).Distinct().ToListAsync();
        if (!branches.Any())
            branches = await (from b in db.BranchMasters join u in db.UserDetails on b.BranchId equals u.FK_BranchId where u.UserName == username && b.ShowInLogin select b).ToListAsync();
        return branches;

    }

    public async Task<List<BranchMaster>> GetAllLoginBranchesAsync(string connectionString)
    {
        await using var db = contextProvider.CreateDbContext(connectionString);
        return await db.BranchMasters.Where(x => x.ShowInLogin).ToListAsync();
    }

    public async Task<List<FinancialYear>> GetOpenFinancialYearsAsync(string connectionString)
    {
        await using var db = contextProvider.CreateDbContext(connectionString);
        return await db.FinancialYears.Where(x => !x.IsClosed).ToListAsync();
    }

    public async Task<UserDetails> GetUserByCredentialsAsync(string username, string password, string connectionString)
    {
        await using var db = contextProvider.CreateDbContext(connectionString);
        return (await db.UserDetails.FirstOrDefaultAsync(x => x.UserName == username && x.PassWord == password))!;
    }
}



