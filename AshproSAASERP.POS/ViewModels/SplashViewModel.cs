using AshproSAASERP.Interface.Others;
using AshproSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AshproSAASERP.POS.ViewModels;

public partial class SplashViewModel : ObservableObject
{
    private readonly IDatabaseMigrationService _migrationService;
    private TaskCompletionSource<bool>? _dbConfigTcs;

    [ObservableProperty]
    private string statusMessage = "Initializing system...";

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private bool showDbConfig;

    [ObservableProperty]
    private string selectedProvider = "MSSQL";

    [ObservableProperty]
    private string connectionString = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public Action? OnReadyToLogin { get; set; }

    public SplashViewModel(IDatabaseMigrationService migrationService)
    {
        _migrationService = migrationService;
    }

    public async Task StartStartupSequenceAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            StatusMessage = "Checking system...";

            var conn = RegistryHelper.GetData("ConString", "");
            var provider = RegistryHelper.GetData("DBProvider", "");

            if (string.IsNullOrWhiteSpace(provider) && !string.IsNullOrWhiteSpace(conn))
            {
                provider = AppUtility.DetectDatabaseProvider(conn);
                if (!string.IsNullOrWhiteSpace(provider))
                {
                    RegistryHelper.SetData("DBProvider", provider);
                }
            }

            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(conn))
            {
                StatusMessage = "Database configuration required...";
                IsLoading = false;
                SelectedProvider = string.IsNullOrWhiteSpace(provider) ? "MSSQL" : provider;
                ConnectionString = conn;
                ShowDbConfig = true;

                _dbConfigTcs = new TaskCompletionSource<bool>();
                await _dbConfigTcs.Task;

                ShowDbConfig = false;
                IsLoading = true;
                conn = ConnectionString;
                provider = SelectedProvider;
            }

            AppData.ConnectionString = conn;
            AppData.DBProvider = provider;

            StatusMessage = "Connecting to database server...";
            var result = await Task.Run(async () =>
            {
                return await _migrationService.MigrateAndSeedAsync(provider, conn, msg =>
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = msg;
                    });
                });
            });

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                StatusMessage = "Database setup failed. Please check configuration.";
                IsLoading = false;
                ShowDbConfig = true;
                return;
            }

            StatusMessage = "Database ready. Loading application...";
            await Task.Delay(600);

            OnReadyToLogin?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusMessage = "Initialization error";
            IsLoading = false;
            ShowDbConfig = true;
        }
    }

    [RelayCommand]
    public void SelectProvider(string provider)
    {
        SelectedProvider = provider;
    }

    [RelayCommand]
    public void SaveDbConfig()
    {
        if (string.IsNullOrWhiteSpace(SelectedProvider))
        {
            ErrorMessage = "Please select a database provider.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            ErrorMessage = "Please enter a connection string.";
            return;
        }

        try
        {
            RegistryHelper.SetData("DBProvider", SelectedProvider);
            RegistryHelper.SetData("ConString", ConnectionString.Trim());

            AppData.DBProvider = SelectedProvider;
            AppData.ConnectionString = ConnectionString.Trim();
            ErrorMessage = string.Empty;

            _dbConfigTcs?.TrySetResult(true);

            // Re-run startup sequence
            _ = StartStartupSequenceAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to save configuration: " + ex.Message;
        }
    }
}

