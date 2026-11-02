using AshproSAASERP.Models;
using AshproSAASERP.Models.Company;
using AshproSAASERP.Models.User;
using AshproStringExtension;
using System.Text.RegularExpressions;
using AshproSAASERP.Repo.Data;
using Microsoft.Data.SqlClient;
using AshproSAASERP.Repo.Others;

namespace AshproSAASERP.Repo.LoginLogic;


public class CompanyHelper(DatabaseSeedService seedService) : ICompanyHelper
{
    #region 🔹 Create Database (ADO.NET)

    private async Task CreateDatabaseAsync(string dbName)
    {
        var masterConnection =
            $"Server={ServerAppData.Server};Database=master;User ID={ServerAppData.UserName};Password={ServerAppData.Password};TrustServerCertificate=True";

        using var connection = new SqlConnection(masterConnection);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            IF DB_ID('{dbName}') IS NULL
            BEGIN
                CREATE DATABASE [{dbName}]
            END";

        await command.ExecuteNonQueryAsync();
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
                $"Server={ServerAppData.Server};Database={dbName};User ID={ServerAppData.UserName};Password={ServerAppData.Password};TrustServerCertificate=True";

            company.ConnectionString = tenantConnection;

            // ✅ Step 4: Apply EF Core Migrations
            var options = new DbContextOptionsBuilder<TenantDBContext>()
                .UseSqlServer(tenantConnection, b => b.MigrationsAssembly("AshproSAASERP.MSSQL"))
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
                .UseSqlServer(company.ConnectionString, b => b.MigrationsAssembly("AshproSAASERP.MSSQL"))
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

