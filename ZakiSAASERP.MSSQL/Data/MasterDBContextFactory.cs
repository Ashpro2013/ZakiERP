using System;
using System.IO;
using System.Text.Json;
using ZakiSAASERP.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZakiSAASERP.MSSQL.Data;

public class MasterDBContextFactory : IDesignTimeDbContextFactory<MasterDBContext>
{
    public MasterDBContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MasterDBContext>();

        var connectionString = (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            ? args[0]
            : GetConnectionString("DefaultConnection")
              ?? "Data Source=SQL8012.site4now.net;Initial Catalog=db_a9e901_logindb;User Id=db_a9e901_logindb_admin;Password=Thannippilavil@1;Connection Timeout=30;TrustServerCertificate=True;";

        builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("ZakiSAASERP.MSSQL"));
        return new MasterDBContext(builder.Options);
    }

    private static string? GetConnectionString(string name)
    {
        var current = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(current))
        {
            var webConfig = Path.Combine(current, "ZakiSAASERP", "ZakiSAASERP.Web", "appsettings.json");
            if (File.Exists(webConfig))
            {
                var conn = ReadFromJson(webConfig, name);
                if (!string.IsNullOrEmpty(conn)) return conn;
            }

            var directConfig = Path.Combine(current, "appsettings.json");
            if (File.Exists(directConfig))
            {
                var conn = ReadFromJson(directConfig, name);
                if (!string.IsNullOrEmpty(conn)) return conn;
            }

            var parent = Directory.GetParent(current);
            if (parent == null || parent.FullName == current) break;
            current = parent.FullName;
        }
        return null;
    }

    private static string? ReadFromJson(string filePath, string name)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                cs.TryGetProperty("MSSQL", out var mssql) &&
                mssql.TryGetProperty(name, out var val))
            {
                return val.GetString();
            }
        }
        catch { }
        return null;
    }
}



