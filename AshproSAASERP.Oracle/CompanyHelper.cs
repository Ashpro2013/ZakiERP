using AshproSAASERP.Interface;
using AshproSAASERP.Interface.Others;
using AshproSAASERP.Models;
using AshproSAASERP.Models.Company;
using AshproStringExtension;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;

namespace AshproSAASERP.Oracle;

public class CompanyHelper(IDatabaseSeedService seedService) : ICompanyHelper
{
    #region 🔹 Create Database (Oracle Schema)

    private async Task CreateDatabaseAsync(string dbName)
    {
        try
        {
            var server = ServerAppData.Server ?? "localhost";
            var port = ServerAppData.Port ?? "1521";
            var serviceName = "FREE";
            if (server.Contains('/'))
            {
                var parts = server.Split('/');
                server = parts[0];
                serviceName = parts[1];
            }
            var dataSource = $"{server}:{port}/{serviceName}";
            var masterConnection = $"User Id={ServerAppData.UserName};Password={ServerAppData.Password};Data Source={dataSource}";

            await using var conn = new OracleConnection(masterConnection);
            await conn.OpenAsync();

            var cleanedDbName = Regex.Replace(dbName, @"[^\w]", "").ToUpper();
            var tenantPassword = ServerAppData.Password;

            bool userExists = false;
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = $"SELECT COUNT(*) FROM all_users WHERE username = '{cleanedDbName}'";
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                userExists = count > 0;
            }

            if (!userExists)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE USER {cleanedDbName} IDENTIFIED BY \"{tenantPassword}\"";
                    await cmd.ExecuteNonQueryAsync();

                    cmd.CommandText = $"GRANT CONNECT, RESOURCE, CREATE VIEW, UNLIMITED TABLESPACE TO {cleanedDbName}";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
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
            // ✅ Step 1: Clean DB Name (Schema Name)
            var database = exlcompany.CompanyName.Replace(" ", "_");
            database = Regex.Replace(database, @"[^\w]", "");
            var dbName = $"{database}_db";

            // ✅ Step 2: Create Oracle User/Schema
            await CreateDatabaseAsync(dbName);

            // ✅ Step 3: Tenant Connection String
            var server = ServerAppData.Server ?? "localhost";
            var port = ServerAppData.Port ?? "1521";
            var serviceName = "FREE";
            if (server.Contains('/'))
            {
                var parts = server.Split('/');
                server = parts[0];
                serviceName = parts[1];
            }
            var dataSource = $"{server}:{port}/{serviceName}";
            var cleanedDbName = Regex.Replace(dbName, @"[^\w]", "").ToUpper();
            var tenantConnection = $"User Id={cleanedDbName};Password={ServerAppData.Password};Data Source={dataSource}";

            company.ConnectionString = tenantConnection;

            // ✅ Step 4: Apply EF Core Migrations
            var options = new DbContextOptionsBuilder<TenantDBContext>()
                .UseOracle(tenantConnection, b => b.MigrationsAssembly("AshproSAASERP.Oracle"))
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
            Console.WriteLine($"Error creating company database: {ex.Message}");
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
                .UseOracle(company.ConnectionString, b => b.MigrationsAssembly("AshproSAASERP.Oracle"))
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
