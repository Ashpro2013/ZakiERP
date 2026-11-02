namespace ZakiSAASERP.MySQL.Data;

using ZakiSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDBContext>
{
    public TenantDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();

        var connectionString = "Server=MYSQL5045.site4now.net;Database=db_a9e901_1001db;Uid=a9e901_1001db;Pwd=Thannippilavil@1;SslMode=None;";

        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)), b => b.MigrationsAssembly("ZakiSAASERP.MySQL"));
        // 👉 If PostgreSQL:
        // optionsBuilder.UseNpgsql("Host=localhost;Database=TestDb;Username=postgres;Password=yourpassword");

        return new TenantDBContext(optionsBuilder.Options);
    }
}
