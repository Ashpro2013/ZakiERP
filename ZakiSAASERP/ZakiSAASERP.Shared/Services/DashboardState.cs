namespace ZakiSAASERP.Shared.Services;

public class DashboardState
{
    public event Action? OnDashboardRefresh;

    public void Refresh()
    {
        OnDashboardRefresh?.Invoke();
    }
}
