using AshproSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AshproSAASERP.SQlite.Data;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDBContext>
{
    public TenantDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();
        var connectionString = "Data Source=Ashpro_db.db";
        optionsBuilder.UseSqlite(connectionString, b => b.MigrationsAssembly("AshproSAASERP.SQlite"));
        return new TenantDBContext(optionsBuilder.Options);
    }
}
