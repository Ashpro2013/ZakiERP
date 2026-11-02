using AshproSAASERP.Interface.Masters;
using AshproSAASERP.Interface.Reports;
using AshproSAASERP.Interface.Services;
using AshproSAASERP.Interface.Settings;
using AshproSAASERP.Interface.Transaction.Accounts;
using AshproSAASERP.Interface.Transaction.Inventory;
using AshproSAASERP.Interface.Transaction.Purchase;
using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.Interface.Transaction.Workshop;
using AshproSAASERP.Repo.Settings;
using AshproSAASERP.ZatcaEngine.Abstractions;

namespace AshproSAASERP.Web.Services;

public static class DIHelper
{
    public static void SetDependency(this WebApplicationBuilder builder)
    {
        #region Masters
        builder.Services.AddScoped<IAccountHeadRepo, AccountHeadRepo>();
        builder.Services.AddScoped<IAccountMasterRepo, AccountMasterRepo>();
        builder.Services.AddScoped<IBranchRepo, BranchRepo>();
        builder.Services.AddScoped<IFinancialYearRepo, FinancialYearRepo>();
        builder.Services.AddScoped<IGeneralRepo, GeneralRepo>();
        builder.Services.AddScoped<IUnitMasterRepo, UnitMasterRepo>();
        builder.Services.AddScoped<ISalesManMasterRepo, SalesManMasterRepo>();
        builder.Services.AddScoped<ICategoryMasterRepo, CategoryMasterRepo>();
        builder.Services.AddScoped<ISubCategoryMasterRepo, SubCategoryMasterRepo>();
        builder.Services.AddScoped<ICustomerGroupMasterRepo, CustomerGroupMasterRepo>();
        builder.Services.AddScoped<ICountryMasterRepo, CountryMasterRepo>();
        builder.Services.AddScoped<ICustomerLocationMasterRepo, CustomerLocationMasterRepo>();
        builder.Services.AddScoped<IVatMasterRepo, VatMasterRepo>();
        builder.Services.AddScoped<IBrandMasterRepo, BrandMasterRepo>();
        builder.Services.AddScoped<ICurrencyMasterRepo, CurrencyMasterRepo>();
        builder.Services.AddScoped<IPurchaserMasterRepo, PurchaserMasterRepo>();
        builder.Services.AddScoped<IProjectMasterRepo, ProjectMasterRepo>();
        builder.Services.AddScoped<IUserRepo, UserRepo>();
        builder.Services.AddScoped<ILoginRepo, LoginRepo>();
        builder.Services.AddScoped<IAccountGroupRepo, AccountGroupRepo>();
        builder.Services.AddScoped<IAccountSubGroupRepo, AccountSubGroupRepo>();
        builder.Services.AddScoped<IAccountTypeRepo, AccountTypeRepo>();
        builder.Services.AddScoped<ICustomerMasterRepo, CustomerMasterRepo>();
        builder.Services.AddScoped<ISupplierMasterRepo, SupplierMasterRepo>();
        builder.Services.AddScoped<IItemMasterRepo, ItemMasterRepo>();
        builder.Services.AddScoped<ICustomerOPEntryRepo, CustomerOPEntryRepo>();
        builder.Services.AddScoped<ISupplierOPEntryRepo, SupplierOPEntryRepo>();
        builder.Services.AddScoped<IStoreMasterRepo, StoreMasterRepo>();
        builder.Services.AddScoped<IOpeningStockEntryRepo, OpeningStockEntryRepo>();
        builder.Services.AddScoped<ICompanyDetailsRepo, CompanyDetailsRepo>();
        builder.Services.AddScoped<IVehicleMasterRepo, VehicleMasterRepo>();
        builder.Services.AddScoped<IRepairMasterRepo, RepairMasterRepo>();
        builder.Services.AddScoped<ITechnicianMasterRepo, TechnicianMasterRepo>();

        #endregion

        #region Services
        builder.Services.AddScoped<CodeSequenceService>();

        var dbProvider = builder.Configuration["DbProvider"] ?? "MSSQL";
        switch (dbProvider.ToUpperInvariant())
        {
            case "SQLITE":
                builder.Services.AddScoped<IDatabaseSeedService, AshproSAASERP.SQlite.DatabaseSeedService>();
                builder.Services.AddScoped<ICompanyHelper, AshproSAASERP.SQlite.CompanyHelper>();
                break;
            case "PSQL":
                builder.Services.AddScoped<IDatabaseSeedService, AshproSAASERP.PSQL.DatabaseSeedService>();
                builder.Services.AddScoped<ICompanyHelper, AshproSAASERP.PSQL.CompanyHelper>();
                break;
            case "MYSQL":
                builder.Services.AddScoped<IDatabaseSeedService, AshproSAASERP.MySQL.DatabaseSeedService>();
                builder.Services.AddScoped<ICompanyHelper, AshproSAASERP.MySQL.CompanyHelper>();
                break;
            case "ORACLE":
                builder.Services.AddScoped<IDatabaseSeedService, AshproSAASERP.Oracle.DatabaseSeedService>();
                builder.Services.AddScoped<ICompanyHelper, AshproSAASERP.Oracle.CompanyHelper>();
                break;
            case "MSSQL":
            default:
                builder.Services.AddScoped<IDatabaseSeedService, AshproSAASERP.MSSQL.DatabaseSeedService>();
                builder.Services.AddScoped<ICompanyHelper, AshproSAASERP.MSSQL.CompanyHelper>();
                break;
        }
        #endregion

        #region Sales
        builder.Services.AddScoped<ISalesQuotationRepo, SalesQuotationRepo>();
        builder.Services.AddScoped<ISalesOrderRepo, SalesOrderRepo>();
        builder.Services.AddScoped<IDeliveryNoteRepo, DeliveryNoteRepo>();
        builder.Services.AddScoped<IDeliveryNoteRtnRepo, DeliveryNoteRtnRepo>();
        builder.Services.AddScoped<IProformaInvoiceRepo, ProformaInvoiceRepo>();
        builder.Services.AddScoped<ISalesInvoiceRepo, SalesInvoiceRepo>();
        builder.Services.AddScoped<ISalesInvoiceHoldRepo, SalesInvoiceHoldRepo>();
        builder.Services.AddTransient<ISalesInvoiceRtnRepo, SalesInvoiceRtnRepo>();
        builder.Services.AddTransient<IPurchaseReturnRepo, PurchaseReturnRepo>();
        builder.Services.AddScoped<IEnquiryRepo, EnquiryRepo>();
        #endregion

        #region Purchase
        builder.Services.AddScoped<IPurchaseOrderRepo, PurchaseOrderRepo>();
        builder.Services.AddScoped<IPurchaseRepo, PurchaseRepo>();
        #endregion

        #region Accounting
        builder.Services.AddScoped<IJournalEntryRepo, JournalEntryRepo>();
        builder.Services.AddScoped<IAccountOpeningEntryRepo, AccountOpeningEntryRepo>();
        builder.Services.AddScoped<IExpenseEntryRepo, ExpenseEntryRepo>();

        builder.Services.AddScoped<IGoodsReceiptRepo, GoodsReceiptRepo>();
        builder.Services.AddScoped<IDamageStockRepo, DamageStockRepo>();
        builder.Services.AddScoped<ISupplierPaymentRepo, SupplierPaymentRepo>();
        builder.Services.AddScoped<ICustomerReceiptRepo, CustomerReceiptRepo>();
        #endregion

        #region General
        builder.Services.AddScoped<IPermanentSettingsRepo, PermanentSettingsRepo>();
        builder.Services.AddScoped<StockService>();
        builder.Services.AddScoped<IStockAdjustmentRepo, StockAdjustmentRepo>();
        builder.Services.AddScoped<IMenuVisibilityRepo, MenuVisibilityRepo>();
        builder.Services.AddScoped<IPrintDesignRepo, PrintDesignRepo>();
        builder.Services.AddScoped<IBarcodeDesignRepo, BarcodeDesignRepo>();
        builder.Services.AddScoped<ICommonImageRepo, CommonImageRepo>();
        builder.Services.AddScoped<IZatcaRepo, ZatcaRepo>();
        builder.Services.AddScoped<IMasterValueRepo, MasterValueRepo>();
        builder.Services.AddScoped<IZatcaUploadService, ZatcaNewUploadService>();
        builder.Services.AddScoped<IICVRepo, ICVRepo>();
        builder.Services.AddScoped<IMaterialTransferRequestRepo, MaterialTransferRequestRepo>();
        builder.Services.AddScoped<IMaterialTransferInRepo, MaterialTransferInRepo>();
        builder.Services.AddScoped<IMaterialTransferOutRepo, MaterialTransferOutRepo>();
        builder.Services.AddScoped<IInternalMaterialTransferRepo, InternalMaterialTransferRepo>();
        builder.Services.AddScoped<IUserPrivilegeRepo, UserPrivilegeRepo>();
        builder.Services.AddScoped<IZatcaInvoiceRepo, ZatcaInvoiceRepo>();
        builder.Services.AddScoped<IZatcaEngine, ZatcaEngine.Core.ZatcaEngine>();
        builder.Services.AddScoped<IServerRepo, ServerRepo>();
        builder.Services.AddScoped<IProductImportRepo, ProductImportRepo>();
        builder.Services.AddScoped<IProductExportRepo, ProductExportRepo>();

        #endregion

        #region Report
        builder.Services.AddScoped<IInventoryReportRepo, InventoryReportRepo>();
        builder.Services.AddScoped<ISalesReportRepo, SalesReportRepo>();
        builder.Services.AddScoped<IPurchaseReportRepo, PurchaseReportRepo>();
        builder.Services.AddScoped<ISupplierReportRepo, SupplierReportRepo>();
        builder.Services.AddScoped<ICustomerReportRepo, CustomerReportRepo>();
        builder.Services.AddScoped<IAccountReportRepo, AccountReportRepo>();
        builder.Services.AddScoped<ITransactionReportRepo, TransactionReportRepo>();
        #endregion

        #region Workshop
        builder.Services.AddScoped<IJobOrderRepo, JobOrderRepo>();
        builder.Services.AddScoped<IJobCardRepo, JobCardRepo>();
        #endregion
    }
}


