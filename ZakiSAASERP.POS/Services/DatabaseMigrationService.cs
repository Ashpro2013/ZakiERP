using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Others;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ZakiSAASERP.POS.Services;

public class DatabaseMigrationService(IServiceProvider serviceProvider) : IDatabaseMigrationService
{
    public async Task<(bool Success, string Message)> MigrateAndSeedAsync(
        string provider,
        string connectionString,
        Action<string>? statusCallback = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return (false, "Please select a database provider.");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return (false, "Please enter a connection string.");
        }

        try
        {
            statusCallback?.Invoke("Connecting to database server...");

            var options = AppUtility.CreateDbContextOptions(provider, connectionString);
            await using var db = new TenantDBContext(options);

            db.Database.SetCommandTimeout(300);

            statusCallback?.Invoke("Checking database schema and applying migrations...");
            await db.Database.MigrateAsync();

            statusCallback?.Invoke("Database schema updated. Verifying initial data...");

            var hasBranches = await db.BranchMasters.AnyAsync();
            if (!hasBranches)
            {
                statusCallback?.Invoke("Seeding new database with default company and admin user...");
                using var scope = serviceProvider.CreateScope();
                var seedService = provider switch
                {
                    "SQLite" => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.SQlite.DatabaseSeedService>(),
                    "PSQL" => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.PSQL.DatabaseSeedService>(),
                    "MySQL" => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.MySQL.DatabaseSeedService>(),
                    "Oracle" => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.Oracle.DatabaseSeedService>(),
                    _ => (IDatabaseSeedService)scope.ServiceProvider.GetRequiredService<ZakiSAASERP.MSSQL.DatabaseSeedService>()
                };

                await seedService.Seed(db, null);
            }

            try
            {
                await ZakiSAASERP.Interface.Others.MenuSeedHelper.EnsureSeedDataAsync(db);
            }
            catch { }

            AppData.DBProvider = provider;
            AppData.ConnectionString = connectionString;
            RegistryHelper.SetData("DBProvider", provider);
            RegistryHelper.SetData("ConString", connectionString);

            statusCallback?.Invoke("Database ready.");
            return (true, "Database updated and connected successfully.");
        }
        catch (Exception ex)
        {
            return (false, "Database connection or migration failed: " + ex.Message);
        }
    }
}

