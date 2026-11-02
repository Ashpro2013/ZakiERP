using AshproSAASERP.Interface.Services;

namespace AshproSAASERP.Desk.Services;

public class MauiSessionStorage : ISessionStorage
{
    public void SetAsync(string key, string value) => RegistryHelper.SetData(key, value);
    public string? GetAsync(string key) => RegistryHelper.GetData(key);
    public void DeleteAsync(string key) => throw new NotImplementedException();
}
