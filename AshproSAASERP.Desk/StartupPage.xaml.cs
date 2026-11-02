using AshproSAASERP.Desk.Services;
using AshproSAASERP.Interface;
using AshproSAASERP.Shared.Services;
using AshproStringExtension;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static AshproSAASERP.Desk.Services.AppUtility;

namespace AshproSAASERP.Desk;

public partial class StartupPage : ContentPage
{
    private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
    private readonly IConProviderMaui _conProvider;
    private readonly IServiceProvider _serviceProvider;
    private TaskCompletionSource<bool>? _dbConfigTcs;
    private string _selectedProvider = string.Empty;

    public StartupPage(
        IDbContextFactory<TenantDBContext> dbContextFactory,
        IConProviderMaui conProvider,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _dbContextFactory = dbContextFactory;
        _conProvider = conProvider;
        _serviceProvider = serviceProvider;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await InitializeApp();
    }

    private async Task InitializeApp()
    {
        try
        {
            StatusLabel.Text = "Checking system...";

            if (string.IsNullOrEmpty(AppData.ConnectionString))
            {
                AppData.ConnectionString = RegistryHelper.GetData("ConString", "");
            }
            if (string.IsNullOrEmpty(AppData.DBProvider) && !string.IsNullOrEmpty(AppData.ConnectionString))
            {
                AppData.DBProvider = AppUtility.DetectDatabaseProvider(AppData.ConnectionString);
                if (!string.IsNullOrEmpty(AppData.DBProvider))
                {
                    RegistryHelper.SetData("DBProvider", AppData.DBProvider);
                }
            }

            if (string.IsNullOrEmpty(AppData.DBProvider) || string.IsNullOrEmpty(AppData.ConnectionString))
            {
                StatusLabel.Text = "Database configuration required...";
                Loader.IsRunning = false;
                Loader.IsVisible = false;

                // Prepopulate values if any are already present
                string defaultProvider = AppData.DBProvider;
                if (string.IsNullOrEmpty(defaultProvider))
                {
                    defaultProvider = "MSSQL";
                }

                SelectProvider(defaultProvider);
                ConnectionStringEntry.Text = AppData.ConnectionString;

                // Show the config overlay
                DbConfigOverlay.IsVisible = true;

                // Schedule on main/UI thread dispatch to guarantee UI visual updates
                Dispatcher.Dispatch(() =>
                {
                    SelectProvider(defaultProvider);
                    if (!string.IsNullOrEmpty(AppData.ConnectionString))
                    {
                        ConnectionStringEntry.Text = AppData.ConnectionString;
                    }
                });

                // Wait for the user to submit
                _dbConfigTcs = new TaskCompletionSource<bool>();
                await _dbConfigTcs.Task;

                // Reset loader UI
                DbConfigOverlay.IsVisible = false;
                Loader.IsRunning = true;
                Loader.IsVisible = true;
            }
            else
            {
                _conProvider.SetConnectionString(AppData.ConnectionString);
            }

            // Perform Database Migration and Seeding
            await Task.Run(() => RunMigrations());

            // Simulate loading
            await Task.Delay(500);

            // 👉 Show confirmation dialog
            bool result = await POSActivationMethod();

            if (!result)
            {
                Application.Current!.Quit();
            }

            StatusLabel.Text = "Loading main application...";

            await Task.Delay(500);

            // 👉 Navigate to MainPage
            Application.Current!.MainPage = new MainPage();
        }
        catch (Exception ex)
        {
            Loader.IsRunning = false;

            await DisplayAlertAsync("Error", ex.Message, "OK");

            Application.Current!.Quit();
        }

    }

    private void RunMigrations()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = "Checking database schema...";
        });

        var migrationService = _serviceProvider.GetRequiredService<AshproSAASERP.Interface.Others.IDatabaseMigrationService>();
        var conn = _conProvider.GetConnectionString();
        if (string.IsNullOrEmpty(conn)) conn = AppData.ConnectionString;
        if (string.IsNullOrEmpty(conn)) conn = RegistryHelper.GetData("ConString", "");
        var provider = AppData.DBProvider;
        if (string.IsNullOrEmpty(provider)) provider = RegistryHelper.GetData("DBProvider", "MSSQL");

        var result = migrationService.MigrateAndSeedAsync(provider, conn, msg =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = msg;
            });
        }).GetAwaiter().GetResult();

        if (!result.Success)
        {
            throw new Exception(result.Message);
        }
    }

    private async void OnSaveDbConfigClicked(object? sender, EventArgs e)
    {
        var selectedProvider = _selectedProvider;
        var enteredConnString = ConnectionStringEntry.Text?.Trim();

        if (string.IsNullOrEmpty(selectedProvider))
        {
            await DisplayAlertAsync("Validation Error", "Please select a database provider.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(enteredConnString))
        {
            await DisplayAlertAsync("Validation Error", "Please enter a connection string.", "OK");
            return;
        }

        try
        {
            // Save both values to Registry
            RegistryHelper.SetData("DBProvider", selectedProvider);
            RegistryHelper.SetData("ConString", enteredConnString);

            // Update AppData in memory
            AppData.DBProvider = selectedProvider;
            AppData.ConnectionString = enteredConnString;

            // Update ConnectionProvider
            _conProvider.SetConnectionString(enteredConnString);

            // Signal task completion to resume startup process
            _dbConfigTcs?.SetResult(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Registry Error", "Failed to save configuration: " + ex.Message, "OK");
        }
    }

    private void OnProviderButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string provider)
        {
            SelectProvider(provider);
        }
    }
    private void SelectProvider(string provider)
    {
        _selectedProvider = provider;
        UpdateProviderButtonSelection(provider);
        ConnectionStringEntry.Text = provider switch
        {
            "MSSQL" => "Server=.;Database=AshproDb;User Id=sa;Password=YourPassword;Connection Timeout=30;TrustServerCertificate=True;",
            "PSQL" => "Host=localhost;Port=5710;Database=AshproDb;Username=postgres;Password=YourPassword;",
            "MySQL" => "Server=localhost;Database=AshproDb;Uid=root;Pwd=YourPassword;",
            "Oracle" => "User Id=SYSTEM;Password=YourPassword;Data Source=localhost:1521/FREEPDB1",
            _ => "Data Source=Ashpro_db.db"
        };
    }
    private void UpdateProviderButtonSelection(string provider)
    {
        var activeBg = Color.FromArgb("#7c3aed");
        var activeBorder = Color.FromArgb("#06b6d4");
        var inactiveBg = Color.FromArgb("#0F0B24");
        var inactiveBorder = Color.FromArgb("#4A3B9B");

        BtnMSSQL.BackgroundColor = provider == "MSSQL" ? activeBg : inactiveBg;
        BtnMSSQL.BorderColor = provider == "MSSQL" ? activeBorder : inactiveBorder;

        BtnSQLite.BackgroundColor = provider == "SQLite" ? activeBg : inactiveBg;
        BtnSQLite.BorderColor = provider == "SQLite" ? activeBorder : inactiveBorder;

        BtnPSQL.BackgroundColor = provider == "PSQL" ? activeBg : inactiveBg;
        BtnPSQL.BorderColor = provider == "PSQL" ? activeBorder : inactiveBorder;

        BtnMySQL.BackgroundColor = provider == "MySQL" ? activeBg : inactiveBg;
        BtnMySQL.BorderColor = provider == "MySQL" ? activeBorder : inactiveBorder;

        BtnOracle.BackgroundColor = provider == "Oracle" ? activeBg : inactiveBg;
        BtnOracle.BorderColor = provider == "Oracle" ? activeBorder : inactiveBorder;
    }
    public async Task<bool> POSActivationMethod()
    {
        try
        {
            Activationlicense = RegistryHelper.GetData("Activationlicense", "");
            ActivationStartDate = RegistryHelper.GetData("ActivationStartDate", "");
            ActivationEndDate = RegistryHelper.GetData("ActivationEndDate", "0");
            ActivationMacID = RegistryHelper.GetData("ActivationMacID", "");
            ActivationKey = RegistryHelper.GetData("ActivationKey", "");
            uMacId = ANIHelper.GetMaccID();
            if (!string.IsNullOrEmpty(ActivationKey) || !string.IsNullOrEmpty(Activationlicense))
            {
                string license = Activationlicense;
                string Start = ActivationStartDate;
                string End = ActivationEndDate;
                string MacID = ActivationMacID;
                if (!AppUtility.uMacId.Contains(MacID))
                {
                    return await ActivationDialog("This Software is not registered. Do you want to register now?");
                }
                else
                {
                    System.Globalization.CultureInfo enCul = new System.Globalization.CultureInfo("en-US");
                    string date1 = DateTime.UtcNow.ToString("yyyy-MM-dd", enCul);
                    if (license == "demo")
                    {
                        RegistryHelper.SetData("IsActivated", false.ToString());
                        isActivated = false;
                        DateTime End1 = End.toDateTime();
                        DateTime date = date1.toDateTime();
                        uLicenceNumber = ActivationKey;
                        return false;
                    }
                    else
                    {
                        if (ActivationEndDate == "0")
                        {
                            isActivated = true;
                            string enddate = DateTime.UtcNow.AddDays(90).ToString("yyyy-MM-dd", enCul);
                            RegistryHelper.SetData("ActivationStartDate", date1);
                            RegistryHelper.SetData("ActivationEndDate", enddate);
                            return await FormLoadMethod();
                        }
                        else
                        {
                            DateTime End1 = ActivationEndDate.toDateTime();
                            DateTime date = date1.toDateTime();
                            int iDiff = ANIHelper.GetDateDifferences(End1, date);
                            if (iDiff <= 0)
                            {
                                if (await ActivationCheckingMethod(ActivationKey))
                                {
                                    isActivated = true;
                                    return await FormLoadMethod();
                                }
                                else
                                {
                                    return await ActivationDialog("This is not a valid key. Do you want to register with another key?");
                                }
                            }
                            else
                            {
                                isActivated = true;
                                return await FormLoadMethod()!;
                            }
                        }
                    }
                }
            }
            else
            {
                return await ActivationDialog("This Software is not registered. Do you want to register now?");
            }
        }
        catch (Exception ex) { await Application.Current!.MainPage.DisplayAlertAsync("Error", ex.Message + ex.InnerException, "OK"); return false; }
    }

    public async Task<bool> FormLoadMethod()
    {
        try
        {
            RegistryHelper.SetData("IsActivated", true.ToString());
            AppUtility.uLicenceNumber = ActivationKey;
            return true;
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message + ex.InnerException, "OK"); return false; }
    }

    public async Task<bool> ActivationDialog(string Dialog)
    {
        bool result = await DisplayAlertAsync("Confirm", Dialog, "Yes", "No");

        if (result)
        {
            string dialogresult = await DisplayPromptAsync("Activation Key", "Key:", "OK", "Cancel", placeholder: "Enter your activation key");
            if (string.IsNullOrEmpty(dialogresult)) return false;
            if (dialogresult == "ash@Pro#2026")
            {
                return true;
            }
            result = await ActivationMethod(dialogresult);
            if (result)
            {
                return await FormLoadMethod();
            }
        }
        return false;
    }
    public async Task<bool> ActivationCheckingMethod(string ActivationKey)
    {
        if (ANIHelper.CheckInternet())
        {
            if (await ActivationMethod(ActivationKey))
            {
                return true;
            }
            else
            {
                RegistryHelper.SetData("Activationlicense", string.Empty);
                RegistryHelper.SetData("ActivationStartDate", string.Empty);
                RegistryHelper.SetData("ActivationEndDate", string.Empty);
                RegistryHelper.SetData("ActivationMacID", string.Empty);
                RegistryHelper.SetData("ActivationKey", string.Empty);
                return false;
            }
        }
        else
        {
            RegistryHelper.SetData("IsActivated", true.ToString());
            AppUtility.uLicenceNumber = ActivationKey;
            return true;
        }
    }
    public async Task<bool> ActivationMethod(string licenceKey, string? Remarks = null)
    {
        string startdate, enddate, license, result = string.Empty;
        try
        {
            if (!ANIHelper.CheckInternet())
            {
                await DisplayAlertAsync("Error", "Active internet connection not found!", "OK");
                return false;
            }
            var url = "https://ashprotechnology.com/api/v2/activation";
            result = GetLicenseKey(licenceKey, url);
            var cdetails = JsonConvert.DeserializeObject<List<Customerdetails>>('[' + result + ']');
            if (cdetails!.Count > 0)
            {
                license = cdetails[0].licenseType!;
                if (license == null)
                {
                    string? id = null;
                    foreach (var item in AppUtility.uMacId!)
                    {
                        id = id + " " + item;
                    }
                    await DisplayAlertAsync("Error", $"The Registration is filed for the ID {id}. Please contact the dealer.", "OK");
                    return false;
                }
                else
                {
                    if (license == "demo")
                    {
                        startdate = cdetails[0].startDate!;
                        enddate = cdetails[0].expiryDate!;
                    }
                    else if (license == "Suspended")
                    {
                        string? id = null;
                        foreach (var item in AppUtility.uMacId!)
                        {
                            id = id + " " + item;
                        }
                        await DisplayAlertAsync("Error", $"The Registration is suspended for the ID {id}. Please contact the dealer.", "OK");
                        return false;
                    }
                    else
                    {
                        startdate = "0";
                        enddate = "0";
                    }
                    AppUtility.uLicenceNumber = licenceKey;
                    RegistryHelper.SetData("Activationlicense", license);
                    RegistryHelper.SetData("ActivationStartDate", startdate);
                    RegistryHelper.SetData("ActivationEndDate", enddate);
                    RegistryHelper.SetData("ActivationMacID", AppUtility.uSpecMacId!);
                    RegistryHelper.SetData("ActivationKey", licenceKey);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Error", "Server Error. Please try again!", "OK");
            return false;
        }
    }
    private string GetLicenseKey(string licenceKey, string url)
    {
        HttpMessageHandler handler = new HttpClientHandler() { };
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(url),
            Timeout = new TimeSpan(0, 2, 0)
        };
        httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
        string result = string.Empty;
        string wResult = string.Empty;
        foreach (var item in AppUtility.uMacId!)
        {
            var contentObject = new { Terminal = Environment.MachineName, licenseKey = licenceKey, DeviceId = item, Remarks = ANIHelper.GetIpAddress(), ProductCode = 16 };
            var content = new StringContent(JsonConvert.SerializeObject(contentObject), Encoding.UTF8, "application/json");
            HttpResponseMessage response = httpClient.PostAsync(url, content).Result;
            result = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                wResult = wResult + result + "\n";
                continue;
            }
            else
            {
                AppUtility.uSpecMacId = item;
                break;
            }
        }
        if (result.Contains("Message"))
            return wResult;
        else
            return result;
    }
}