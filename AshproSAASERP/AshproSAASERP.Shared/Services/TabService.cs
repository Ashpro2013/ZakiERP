namespace AshproSAASERP.Shared.Services;

public class TabService
{
    public event Action<int>? OnCloseTab;
    public event Func<TabRequest, Task>? OnOpenTab;

    // Request to open tab
    public async Task OpenTabAsync(TabRequest request)
    {
        if (OnOpenTab is not null)
            await OnOpenTab.Invoke(request);
    }

    // Request to close tab
    public void CloseTab(int id)
    {
        OnCloseTab?.Invoke(id);
    }
}
public class TabRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string View { get; set; } = "";

    // 👇 Parameters for DynamicComponent
    public Dictionary<string, object>? Parameters { get; set; }
}

