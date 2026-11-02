using ZakiSAASERP.Models.Company;

namespace ZakiSAASERP.Repo
{
    public interface ITenantRepository
    {
        Task<string?> GetConnectionStringAsync(string companyCode);
        Task<LoginCompany?> GetLoginCompanyAsync(string companyCode);
    }

}



