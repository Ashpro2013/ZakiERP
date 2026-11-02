using AshproSAASERP.Models.Company;

namespace AshproSAASERP.Interface.Others;

public interface IDatabaseSeedService
{
    Task Seed(TenantDBContext? context = null, ExLCompany? exlcompany = null);
}
