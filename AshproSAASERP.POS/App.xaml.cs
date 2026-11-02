using AshproSAASERP.POS.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AshproSAASERP.POS;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Read connection string & db provider from registry (matching .Desk)
        AppData.IsWindows = true;
        AppData.IsMaui = false;
        AppData.ConnectionString = RegistryHelper.GetData("ConString", "");
        AppData.DBProvider = RegistryHelper.GetData("DBProvider", "");

        if (string.IsNullOrEmpty(AppData.DBProvider) && !string.IsNullOrEmpty(AppData.ConnectionString))
        {
            AppData.DBProvider = AppUtility.DetectDatabaseProvider(AppData.ConnectionString);
            if (!string.IsNullOrEmpty(AppData.DBProvider))
            {
                RegistryHelper.SetData("DBProvider", AppData.DBProvider);
            }
        }

        if (string.IsNullOrEmpty(AppData.DBProvider))
        {
            AppData.DBProvider = "MSSQL";
        }

        // Configure Dependency Injection
        var services = new ServiceCollection();
        services.ConfigureServices();
        ServiceProvider = services.BuildServiceProvider();

        // Launch Splash Screen (matches .Desk startup flow)
        var splashWindow = ServiceProvider.GetRequiredService<AshproSAASERP.POS.Views.SplashWindow>();
        splashWindow.Show();
    }
}

