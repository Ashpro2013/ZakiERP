using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Models;
using ZakiSAASERP.Models.Company;
using AshproStringExtension;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.RegularExpressions;
namespace ZakiSAASERP.PSQL;

public class CompanyHelper(IDatabaseSeedService seedService) : ICompanyHelper
{
    #region 🔹 Create Database (ADO.NET)

    private async Task CreateDatabaseAsync(string dbName)
    {
        try
        {
            var masterConnection =
                $"Host={ServerAppData.Server};Port={ServerAppData.Port};Username={ServerAppData.UserName};Password={ServerAppData.Password};Database=postgres";

            await using var conn = new NpgsqlConnection(masterConnection);
            await conn.OpenAsync();

            var cmdText = $"CREATE DATABASE \"{dbName}\"";

            await using var cmd = new NpgsqlCommand(cmdText, conn);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

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
            var tenantConnection =
            $"Host={ServerAppData.Server};Port={ServerAppData.Port};Username={ServerAppData.UserName};Password={ServerAppData.Password};Database={dbName}";

            company.ConnectionString = tenantConnection;

            // ✅ Step 4: Apply EF Core Migrations
            var options = new DbContextOptionsBuilder<TenantDBContext>()
                .UseNpgsql(tenantConnection, b => b.MigrationsAssembly("ZakiSAASERP.PSQL"))
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
        catch (Exception ex)
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
                .UseNpgsql(company.ConnectionString, b => b.MigrationsAssembly("ZakiSAASERP.PSQL"))
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var context = new TenantDBContext(options);

            // ✅ Applies only pending migrations
            await context.Database.MigrateAsync();

            return company;
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion
}

