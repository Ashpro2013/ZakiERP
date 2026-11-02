using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Models;
using ZakiSAASERP.Models.Company;
using AshproStringExtension;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Text.RegularExpressions;
namespace ZakiSAASERP.MySQL;


public class CompanyHelper(IDatabaseSeedService seedService) : ICompanyHelper
{
    #region 🔹 Create Database (ADO.NET)
    private async Task CreateDatabaseAsync(string dbName)
    {
        try
        {
            var connectionString =
                $"Server={ServerAppData.Server};Port={ServerAppData.Port};User ID={ServerAppData.UserName};Password={ServerAppData.Password};SslMode=None;";

            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            var cmdText = $"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";

            await using var cmd = new MySqlCommand(cmdText, conn);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    public async Task<(bool, LoginCompany)> CreateCompanyDatabase(ExLCompany exlcompany)
    {
        var company = exlcompany.CopyTo<LoginCompany>();

        try
        {
            var tenantConnection =
               "Server=MYSQL5045.site4now.net;Database=db_a9e901_1001db;Uid=a9e901_1001db;Pwd=Thannippilavil@1;SslMode=None;";
            if (string.IsNullOrEmpty(tenantConnection))
            {
                // ✅ Step 1: Clean DB Name
                var database = exlcompany.CompanyName.Replace(" ", "_");
                database = Regex.Replace(database, @"[^\w]", "");
                var dbName = $"{database}_db";

                // ✅ Step 2: Create Database
                await CreateDatabaseAsync(dbName);

                // ✅ Step 3: Tenant Connection String
                tenantConnection =
                    $"Server={ServerAppData.Server};Database={dbName};user={ServerAppData.UserName};Password={ServerAppData.Password};SslMode=None;";
            }

            company.ConnectionString = tenantConnection;

            // ✅ Step 4: Apply EF Core Migrations
            var options = new DbContextOptionsBuilder<TenantDBContext>()
                .UseMySql(tenantConnection, ServerVersion.AutoDetect(tenantConnection), b => b.MigrationsAssembly("ZakiSAASERP.MySQL"))
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
            string errorMsg = ex.Message;
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
                .UseMySql(company.ConnectionString, ServerVersion.AutoDetect(company.ConnectionString), b => b.MigrationsAssembly("ZakiSAASERP.MySQL"))
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

