using System.Text.Json;
using AshproSAASERP.Models.Settings;
using Microsoft.JSInterop;

namespace AshproSAASERP.Shared.Services;

public class POSSettingsService
{
    private readonly JSService _jsService;
    private readonly IJSRuntime _jsRuntime;
    private POSSettings _currentSettings = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    private const string StorageKey = "Ashpro_POS_Settings";

    public event Action? OnSettingsChanged;

    public POSSettings CurrentSettings => _currentSettings;

    public POSSettingsService(JSService jsService, IJSRuntime jsRuntime)
    {
        _jsService = jsService;
        _jsRuntime = jsRuntime;
        TryLoadFromDisk();
    }

    private string GetLocalFilePath()
    {
        try
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AshproSAAS");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "pos_settings.json");
        }
        catch
        {
            return string.Empty;
        }
    }

    private void TryLoadFromDisk()
    {
        try
        {
            var path = GetLocalFilePath();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                var json = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var loaded = JsonSerializer.Deserialize<POSSettings>(json, JsonOptions);
                    if (loaded is not null)
                    {
                        _currentSettings = loaded;
                    }
                }
            }
        }
        catch
        {
            // Ignore disk I/O errors (e.g. sandbox permissions)
        }
    }

    public async Task<POSSettings> LoadSettingsAsync()
    {
        // 1. First attempt to load from browser/webview localStorage
        try
        {
            var json = await _jsService.GetLocal(StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var loaded = JsonSerializer.Deserialize<POSSettings>(json, JsonOptions);
                if (loaded is not null)
                {
                    _currentSettings = loaded;
                    return _currentSettings;
                }
            }
        }
        catch
        {
            // If JS interop is unavailable or fails, fall through to disk
        }

        // 2. If localStorage was empty or failed, reload from local disk file
        TryLoadFromDisk();
        return _currentSettings;
    }

    public async Task SaveSettingsAsync(POSSettings settings)
    {
        _currentSettings = settings.Clone();
        var json = JsonSerializer.Serialize(_currentSettings, JsonOptions);

        // Save to browser/webview localStorage
        try
        {
            await _jsService.SetLocal(json, StorageKey);
        }
        catch
        {
            // Fall through to disk save
        }

        // Save to local machine application data directory
        try
        {
            var path = GetLocalFilePath();
            if (!string.IsNullOrEmpty(path))
            {
                await File.WriteAllTextAsync(path, json);
            }
        }
        catch
        {
            // Ignore file system errors
        }

        // Notify any active views/listeners
        OnSettingsChanged?.Invoke();
    }

    public async Task<POSSettings> ResetToDefaultsAsync()
    {
        var defaultSettings = new POSSettings();
        await SaveSettingsAsync(defaultSettings);
        return defaultSettings;
    }
}
