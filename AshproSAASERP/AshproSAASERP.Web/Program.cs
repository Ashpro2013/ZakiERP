using AshproSAASERP.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.Licensing;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();
var configuration = builder.Configuration;
builder.Services.AddValidation();
builder.Services.AddHttpClient();
var dbProvider = configuration["DbProvider"] ?? "MSSQL";

builder.Services.AddDbContext<MasterDBContext>(options =>
{
    var connectionString = configuration[$"ConnectionStrings:{dbProvider}:DefaultConnection"];
    switch (dbProvider.ToUpperInvariant())
    {
        case "SQLITE":
            options.UseSqlite(connectionString, b => b.MigrationsAssembly("AshproSAASERP.SQlite"));
            break;
        case "PSQL":
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("AshproSAASERP.PSQL"));
            break;
        case "MYSQL":
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), b => b.MigrationsAssembly("AshproSAASERP.MySQL"));
            break;
        case "ORACLE":
            options.UseOracle(connectionString, b => b.MigrationsAssembly("AshproSAASERP.Oracle"));
            break;
        case "MSSQL":
        default:
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly("AshproSAASERP.MSSQL"));
            break;
    }
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDbContextProvider, ApiDbContextProvider>();
builder.Services.AddDbContext<TenantDBContext>(options => options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))); // for options only

builder.Services.AddScoped<ICompanyCreationRepo, CompanyCreationRepo>();
builder.Services.AddScoped<IAdminUserRepo, AdminUserRepo>();
//4
var jwtSection = configuration.GetSection("Jwt");
var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var dbsection = configuration.GetSection($"db:{dbProvider}");
ServerAppData.Server = dbsection["Server"];
ServerAppData.Port = dbsection["Port"];
ServerAppData.UserName = dbsection["UserName"];
ServerAppData.Password = dbsection["Password"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
//5
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null; // Use PascalCase (default C# naming)
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddAuthorization();
builder.SetDependency();

SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn9SdkZiW35bdHFRTmle;Mjc5NTc5OUAzMjMzMmUzMDJlMzBScFAxZ2RFR29seXNsM3J5UnlhZlhCRDFSRnZzNVJVSjRaUkJ6QWhkaVY0PQ==");
builder.Services.AddScoped<SyncfusionPrintHelper>();
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var tenantType = configuration["TenantType"];
    if (string.Equals(tenantType, "single", StringComparison.OrdinalIgnoreCase))
    {
        var dbContextProvider = scope.ServiceProvider.GetRequiredService<IDbContextProvider>();
        using (var dbContext = dbContextProvider.CreateDbContext())
        {
            await dbContext.Database.MigrateAsync();

            // Seed data if not already seeded or ensure missing data on existing database
            var seedService = scope.ServiceProvider.GetRequiredService<AshproSAASERP.Interface.Others.IDatabaseSeedService>();
            if (!await dbContext.BranchMasters.AnyAsync())
            {
                var defaultEx = new AshproSAASERP.Models.Company.ExLCompany
                {
                    CompanyName = "Default Company",
                    UserName = "admin",
                    PassWord = "123",
                    ContactNo = "1234567890",
                    Email = "admin@company.com",
                    SubscriptionStartedAt = DateTime.UtcNow,
                    SubscriptionEndingDate = DateTime.UtcNow.AddDays(7),
                    CompanyCode = "1001",
                    SubscriptionType = "Standard"
                };
                await seedService.Seed(dbContext, defaultEx);
            }
            else
            {
                await seedService.Seed(dbContext, null);
            }
        }
    }
    else
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MasterDBContext>();
        await dbContext.Database.MigrateAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Prevent browser caching of Blazor WebAssembly framework files and index.html in Development
string currentWebClientWasm = string.Empty;
try
{
    var fwDir = Path.Combine(app.Environment.ContentRootPath, "..", "AshproSAASERP.Web.Client", "bin", "Debug", "net10.0", "wwwroot", "_framework");
    if (Directory.Exists(fwDir))
    {
        var wasmFile = Directory.GetFiles(fwDir, "AshproSAASERP.Web.Client.*.wasm").FirstOrDefault();
        if (wasmFile != null)
        {
            currentWebClientWasm = "/_framework/" + Path.GetFileName(wasmFile);
        }
    }
}
catch { }

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    if (path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(path, "/", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(path, "/index.html", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";

        if (app.Environment.IsDevelopment() &&
            (string.Equals(path, "/", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(path, "/index.html", StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.Headers["Clear-Site-Data"] = "\"cache\", \"storage\"";
        }
    }

    if (!string.IsNullOrEmpty(currentWebClientWasm) &&
        path.StartsWith("/_framework/AshproSAASERP.Web.Client.", StringComparison.OrdinalIgnoreCase) &&
        path.EndsWith(".wasm", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(path, currentWebClientWasm, StringComparison.OrdinalIgnoreCase))
    {
        context.Request.Path = currentWebClientWasm;
    }

    await next();
});

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();

// ?? Important
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

// Unregister any stale service workers
app.MapGet("/service-worker.js", () => Results.Content(
    "self.addEventListener('install', e => self.skipWaiting()); self.addEventListener('activate', e => self.registration.unregister());",
    "text/javascript"
));
app.MapGet("/_content/AshproSAASERP.Shared/service-worker.js", () => Results.Content(
    "self.addEventListener('install', e => self.skipWaiting()); self.addEventListener('activate', e => self.registration.unregister());",
    "text/javascript"
));

app.MapEndpoints();
app.MapFallbackToFile("index.html");

app.Run();


