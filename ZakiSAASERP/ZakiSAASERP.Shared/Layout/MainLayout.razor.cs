using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Shared.Layout;

public sealed partial class MainLayout
{
    public MudDynamicTabs? DynamicTabs;
    public List<TabView> UserTabs = new();
    private bool _drawerOpen = false;
    private Position Position = Position.Top;
    private bool _showPermanentSettings = false;
    string sActivationMessage = string.Empty;
    private DotNetObjectReference<MainLayout>? _objRef;
    string? time;
    Timer? aTimer;
    private MudTheme _theme = new();
    private bool _isDarkMode;
    private string _currentTheme = "white";
    private void DrawerToggle() => _drawerOpen = !_drawerOpen;
    private int _userIndex;

    public int UserIndex
    {
        get => _userIndex;
        set
        {
            if (_userIndex != value)
            {
                _userIndex = value;
                TabChanged(value);
            }
        }
    }

    private void TabChanged(int index)
    {
        if (index == 0) // Dashboard tab
        {
            DashboardState.Refresh();
        }
    }
    private bool _quickNavOpen = false;
    private void QuickNavToggle() => _quickNavOpen = !_quickNavOpen;

    private List<MenuDetails> _allMenus = new();
    private MenuDetails? _customerMenu;
    private MenuDetails? _supplierMenu;
    private MenuDetails? _expenseMenu;
    private MenuDetails? _productMenu;
    private MenuDetails? _salesMenu;
    private MenuDetails? _returnMenu;
    private MenuDetails? _orderMenu;
    private MenuDetails? _purchaseMenu;
    private MenuDetails? _receiptMenu;
    private MenuDetails? _paymentMenu;

    private async Task LoadQuickNavMenus()
    {
        try
        {
            var menus = await IndexDBHelper.LoadMenuDetails();
            if (menus != null && menus.Any())
            {
                _allMenus = menus;
                _customerMenu = menus.FirstOrDefault(m => m.Name == "CustomerMaster");
                _supplierMenu = menus.FirstOrDefault(m => m.Name == "SupplierMaster");
                _expenseMenu = menus.FirstOrDefault(m => m.Name == "ExpenseEntry");
                _productMenu = menus.FirstOrDefault(m => m.Name == "ItemMaster");
                _salesMenu = menus.FirstOrDefault(m => m.Name == "SalesInvoiceTitle");
                _returnMenu = menus.FirstOrDefault(m => m.Name == "SalesInvoiceRtnTitle");
                _orderMenu = menus.FirstOrDefault(m => m.Name == "SalesOrderTitle");
                _purchaseMenu = menus.FirstOrDefault(m => m.Name == "PurchaseTitle");
                _receiptMenu = menus.FirstOrDefault(m => m.Name == "CustomerReceipt");
                _paymentMenu = menus.FirstOrDefault(m => m.Name == "SupplierPayment");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading quick nav menus: {ex.Message}");
        }
    }

    private async Task OpenQuickMenu(MenuDetails? menu, int defaultId, string defaultName, string defaultCaption, string defaultAssembly)
    {
        _quickNavOpen = false;
        if (menu != null)
        {
            await AddTab(menu.Id, AppData.IsArabic ? menu.ArabicCaption : menu.MenuCaption, menu.AssemblyName ?? "");
        }
        else
        {
            await AddTab(defaultId, defaultCaption, defaultAssembly);
        }
    }

    protected async override Task OnInitializedAsync()
    {
        try
        {
            if (AppData.FullScreen == "openFullscreen")
            {
                var succ = await iJsRuntime!.InvokeAsync<bool>("openFullscreen");
            }
            Position = AppData.IsWindows ? Position.Bottom : Position.Top;
            TabService.OnCloseTab += RemoveTab;
            TabService.OnOpenTab += AddTabFromService;
            AppBarState.OnChange += OnAppBarStateChanged;
            if (PermanentSettingsData.IsZatca && PermanentSettingsData.ZatcaPhase == 2)
            {
                try
                {
                    var failedCount = await api.GetFailedCount();
                    AppBarState.SetCount(failedCount);
                }
                catch { }
            }

            aTimer = new Timer(Tick!, null, 0, 1000);
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
            if (ResizeService != null)
            {
                try
                {
                    var width = await ResizeService.GetCurrentBrowserWindowSizeAsync();
                    if (width != null)
                    {
                        AppData.viewWidth = width.Width;
                    }
                }
                catch (Exception) { }
            }
            if (!AppData.IsMaui)
            {
                int daysDifference = (AppData.SubEndDate.Value.Date - DateTime.Now.Date).Days;
                if (daysDifference <= 15)
                {
                    sActivationMessage = $"The subscription will expire in {daysDifference} days.";
                }
            }
            await LoadQuickNavMenus();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine("MainLayout OnInitializedAsync error: " + ex.Message);
        }
    }
    private async Task AddTabFromService(TabRequest request)
    {
        await AddTab(request.Id, request.Name, request.View, request.Parameters);
    }

        private void OnAppBarStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }
    public void Dispose()
    {
        AppBarState.OnChange -= OnAppBarStateChanged;
        TabService.OnCloseTab -= RemoveTab;
        TabService.OnOpenTab -= AddTabFromService;
        _objRef?.Dispose();
        aTimer?.Dispose();
    }
    private void Tick(object _)
    {
        time = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss");
        InvokeAsync(StateHasChanged);
    }
    private async Task OpenChangePasswordDialog()
    {
        if (DialogService == null) return;
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        await DialogService.ShowAsync<ZakiSAASERP.Shared.SubWindows.ChangePasswordDialog>(Localizer("ChangePassword"), options);
    }

    async Task Logout()
    {
        var dialog = await Dialog!.DialogMethod(Localizer("LogoutDialog"), true);
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
        var dialog = await Dialog!.DialogMethod(Localizer("CloseDialog"), true);
        var result = await dialog.Result;
        if (!result!.Canceled)
            await iJsRuntime!.InvokeVoidAsync("closeBrowserTab");
    }
    private async Task FullScreen()
    {
        try
        {
            AppData.FullScreen = AppData.FullScreen == "openFullscreen" ? "closeFullscreen" : "openFullscreen";
            await JSService!.SetLocal(AppData.FullScreen, "FullScreen");
            await iJsRuntime!.InvokeVoidAsync(AppData.FullScreen);
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }
    private async Task Refresh()
    {
        if (await Dialog!.DeleteDialogMethod(Localizer("CacheClearConfirmation")))
        {
            await IndexDBHelper.ClearIndexDBStore();
            Snackbar.Add(Localizer("CacheCleared"), Severity.Success);
        }
    }
    private async Task RightToLeft()
    {
        if (AppData.IsArabic)
            AppData.IsArabic = false;
        else
            AppData.IsArabic = true;
        await JSService!.SetLocal(AppData.IsArabic.ToString(), "RightToLeft");
    }
    private async Task DarkMode()
    {
        try
        {
            _isDarkMode = !_isDarkMode;
            _currentTheme = _isDarkMode ? "dark" : "white";
            await JSService!.SetLocal(_currentTheme, "Currenttheme");
        }
        catch (Exception ex) { await Dialog!.DialogMethod(ex.Message); }
    }
    #endregion

    #region Dynamic Tab
    private async Task ZatcaRemiderClick()
    {
        _drawerOpen = false;
        var menu = AppData.menuList!.FirstOrDefault(x => x.Name == "PendingZatca")!;
        await AddTab(menu.Id, AppData.IsArabic ? menu.ArabicCaption : menu.MenuCaption, menu.AssemblyName ?? "");

    }
    private async Task LoadComponent(int Id, string name, string view)
    {
        _drawerOpen = false;
        await AddTab(Id, name, view);
    }
    public async Task AddTab(int Id, string name, string view,
    Dictionary<string, object>? parameters = null)
    {
        if (!UserTabs.Any(x => x.Id == Id) && Id != 0)
        {
            UserTabs.Add(new TabView
            {
                Id = Id,
                Name = name,
                View = view,
                Parameters = parameters
            });
        }

        UserIndex = UserTabs.FindIndex(x => x.Id == Id) + 1;

        await DynamicTabs!.ActivatePanelAsync(UserIndex);
    }
    private Dictionary<string, object>? GetParameters(TabView tab)
    {
        if (string.IsNullOrWhiteSpace(tab.View))
            return null;

        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes());
        // Find component type
        var type = assemblies
            .FirstOrDefault(t => t.FullName == tab.View);

        if (type == null)
            return null;

        // Get all [Parameter] names
        var validParams = type.GetProperties()
            .Where(p => Attribute.IsDefined(p, typeof(ParameterAttribute)))
            .Select(p => p.Name)
            .ToHashSet();

        if (validParams.Count == 0)
            return null;

        var result = new Dictionary<string, object>();

        // Always try TabId if supported
        if (validParams.Contains("TabId"))
        {
            result["TabId"] = tab.Id;
        }

        // Add extra parameters if supported
        if (tab.Parameters is not null)
        {
            foreach (var kv in tab.Parameters)
            {
                if (validParams.Contains(kv.Key))
                {
                    result[kv.Key] = kv.Value;
                }
            }
        }

        return result.Count == 0 ? null : result;
    }


    public void RemoveTab(int id)
    {
        var tabView = UserTabs.SingleOrDefault(t => t.Id == id);
        if (tabView is not null)
        {
            UserTabs.Remove(tabView);
            StateHasChanged();
        }
    }
    void CloseTabCallback(MudTabPanel panel) => RemoveTab((int)panel.ID!);

    public class TabView
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? View { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }
    #endregion

    public class WindowDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }



    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _objRef = DotNetObjectReference.Create(this);
                await iJsRuntime!.InvokeVoidAsync("registerSettingsShortcut", _objRef);
            }
            catch (Exception) { /* JS might not be ready or other issue */ }
        }
        await base.OnAfterRenderAsync(firstRender);
    }


    public void OpenPermanentSettingsDirectly()
    {
        _showPermanentSettings = true;
        StateHasChanged();
    }

    [JSInvokable]
    public async Task OpenPermanentSettings()
    {
        // Prevent opening multiple times if already open or dialog is showing
        if (_showPermanentSettings) return;

        try
        {
            var pass = await Dialog!.InputDialogMethod("Enter Password", "Authentication Required", true, true);

            if (pass == null) return;

            var today = DateTime.Today;
            var expectedPassword = (today.Year + today.Month + today.Day).ToString();

            if (pass?.Trim() == expectedPassword)
            {
                // Show the permanent settings overlay
                _showPermanentSettings = true;
                StateHasChanged();
            }
            else
            {
                await Dialog.ShowMessage($"Invalid Password: {pass}");
            }
        }
        catch (Exception ex)
        {
            await DialogService!.ShowMessageBox("Error", ex.Message);
        }
    }
    private void ClosePermanentSettings()
    {
        _showPermanentSettings = false;
        StateHasChanged();
    }


    private async Task ShiftClosingClick()
    {
        try
        {
            if (DialogService == null) return;

            bool? confirmed = await DialogService.ShowMessageBox(
                Localizer("ShiftClosing"),
                "Do you want to close the Shift",
                yesText: Localizer("Yes"),
                cancelText: Localizer("No"));

            if (confirmed == true)
            {
                if (ShiftService == null)
                {
                    Snackbar.Add("Shift service is not available.", Severity.Warning);
                    return;
                }
                var terminal = "Terminal 1";
                var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "User";

                // Print shift details from local storage
                var shiftDetails = await ShiftService.GetShiftDetailsAsync(terminal, user);
                await ShiftService.PrintShiftDetailsAsync(shiftDetails, AppData.CompanyDetails);

                // Make all values in the local storage as zero
                await ShiftService.ResetShiftDetailsAsync(terminal, user);

                Snackbar.Add(Localizer("ShiftClosedSuccessfully"), Severity.Success);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Shift closing error: {ex.Message}", Severity.Error);
        }
    }
}
