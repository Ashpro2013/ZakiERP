using AshproSAASERP.Models.Others;
using AshproSAASERP.Models.Transactions.Accounts;
using AshproSAASERP.Models.Transactions.Inventory;
using AshproSAASERP.Models.Transactions.Purchase;
using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.Models.Transactions.Workshop;
using AshproSAASERP.Models.Zatca;
using Microsoft.EntityFrameworkCore;
namespace AshproSAASERP.Interface;

public class TenantDBContext : DbContext
{

    public TenantDBContext(DbContextOptions<TenantDBContext> options) : base(options)
    {
    }

    #region Model Creation
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MaterialTransferInHdr>(entity =>
        {
            // 🔥 FromBranch FK
            entity.HasOne(x => x.FromBranch)
                  .WithMany()
                  .HasForeignKey(x => x.FK_FromBranchId)
                  .OnDelete(DeleteBehavior.NoAction);

            // 🔥 ToBranch FK
            entity.HasOne(x => x.ToBranch)
                  .WithMany()
                  .HasForeignKey(x => x.FK_ToBranchId)
                  .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<BarcodeData>()
            .HasNoKey()
            .ToView("BarcodeData");
        modelBuilder.Entity<AccountMaster>()
    .HasIndex(x => x.Code)
    .IsUnique();
        modelBuilder.Entity<AccountSubGroup>()
    .HasIndex(x => x.Code)
    .IsUnique();
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }
    #endregion

    #region Masters
    public DbSet<AccountGroup> AccountGroups { get; set; } = default!;
    public DbSet<AccountSubGroup> AccountSubGroups { get; set; } = default!;
    public DbSet<AccountHeadMaster> AccountHeadMasters { get; set; } = default!;
    public DbSet<AccountType> AccountTypes { get; set; } = default!;
    public DbSet<BranchMaster> BranchMasters { get; set; } = default!;
    public DbSet<AccountMaster> AccountMasters { get; set; } = default!;
    public DbSet<FinancialYear> FinancialYears { get; set; } = default!;
    public DbSet<UnitMaster> UnitMasters { get; set; } = default!;
    public DbSet<SalesManMaster> SalesManMasters { get; set; } = default!;
    public DbSet<CategoryMaster> CategoryMasters { get; set; } = default!;
    public DbSet<SubCategoryMaster> SubCategoryMasters { get; set; } = default!;
    public DbSet<CustomerGroupMaster> CustomerGroupMasters { get; set; } = default!;
    public DbSet<CountryMaster> CountryMasters { get; set; } = default!;
    public DbSet<CustomerLocationMaster> CustomerLocationMasters { get; set; } = default!;
    public DbSet<VatMaster> VatMasters { get; set; } = default!;
    public DbSet<BrandMaster> BrandMasters { get; set; } = default!;
    public DbSet<CurrencyMaster> CurrencyMasters { get; set; } = default!;
    public DbSet<PurchaserMaster> PurchaserMasters { get; set; } = default!;
    public DbSet<ProjectMaster> ProjectMasters { get; set; } = default!;
    public DbSet<CustomerMaster> CustomerMasters { get; set; } = default!;
    public DbSet<SupplierMaster> SupplierMasters { get; set; } = default!;
    public DbSet<CustomerLocationDetails> CustomerLocationDetails { get; set; } = default!;
    public DbSet<ItemMaster> ItemMasters { get; set; } = default!;
    public DbSet<ItemMultiUnit> ItemMultiUnits { get; set; }
    public DbSet<Models.Masters.VATCategoryDetails> VATCategoryDetails { get; set; }
    public DbSet<CustomerOPEntry> CustomerOPEntries { get; set; } = default!;
    public DbSet<CustomerOPEntryDetails> CustomerOPEntryDetails { get; set; } = default!;
    public DbSet<CustomerOPEntryUpdated> CustomerOPEntryUpdated { get; set; } = default!;
    public DbSet<SupplierOPEntry> SupplierOPEntries { get; set; } = default!;
    public DbSet<SupplierOPEntryDetails> SupplierOPEntryDetails { get; set; } = default!;
    public DbSet<SupplierOPEntryUpdated> SupplierOPEntryUpdated { get; set; } = default!;
    public DbSet<StoreMaster> StoreMasters { get; set; } = default;
    public DbSet<OpeningStockEntry> OpeningStockEntries { get; set; } = default!;
    public DbSet<OpeningStockEntryItem> OpeningStockEntryItems { get; set; } = default!;
    public DbSet<OpeningStockEntryItemForUpdate> OpeningStockEntryItemForUpdate { get; set; } = default!;
    public DbSet<AccountOpeningEntryHdr> AccountOpeningEntryHdrs { get; set; } = default!;
    public DbSet<AccountOpeningEntryDtls> AccountOpeningEntryDtls { get; set; } = default!;
    public DbSet<PermanentSettings> PermanentSettings { get; set; } = default!;
    public DbSet<CodeSequence> CodeSequences { get; set; } = default!;
    public DbSet<CompanyDetails> CompanyDetails { get; set; } = default!;

    // Menu Visibility Settings
    public DbSet<ERPType> ERPTypes { get; set; } = default!;
    public DbSet<ERPTypePreset> ERPTypePresets { get; set; } = default!;
    public DbSet<ClientERPSettings> ClientERPSettings { get; set; } = default!;
    public DbSet<MenuVisibility> MenuVisibilities { get; set; } = default!;
    public DbSet<VehicleMaster> VehicleMasters { get; set; } = default!;
    public DbSet<RepairMaster> RepairMasters { get; set; } = default!;
    public DbSet<TechnicianMaster> TechnicianMasters { get; set; } = default!;
    #endregion

    #region Transactions
    public DbSet<SalesQuotationHdr> SalesQuotationHdrs { get; set; } = default!;
    public DbSet<SalesQuotationDtls> SalesQuotationDtls { get; set; } = default!;
    public DbSet<SalesInvoiceHdr> SalesInvoiceHdrs { get; set; } = default!;
    public DbSet<SalesInvoiceDtls> SalesInvoiceDtls { get; set; } = default!;
    public DbSet<SalesOrderHdr> SalesOrderHdrs { get; set; } = default!;
    public DbSet<SalesOrderDtls> SalesOrderDtls { get; set; } = default!;
    public DbSet<DeliveryNoteHdr> DeliveryNoteHdrs { get; set; } = default!;
    public DbSet<DeliveryNoteDtls> DeliveryNoteDtls { get; set; } = default!;
    public DbSet<DeliveryNoteRtnHdr> DeliveryNoteRtnHdrs { get; set; } = default!;
    public DbSet<DeliveryNoteRtnDtls> DeliveryNoteRtnDtls { get; set; } = default!;
    public DbSet<ProformaInvoiceHdr> ProformaInvoiceHdrs { get; set; } = default!;
    public DbSet<ProformaInvoiceDtls> ProformaInvoiceDtls { get; set; } = default!;
    public DbSet<SalesInvoiceRtnHdr> SalesInvoiceRtnHdrs { get; set; } = default!;
    public DbSet<SalesInvoiceRtnDtls> SalesInvoiceRtnDtls { get; set; } = default!;
    public DbSet<TblAccounting> TblAccountings { get; set; } = default!;
    public DbSet<JournalEntryHdr> JournalEntryHdrs { get; set; } = default!;
    public DbSet<JournalEntryDtls> JournalEntryDtls { get; set; } = default!;
    public DbSet<ExpenseEntryHdr> ExpenseEntryHdrs { get; set; } = default!;
    public DbSet<ExpenseEntryDtls> ExpenseEntryDtls { get; set; } = default!;
    public DbSet<PurchaseOrderHdr> PurchaseOrderHdrs { get; set; } = default!;
    public DbSet<PurchaseOrderDtls> PurchaseOrderDtls { get; set; } = default!;
    public DbSet<StockTranLog> StockTranLogs { get; set; } = default!;
    public DbSet<ItemStockBalance> ItemStockBalances { get; set; } = default!;
    public DbSet<GoodsReceiptHdr> GoodsReceiptHdrs { get; set; } = default!;
    public DbSet<GoodsReceiptDtls> GoodsReceiptDtls { get; set; } = default!;
    public DbSet<PurchaseHdr> PurchaseHdrs { get; set; } = default!;
    public DbSet<PurchaseDtls> PurchaseDtls { get; set; } = default!;
    public DbSet<PurchaseReturnHdr> PurchaseReturnHdrs { get; set; } = default!;
    public DbSet<PurchaseReturnDtls> PurchaseReturnDtls { get; set; } = default!;
    public DbSet<InvoiceICV> InvoiceICVs { get; set; } = default!;
    public DbSet<SupplierPaymentHdr> SupplierPaymentHdrs { get; set; } = default!;
    public DbSet<SupplierPaymentDtls> SupplierPaymentDtls { get; set; } = default!;
    public DbSet<DamageStockHdr> DamageStockHdrs { get; set; } = default!;
    public DbSet<DamageStockDtls> DamageStockDtls { get; set; } = default!;
    public DbSet<CustomerReceiptHdr> CustomerReceiptHdrs { get; set; } = default!;
    public DbSet<CustomerReceiptDtls> CustomerReceiptDtls { get; set; } = default!;
    public DbSet<StockAdjustmentHdr> StockAdjustmentHdrs { get; set; }
    public DbSet<StockAdjustmentDtls> StockAdjustmentDtls { get; set; }
    public DbSet<MaterialTransferRequestHdr> MaterialTransferRequestHdrs { get; set; }
    public DbSet<MaterialTransferRequestDtls> MaterialTransferRequestDtls { get; set; } = default!;
    public DbSet<MaterialTransferInHdr> MaterialTransferInHdrs { get; set; }
    public DbSet<MaterialTransferInDtls> MaterialTransferInDtls { get; set; } = default!;
    public DbSet<MaterialTransferOutHdr> MaterialTransferOutHdrs { get; set; }
    public DbSet<MaterialTransferOutDtls> MaterialTransferOutDtls { get; set; } = default!;
    public DbSet<JobOrderHdr> JobOrderHdrs { get; set; } = default!;
    public DbSet<JobOrderDtls> JobOrderDtls { get; set; } = default!;
    public DbSet<JobCardHdr> JobCardHdrs { get; set; } = default!;
    public DbSet<JobCardDtls> JobCardDtls { get; set; } = default!;
    public DbSet<InternalMaterialTransferHdr> InternalMaterialTransferHdrs { get; set; } = default!;
    public DbSet<InternalMaterialTransferDtls> InternalMaterialTransferDtls { get; set; } = default!;
    public DbSet<RFQHdr> RFQHdrs { get; set; } = default!;
    public DbSet<RFQDtls> RFQDtls { get; set; } = default!;
    public DbSet<SalesInvoiceHoldHdr> SalesInvoiceHoldHdrs { get; set; } = default!;
    public DbSet<SalesInvoiceHoldDtls> SalesInvoiceHoldDtls { get; set; } = default!;
    #endregion

    #region Others
    public DbSet<PrintDesignHdr> PrintDesignHdrs { get; set; } = default!;
    public DbSet<PrintDesignDtls> PrintDesignDtls { get; set; } = default!;
    public DbSet<BarcodeDesignHdr> BarcodeDesignHdrs { get; set; } = default!;
    public DbSet<BarcodeDesignDtls> BarcodeDesignDtls { get; set; } = default!;
    public DbSet<MasterValue> MasterValues { get; set; } = default!;
    public DbSet<BarcodeData> BarcodeData => Set<BarcodeData>();
    public DbSet<ZatcaInvoices> ZatcaInvoices { get; set; }

    #endregion

    #region User
    public DbSet<UserDetails> UserDetails { get; set; } = default!;
    public DbSet<UserPrivilegeHdr> UserPrivilegeHdrs { get; set; } = default!;
    public DbSet<MenuDetails> MenuDetails { get; set; } = default!;
    public DbSet<FunctionDetails> FunctionDetails { get; set; } = default!;
    public DbSet<UserPrivilegeDtls> UserPrivilegeDtls { get; set; } = default!;
    public DbSet<FunctionalPrivilege> FunctionalPrivileges { get; set; } = default!;
    public DbSet<UserBranch> UserBranches { get; set; } = default!;
    #endregion

}



