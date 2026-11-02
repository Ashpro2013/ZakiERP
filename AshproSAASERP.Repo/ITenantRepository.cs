using AshproSAASERP.Models.Company;

namespace AshproSAASERP.Repo
{
    public interface ITenantRepository
    {
        Task<string?> GetConnectionStringAsync(string companyCode);
        Task<LoginCompany?> GetLoginCompanyAsync(string companyCode);
    }

}



