using ZakiSAASERP.Interface;
using ZakiSAASERP.Models.Company;
using Microsoft.EntityFrameworkCore;
namespace ZakiSAASERP.Web.Services;

public class TenantRepository(MasterDBContext _db, IConfiguration config) : ITenantRepository
{
    public async Task<string?> GetConnectionStringAsync(string tenantCode)
    {
        if (string.IsNullOrWhiteSpace(tenantCode))
            return null;
        string connection = string.Empty;
        if (tenantCode != "maui")
        {
            var result = await _db.LoginCompanies
                .Where(x => x.CompanyCode == tenantCode)
                .FirstOrDefaultAsync();
            connection = result?.ConnectionString!;
        }
        else
        {
            connection = config.GetConnectionString("Master")!;
        }
        return connection;
    }

    public async Task<LoginCompany?> GetLoginCompanyAsync(string companyCode)
    {

        if (string.IsNullOrWhiteSpace(companyCode))
            return null;
        return await _db.LoginCompanies
               .Where(x => x.CompanyCode == companyCode)
               .FirstOrDefaultAsync();
    }
}



