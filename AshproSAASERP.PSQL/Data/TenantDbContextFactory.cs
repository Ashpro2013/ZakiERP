using AshproSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AshproSAASERP.PSQL.Data;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDBContext>
{
    public TenantDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();

        // 🔥 IMPORTANT: Use your default/dev connection here
        var connectionString = $"Host=localhost;Port=5710;Database=AshproERP_DB;Username=postgres;Password=571100";


        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("AshproSAASERP.PSQL"));
        // 👉 If PostgreSQL:
        // optionsBuilder.UseNpgsql("Host=localhost;Database=TestDb;Username=postgres;Password=yourpassword");

        return new TenantDBContext(optionsBuilder.Options);
    }
}
