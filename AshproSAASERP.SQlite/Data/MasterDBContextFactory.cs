using AshproSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AshproSAASERP.SQlite.Data;

public class MasterDBContextFactory : IDesignTimeDbContextFactory<MasterDBContext>
{
    public MasterDBContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MasterDBContext>();
        builder.UseSqlite("Data Source=LoginDB.db");
        return new MasterDBContext(builder.Options);
    }
}
