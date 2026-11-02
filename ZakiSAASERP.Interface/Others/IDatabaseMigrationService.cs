namespace ZakiSAASERP.Interface.Others;

public interface IDatabaseMigrationService
{
    Task<(bool Success, string Message)> MigrateAndSeedAsync(
        string provider,
        string connectionString,
        Action<string>? statusCallback = null);
}
