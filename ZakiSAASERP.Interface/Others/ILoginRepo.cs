namespace ZakiSAASERP.Interface.Others;

public interface ILoginRepo
{
    Task<List<BranchMaster>> GetBranchesForUserAsync(string username, string connectionString);
    Task<List<BranchMaster>> GetAllLoginBranchesAsync(string connectionString);
    Task<List<FinancialYear>> GetOpenFinancialYearsAsync(string connectionString);
    Task<UserDetails> GetUserByCredentialsAsync(string username, string password, string connectionString);
}
