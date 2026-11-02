namespace ZakiSAASERP.Interface.Others;

public interface IPermanentSettingsRepo
{
    Task<List<PermanentSettings>> GetAllSettings();
    Task<OutputResponse> UpdateSettings(List<PermanentSettings> settings);
    Task<string> GetSetting(string key);
}
