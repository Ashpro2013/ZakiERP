using System.IO;
using System.Text.Json;
using AshproSAASERP.Models.Settings;

namespace AshproSAASERP.POS.Services;

public class POSSettingsService
{
    private POSSettings _currentSettings = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public event Action? OnSettingsChanged;

    public POSSettings CurrentSettings => _currentSettings;

    public POSSettingsService()
    {
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
            // Fall back to default settings on error
            _currentSettings = new POSSettings();
        }
    }

    public Task<POSSettings> LoadSettingsAsync()
    {
        TryLoadFromDisk();
        return Task.FromResult(_currentSettings);
    }

    public async Task SaveSettingsAsync(POSSettings settings)
    {
        _currentSettings = settings.Clone();
        var json = JsonSerializer.Serialize(_currentSettings, JsonOptions);

        var path = GetLocalFilePath();
        if (!string.IsNullOrEmpty(path))
        {
            await File.WriteAllTextAsync(path, json);
        }

        OnSettingsChanged?.Invoke();
    }

    public async Task ResetToDefaultsAsync()
    {
        await SaveSettingsAsync(new POSSettings());
    }
}

