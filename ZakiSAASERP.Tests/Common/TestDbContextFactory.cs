using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ZakiSAASERP.Interface;

namespace ZakiSAASERP.Tests.Common;

public class TestDbContextFactoryWrapper : IDisposable
{
    private readonly SqliteConnection _connection;
    public TenantDBContext DbContext { get; }

    public TestDbContextFactoryWrapper()
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = ":memory:",
            ForeignKeys = false
        };

        _connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        _connection.Open();

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = OFF;";
            cmd.ExecuteNonQuery();
        }

        var options = new DbContextOptionsBuilder<TenantDBContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new TenantDBContext(options);
        DbContext.Database.EnsureCreated();

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = OFF;";
            cmd.ExecuteNonQuery();
        }
    }

    public TenantDBContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<TenantDBContext>()
            .UseSqlite(_connection)
            .Options;
        return new TenantDBContext(options);
    }

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
    }
}
