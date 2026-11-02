using AshproSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AshproSAASERP.PSQL.Data;

public class MasterDBContextFactory : IDesignTimeDbContextFactory<MasterDBContext>
{
    public MasterDBContext CreateDbContext(string[] args)
    {
        var builder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<MasterDBContext>();
        builder.UseNpgsql("Host=localhost;Port=5710;Database=Login_DB;Username=postgres;Password=571100");
        return new MasterDBContext(builder.Options);
    }
}


