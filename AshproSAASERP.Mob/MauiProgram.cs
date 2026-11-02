using AshproSAASERP.Interface;
using AshproSAASERP.Mob.Services;
using AshproSAASERP.Repo;
using AshproSAASERP.Shared.Services;
using FileDownloader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;

namespace AshproSAASERP.Mob;

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

        builder.Services.AddScoped<AppData>();
        builder.Services.AddTransient<StartupPage>();
        builder.Services.AddSingleton<ICustomerDisplayService, CustomerDisplayService>();
        builder.Services.AddTransient<CustomerDisplayPage>();
        builder.Services.AddTransient<CustomerDisplayViewModel>();

        AppData.IsMaui = true;
        AppData.IsMauiApi = false;
        AppData.IsWindows = OperatingSystem.IsWindows();
        AppData.DBProvider = "SQLite";
        AppData.ConnectionString = "Data Source=Ashpro_db.db";

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddSingleton<IConProviderMaui, ConProviderMob>();
        string conn = string.Empty;
        builder.Services.AddDbContextFactory<TenantDBContext>((sp, options) =>
        {
            var csProvider = sp.GetRequiredService<IConProviderMaui>();
            conn = csProvider.GetConnectionString()
                    ?? RegistryHelper.GetData("ConString");

            options.UseSqlite(conn, x => x.MigrationsAssembly("AshproSAASERP.SQlite"));
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
        builder.Services.AddSingleton<IDbContextProvider, MobileDbContextProvider>();
        builder.Services.AddScoped<TenantDBContext>(sp => sp.GetRequiredService<IDbContextProvider>().CreateDbContext());

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
        builder.Services.AddHttpClient();
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
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn9SdkZiW35bdHFRTmle;Mjc5NTc5OUAzMjMzMmUzMDJlMzBScFAxZ2RFR29seXNsM3J5UnlhZlhCRDFSRnZzNVJVSjRaUkJ6QWhkaVY0PQ==");
        builder.Services.AddSingleton<AshproSAASERP.Shared.Services.DashboardState>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
