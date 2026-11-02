using ZakiSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZakiSAASERP.SQlite.Data;

public class MasterDBContextFactory : IDesignTimeDbContextFactory<MasterDBContext>
{
    public MasterDBContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MasterDBContext>();
        builder.UseSqlite("Data Source=LoginDB.db");
        return new MasterDBContext(builder.Options);
    }
}
