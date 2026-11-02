using AshproSAASERP.Models.Company;
using AshproSAASERP.Repo;

namespace AshproSAASERP.Desk.Services;

public class TenantRepository : ITenantRepository
{
    public Task<string?> GetConnectionStringAsync(string companyCode)
    {
        throw new NotImplementedException();
    }

    public Task<LoginCompany?> GetLoginCompanyAsync(string companyCode)
    {
        throw new NotImplementedException();
    }
}


