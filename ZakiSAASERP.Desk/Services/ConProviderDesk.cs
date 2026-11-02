using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Desk.Services;

public class ConProviderDesk : IConProviderMaui
{
    private volatile string? _connectionString;

    public string GetConnectionString()
    {
        return RewriteConnectionString(_connectionString);
    }

    public void SetConnectionString(string conn)
    {
        _connectionString = conn;
    }

    private string RewriteConnectionString(string? conn)
    {
        if (string.IsNullOrEmpty(conn))
        {
            return conn!;
        }

        if (!OperatingSystem.IsWindows())
        {
            var dbProvider = RegistryHelper.GetData("DBProvider", "");
            if (dbProvider == "SQLite" || string.IsNullOrEmpty(dbProvider))
            {
                var parts = conn.Split('=', 2);
                if (parts.Length == 2 && parts[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                {
                    var dbFile = parts[1].Trim();
                    if (!Path.IsPathRooted(dbFile))
                    {
                        var writablePath = Path.Combine(FileSystem.AppDataDirectory, dbFile);
                        var dir = Path.GetDirectoryName(writablePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        return $"Data Source={writablePath}";
                    }
                }
            }
        }
        return conn;
    }
}
