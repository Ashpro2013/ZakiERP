namespace AshproSAASERP.Repo.Masters;

public class AccountTypeRepo(IDbContextProvider contextProvider) : IAccountTypeRepo
{
    public async Task<List<AccountType>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.AccountTypes.AsNoTracking().ToListAsync();
    }
}



