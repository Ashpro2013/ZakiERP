using AshproSAASERP.Interface.Masters;
using AshproSAASERP.Interface.Others;
using AshproSAASERP.Interface.Reports;
using AshproSAASERP.Interface.Services;
using AshproSAASERP.Interface.Settings;
using AshproSAASERP.Interface.Transaction.Accounts;
using AshproSAASERP.Interface.Transaction.Inventory;
using AshproSAASERP.Interface.Transaction.Purchase;
using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.Interface.Transaction.Workshop;
using AshproSAASERP.Repo.Accounts;
using AshproSAASERP.Repo.CodeGenerationServices;
using AshproSAASERP.Repo.Masters;
using AshproSAASERP.Repo.Others;
using AshproSAASERP.Repo.Reports;
using AshproSAASERP.Repo.Services;
using AshproSAASERP.Repo.Settings;
using AshproSAASERP.Repo.Transaction.Inventory;
using AshproSAASERP.Repo.Transaction.Purchase;
using AshproSAASERP.Repo.Transaction.Sales;
using AshproSAASERP.Repo.Transaction.Workshop;
using AshproSAASERP.ZatcaEngine.Abstractions;

namespace AshproSAASERP.Mob.Services;

public static class DIHelper
{
    public static void SetDependency(this IServiceCollection Services)
    {
        #region Masters
        Services.AddScoped<IAccountHeadRepo, AccountHeadRepo>();
        Services.AddScoped<IAccountMasterRepo, AccountMasterRepo>();
        Services.AddScoped<IBranchRepo, BranchRepo>();
        Services.AddScoped<IFinancialYearRepo, FinancialYearRepo>();
        Services.AddScoped<IGeneralRepo, GeneralRepo>();
        Services.AddScoped<IPermanentSettingsRepo, PermanentSettingsRepo>();
        Services.AddScoped<IUnitMasterRepo, UnitMasterRepo>();
        Services.AddScoped<ISalesManMasterRepo, SalesManMasterRepo>();
        Services.AddScoped<ICategoryMasterRepo, CategoryMasterRepo>();
        Services.AddScoped<ISubCategoryMasterRepo, SubCategoryMasterRepo>();
        Services.AddScoped<ICustomerGroupMasterRepo, CustomerGroupMasterRepo>();
        Services.AddScoped<ICountryMasterRepo, CountryMasterRepo>();
        Services.AddScoped<ICustomerLocationMasterRepo, CustomerLocationMasterRepo>();
        Services.AddScoped<IVatMasterRepo, VatMasterRepo>();
        Services.AddScoped<IBrandMasterRepo, BrandMasterRepo>();
        Services.AddScoped<ICurrencyMasterRepo, CurrencyMasterRepo>();
        Services.AddScoped<IPurchaserMasterRepo, PurchaserMasterRepo>();
        Services.AddScoped<IProjectMasterRepo, ProjectMasterRepo>();
        Services.AddScoped<IUserRepo, UserRepo>();
        Services.AddScoped<IAccountGroupRepo, AccountGroupRepo>();
        Services.AddScoped<IAccountSubGroupRepo, AccountSubGroupRepo>();
        Services.AddScoped<IAccountTypeRepo, AccountTypeRepo>();
        Services.AddScoped<ICustomerMasterRepo, CustomerMasterRepo>();
        Services.AddScoped<ISupplierMasterRepo, SupplierMasterRepo>();
        Services.AddScoped<IItemMasterRepo, ItemMasterRepo>();
        Services.AddScoped<ICustomerOPEntryRepo, CustomerOPEntryRepo>();
        Services.AddScoped<IStoreMasterRepo, StoreMasterRepo>();
        Services.AddScoped<IOpeningStockEntryRepo, OpeningStockEntryRepo>();
        Services.AddScoped<ISupplierOPEntryRepo, SupplierOPEntryRepo>();
        Services.AddScoped<ICompanyDetailsRepo, CompanyDetailsRepo>();
        Services.AddScoped<IVehicleMasterRepo, VehicleMasterRepo>();
        Services.AddScoped<IRepairMasterRepo, RepairMasterRepo>();
        Services.AddScoped<ITechnicianMasterRepo, TechnicianMasterRepo>();

        #region Workshop
        Services.AddScoped<IJobOrderRepo, JobOrderRepo>();
        Services.AddScoped<IJobCardRepo, JobCardRepo>();
        #endregion

        #endregion

        #region Services
        Services.AddScoped<CodeSequenceService>();
        #endregion

        #region Inventory
        Services.AddScoped<IMaterialTransferRequestRepo, MaterialTransferRequestRepo>();
        Services.AddScoped<IMaterialTransferInRepo, MaterialTransferInRepo>();
        Services.AddScoped<IMaterialTransferOutRepo, MaterialTransferOutRepo>();
        Services.AddScoped<IStockAdjustmentRepo, StockAdjustmentRepo>();
        #endregion

        #region Sales
        Services.AddScoped<ISalesQuotationRepo, SalesQuotationRepo>();
        Services.AddScoped<ISalesOrderRepo, SalesOrderRepo>();
        Services.AddScoped<IDeliveryNoteRepo, DeliveryNoteRepo>();
        Services.AddScoped<IDeliveryNoteRtnRepo, DeliveryNoteRtnRepo>();
        Services.AddScoped<IProformaInvoiceRepo, ProformaInvoiceRepo>();
        Services.AddScoped<ISalesInvoiceRepo, SalesInvoiceRepo>();
        Services.AddScoped<ISalesInvoiceHoldRepo, SalesInvoiceHoldRepo>();
        Services.AddScoped<ISalesInvoiceRtnRepo, SalesInvoiceRtnRepo>();
        Services.AddScoped<IEnquiryRepo, EnquiryRepo>();
        Services.AddScoped<IZatcaRepo, ZatcaRepo>();
        Services.AddScoped<IMasterValueRepo, MasterValueRepo>();
        Services.AddScoped<IZatcaUploadService, ZatcaNewUploadService>();
        Services.AddScoped<IICVRepo, ICVRepo>();
        Services.AddScoped<IZatcaEngine, AshproSAASERP.ZatcaEngine.Core.ZatcaEngine>();
        #endregion

        #region Purchase
        Services.AddScoped<IPurchaseOrderRepo, PurchaseOrderRepo>();
        Services.AddScoped<IPurchaseRepo, PurchaseRepo>();
        Services.AddScoped<IPurchaseReturnRepo, PurchaseReturnRepo>();
        Services.AddScoped<IGoodsReceiptRepo, GoodsReceiptRepo>();
        #endregion

        #region Accounting
        Services.AddScoped<IJournalEntryRepo, JournalEntryRepo>();
        Services.AddScoped<IAccountOpeningEntryRepo, AccountOpeningEntryRepo>();
        Services.AddScoped<IExpenseEntryRepo, ExpenseEntryRepo>();
        Services.AddScoped<ISupplierPaymentRepo, SupplierPaymentRepo>();
        Services.AddScoped<ICustomerReceiptRepo, CustomerReceiptRepo>();
        #endregion

        #region General
        Services.AddScoped<IPermanentSettingsRepo, PermanentSettingsRepo>();
        Services.AddScoped<StockService>();
        Services.AddScoped<IMenuVisibilityRepo, MenuVisibilityRepo>();
        Services.AddScoped<IPrintDesignRepo, PrintDesignRepo>();
        Services.AddScoped<IBarcodeDesignRepo, BarcodeDesignRepo>();
        Services.AddScoped<ICommonImageRepo, CommonImageRepo>();
        Services.AddScoped<ILoginRepo, LoginRepo>();
        Services.AddScoped<IInternalMaterialTransferRepo, InternalMaterialTransferRepo>();
        Services.AddScoped<IZatcaInvoiceRepo, ZatcaInvoiceRepo>();
        Services.AddScoped<IServerRepo, ServerRepo>();
        Services.AddScoped<IProductImportRepo, ProductImportRepo>();
        Services.AddScoped<IProductExportRepo, ProductExportRepo>();

        // Register all implementations as transient
        Services.AddTransient<AshproSAASERP.SQlite.DatabaseSeedService>();
        Services.AddTransient<AshproSAASERP.PSQL.DatabaseSeedService>();
        Services.AddTransient<AshproSAASERP.MySQL.DatabaseSeedService>();
        Services.AddTransient<AshproSAASERP.Oracle.DatabaseSeedService>();
        Services.AddTransient<AshproSAASERP.MSSQL.DatabaseSeedService>();

        Services.AddTransient<AshproSAASERP.SQlite.CompanyHelper>();
        Services.AddTransient<AshproSAASERP.PSQL.CompanyHelper>();
        Services.AddTransient<AshproSAASERP.MySQL.CompanyHelper>();
        Services.AddTransient<AshproSAASERP.Oracle.CompanyHelper>();
        Services.AddTransient<AshproSAASERP.MSSQL.CompanyHelper>();

        // Register the dynamic factories
        Services.AddScoped<IDatabaseSeedService>(sp =>
        {
            var dbProvider = AshproSAASERP.Shared.Services.AppData.DBProvider;
            return dbProvider switch
            {
                "SQLite" => sp.GetRequiredService<AshproSAASERP.SQlite.DatabaseSeedService>(),
                "PSQL" => sp.GetRequiredService<AshproSAASERP.PSQL.DatabaseSeedService>(),
                "MySQL" => sp.GetRequiredService<AshproSAASERP.MySQL.DatabaseSeedService>(),
                "Oracle" => sp.GetRequiredService<AshproSAASERP.Oracle.DatabaseSeedService>(),
                _ => sp.GetRequiredService<AshproSAASERP.MSSQL.DatabaseSeedService>()
            };
        });

        Services.AddScoped<ICompanyHelper>(sp =>
        {
            var dbProvider = AshproSAASERP.Shared.Services.AppData.DBProvider;
            return dbProvider switch
            {
                "SQLite" => sp.GetRequiredService<AshproSAASERP.SQlite.CompanyHelper>(),
                "PSQL" => sp.GetRequiredService<AshproSAASERP.PSQL.CompanyHelper>(),
                "MySQL" => sp.GetRequiredService<AshproSAASERP.MySQL.CompanyHelper>(),
                "Oracle" => sp.GetRequiredService<AshproSAASERP.Oracle.CompanyHelper>(),
                _ => sp.GetRequiredService<AshproSAASERP.MSSQL.CompanyHelper>()
            };
        });
        #endregion

        #region Report
        Services.AddScoped<IInventoryReportRepo, InventoryReportRepo>();
        Services.AddScoped<ISalesReportRepo, SalesReportRepo>();
        Services.AddScoped<IPurchaseReportRepo, PurchaseReportRepo>();
        Services.AddScoped<IDamageStockRepo, DamageStockRepo>();
        Services.AddScoped<ISupplierReportRepo, SupplierReportRepo>();
        Services.AddScoped<ICustomerReportRepo, CustomerReportRepo>();
        Services.AddScoped<IAccountReportRepo, AccountReportRepo>();
        Services.AddScoped<IUserPrivilegeRepo, UserPrivilegeRepo>();
        Services.AddScoped<ITransactionReportRepo, TransactionReportRepo>();
        #endregion

    }
}
