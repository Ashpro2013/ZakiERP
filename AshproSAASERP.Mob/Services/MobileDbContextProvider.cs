using AshproSAASERP.Interface;
using AshproSAASERP.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace AshproSAASERP.Mob.Services;

public class MobileDbContextProvider(
        IDbContextFactory<TenantDBContext> _factory,
        IConProviderMaui _conProvider) : IDbContextProvider
{

    public TenantDBContext CreateDbContext(string? con = null)
    {
        var conn = _conProvider.GetConnectionString();

        if (!string.IsNullOrEmpty(conn))
        {
            con = conn;
        }
        if (string.IsNullOrEmpty(con))
            throw new InvalidOperationException("Connection string not set");

        var db = _factory.CreateDbContext();
        db.Database.SetConnectionString(con);
        return db;
    }
}
