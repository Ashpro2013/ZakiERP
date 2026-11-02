namespace ZakiSAASERP.Shared.Layout;

public partial class AdminLayout
{
    public MudDynamicTabs DynamicTabs;
    public List<TabView> UserTabs = new();
    public int UserIndex;
    private bool _drawerOpen = false;
    [Inject] ModelService Dialog { get; set; }
    [Inject] public IJSRuntime? iJsRuntime { get; set; }
    [Inject] IBrowserViewportService ResizeService { get; set; }
    [Inject] JSService JSService { get; set; }

    string time;
    Timer aTimer;
    private MudTheme _theme = new();
    private bool _isDarkMode;
    private string _currentTheme = "white";
    private void DrawerToggle() => _drawerOpen = !_drawerOpen;
    protected async override Task OnInitializedAsync()
    {
        if (AppData.FullScreen == "openFullscreen")
        {
            var succ = await iJsRuntime.InvokeAsync<bool>("openFullscreen");
        }
        aTimer = new Timer(Tick, null, 0, 1000);
        if (AppData.sTheme == null || AppData.sTheme == "dark")
        {
            _isDarkMode = true;
            _currentTheme = "dark";
        }
        else
        {
            _isDarkMode = false;
            _currentTheme = "white";
        }
        var width = await ResizeService.GetCurrentBrowserWindowSizeAsync();
        AppData.viewWidth = width.Width;
        StateHasChanged();
    }
    private void Tick(object _)
    {
        time = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss");
        InvokeAsync(StateHasChanged);
    }
    async Task Logout()
    {
        var dialog = await Dialog.DialogMethod("Are you want Logout?", true);
        var result = await dialog.Result;
        if (!result!.Canceled)
        {
            AppData.IsLogin = false;
            AppData.UserName = null;
            AppData.UserId = null;
            navManager.NavigateTo("login");
        }
    }
    #region Theme
    private async Task CloseTab()
    {
        await iJsRuntime.InvokeVoidAsync("closeBrowserTab");
    }
    private async Task FullScreen()
    {
        try
        {
            AppData.FullScreen = AppData.FullScreen == "openFullscreen" ? "closeFullscreen" : "openFullscreen";
            await JSService.SetLocal(AppData.FullScreen, "FullScreen");
            await iJsRuntime.InvokeVoidAsync(AppData.FullScreen);
        }
        catch (Exception ex)
        {

            string s = ex.Message;
        }
    }
    private async Task RightToLeft()
    {
        if (AppData.IsArabic)
            AppData.IsArabic = false;
        else
            AppData.IsArabic = true;
        await JSService.SetLocal(AppData.IsArabic.ToString(), "RightToLeft");
    }
    private async Task DarkMode()
    {
        try
        {
            _isDarkMode = !_isDarkMode;
            _currentTheme = _isDarkMode ? "dark" : "white";
            await JSService.SetLocal(_currentTheme, "Currenttheme");
        }
        catch (Exception ex) { Dialog.DialogMethod(ex.Message); }
    }
    #endregion

    #region Dynamic Tab
    private void LoadComponent(int iValue, string Name, string View)
    {
        _drawerOpen = false;
        AddTab(iValue, Name, View);
    }
    public void AddTab(int id, string sName, string vName)
    {
        try
        {
            if (!UserTabs.Any(x => x.Id == id))
            {
                UserTabs.Add(new TabView { Id = id, Name = sName, View = vName });
            }
            UserIndex = (UserTabs.IndexOf(UserTabs.FirstOrDefault(x => x.Id == id))) + 1;
            DynamicTabs.ActivatePanel(UserIndex);
            StateHasChanged();
        }
        catch (Exception) { }
    }
    public void RemoveTab(int id)
    {
        var tabView = UserTabs.SingleOrDefault((t) => Equals(t.Id, id));
        if (tabView is not null)
        {
            UserTabs.Remove(tabView);
            StateHasChanged();
        }
    }
    void CloseTabCallback(MudTabPanel panel) => RemoveTab((int)panel.ID);
    public void Dispose() => aTimer?.Dispose();
    public class TabView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string View { get; set; }
    }
    #endregion

    public class WindowDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
