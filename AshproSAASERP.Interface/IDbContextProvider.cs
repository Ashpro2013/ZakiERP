namespace AshproSAASERP.Interface;

public interface IDbContextProvider
{
    TenantDBContext CreateDbContext(string? con = null);
}
