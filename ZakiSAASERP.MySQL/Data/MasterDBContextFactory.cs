using ZakiSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZakiSAASERP.MySQL.Data;

public class MasterDBContextFactory : IDesignTimeDbContextFactory<MasterDBContext>
{
    public MasterDBContext CreateDbContext(string[] args)
    {
        var connectionString = "Server=MYSQL5045.site4now.net;Database=db_a9e901_logindb;Uid=a9e901_logindb;Pwd=Thannippilavil@1;SslMode=None;";
        var builder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<MasterDBContext>();
        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        return new MasterDBContext(builder.Options);
    }
}


