namespace AshproSAASERP.Repo;

public class TenantService : ITenantService
{
    public string? CompanyCode { get; private set; }
    public string? ConnectionString { get; private set; }
    public void SetTenant(string companyCode, string connectionString)
    {
        CompanyCode = companyCode;
        ConnectionString = connectionString;
    }
}