using ZakiSAASERP.Desk.Services;
using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Interface.Services;
using ZakiSAASERP.Repo;
using ZakiSAASERP.Shared.Services;
using FileDownloader;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using MudBlazor;
using MudBlazor.Services;
namespace ZakiSAASERP.Desk;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(windows => windows.OnWindowCreated(window =>
            {
                try
                {
                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                    var iconPath = Path.Combine(AppContext.BaseDirectory, "appicon.ico");
                    if (!File.Exists(iconPath))
                    {
                        iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "AppIcon", "appicon.ico");
                    }
                    if (File.Exists(iconPath))
                    {
                        appWindow.SetIcon(iconPath);
                    }
                }
                catch { }
            }));
        });
#endif

        builder.Services.AddScoped<AppData>();
        builder.Services.AddTransient<StartupPage>();
        builder.Services.AddSingleton<ICustomerDisplayService, CustomerDisplayService>();
        builder.Services.AddTransient<CustomerDisplayPage>();
        builder.Services.AddTransient<CustomerDisplayViewModel>();
        AppData.IsMaui = true;
        AppData.IsMauiApi = false;
        AppData.IsWindows = OperatingSystem.IsWindows();



        AppData.DBProvider = RegistryHelper.GetData("DBProvider", "");
        AppData.ConnectionString = RegistryHelper.GetData("ConString", "");
        if (string.IsNullOrEmpty(AppData.DBProvider) && !string.IsNullOrEmpty(AppData.ConnectionString))
        {
            AppData.DBProvider = AppUtility.DetectDatabaseProvider(AppData.ConnectionString);
            if (!string.IsNullOrEmpty(AppData.DBProvider))
            {
                RegistryHelper.SetData("DBProvider", AppData.DBProvider);
            }
        }
        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddSingleton<IConProviderMaui, ConProviderDesk>();
        builder.Services.AddDbContextFactory<TenantDBContext>((sp, options) =>
        {
            var csProvider = sp.GetRequiredService<IConProviderMaui>();
            var connStr = csProvider.GetConnectionString();
            if (string.IsNullOrEmpty(connStr))
            {
                connStr = AppData.ConnectionString;
            }
            if (string.IsNullOrEmpty(connStr))
            {
                connStr = RegistryHelper.GetData("ConString");
            }

            var provider = AppData.DBProvider;
            if (string.IsNullOrEmpty(provider))
            {
                provider = RegistryHelper.GetData("DBProvider", "");
            }
            if (string.IsNullOrEmpty(provider) && !string.IsNullOrEmpty(connStr))
            {
                provider = AppUtility.DetectDatabaseProvider(connStr);
            }
            if (string.IsNullOrEmpty(provider))
            {
                provider = "MSSQL";
            }

            AppUtility.ConfigureDbContextOptions(options, provider, connStr);
        });
        builder.Services.AddSingleton<IDbContextProvider, DesktopDbContextProvider>();
        builder.Services.AddScoped<TenantDBContext>(sp => sp.GetRequiredService<IDbContextProvider>().CreateDbContext());
        builder.Services.AddScoped<IDatabaseMigrationService, DatabaseMigrationServiceDesk>();

        builder.Services.AddScoped<AppBarState>();
        builder.Services.SetDependency();
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
        });
        builder.Services.AddFileDownloder();
        builder.Services.AddScoped<JSService>();
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<IndexedDbFactory>();
        builder.Services.AddTransient<AuthHeaderHandler>();
        builder.Services.AddHttpClient("DynamicClient")
                .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddSingleton<IApiAccessor, MauiApiAccessor>();
        builder.Services.AddSingleton<RefitServiceFactory>();

        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<IViewService, ViewServiceMSSql>();
        builder.Services.AddScoped<ITenantRepository, TenantRepository>();
        builder.Services.AddScoped<ModelService>();
        builder.Services.AddScoped<IndexDBHelper>();
        builder.Services.AddScoped<TabService>();
        builder.Services.AddScoped<POSSettingsService>();
        builder.Services.AddScoped<ShiftService>();
        builder.Services.AddScoped<IInvoiceOcrService, InvoiceOcrService>();
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn9SdkZiW35bdHFRTmle;Mjc5NTc5OUAzMjMzMmUzMDJlMzBScFAxZ2RFR29seXNsM3J5UnlhZlhCRDFSRnZzNVJVSjRaUkJ6QWhkaVY0PQ==");
        builder.Services.AddSingleton<ZakiSAASERP.Shared.Services.DashboardState>();


        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();

        return builder.Build();
    }
}
