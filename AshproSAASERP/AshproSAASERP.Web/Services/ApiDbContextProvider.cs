using AshproSAASERP.Interface;
using Microsoft.EntityFrameworkCore;

namespace AshproSAASERP.Web.Services;

public class ApiDbContextProvider : IDbContextProvider
{
    private readonly ITenantService _tenantService;
    private readonly DbContextOptions<TenantDBContext> _options;
    private readonly IConfiguration _config;

    public ApiDbContextProvider(
        ITenantService tenantService,
        DbContextOptions<TenantDBContext> options,
        IConfiguration config)
    {
        _tenantService = tenantService;
        _options = options;
        _config = config;
    }

    public TenantDBContext CreateDbContext(string? con = null)
    {
        var dbProvider = _config["DbProvider"] ?? "MSSQL";
        var tenantType = _config["TenantType"];
        if (string.Equals(tenantType, "single", StringComparison.OrdinalIgnoreCase))
        {
            con = _config[$"ConnectionStrings:{dbProvider}:Master"];
        }
        else
        {
            if (!string.IsNullOrEmpty(_tenantService.ConnectionString))
            {
                con = _tenantService.ConnectionString;
            }
        }

        if (string.IsNullOrEmpty(con))
            throw new InvalidOperationException("Tenant not resolved");

        var builder = new DbContextOptionsBuilder<TenantDBContext>(_options);
        builder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        switch (dbProvider.ToUpperInvariant())
        {
            case "SQLITE":
                builder.UseSqlite(con, b => b.MigrationsAssembly("AshproSAASERP.SQlite"));
                break;
            case "PSQL":
                builder.UseNpgsql(con, b => b.MigrationsAssembly("AshproSAASERP.PSQL"));
                break;
            case "MYSQL":
                builder.UseMySql(con, ServerVersion.AutoDetect(con), b => b.MigrationsAssembly("AshproSAASERP.MySQL"));
                break;
            case "ORACLE":
                builder.UseOracle(con, b => b.MigrationsAssembly("AshproSAASERP.Oracle"));
                break;
            case "MSSQL":
            default:
                builder.UseSqlServer(con, b => b.MigrationsAssembly("AshproSAASERP.MSSQL"));
                break;
        }

        builder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        return new TenantDBContext(builder.Options);
    }
}

