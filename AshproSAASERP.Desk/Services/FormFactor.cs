using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Shared.Services;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
namespace AshproSAASERP.Desk.Services;

public class FormFactor : IFormFactor
{
    public void DirectPrinting(PrintParameterDTO dto)
    {
        bool isThermal = dto.hdr?.PrintType?.Equals("Thermal", StringComparison.OrdinalIgnoreCase) == true
            || dto.hdr?.VoucherType?.Equals("Sales_Invoice_Thermal", StringComparison.OrdinalIgnoreCase) == true;

        if (isThermal)
        {
            PrintHelper printHelper = new PrintHelper();
            printHelper.PrintInvoice(dto);
        }
        else
        {
            A4PrintHelper a4PrintHelper = new A4PrintHelper();
            a4PrintHelper.PrintInvoice(dto);
        }
    }
    public string GetConFromRegistryForWin() => RegistryHelper.GetData("ConString");

    public string GetDeviceIdName()=> System.Environment.MachineName;

    public string GetFormFactor() => "Windows Desktop";
    public string GetPlatform() => RuntimeInformation.OSDescription;

    public string GetProductCode() => "16";

    public async Task PrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> data)
    {
        PrintHelper printHelper = new PrintHelper();
        await printHelper.PrintBarcode(dto, data);
    }

    public async Task SyncfusionPrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> data)
    {
        PrintHelper printHelper = new PrintHelper();
        await printHelper.SyncfusionPrintBarcode(dto, data);
    }

    public Task<string?> GenerateBarcodeImageBase64(BarcodeDesignDTO dto, BarcodeData data)
    {
        PrintHelper printHelper = new PrintHelper();
        var base64 = printHelper.GenerateBarcodeLabelImageBase64(dto, data);
        return Task.FromResult<string?>(base64);
    }


    public void SetConInRegistryForWin(string value) => RegistryHelper.SetData("ConString", value);

    public List<string> GetInstalledPrinters()
    {
        var printers = new List<string>();
        try
        {
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
        }
        catch
        {
        }
        return printers;
    }

    public async Task OpenCashDrawer()
    {
        PrintHelper printHelper = new PrintHelper();
        await printHelper.OpenCashDrawer();
    }

    public string GetMydocumentsPath()
    {
      var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = Path.Combine(path, "Zaki ERP","Backup");
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }

    public async Task<(bool Success, string Message, string? BackupPath)> BackupDatabaseAsync(string? targetFolder = null)
    {
        try
        {
            var provider = AshproSAASERP.Shared.Services.AppData.DBProvider;
            if (string.IsNullOrWhiteSpace(provider))
            {
                provider = RegistryHelper.GetData("DBProvider", "MSSQL");
            }

            var connStr = !string.IsNullOrWhiteSpace(AshproSAASERP.Shared.Services.AppData.ConnectionString)
                ? AshproSAASERP.Shared.Services.AppData.ConnectionString
                : RegistryHelper.GetData("ConString", "");

            if (string.Equals(provider, "SQLite", StringComparison.OrdinalIgnoreCase))
            {
                return await BackupSqliteDatabaseAsync(connStr, targetFolder);
            }
            else
            {
                return await BackupMssqlDatabaseAsync(connStr, targetFolder);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Backup failed: {ex.Message}", null);
        }
    }

    private async Task<(bool Success, string Message, string? BackupPath)> BackupMssqlDatabaseAsync(string? connStr, string? targetFolder)
    {
        if (string.IsNullOrWhiteSpace(connStr))
        {
            return (false, "SQL Server connection string is not configured.", null);
        }

        string destFolder = !string.IsNullOrWhiteSpace(targetFolder)
            ? targetFolder.Trim()
            : (!string.IsNullOrWhiteSpace(AshproSAASERP.Models.Services.PermanentSettingsData.BackupFolder)
                ? AshproSAASERP.Models.Services.PermanentSettingsData.BackupFolder.Trim()
                : GetMydocumentsPath());

        try
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
        }
        catch { }

        // Attempt 1: direct backup to destFolder
        var result = await AshproSAASERP.MSSQL.MssqlBackupHelper.BackupDatabaseAsync(connStr, destFolder);
        if (result.Success && !string.IsNullOrWhiteSpace(result.BackupPath) && File.Exists(result.BackupPath))
        {
            return result;
        }

        // Attempt 2: fallback to SQL Server default directory, then copy to destFolder
        var fallbackResult = await AshproSAASERP.MSSQL.MssqlBackupHelper.BackupDatabaseAsync(connStr, null);
        if (fallbackResult.Success && !string.IsNullOrWhiteSpace(fallbackResult.BackupPath) && File.Exists(fallbackResult.BackupPath))
        {
            try
            {
                if (Directory.Exists(destFolder))
                {
                    string targetFile = Path.Combine(destFolder, Path.GetFileName(fallbackResult.BackupPath));
                    File.Copy(fallbackResult.BackupPath, targetFile, true);
                    return (true, $"Database backed up successfully to: {targetFile}", targetFile);
                }
            }
            catch (Exception copyEx)
            {
                return (true, $"Database backed up to SQL Server default directory: {fallbackResult.BackupPath} (could not copy to {destFolder}: {copyEx.Message})", fallbackResult.BackupPath);
            }

            return fallbackResult;
        }

        return result.Success ? fallbackResult : result;
    }

    private Task<(bool Success, string Message, string? BackupPath)> BackupSqliteDatabaseAsync(string? connStr, string? targetFolder)
    {
        try
        {
            string dbPath = "";
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                var match = System.Text.RegularExpressions.Regex.Match(connStr, @"Data Source=([^;]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    dbPath = match.Groups[1].Value.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(dbPath) || !File.Exists(dbPath))
            {
                var common = Path.Combine(AppContext.BaseDirectory, "Ashpro_db.db");
                if (File.Exists(common)) dbPath = common;
            }

            if (string.IsNullOrWhiteSpace(dbPath) || !File.Exists(dbPath))
            {
                return Task.FromResult<(bool, string, string?)>((false, "SQLite database file could not be found.", null));
            }

            string destFolder = !string.IsNullOrWhiteSpace(targetFolder)
                ? targetFolder.Trim()
                : GetMydocumentsPath();

            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"Ashpro_db_{timestamp}.db";
            string targetFile = Path.Combine(destFolder, fileName);

            File.Copy(dbPath, targetFile, true);
            return Task.FromResult<(bool, string, string?)>((true, $"SQLite database backed up successfully to: {targetFile}", targetFile));
        }
        catch (Exception ex)
        {
            return Task.FromResult<(bool, string, string?)>((false, $"SQLite backup failed: {ex.Message}", null));
        }
    }

    public List<BackupFileInfo> GetExistingBackups(string? targetFolder = null)
    {
        var list = new List<BackupFileInfo>();
        var scannedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void ScanDirectory(string? dir)
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir) || scannedDirs.Contains(dir))
                return;

            scannedDirs.Add(dir);
            try
            {
                var files = Directory.GetFiles(dir, "*.*")
                    .Where(f => f.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".db", StringComparison.OrdinalIgnoreCase));

                foreach (var file in files)
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        list.Add(new BackupFileInfo
                        {
                            FileName = fi.Name,
                            FullPath = fi.FullName,
                            FileSizeBytes = fi.Length,
                            FileSizeFormatted = FormatFileSize(fi.Length),
                            CreatedDate = fi.LastWriteTime
                        });
                    }
                    catch { }
                }
            }
            catch { }
        }

        ScanDirectory(targetFolder);
        ScanDirectory(AshproSAASERP.Models.Services.PermanentSettingsData.BackupFolder);
        ScanDirectory(GetMydocumentsPath());

        try
        {
            ScanDirectory(@"C:\AshproBackups");
        }
        catch { }

        return list
            .GroupBy(b => b.FullPath, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderByDescending(b => b.CreatedDate)
            .ToList();
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{(bytes / 1024.0):F2} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{(bytes / (1024.0 * 1024.0)):F2} MB";
        return $"{(bytes / (1024.0 * 1024.0 * 1024.0)):F2} GB";
    }

    public void OpenFolder(string folderPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                folderPath = GetMydocumentsPath();
            }

            if (File.Exists(folderPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{folderPath}\"",
                    UseShellExecute = true
                });
            }
            else if (Directory.Exists(folderPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                Directory.CreateDirectory(folderPath);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }
        catch { }
    }
}
