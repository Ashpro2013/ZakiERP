namespace ZakiSAASERP.Repo;

public interface ITenantService
{
    string? CompanyCode { get; }
    string? ConnectionString { get; }

    void SetTenant(string companyCode, string connectionString);
}



