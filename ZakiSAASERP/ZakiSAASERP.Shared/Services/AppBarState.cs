namespace ZakiSAASERP.Shared.Services;

public class AppBarState
{
    public int NotificationCount { get; private set; }

    public event Action? OnChange;

    public void SetCount(int count)
    {
        NotificationCount = count;
        OnChange?.Invoke();
    }

    public void Increment()
    {
        NotificationCount++;
        OnChange?.Invoke();
    }
}