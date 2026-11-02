using AshproSAASERP.Interface;
using AshproSAASERP.Interface.Masters;
using AshproSAASERP.Interface.Others;
using AshproSAASERP.Interface.Services;
using AshproSAASERP.Interface.Settings;
using AshproSAASERP.Interface.Transaction.Accounts;
using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.POS.ViewModels;
using AshproSAASERP.POS.Views;
using AshproSAASERP.Repo.Accounts;
using AshproSAASERP.Repo.CodeGenerationServices;
using AshproSAASERP.Repo.Masters;
using AshproSAASERP.Repo.Others;
using AshproSAASERP.Repo.Services;
using AshproSAASERP.Repo.Transaction.Inventory;
using AshproSAASERP.Repo.Transaction.Sales;
using AshproSAASERP.ZatcaEngine.Abstractions;
using AshproSAASERP.ZatcaEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace AshproSAASERP.POS.Services;

public static class DIHelper
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        // Database & Registry Setup
        services.AddSingleton<IDbContextProvider, PosDbContextProvider>();
        services.AddScoped(sp => sp.GetRequiredService<IDbContextProvider>().CreateDbContext());

        // HTTP Client
        services.AddHttpClient();
        services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient());

        // Masters & Core Repositories
        services.AddScoped<IAccountHeadRepo, AccountHeadRepo>();
        services.AddScoped<IAccountMasterRepo, AccountMasterRepo>();
        services.AddScoped<IBranchRepo, BranchRepo>();
        services.AddScoped<IFinancialYearRepo, FinancialYearRepo>();
        services.AddScoped<IGeneralRepo, GeneralRepo>();
        services.AddScoped<IPermanentSettingsRepo, PermanentSettingsRepo>();
        services.AddScoped<IUnitMasterRepo, UnitMasterRepo>();
        services.AddScoped<ISalesManMasterRepo, SalesManMasterRepo>();
        services.AddScoped<ICategoryMasterRepo, CategoryMasterRepo>();
        services.AddScoped<ISubCategoryMasterRepo, SubCategoryMasterRepo>();
        services.AddScoped<ICustomerMasterRepo, CustomerMasterRepo>();
        services.AddScoped<IItemMasterRepo, ItemMasterRepo>();
        services.AddScoped<ICompanyDetailsRepo, CompanyDetailsRepo>();
        services.AddScoped<ICommonImageRepo, CommonImageRepo>();
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<ILoginRepo, LoginRepo>();
        services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();

        // Database Seed Services
        services.AddTransient<AshproSAASERP.SQlite.DatabaseSeedService>();
        services.AddTransient<AshproSAASERP.PSQL.DatabaseSeedService>();
        services.AddTransient<AshproSAASERP.MySQL.DatabaseSeedService>();
        services.AddTransient<AshproSAASERP.Oracle.DatabaseSeedService>();
        services.AddTransient<AshproSAASERP.MSSQL.DatabaseSeedService>();

        // Sales & Invoice Repositories
        services.AddScoped<ISalesInvoiceRepo, SalesInvoiceRepo>();
        services.AddScoped<ISalesInvoiceHoldRepo, SalesInvoiceHoldRepo>();
        services.AddScoped<ISalesInvoiceRtnRepo, SalesInvoiceRtnRepo>();
        services.AddScoped<IPrintDesignRepo, PrintDesignRepo>();
        services.AddScoped<IServerRepo, ServerRepo>();

        // ZATCA & Sequence Services
        services.AddScoped<IZatcaRepo, ZatcaRepo>();
        services.AddScoped<IMasterValueRepo, MasterValueRepo>();
        services.AddScoped<IICVRepo, ICVRepo>();
        services.AddScoped<IZatcaInvoiceRepo, ZatcaInvoiceRepo>();
        services.AddScoped<IZatcaUploadService, ZatcaNewUploadService>();
        services.AddScoped<CodeSequenceService>();
        services.AddScoped<StockService>();
        services.AddScoped<IZatcaEngine, AshproSAASERP.ZatcaEngine.Core.ZatcaEngine>();

        // POS Core Services
        services.AddScoped<IPosDataService, PosDataService>();
        services.AddTransient<PrintHelper>();
        services.AddSingleton<POSSettingsService>();
        services.AddSingleton<ShiftService>();
        services.AddSingleton<ICustomerDisplayService, CustomerDisplayService>();

        // ViewModels
        services.AddTransient<SplashViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<POSViewModel>();
        services.AddTransient<PaymentViewModel>();
        services.AddTransient<CustomerSearchViewModel>();
        services.AddTransient<HeldInvoicesViewModel>();
        services.AddTransient<ItemSearchViewModel>();
        services.AddTransient<DiscountViewModel>();
        services.AddTransient<POSSettingsViewModel>();
        services.AddTransient<CustomerDisplayViewModel>();
        services.AddTransient<ZatcaPendingViewModel>();
        services.AddTransient<ShiftViewModel>();

        // Views & Windows
        services.AddTransient<SplashWindow>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<POSView>();
        services.AddTransient<PaymentDialog>();
        services.AddTransient<CustomerSearchDialog>();
        services.AddTransient<HeldInvoicesDialog>();
        services.AddTransient<ItemSearchDialog>();
        services.AddTransient<DiscountDialog>();
        services.AddTransient<POSSettingsDialog>();
        services.AddTransient<CustomerDisplayWindow>();
        services.AddTransient<ZatcaPendingDialog>();
        services.AddTransient<ShiftDialog>();
    }
}

