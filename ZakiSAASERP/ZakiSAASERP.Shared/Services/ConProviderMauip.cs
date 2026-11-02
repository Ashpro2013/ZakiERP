namespace ZakiSAASERP.Shared.Services;

public class ConProviderMaui : IConProviderMaui
{
    private volatile string? _connectionString;
    public string GetConnectionString() => _connectionString!;
    public void SetConnectionString(string conn) => _connectionString = conn;
}
