namespace AshproSAASERP.Shared.Services;

public class BackupFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public interface IFormFactor
{
    public string GetFormFactor();
    public string GetPlatform();
    public string GetConFromRegistryForWin();//this is only for windows desktop
    public void SetConInRegistryForWin(string value);
    public void DirectPrinting(PrintParameterDTO dto); //for mobile and desktop
    public string GetDeviceIdName();
    string GetProductCode();
    Task PrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> data);
    Task SyncfusionPrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> data);
    Task<string?> GenerateBarcodeImageBase64(BarcodeDesignDTO dto, BarcodeData data) => Task.FromResult<string?>(null);
    public List<string> GetInstalledPrinters() => new();
    public Task OpenCashDrawer() => Task.CompletedTask;
    string GetMydocumentsPath();

    Task<(bool Success, string Message, string? BackupPath)> BackupDatabaseAsync(string? targetFolder = null)
        => Task.FromResult((false, "Database backup is only supported in the Desktop application.", (string?)null));

    List<BackupFileInfo> GetExistingBackups(string? targetFolder = null) => new();

    void OpenFolder(string folderPath) { }
}
