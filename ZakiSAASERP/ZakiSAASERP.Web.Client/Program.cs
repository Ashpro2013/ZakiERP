using ZakiSAASERP.Shared.Services;
using ZakiSAASERP.Web.Client.Components;
using ZakiSAASERP.Web.Client.Services;
using FileDownloader;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
//var apiUrl = builder.Configuration["ApiBaseUrl"];
AppData.ShowTaxRegimeChange = true;

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
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn9SdkZiW35bdHFRTmle;Mjc5NTc5OUAzMjMzMmUzMDJlMzBScFAxZ2RFR29seXNsM3J5UnlhZlhCRDFSRnZzNVJVSjRaUkJ6QWhkaVY0PQ==");

// Configure Refit with PascalCase JSON serialization to match server
var refitSettings = new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(
        new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = null // Use PascalCase
        })
};

builder.Services.AddRefitClient<IRefitService>(refitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    //.ConfigureHttpClient(c => c.BaseAddress = new Uri(apiUrl)) // Your API URL
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddScoped<IApiAccessor, WasmApiAccessor>();
builder.Services.AddScoped<IFormFactor, FormFactor>();
builder.Services.AddScoped<IViewService, ViewService>();
builder.Services.AddScoped<AppBarState>();
builder.Services.AddScoped<AppData>();
builder.Services.AddScoped<ModelService>();
builder.Services.AddScoped<IConProviderMaui, ConProviderMaui>();
builder.Services.AddSingleton<RefitServiceFactory>();
builder.Services.AddScoped<IndexDBHelper>();
builder.Services.AddScoped<TabService>();
builder.Services.AddScoped<POSSettingsService>();
builder.Services.AddScoped<ShiftService>();
builder.Services.AddSingleton<ZakiSAASERP.Shared.Services.DashboardState>();
AppData.IsMaui = false;
AppData.IsMauiApi = false;
await builder.Build().RunAsync();
