using ZakiSAASERP.Models.Company;

namespace ZakiSAASERP.Interface.Others;

public interface IDatabaseSeedService
{
    Task Seed(TenantDBContext? context = null, ExLCompany? exlcompany = null);
}
