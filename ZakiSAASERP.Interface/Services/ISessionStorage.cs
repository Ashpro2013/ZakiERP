namespace ZakiSAASERP.Interface.Services;

public interface ISessionStorage
{
    void SetAsync(string key, string value);
    string? GetAsync(string key);
    void DeleteAsync(string key);
}
