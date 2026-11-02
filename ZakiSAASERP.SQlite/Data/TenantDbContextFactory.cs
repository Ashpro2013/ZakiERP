using ZakiSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZakiSAASERP.SQlite.Data;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDBContext>
{
    public TenantDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();
        var connectionString = "Data Source=Ashpro_db.db";
        optionsBuilder.UseSqlite(connectionString, b => b.MigrationsAssembly("ZakiSAASERP.SQlite"));
        return new TenantDBContext(optionsBuilder.Options);
    }
}
