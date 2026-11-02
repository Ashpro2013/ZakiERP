using AshproSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AshproSAASERP.Oracle.Data;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDBContext>
{
    public TenantDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();
        optionsBuilder.UseOracle("User Id=SYSTEM;Password=ash@Pro#;Data Source=localhost:1521/FREEPDB1",
            b => b.MigrationsAssembly("AshproSAASERP.Oracle"));
        return new TenantDBContext(optionsBuilder.Options);
    }
}
