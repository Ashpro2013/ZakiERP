namespace ZakiSAASERP.Repo.Others;

public class PermanentSettingsRepo(IDbContextProvider contextProvider) : IPermanentSettingsRepo
{
    public async Task<List<PermanentSettings>> GetAllSettings()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.PermanentSettings.ToListAsync();
    }

    public async Task<string> GetSetting(string key)
    {
        await using var db = contextProvider.CreateDbContext();
        var setting = await db.PermanentSettings.FirstOrDefaultAsync(x => x.SettingKey == key);
        return setting?.SettingValue ?? "";
    }

    public async Task<OutputResponse> UpdateSettings(List<PermanentSettings> settings)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            foreach (var setting in settings)
            {
                var existing = await db.PermanentSettings.FirstOrDefaultAsync(x => x.SettingKey == setting.SettingKey);
                if (existing != null)
                {
                    existing.SettingValue = setting.SettingValue;
                    db.PermanentSettings.Update(existing);
                }
                else
                {
                    await db.PermanentSettings.AddAsync(setting);
                }
            }
            await db.SaveChangesAsync();
            return new OutputResponse(1, "Settings Updated Successfully");
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message);
        }
    }
}



