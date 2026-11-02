using ZakiSAASERP.Interface.Masters;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Models.User;
using ZakiSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ZakiSAASERP.POS.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ILoginRepo _loginRepo;
    private readonly IUserRepo _userRepo;
    private readonly ICompanyDetailsRepo _companyDetailsRepo;
    private readonly IPermanentSettingsRepo _permanentSettingsRepo;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isRememberMe = true;

    [ObservableProperty]
    private bool showPassword;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BranchMaster> branches = [];

    [ObservableProperty]
    private BranchMaster? selectedBranch;

    [ObservableProperty]
    private ObservableCollection<FinancialYear> financialYears = [];

    [ObservableProperty]
    private FinancialYear? selectedFinancialYear;

    public Action? OnLoginSuccess { get; set; }
    public Action? OnOpenDatabaseConfig { get; set; }

    public LoginViewModel(
        ILoginRepo loginRepo,
        IUserRepo userRepo,
        ICompanyDetailsRepo companyDetailsRepo,
        IPermanentSettingsRepo permanentSettingsRepo)
    {
        _loginRepo = loginRepo;
        _userRepo = userRepo;
        _companyDetailsRepo = companyDetailsRepo;
        _permanentSettingsRepo = permanentSettingsRepo;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var savedUser = RegistryHelper.GetData("RememberUser", "");
            var savedPass = RegistryHelper.GetData("RememberPass", "");

            if (!string.IsNullOrEmpty(savedUser))
            {
                UserName = savedUser;
                Password = savedPass;
                IsRememberMe = true;
            }

            await LoadPreLoginDetailsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load pre-login info: " + ex.Message;
        }
    }

    [RelayCommand]
    public async Task LoadPreLoginDetailsAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            var con = AppData.ConnectionString;
            if (string.IsNullOrEmpty(con))
            {
                con = RegistryHelper.GetData("ConString", "");
            }

            var cleanUser = UserName.Contains('@') ? UserName.Split('@')[0] : UserName;
            if (string.IsNullOrWhiteSpace(cleanUser))
            {
                cleanUser = "Admin";
            }

            var branchList = await _loginRepo.GetBranchesForUserAsync(cleanUser, con);
            if (!branchList.Any())
            {
                branchList = await _loginRepo.GetAllLoginBranchesAsync(con);
            }

            Branches.Clear();
            foreach (var b in branchList)
            {
                Branches.Add(b);
            }

            SelectedBranch = Branches.FirstOrDefault();

            var fyearList = await _loginRepo.GetOpenFinancialYearsAsync(con);
            FinancialYears.Clear();
            foreach (var fy in fyearList)
            {
                FinancialYears.Add(fy);
            }

            SelectedFinancialYear = FinancialYears.LastOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Pre-login error: " + ex.Message;
        }
    }

    [RelayCommand]
    public void ToggleShowPassword()
    {
        ShowPassword = !ShowPassword;
    }

    [RelayCommand]
    public void OpenDatabaseConfig()
    {
        OnOpenDatabaseConfig?.Invoke();
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            ErrorMessage = "Please enter your username.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your password.";
            return;
        }

        if (SelectedBranch == null)
        {
            ErrorMessage = "Please select a branch.";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var con = AppData.ConnectionString;
            if (string.IsNullOrEmpty(con)) con = RegistryHelper.GetData("ConString", "");

            var cleanUser = UserName.Contains('@') ? UserName.Split('@')[0] : UserName;
            var user = await _loginRepo.GetUserByCredentialsAsync(cleanUser, Password.Trim(), con);

            if (user == null)
            {
                user = await _userRepo.GetForLogin(cleanUser, Password.Trim());
            }

            if (user == null)
            {
                ErrorMessage = "Invalid username or password.";
                IsLoading = false;
                return;
            }

            if (IsRememberMe)
            {
                RegistryHelper.SetData("RememberUser", UserName);
                RegistryHelper.SetData("RememberPass", Password);
            }
            else
            {
                RegistryHelper.SetData("RememberUser", "");
                RegistryHelper.SetData("RememberPass", "");
            }

            AppData.IsLogin = true;
            AppData.UserId = user.UserId;
            AppData.UserName = user.UserName;
            AppData.BranchId = SelectedBranch.BranchId;
            AppData.FYearId = SelectedFinancialYear?.FinancialYearId ?? 1;

            try
            {
                var company = await _companyDetailsRepo.GetByBranchIdAsync(AppData.BranchId);
                if (company != null)
                {
                    AppData.CompanyDetails = company;
                }
            }
            catch { }

            try
            {
                var settings = await _permanentSettingsRepo.GetAllSettings();
                if (settings != null && settings.Any())
                {
                    PermanentSettingsData.Load(settings);
                }
            }
            catch { }

            OnLoginSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Login error: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

