using AshproSAASERP.Interface;
using AshproSAASERP.Interface.Others;
using AshproSAASERP.Models;
using AshproSAASERP.Models.Company;
using AshproStringExtension;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AshproSAASERP.SQlite;

public class CompanyHelper(IDatabaseSeedService seedService) : ICompanyHelper
{
    #region 🔹 Create Database

    private async Task CreateDatabaseAsync(string dbName)
    {
        // For SQLite, database file is created automatically when the connection is opened.
        // We just ensure the target directory exists if we use a specific path.
        await Task.CompletedTask;
    }

    #endregion

    #region 🔹 Create Tenant Database

    public async Task<(bool, LoginCompany)> CreateCompanyDatabase(ExLCompany exlcompany)
    {
        var company = exlcompany.CopyTo<LoginCompany>();

        try
        {
            // ✅ Step 1: Clean DB Name
            var database = exlcompany.CompanyName.Replace(" ", "_");
            database = Regex.Replace(database, @"[^\w]", "");
            var dbName = $"{database}_db";

            // ✅ Step 2: Create Database
            await CreateDatabaseAsync(dbName);

            // ✅ Step 3: Tenant Connection String
            string tenantConnection = $"Data Source={dbName}.db";

            // If Server is specified and represents a valid directory path, use it.
            if (!string.IsNullOrEmpty(ServerAppData.Server))
            {
                try
                {
                    if (Directory.Exists(ServerAppData.Server))
                    {
                        tenantConnection = $"Data Source={Path.Combine(ServerAppData.Server, dbName + ".db")}";
                    }
                }
                catch
                {
                    // Fallback to local file in execution folder
                }
            }

            company.ConnectionString = tenantConnection;

            // ✅ Step 4: Apply EF Core Migrations
            var options = new DbContextOptionsBuilder<TenantDBContext>()
                .UseSqlite(tenantConnection, b => b.MigrationsAssembly("AshproSAASERP.SQlite"))
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
                .Options;

            using (var context = new TenantDBContext(options))
            {
                await context.Database.MigrateAsync();
            }

            // ✅ Step 5: Insert Default User
            using (var context = new TenantDBContext(options))
            {
                await seedService.Seed(context, exlcompany);
            }

            return (true, company);
        }
        catch (Exception)
        {
            // 🔴 Optional: Log error here
            return (false, company);
        }
    }
    #endregion

    #region 🔹 Update Tenant Database (Migrations)

    public async Task<LoginCompany?> UpdateDatabase(LoginCompany company)
    {
        try
        {
            var options = new DbContextOptionsBuilder<TenantDBContext>()
                .UseSqlite(company.ConnectionString, b => b.MigrationsAssembly("AshproSAASERP.SQlite"))
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var context = new TenantDBContext(options);

            // ✅ Applies only pending migrations
            await context.Database.MigrateAsync();

            // ✅ Seed missing data for existing databases
            await seedService.Seed(context);

            return company;
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion
}
