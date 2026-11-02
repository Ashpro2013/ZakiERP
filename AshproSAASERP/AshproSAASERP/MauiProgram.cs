using AshproSAASERP.Repo;
using AshproSAASERP.Services;
using AshproSAASERP.Shared.Services;
using FileDownloader;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;


namespace AshproSAASERP;

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

        // Add device-specific services used by the AshproSAASERP.Shared project
        builder.Services.AddScoped<IConProviderMaui, ConProviderMaui>();
        //builder.Services.AddDbContextFactory<TenantDBContext>((sp, options) =>
        //{
        //    var csProvider = sp.GetRequiredService<IConProviderMaui>();
        //    var conn = csProvider.GetConnectionString();
        //    options.UseSqlite(conn);
        //});
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
        });
        builder.Services.AddScoped<ICustomerDisplayService, CustomerDisplayService>();
        builder.Services.AddFileDownloder();
        builder.Services.AddScoped<JSService>();
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<IndexedDbFactory>();
        builder.Services.AddTransient<AuthHeaderHandler>();
        builder.Services.AddHttpClient("DynamicClient")
                .AddHttpMessageHandler<AuthHeaderHandler>();
        builder.Services.AddScoped<AppBarState>();
        builder.Services.AddSingleton<IApiAccessor, MauiApiAccessor>();
        builder.Services.AddSingleton<RefitServiceFactory>();
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<IViewService, ViewService>();
        builder.Services.AddScoped<ITenantRepository, TenantRepository>();
        builder.Services.AddScoped<ITenantService, TenantService>();
        builder.Services.AddSingleton<AshproSAASERP.Shared.Services.DashboardState>();
        builder.Services.AddScoped<AppData>();
        builder.Services.AddScoped<ModelService>();
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn9SdkZiW35bdHFRTmle;Mjc5NTc5OUAzMjMzMmUzMDJlMzBScFAxZ2RFR29seXNsM3J5UnlhZlhCRDFSRnZzNVJVSjRaUkJ6QWhkaVY0PQ==");
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddScoped<IndexDBHelper>();
        builder.Services.AddScoped<TabService>();
        builder.Services.AddScoped<POSSettingsService>();
        builder.Services.AddScoped<ShiftService>();
        AppData.IsMaui = true;
        AppData.IsMauiApi = true;
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
