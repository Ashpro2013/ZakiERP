namespace AshproSAASERP.Shared.Services;

public interface IConProviderMaui
{
    string GetConnectionString();
    void SetConnectionString(string conn);
}
