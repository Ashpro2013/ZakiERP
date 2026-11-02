namespace AshproSAASERP.MSSQL;

using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Threading.Tasks;

public static class MssqlBackupHelper
{
    public static (bool Success, string Message, string? BackupPath) BackupDatabase(string? connectionString, string? targetFolder = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return (false, "Connection string is empty.", null);
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(dbName))
            {
                return (false, "Could not identify database name from connection string.", null);
            }

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string backupDir = targetFolder?.Trim() ?? string.Empty;

            // If targetFolder specified, ensure it exists or create it
            if (!string.IsNullOrWhiteSpace(backupDir))
            {
                try
                {
                    if (!Directory.Exists(backupDir))
                    {
                        Directory.CreateDirectory(backupDir);
                    }
                }
                catch { }
            }

            // If no targetFolder specified, query SQL Server instance default backup directory
            if (string.IsNullOrWhiteSpace(backupDir))
            {
                try
                {
                    using var regCmd = connection.CreateCommand();
                    regCmd.CommandText = @"
DECLARE @dir NVARCHAR(4000);
EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'BackupDirectory', @dir OUTPUT;
SELECT @dir;";
                    var regRes = regCmd.ExecuteScalar();
                    if (regRes != null && regRes != DBNull.Value && !string.IsNullOrWhiteSpace(regRes.ToString()))
                    {
                        backupDir = regRes.ToString()!;
                    }
                }
                catch { }
            }

            // Fallback to SERVERPROPERTY('InstanceDefaultBackupPath')
            if (string.IsNullOrWhiteSpace(backupDir))
            {
                try
                {
                    using var dirCmd = connection.CreateCommand();
                    dirCmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(4000));";
                    var result = dirCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value && !string.IsNullOrWhiteSpace(result.ToString()))
                    {
                        backupDir = result.ToString()!;
                    }
                }
                catch { }
            }

            // Fallback to C:\AshproBackups
            if (string.IsNullOrWhiteSpace(backupDir))
            {
                backupDir = @"C:\AshproBackups";
                try
                {
                    if (!Directory.Exists(backupDir))
                    {
                        Directory.CreateDirectory(backupDir);
                    }
                }
                catch { }
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{dbName}_{timestamp}.bak";
            string fullBackupPath = Path.Combine(backupDir, fileName);

            string safeDbName = dbName.Replace("]", "]]");
            string safeBackupPath = fullBackupPath.Replace("'", "''");
            string safeName = $"{dbName} - Full Database Backup ({timestamp})".Replace("'", "''");

            using var backupCmd = connection.CreateCommand();
            backupCmd.CommandTimeout = 300; // 5 minutes timeout for large databases
            backupCmd.CommandText = $"BACKUP DATABASE [{safeDbName}] TO DISK = N'{safeBackupPath}' WITH FORMAT, INIT, NAME = N'{safeName}', STATS = 10";

            backupCmd.ExecuteNonQuery();

            return (true, $"Database backed up successfully to: {fullBackupPath}", fullBackupPath);
        }
        catch (Exception ex)
        {
            return (false, $"Backup failed: {ex.Message}", null);
        }
    }

    public static async Task<(bool Success, string Message, string? BackupPath)> BackupDatabaseAsync(string? connectionString, string? targetFolder = null)
    {
        return await Task.Run(() => BackupDatabase(connectionString, targetFolder));
    }
}

