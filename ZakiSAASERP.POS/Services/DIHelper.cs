using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Masters;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Interface.Services;
using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Interface.Transaction.Accounts;
using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.POS.ViewModels;
using ZakiSAASERP.POS.Views;
using ZakiSAASERP.Repo.Accounts;
using ZakiSAASERP.Repo.CodeGenerationServices;
using ZakiSAASERP.Repo.Masters;
using ZakiSAASERP.Repo.Others;
using ZakiSAASERP.Repo.Services;
using ZakiSAASERP.Repo.Transaction.Inventory;
using ZakiSAASERP.Repo.Transaction.Sales;
using ZakiSAASERP.ZatcaEngine.Abstractions;
using ZakiSAASERP.ZatcaEngine.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace ZakiSAASERP.POS.Services;

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
        services.AddTransient<ZakiSAASERP.SQlite.DatabaseSeedService>();
        services.AddTransient<ZakiSAASERP.PSQL.DatabaseSeedService>();
        services.AddTransient<ZakiSAASERP.MySQL.DatabaseSeedService>();
        services.AddTransient<ZakiSAASERP.Oracle.DatabaseSeedService>();
        services.AddTransient<ZakiSAASERP.MSSQL.DatabaseSeedService>();

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
        services.AddScoped<IZatcaEngine, ZakiSAASERP.ZatcaEngine.Core.ZatcaEngine>();

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
        services.AddTransient<BillSearchViewModel>();
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
        services.AddTransient<BillSearchDialog>();
        services.AddTransient<ItemSearchDialog>();
        services.AddTransient<DiscountDialog>();
        services.AddTransient<POSSettingsDialog>();
        services.AddTransient<CustomerDisplayWindow>();
        services.AddTransient<ZatcaPendingDialog>();
        services.AddTransient<ShiftDialog>();
    }
}

