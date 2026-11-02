using ZakiSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZakiSAASERP.Oracle.Data;

public class MasterDBContextFactory : IDesignTimeDbContextFactory<MasterDBContext>
{
    public MasterDBContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MasterDBContext>();
        builder.UseOracle("User Id=SYSTEM;Password=your_password;Data Source=localhost:1521/FREE",
            b => b.MigrationsAssembly("ZakiSAASERP.Oracle"));
        return new MasterDBContext(builder.Options);
    }
}
