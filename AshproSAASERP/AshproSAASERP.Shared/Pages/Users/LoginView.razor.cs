using Microsoft.AspNetCore.Components.Web;

namespace AshproSAASERP.Shared.Pages.Users;

public sealed partial class LoginView
{
    #region Variables
    List<BranchMaster> Branches = new();
    List<FinancialYear> FinancialYears = new();
    LoginModel LoginModel = new();
    MudTextField<string> txtUserName;
    MudTextField<string> txtPassword;
    string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
    bool isShow;
    InputType PasswordInput = InputType.Password;
    string sErrorMessage = string.Empty;
    bool IsRememberMe = false;
    private DotNetObjectReference<LoginView>? _dotNetRef;
    private bool _isSwitchDialogOpen;
    #endregion

    #region Events
    protected override async Task OnInitializedAsync()
    {
        try
        {
            AppData.privileges = new();
            Console.WriteLine("LOGIN VIEW CREATED: " + Guid.NewGuid());
            AppData.IsLogin = false;
            await JS.RemoveLocal("jwt");

            await ShowModelMethod();
        }
        catch (Exception ex)
        {
            Console.WriteLine("LoginView OnInitializedAsync error: " + ex.Message);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && AppData.IsWindows)
        {
            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await jsRuntime.InvokeVoidAsync("registerLoginShortcuts", _dotNetRef);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Shortcut registration error: " + ex.Message);
            }
        }
    }

    [JSInvokable]
    public async Task OnAltF3Pressed()
    {
        if (AppData.IsWindows)
        {
            await OpenDatabaseSwitchDialog();
        }
    }

    public async Task OpenDatabaseSwitchDialog()
    {
        if (!AppData.IsWindows) return;
        if (_isSwitchDialogOpen) return;

        try
        {
            _isSwitchDialogOpen = true;
            var options = new DialogOptions
            {
                CloseOnEscapeKey = false,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                BackdropClick = false
            };

            var dialog = await DialogService.ShowAsync<DatabaseSwitchDialog>("Database Switching", options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled && result.Data is bool success && success)
            {
                await ApplyDatabaseSwitchEffectAsync();
            }
        }
        finally
        {
            _isSwitchDialogOpen = false;
        }
    }

    private async Task ApplyDatabaseSwitchEffectAsync()
    {
        try
        {
            sErrorMessage = string.Empty;
            Branches.Clear();
            FinancialYears.Clear();
            LoginModel.BranchName = string.Empty;
            LoginModel.FYear = string.Empty;

            var username = !string.IsNullOrEmpty(LoginModel.UserName) ? LoginModel.UserName : "admin";
            var tenant = "maui";
            if (username.Contains('@'))
            {
                var val = username.Split('@');
                username = val[0];
                tenant = val[1];
            }

            var data = await Api.GetPreLoginDetails(tenant, username);
            Branches = data.branches;
            FinancialYears = data.fYears;
            AppData.SubEndDate = data.SubEndDate;
            AppData.IsExpaired = data.IsExpired;
            AppData.IsTrial = data.IsTrial;

            if (Branches.Any())
            {
                LoginModel.BranchName = Branches.FirstOrDefault()!.Name;
            }
            else
            {
                sErrorMessage = "No branches found in the selected database.";
            }

            if (FinancialYears.Any())
            {
                LoginModel.FYear = FinancialYears.LastOrDefault()!.Name;
            }

            Snackbar.Add($"Switched to {AppData.DBProvider} database successfully!", Severity.Success);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            sErrorMessage = $"Database switched, but error loading pre-login details: {ex.Message}";
            StateHasChanged();
        }
    }

    private async Task HandleCardKeyDown(KeyboardEventArgs e)
    {
        if (e.AltKey && (e.Key == "F3" || e.Code == "F3"))
        {
            if (AppData.IsWindows)
            {
                await OpenDatabaseSwitchDialog();
            }
        }
    }

    void ButtonTestclick()
    {
        if (isShow)
        {
            isShow = false;
            PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
            PasswordInput = InputType.Password;
        }
        else
        {
            isShow = true;
            PasswordInputIcon = Icons.Material.Filled.Visibility;
            PasswordInput = InputType.Text;
        }
    }
    private async Task ShowModelMethod(bool isEditRequeired = false)
    {
        if (AppData.IsWindows)
        {
            if (isEditRequeired)
            {
                await OpenDatabaseSwitchDialog();
            }
            return;
        }

        if (AppData.IsMaui)
        {
            string? result = string.Empty;

            if (AppData.IsMauiApi)
            {
                var apiURL = await JS.GetLocal("BaseURL");
                if (string.IsNullOrEmpty(apiURL) || isEditRequeired)
                {
                    result = await model.InputDialogMethod("Please enter the API URL", "Api URL", true, false, apiURL);
                    if (!string.IsNullOrEmpty(result))
                    {
                        apiURL = result.ToString();
                        var api = ApiFactory.Create(apiURL);
                        ApiAccessor.SetApi(api);
                        await JS.SetLocal(result.ToString(), "BaseURL");
                    }
                }
                else
                {
                    var api = ApiFactory.Create(apiURL);
                    ApiAccessor.SetApi(api);
                }
            }
            else
            {
                var ConString = FormFactor.GetConFromRegistryForWin() ?? await JS.GetLocal("ConString");
                if (string.IsNullOrEmpty(ConString) || isEditRequeired)
                {
                    result = await model.InputDialogMethod("Please enter the Connection String", "Connection String", true, false, ConString);
                    if (!string.IsNullOrEmpty(result))
                    {
                        ConString = result.ToString();
                        conProvider.SetConnectionString(ConString);
                        await JS.SetLocal(ConString, "ConString");
                        FormFactor.SetConInRegistryForWin(ConString);
                    }
                }
                else
                {
                    conProvider.SetConnectionString(ConString);
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (AppData.IsWindows)
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("unregisterLoginShortcuts");
            }
            catch { }

            _dotNetRef?.Dispose();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (LoginModel == null || LoginModel.UserName == null || LoginModel.Password == null)
        {
            var username = await JS.GetLocal("username");
            var password = await JS.GetLocal("password");
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                IsRememberMe = true;
                LoginModel = new LoginModel { UserName = username, Password = password };
                await UserNameLostFocusEvent();
                StateHasChanged();
            }
        }
    }
    private async Task UserNameLostFocusEvent()
    {
        try
        {
            sErrorMessage = "";
            if (string.IsNullOrEmpty(LoginModel.UserName)) { return; }
            string tenant = "";
            tenant = "maui";
            if (AppData.IsMauiApi && !LoginModel.UserName.Contains('@'))
            {
                if (string.IsNullOrEmpty(LoginModel.UserName) || (!LoginModel.UserName.Contains('@') && !AppData.IsMaui))
                    return;
                var val = LoginModel.UserName.Split('@');

                var username = LoginModel.UserName;
                if (!AppData.IsMaui)
                {
                    tenant = val[1];
                    username = val[0];
                }
                else { tenant = "maui"; }
                if (tenant != "79891")
                {
                    var data = await Api.GetPreLoginDetails(tenant, username);
                    Branches = data.branches;
                    FinancialYears = data.fYears;
                }
            }
            else
            {
                var username = LoginModel.UserName;
                if (LoginModel.UserName.Contains('@'))
                {
                    var val = LoginModel.UserName.Split('@');
                    username = val[0];
                    tenant = val[1];
                }
                var data = await Api.GetPreLoginDetails(tenant, username);
                Branches = data.branches;
                FinancialYears = data.fYears;
                AppData.SubEndDate = data.SubEndDate;
                AppData.IsExpaired = data.IsExpired;
                AppData.IsTrial = data.IsTrial;
            }

            if (Branches.Any())
            {
                LoginModel.BranchName = Branches.FirstOrDefault()!.Name;
            }
            else
            {
                sErrorMessage = "No branches assigned to this user. Please contact administrator.";
            }

            if (FinancialYears.Any())
                LoginModel.FYear = FinancialYears.LastOrDefault()!.Name;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            sErrorMessage = $"Failed to load pre-login details: {ex.Message}";
            StateHasChanged();
        }
    }
    private async Task Login()
    {
        try
        {
            var userdata = LoginModel.UserName.Split('@');
            if (userdata.Length == 2)
            {
                if (AppData.IsExpaired)
                {
                    sErrorMessage = "Your subscription has expired. Please renew the same.";
                    return;
                }
                if (string.IsNullOrEmpty(LoginModel.BranchName))
                {
                    sErrorMessage = "Invalid Branch Name";
                    return;
                }
                if (string.IsNullOrEmpty(LoginModel.FYear))
                {
                    sErrorMessage = "Invalid Financial Year";
                    return;
                }
            }
            var response = await Api.Login(new LoginRequest
            {
                EmailOrUserName = LoginModel.UserName,
                Password = LoginModel.Password.Trim(),
                IsMaui = userdata.Length == 2 ? false : AppData.IsMaui
            });
            if (response is not null)
            {
                if (response.IsLogin)
                {
                    if (IsRememberMe)
                    {
                        await JS.SetLocal(LoginModel.UserName, "username");
                        await JS.SetLocal(LoginModel.Password, "password");
                    }
                    else
                    {
                        await JS.RemoveLocal("username");
                        await JS.RemoveLocal("password");
                    }
                    AppData.IsLogin = true;
                    if (!response.IsAdmin)
                    {
                        AppData.BranchId = Branches.FirstOrDefault(x => x.Name == LoginModel.BranchName).BranchId;
                        AppData.FYearId = FinancialYears.FirstOrDefault(x => x.Name == LoginModel.FYear)?.FinancialYearId;
                        AppData.UserId = response.UserId;
                        AppData.UserName = LoginModel.UserName;
                        if (!string.IsNullOrEmpty(response.tocken))
                        {
                            await tService.SaveToken(response.tocken);
                            AppData.tocken = response.tocken;
                        }
                        AppData.CompanyDetails = await Api.GetCompanyDetailsByBranch(AppData.BranchId);
                        // Load permanent settings cache AFTER token is saved (needs auth)
                        await LoadPermanentSettingsAsync();
                        try
                        {
                            var privilages = await Api.GetWebUserprivilege(new PrivilegeDTO(AppData.UserId ?? 0));
                            if (privilages is not null && privilages.Any())
                            {
                                AppData.privileges = privilages;
                            }
                        }
                        catch (Exception ex) { string s = ex.Message; }
                        await JS.SetLocal(AppData.IsArabic.ToString(), "RightToLeft");
                        Navigation.NavigateTo("/home");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(response.tocken))
                        {
                            await tService.SaveToken(response.tocken);
                            AppData.tocken = response.tocken;
                        }

                        // Load permanent settings cache for admin too
                        await LoadPermanentSettingsAsync();

                        Navigation.NavigateTo("/admin");
                    }
                }
                else
                {
                    sErrorMessage = "Invalid Credential";
                    return;
                }
            }
        }
        catch (Exception)
        {
            sErrorMessage = "Invalid Credential";
        }
    }

    /// <summary>
    /// Loads permanent settings into static class after successful login
    /// </summary>
    private async Task LoadPermanentSettingsAsync()
    {
        try
        {
            var settings = await Api.GetAllPermanentSettings();
            if (settings is not null && settings.Any())
            {
                PermanentSettingsData.Load(settings);
                Console.WriteLine($"Loaded {settings.Count} permanent settings");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load permanent settings: {ex.Message}");
            // Continue even if settings fail to load - user can still use app
        }
    }
    private async Task Keydown(KeyboardEventArgs e, int fieldId)
    {
        if (e.AltKey && (e.Key == "F3" || e.Code == "F3"))
        {
            if (AppData.IsWindows)
            {
                await OpenDatabaseSwitchDialog();
                return;
            }
        }

        if (e.Key == "Enter")
        {
            if (fieldId == 1)
            {
                await txtPassword.FocusAsync();
                return;
            }
            else
            {
                if (string.IsNullOrEmpty(LoginModel.UserName) || string.IsNullOrEmpty(LoginModel.Password))
                {
                    return;
                }
                await Login();
            }
        }
        else if (e.Key == "ArrowUp")
        {
            if (fieldId == 1)
            {
                await txtPassword.FocusAsync();
            }
            else
            {
                await txtUserName.FocusAsync();
            }
        }
        else if (e.Key == "ArrowDown")
        {
            if (fieldId == 1)
            {
                await txtPassword.FocusAsync();
            }
            else
            {
                await txtUserName.FocusAsync();
            }
        }
    }
    #endregion
}
