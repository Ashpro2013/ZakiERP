using AshproSAASERP.Interface;

namespace AshproSAASERP.POS.Services;

public class PosDbContextProvider : IDbContextProvider
{
    public TenantDBContext CreateDbContext(string? con = null)
    {
        if (string.IsNullOrWhiteSpace(con))
        {
            con = AppData.ConnectionString;
        }

        if (string.IsNullOrWhiteSpace(con))
        {
            con = RegistryHelper.GetData("ConString", "");
        }

        if (string.IsNullOrWhiteSpace(con))
        {
            throw new InvalidOperationException("Database connection string is not set in Registry (SOFTWARE\\Tech\\ConString).");
        }

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

