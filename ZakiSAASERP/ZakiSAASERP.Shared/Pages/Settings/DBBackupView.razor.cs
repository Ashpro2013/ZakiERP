using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.RegularExpressions;

namespace ZakiSAASERP.Shared.Pages.Settings;

public partial class DBBackupView : ComponentBase, ITabAware
{
    [Parameter] public int TabId { get; set; }

    private string targetFolder = string.Empty;
    private string dbProvider = "MSSQL";
    private string dbName = "ABD";
    private string dbServer = "LocalHost";

    private bool isBackingUp = false;
    private bool isLoadingList = false;
    private (bool Success, string Message, string? BackupPath)? lastBackupResult;
    private List<BackupFileInfo> existingBackups = new();
    private string searchString = string.Empty;
    private WebUserPrivilege? privilege;

    private IEnumerable<BackupFileInfo> FilteredBackups
    {
        get
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return existingBackups;

            return existingBackups.Where(b =>
                (b.FileName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (b.FullPath?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }

    protected override async Task OnInitializedAsync()
    {
        InitializeDatabaseInfo();

        if (AppData.privileges is not null)
        {
            privilege = AppData.privileges.FirstOrDefault(x =>
                string.Equals(x.Caption, "DBBackup", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Caption, "Database Backup", StringComparison.OrdinalIgnoreCase));
        }

        if (AppData.IsWindows)
        {
            if (!string.IsNullOrWhiteSpace(PermanentSettingsData.BackupFolder))
            {
                targetFolder = PermanentSettingsData.BackupFolder;
            }
            else
            {
                try
                {
                    targetFolder = FormFactor.GetMydocumentsPath();
                }
                catch { }
            }

            await LoadExistingBackupsAsync();
        }
    }

    private void InitializeDatabaseInfo()
    {
        try
        {
            dbProvider = !string.IsNullOrWhiteSpace(AppData.DBProvider)
                ? AppData.DBProvider
                : "MSSQL";

            var connStr = !string.IsNullOrWhiteSpace(AppData.ConnectionString)
                ? AppData.ConnectionString
                : FormFactor.GetConFromRegistryForWin();

            if (!string.IsNullOrWhiteSpace(connStr))
            {
                // Parse DB Name
                var matchDb = Regex.Match(connStr, @"(?:Initial Catalog|Database)\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
                if (matchDb.Success)
                {
                    dbName = matchDb.Groups[1].Value.Trim();
                }
                else if (string.Equals(dbProvider, "SQLite", StringComparison.OrdinalIgnoreCase))
                {
                    var matchSqlite = Regex.Match(connStr, @"Data Source\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
                    if (matchSqlite.Success)
                    {
                        dbName = System.IO.Path.GetFileName(matchSqlite.Groups[1].Value.Trim());
                    }
                }

                // Parse Server / DataSource
                var matchServer = Regex.Match(connStr, @"(?:Data Source|Server)\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
                if (matchServer.Success)
                {
                    dbServer = matchServer.Groups[1].Value.Trim();
                }
            }
            else
            {
                dbServer = Environment.MachineName;
            }
        }
        catch
        {
            dbServer = Environment.MachineName;
        }
    }

    private async Task PerformBackupAsync()
    {
        if (isBackingUp) return;

        if (privilege is not null && !privilege.SaveData)
        {
            Snackbar.Add(AppData.IsArabic ? "ليس لديك صلاحية النسخ الاحتياطي" : "No backup privilege", Severity.Warning);
            return;
        }

        try
        {
            isBackingUp = true;
            lastBackupResult = null;
            StateHasChanged();

            await Task.Yield(); // Allow UI to update progress overlay

            var result = await FormFactor.BackupDatabaseAsync(targetFolder);
            lastBackupResult = result;

            if (result.Success)
            {
                Snackbar.Add(
                    AppData.IsArabic
                        ? "تم إنشاء النسخة الاحتياطية لقاعدة البيانات بنجاح!"
                        : "Database backup completed successfully!",
                    Severity.Success);

                await LoadExistingBackupsAsync();
            }
            else
            {
                Snackbar.Add(result.Message, Severity.Error);
            }
        }
        catch (Exception ex)
        {
            lastBackupResult = (false, $"Backup failed: {ex.Message}", null);
            Snackbar.Add($"Backup error: {ex.Message}", Severity.Error);
        }
        finally
        {
            isBackingUp = false;
            StateHasChanged();
        }
    }

    private Task LoadExistingBackupsAsync()
    {
        try
        {
            isLoadingList = true;
            existingBackups = FormFactor.GetExistingBackups(targetFolder);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading backups: {ex.Message}", Severity.Warning);
        }
        finally
        {
            isLoadingList = false;
            StateHasChanged();
        }
        return Task.CompletedTask;
    }

    private void OpenFolderInExplorer()
    {
        try
        {
            FormFactor.OpenFolder(targetFolder);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error opening folder: {ex.Message}", Severity.Error);
        }
    }

    private async Task ResetToDefaultFolder()
    {
        try
        {
            targetFolder = FormFactor.GetMydocumentsPath();
            await LoadExistingBackupsAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error resetting folder: {ex.Message}", Severity.Error);
        }
    }
}

/// <summary>
/// Alias class for DBBackup navigation matching
/// </summary>
public class DBBackup : DBBackupView
{
}

