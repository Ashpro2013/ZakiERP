using ZakiSAASERP.Interface.Masters;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Interface.Reports;
using ZakiSAASERP.Interface.Services;
using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Interface.Transaction.Accounts;
using ZakiSAASERP.Interface.Transaction.Inventory;
using ZakiSAASERP.Interface.Transaction.Purchase;
using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Interface.Transaction.Workshop;
using ZakiSAASERP.Repo.Accounts;
using ZakiSAASERP.Repo.CodeGenerationServices;
using ZakiSAASERP.Repo.Masters;
using ZakiSAASERP.Repo.Others;
using ZakiSAASERP.Repo.Reports;
using ZakiSAASERP.Repo.Services;
using ZakiSAASERP.Repo.Settings;
using ZakiSAASERP.Repo.Transaction.Inventory;
using ZakiSAASERP.Repo.Transaction.Purchase;
using ZakiSAASERP.Repo.Transaction.Sales;
using ZakiSAASERP.Repo.Transaction.Workshop;
using ZakiSAASERP.ZatcaEngine.Abstractions;

namespace ZakiSAASERP.Mob.Services;

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
        Services.AddScoped<IZatcaEngine, ZakiSAASERP.ZatcaEngine.Core.ZatcaEngine>();
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
        Services.AddScoped<IShiftClosingRepo, ShiftClosingRepo>();

        // Register all implementations as transient
        Services.AddTransient<ZakiSAASERP.SQlite.DatabaseSeedService>();
        Services.AddTransient<ZakiSAASERP.PSQL.DatabaseSeedService>();
        Services.AddTransient<ZakiSAASERP.MySQL.DatabaseSeedService>();
        Services.AddTransient<ZakiSAASERP.Oracle.DatabaseSeedService>();
        Services.AddTransient<ZakiSAASERP.MSSQL.DatabaseSeedService>();

        Services.AddTransient<ZakiSAASERP.SQlite.CompanyHelper>();
        Services.AddTransient<ZakiSAASERP.PSQL.CompanyHelper>();
        Services.AddTransient<ZakiSAASERP.MySQL.CompanyHelper>();
        Services.AddTransient<ZakiSAASERP.Oracle.CompanyHelper>();
        Services.AddTransient<ZakiSAASERP.MSSQL.CompanyHelper>();

        // Register the dynamic factories
        Services.AddScoped<IDatabaseSeedService>(sp =>
        {
            var dbProvider = ZakiSAASERP.Shared.Services.AppData.DBProvider;
            return dbProvider switch
            {
                "SQLite" => sp.GetRequiredService<ZakiSAASERP.SQlite.DatabaseSeedService>(),
                "PSQL" => sp.GetRequiredService<ZakiSAASERP.PSQL.DatabaseSeedService>(),
                "MySQL" => sp.GetRequiredService<ZakiSAASERP.MySQL.DatabaseSeedService>(),
                "Oracle" => sp.GetRequiredService<ZakiSAASERP.Oracle.DatabaseSeedService>(),
                _ => sp.GetRequiredService<ZakiSAASERP.MSSQL.DatabaseSeedService>()
            };
        });

        Services.AddScoped<ICompanyHelper>(sp =>
        {
            var dbProvider = ZakiSAASERP.Shared.Services.AppData.DBProvider;
            return dbProvider switch
            {
                "SQLite" => sp.GetRequiredService<ZakiSAASERP.SQlite.CompanyHelper>(),
                "PSQL" => sp.GetRequiredService<ZakiSAASERP.PSQL.CompanyHelper>(),
                "MySQL" => sp.GetRequiredService<ZakiSAASERP.MySQL.CompanyHelper>(),
                "Oracle" => sp.GetRequiredService<ZakiSAASERP.Oracle.CompanyHelper>(),
                _ => sp.GetRequiredService<ZakiSAASERP.MSSQL.CompanyHelper>()
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
