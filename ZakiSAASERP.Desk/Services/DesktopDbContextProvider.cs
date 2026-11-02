using ZakiSAASERP.Interface;
using ZakiSAASERP.Shared.Services;
using Microsoft.EntityFrameworkCore;
namespace ZakiSAASERP.Desk.Services;

public class DesktopDbContextProvider(
        IDbContextFactory<TenantDBContext> _factory,
        IConProviderMaui _conProvider) : IDbContextProvider
{

    public TenantDBContext CreateDbContext(string? con = null)
    {
        if (string.IsNullOrWhiteSpace(con))
        {
            con = _conProvider.GetConnectionString();
        }

        if (string.IsNullOrWhiteSpace(con))
        {
            con = AppData.ConnectionString;
        }

        if (string.IsNullOrWhiteSpace(con))
        {
            con = RegistryHelper.GetData("ConString", "");
        }

        if (string.IsNullOrWhiteSpace(con))
            throw new InvalidOperationException("Connection string not set");

        if (string.IsNullOrEmpty(AppData.ConnectionString))
        {
            AppData.ConnectionString = con;
        }

        var provider = AppData.DBProvider;
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = RegistryHelper.GetData("DBProvider", "");
        }
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = AppUtility.DetectDatabaseProvider(con);
            if (!string.IsNullOrWhiteSpace(provider))
            {
                AppData.DBProvider = provider;
                RegistryHelper.SetData("DBProvider", provider);
            }
        }
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = "MSSQL";
        }

        var options = AppUtility.CreateDbContextOptions(provider, con);
        return new TenantDBContext(options);
    }
}
