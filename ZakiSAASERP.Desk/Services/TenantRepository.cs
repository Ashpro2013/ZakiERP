using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Repo;

namespace ZakiSAASERP.Desk.Services;

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


