using ZakiSAASERP.Interface;
using Microsoft.EntityFrameworkCore;

namespace ZakiSAASERP.Desk.Services;

public static class AppUtility
{
    public static List<string>? uMacId { get; set; }
    public static string? ActivationKey { get; set; }
    public static string? Activationlicense { get; set; }
    public static string? ActivationStartDate { get; set; }
    public static string? ActivationEndDate { get; set; }
    public static string? ActivationMacID { get; set; }
    public static string? uLicenceNumber { get; set; }
    public static bool isActivated { get; set; }
    public static string? uManufacturer { get; set; }
    public static string? uSpecMacId { get; set; }

    public static string DetectDatabaseProvider(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return "SQLite";

        var lower = connectionString.ToLowerInvariant();

        if (lower.Contains(".db") || lower.Contains(".sqlite") || (lower.Contains("data source=") && !lower.Contains(";") && !lower.Contains("user id") && !lower.Contains("server=")))
        {
            return "SQLite";
        }
        if (lower.Contains("uid=") || lower.Contains("pwd=") || lower.Contains("port=3306"))
        {
            return "MySQL";
        }
        if (lower.Contains("port=5432") || lower.Contains("username=postgres") || lower.Contains("npgsql"))
        {
            return "PSQL";
        }
        if (lower.Contains("1521") || lower.Contains("freepdb") || lower.Contains("orcl"))
        {
            return "Oracle";
        }
        if (lower.Contains("server=") || lower.Contains("initial catalog=") || lower.Contains("trusted_connection") || lower.Contains("trustservercertificate") || lower.Contains("user id=sa") || lower.Contains("database="))
        {
            return "MSSQL";
        }

        return "SQLite";
    }

    public static void ConfigureDbContextOptions(DbContextOptionsBuilder builder, string provider, string? connectionString)
    {
        switch (provider.ToUpperInvariant())
        {
            case "MSSQL":
                builder.UseSqlServer(connectionString ?? "", x => x.MigrationsAssembly("ZakiSAASERP.MSSQL"));
                break;
            case "PSQL":
                builder.UseNpgsql(connectionString ?? "", x => x.MigrationsAssembly("ZakiSAASERP.PSQL"));
                break;
            case "MYSQL":
                if (!string.IsNullOrEmpty(connectionString))
                {
                    try
                    {
                        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), x => x.MigrationsAssembly("ZakiSAASERP.MySQL"));
                    }
                    catch
                    {
                        builder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)), x => x.MigrationsAssembly("ZakiSAASERP.MySQL"));
                    }
                }
                else
                {
                    builder.UseMySql("Server=localhost;Database=AshproDb;Uid=root;Pwd=;", new MySqlServerVersion(new Version(8, 0, 36)), x => x.MigrationsAssembly("ZakiSAASERP.MySQL"));
                }
                break;
            case "ORACLE":
                builder.UseOracle(connectionString ?? "", x => x.MigrationsAssembly("ZakiSAASERP.Oracle"));
                break;
            default:
                builder.UseSqlite(connectionString ?? "", x => x.MigrationsAssembly("ZakiSAASERP.SQlite"));
                break;
        }
        builder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public static DbContextOptions<TenantDBContext> CreateDbContextOptions(string provider, string connectionString)
    {
        var builder = new DbContextOptionsBuilder<TenantDBContext>();
        ConfigureDbContextOptions(builder, provider, connectionString);
        return builder.Options;
    }
}
