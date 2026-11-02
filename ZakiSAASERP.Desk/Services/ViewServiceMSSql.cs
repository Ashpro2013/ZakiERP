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
using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Models.DTO;  // Added this crucial namespace
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Services; // For PermanentSettingsData
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Models.Transactions.Accounts;
using ZakiSAASERP.Models.Transactions.Inventory;
using ZakiSAASERP.Models.Transactions.Purchase;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.Transactions.Workshop;
using ZakiSAASERP.Models.User;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Services;
using Refit;


namespace ZakiSAASERP.Desk.Services;

public class ViewServiceMSSql(
    IAccountHeadRepo accountHead,
    IAccountGroupRepo accountGroupRepo,
    IAccountSubGroupRepo accountSubGroupRepo,
    IAccountMasterRepo accountMasterRepo,
    IAccountTypeRepo accountTypeRepo,
    IBranchRepo branchRepo,
    IFinancialYearRepo financialYearRepo,
    IGeneralRepo generalRepo, HttpClient http,
    IUnitMasterRepo unitRepo, IConProviderMaui conProvider,
    ISalesManMasterRepo salesManRepo,
    ICategoryMasterRepo categoryRepo,
    ISubCategoryMasterRepo subCategoryRepo,
    ICustomerGroupMasterRepo customerGroupRepo,
    ICountryMasterRepo countryMasterRepo,
    ICustomerLocationMasterRepo customerLocationRepo,
   IVatMasterRepo vatRepo, IBrandMasterRepo brandRepo,
    ICurrencyMasterRepo currencyRepo,
    IPurchaserMasterRepo purchaseRepo,
    IProjectMasterRepo projectRepo,
    ICustomerMasterRepo customerMasterRepo,
    ISupplierMasterRepo supplierMasterRepo,
    IUserRepo userRepo,
    IItemMasterRepo itemRepo,
    ICustomerOPEntryRepo customerOPEntryRepo,
    ISupplierOPEntryRepo supplierOPEntryRepo,
    ISalesQuotationRepo quotationRepo,
    ISalesOrderRepo orderRepo,
    IDeliveryNoteRepo deliveryNoteRepo,
    IDeliveryNoteRtnRepo deliveryNoteRtnRepo,
    IStoreMasterRepo storeMasterRepo,

    IOpeningStockEntryRepo openingStockEntryRepo,
    IPurchaseOrderRepo purchaseOrderRepo,
    IPermanentSettingsRepo permanentSettingsRepo,
    IMenuVisibilityRepo menuVisibilityRepo,
    IPrintDesignRepo printDesignRepo,
    IBarcodeDesignRepo barcodeDesignRepo,
    IGoodsReceiptRepo goodsReceiptRepo,
    ICommonImageRepo commonImageRepo,
    IProformaInvoiceRepo proformaInvoiceRepo,
    ISalesInvoiceRepo invoiceRepo,
    ISalesInvoiceHoldRepo invoiceHoldRepo,
    IEnquiryRepo enquiryRepo,
    ISalesInvoiceRtnRepo salesInvoiceRtnRepo,
    ILoginRepo loginRepo,
    IPurchaseRepo purchaseInvoiceRepo,
    ICompanyDetailsRepo companyDetailsRepo,
    IPurchaseReturnRepo purchaseReturnRepo,
    IInventoryReportRepo inventReportRepo,
    ISalesReportRepo salesReportRepo,
    IAccountReportRepo accountReportRepo,
    IJournalEntryRepo journalEntryRepo,
    IAccountOpeningEntryRepo accountOpeningEntryRepo,
    IExpenseEntryRepo expenseEntryRepo,
    IZatcaRepo zatcaRepo,
    ISupplierPaymentRepo supplierPaymentRepo,
    IDamageStockRepo damageStockRepo,
    ICustomerReceiptRepo customerReceiptRepo,
    IStockAdjustmentRepo stockAdjustmentRepo,
    IMaterialTransferRequestRepo materialTransferRequestRepo,
    IMaterialTransferInRepo materialTransferInRepo,
    IMaterialTransferOutRepo materialTransferOutRepo,
    IVehicleMasterRepo vehicleRepo,
    IRepairMasterRepo repairRepo,
    IUserPrivilegeRepo userPrevilegeRepo,
    ITechnicianMasterRepo technicianRepo,
    IJobOrderRepo jobOrderRepo,
    IJobCardRepo jobCardRepo,
    IPurchaseReportRepo purchaseReportRepo,
    IInternalMaterialTransferRepo internalMaterialTransferRepo,
    ISupplierReportRepo supplierReportRepo,
    ICustomerReportRepo customerReportRepo,
    ITransactionReportRepo transactionReportRepo,
    IZatcaInvoiceRepo zatcaInvoiceRepo,
    IZatcaUploadService zatcaUploadService,
    IProductImportRepo productImportRepo,
    IProductExportRepo productExportRepo
    ) : IViewService
{

    #region Masters


    public async Task<List<SupplierStatementDTO>> GetSupplierStatementAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        => await supplierReportRepo.GetSupplierStatementAsync(fromDate, toDate, branchId, supplierId);

    public async Task<List<SupplierPendingInvoiceDTO>> GetSupplierPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        => await supplierReportRepo.GetSupplierPendingInvoiceAsync(fromDate, toDate, branchId, supplierId);

    public async Task<List<SupplierPaymentReportDTO>> GetSupplierPaymentReportAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId, string? billType)
        => await supplierReportRepo.GetSupplierPaymentReportAsync(fromDate, toDate, branchId, supplierId, billType);

    public async Task<decimal> GetSupplierBalance(int supplierId, int branchId)
        => await supplierPaymentRepo.GetSupplierBalanceAsync(supplierId, branchId);



    #region Account Head
    public async Task<List<AccountHeadMaster>> GetAllAccountHeads() => await accountHead.GetAllAsync();
    public async Task<AccountHeadMaster> GetAccountHeadById(int id) => await accountHead.GetAsync(id);
    public async Task<OutputResponse> CreateAccountHead(AccountHeadMaster accountHeadMaster) => await accountHead.CreateAsync(accountHeadMaster);
    public async Task<OutputResponse> UpdateAccountHead(AccountHeadMaster accountHeadMaster) => await accountHead.UpdateAsync(accountHeadMaster);
    public async Task<OutputResponse> DeleteAccountHead(int id) => await accountHead.DeleteAsync(id);
    public async Task<OutputResponse> GetAccountHeadNextCode(int typeId) => await accountHead.GetNextCode(typeId);
    public async Task<AccountHeadPDTO> GetAccountHeadPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await accountHead.GetAllWithPagination(pagination);
        return new AccountHeadPDTO(items, totalCount);
    }
    #endregion

    #region AccountGroup
    public async Task<List<AccountGroup>> GetAllAccountGroups() => await accountGroupRepo.GetAllAsync();
    public async Task<AccountGroup> GetAccountGroupById(int id) => await accountGroupRepo.GetAsync(id);
    public async Task<OutputResponse> CreateAccountGroup(AccountGroup accountGroup) => await accountGroupRepo.CreateAsync(accountGroup);
    public async Task<OutputResponse> UpdateAccountGroup(AccountGroup accountGroup) => await accountGroupRepo.UpdateAsync(accountGroup);
    public async Task<OutputResponse> DeleteAccountGroup(int id) => await accountGroupRepo.DeleteAsync(id);
    public async Task<OutputResponse> GetAccountGroupNextCode(int accountHeadId) => await accountGroupRepo.GetNextCode(accountHeadId);
    public async Task<AccountGroupPDTO> GetAccountGroupPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await accountGroupRepo.GetAllWithPagination(pagination);
        return new AccountGroupPDTO(items, totalCount);
    }
    #endregion

    #region AccountSubGroup
    public async Task<List<AccountSubGroup>> GetAllAccountSubGroups() => await accountSubGroupRepo.GetAllAsync();
    public async Task<AccountSubGroup> GetAccountSubGroupById(int id) => await accountSubGroupRepo.GetAsync(id);
    public async Task<OutputResponse> CreateAccountSubGroup(AccountSubGroup accountSubGroup) => await accountSubGroupRepo.CreateAsync(accountSubGroup);
    public async Task<OutputResponse> UpdateAccountSubGroup(AccountSubGroup accountSubGroup) => await accountSubGroupRepo.UpdateAsync(accountSubGroup);
    public async Task<OutputResponse> DeleteAccountSubGroup(int id) => await accountSubGroupRepo.DeleteAsync(id);
    public async Task<OutputResponse> GetAccountSubGroupNextCode(int accountGroupId) => await accountSubGroupRepo.GetNextCode(accountGroupId);
    public async Task<AccountSubGroupPDTO> GetAccountSubGroupPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await accountSubGroupRepo.GetAllWithPagination(pagination);
        return new AccountSubGroupPDTO(items, totalCount);
    }
    #endregion

    #region Account
    public async Task<List<AccountMaster>> GetAllAccounts() => await accountMasterRepo.GetAllAsync();
    public async Task<List<AccountType>> GetAllAccountTypes() => await accountTypeRepo.GetAllAsync();
    public async Task<AccountMaster> GetAccountMasterById(int id) => await accountMasterRepo.GetAsync(id);
    public async Task<OutputResponse> CreateAccountMaster(AccountMaster accountMaster) => await accountMasterRepo.CreateAsync(accountMaster);
    public async Task<OutputResponse> UpdateAccountMaster(AccountMaster accountMaster) => await accountMasterRepo.UpdateAsync(accountMaster);
    public async Task<OutputResponse> DeleteAccountMaster(int id) => await accountMasterRepo.DeleteAsync(id);
    public async Task<OutputResponse> GetAccountMasterNextCode(int subGroupId) => await accountMasterRepo.GetNextCode(subGroupId);
    public async Task<AccountPDTO> GetAccountPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await accountMasterRepo.GetAllWithPagination(pagination);
        return new AccountPDTO(items, totalCount);
    }
    #endregion

    #region Branch
    public async Task<OutputResponse> CreateBranch(BranchMaster branch) => await branchRepo.CreateAsync(branch);
    public async Task<OutputResponse> DeleteBranch(int id) => await branchRepo.DeleteAsync(id);
    public async Task<List<BranchMaster>> GetAllBranches() => await branchRepo.GetAllAsync();
    public async Task<BranchMaster> GetBranchById(int id) => await branchRepo.GetAsync(id);
    public async Task<OutputResponse> UpdateBranch(BranchMaster branch) => await branchRepo.UpdateAsync(branch);
    public async Task<BranchPDTO> GetBranchPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await branchRepo.GetAllWithPagination(pagination);
        return new BranchPDTO(items, totalCount);
    }
    #endregion

    #region Financial Year
    public async Task<List<FinancialYear>> GetAllFinancialYears() => await financialYearRepo.GetAllAsync();
    public async Task<FinancialYear> GetFinancialYearById(int id) => await financialYearRepo.GetAsync(id);
    public async Task<OutputResponse> CreateFinancialYear(FinancialYear financialYear) => await financialYearRepo.CreateAsync(financialYear);
    public async Task<OutputResponse> UpdateFinancialYear(FinancialYear financialYear) => await financialYearRepo.UpdateAsync(financialYear);
    public async Task<OutputResponse> DeleteFinancialYear(int id) => await financialYearRepo.DeleteAsync(id);
    public async Task<int> GetMaxRefNo() => await financialYearRepo.GetMaxRefNo();
    public async Task<FinancialYearPDTO> GetFinancialYearPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await financialYearRepo.GetAllWithPagination(pagination);
        return new FinancialYearPDTO(items, totalCount);
    }
    #endregion

    #region SalesMan
    public async Task<List<SalesManMaster>> GetAllSalesMan() => await salesManRepo.GetAllAsync();
    public async Task<SalesManMaster> GetSalesManById(int id) => await salesManRepo.GetAsync(id);
    public async Task<OutputResponse> CreateSalesMan(SalesManMaster salesMan) => await salesManRepo.CreateAsync(salesMan);
    public async Task<OutputResponse> UpdateSalesMan(SalesManMaster salesMan) => await salesManRepo.UpdateAsync(salesMan);
    public async Task<OutputResponse> DeleteSalesMan(int id) => await salesManRepo.DeleteAsync(id);
    public async Task<SalesManPDTO> GetSalesManPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await salesManRepo.GetAllWithPagination(pagination);
        return new SalesManPDTO(items, totalCount);
    }
    #endregion

    #region User
    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var conn = conProvider.GetConnectionString();
        var user = await userRepo.GetForLogin(request.EmailOrUserName, request.Password);
        if (user == null)
        {
            return new LoginResponse(0, "Invalid credentials", false);
        }
        return new LoginResponse(user.UserId, "", true);
    }
    public async Task<PreLoginDatas> GetPreLoginDetails(string tenant, string username)
    {
        var con = conProvider.GetConnectionString();
        var branches = await loginRepo.GetBranchesForUserAsync(username, con);
        if (!branches.Any())
        {
            branches = await loginRepo.GetAllLoginBranchesAsync(con);
        }
        var fyears = await loginRepo.GetOpenFinancialYearsAsync(con);
        return new PreLoginDatas(branches, fyears, null);
    }
    #endregion

    #region Unit
    public async Task<List<UnitMaster>> GetAllUnits() => await unitRepo.GetAllAsync();
    public async Task<UnitMaster> GetUnitById(int id) => await unitRepo.GetAsync(id);
    public async Task<OutputResponse> CreateUnit(UnitMaster unit) => await unitRepo.CreateAsync(unit);
    public async Task<OutputResponse> UpdateUnit(UnitMaster unit) => await unitRepo.UpdateAsync(unit);
    public async Task<OutputResponse> DeleteUnit(int id) => await unitRepo.DeleteAsync(id);
    public async Task<UnitPDTO> GetUnitPDetails(PaginationModel pagination)
    {
        var (units, totalCount) = await unitRepo.GetAllWithPagination(pagination);
        return new UnitPDTO(units, totalCount);
    }
    #endregion

    #region Category
    public async Task<List<CategoryMaster>> GetAllCategory() => await categoryRepo.GetAllAsync();
    public async Task<CategoryMaster> GetCategoryById(int id) => await categoryRepo.GetAsync(id);
    public async Task<OutputResponse> CreateCategory(CategoryMaster Category) => await categoryRepo.CreateAsync(Category);
    public async Task<OutputResponse> UpdateCategory(CategoryMaster Category) => await categoryRepo.UpdateAsync(Category);
    public async Task<OutputResponse> DeleteCategory(int id) => await categoryRepo.DeleteAsync(id);
    public async Task<CategoryPDTO> GetCategoryPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await categoryRepo.GetAllWithPagination(pagination);
        return new CategoryPDTO(items, totalCount);
    }
    public async Task<string> GetNextCategoryCode() => await categoryRepo.GetNextCategoryCodeAsync();
    #endregion

    #region SubSubCategory
    public async Task<List<SubCategoryMaster>> GetAllSubCategory() => await subCategoryRepo.GetAllAsync();
    public async Task<SubCategoryMaster> GetSubCategoryById(int id) => await subCategoryRepo.GetAsync(id);
    public async Task<OutputResponse> CreateSubCategory(SubCategoryMaster SubCategory) => await subCategoryRepo.CreateAsync(SubCategory);
    public async Task<OutputResponse> UpdateSubCategory(SubCategoryMaster SubCategory) => await subCategoryRepo.UpdateAsync(SubCategory);
    public async Task<OutputResponse> DeleteSubCategory(int id) => await subCategoryRepo.DeleteAsync(id);
    public async Task<SubCategoryPDTO> GetSubCategoryPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await subCategoryRepo.GetAllWithPagination(pagination);
        return new SubCategoryPDTO(items, totalCount);
    }
    public async Task<string> GetNextSubCategoryCode(int categoryId, string? categoryCode) => await subCategoryRepo.GetNextSubCategoryCodeAsync(categoryId, categoryCode);
    #endregion

    #region CustomerGroupMaster
    public async Task<List<CustomerGroupMaster>> GetAllCustomerGroup() => await customerGroupRepo.GetAllAsync();
    public async Task<CustomerGroupMaster> GetCustomerGroupById(int id) => await customerGroupRepo.GetAsync(id);
    public async Task<OutputResponse> CreateCustomerGroup(CustomerGroupMaster CustomerGroup) => await customerGroupRepo.CreateAsync(CustomerGroup);
    public async Task<OutputResponse> UpdateCustomerGroup(CustomerGroupMaster CustomerGroup) => await customerGroupRepo.UpdateAsync(CustomerGroup);
    public async Task<OutputResponse> DeleteCustomerGroup(int id) => await customerGroupRepo.DeleteAsync(id);
    public async Task<CustomerGroupPDTO> GetCustomerGroupPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await customerGroupRepo.GetAllWithPagination(pagination);
        return new CustomerGroupPDTO(items, totalCount);
    }
    #endregion

    #region CountryMaster
    public async Task<List<CountryMaster>> GetAllCountry() => await countryMasterRepo.GetAllAsync();
    public async Task<CountryMaster> GetCountryById(int id) => await countryMasterRepo.GetAsync(id);
    public async Task<OutputResponse> CreateCountry(CountryMaster Country) => await countryMasterRepo.CreateAsync(Country);
    public async Task<OutputResponse> UpdateCountry(CountryMaster Country) => await countryMasterRepo.UpdateAsync(Country);
    public async Task<OutputResponse> DeleteCountry(int id) => await countryMasterRepo.DeleteAsync(id);
    public async Task<CountryPDTO> GetCountryPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await countryMasterRepo.GetAllWithPagination(pagination);
        return new CountryPDTO(items, totalCount);
    }
    #endregion

    #region CustomerLocation
    public async Task<List<CustomerLocationMaster>> GetAllCustomerLocation() => await customerLocationRepo.GetAllAsync();
    public async Task<CustomerLocationMaster> GetCustomerLocationById(int id) => await customerLocationRepo.GetAsync(id);
    public async Task<OutputResponse> CreateCustomerLocation(CustomerLocationMaster CustomerLocation) => await customerLocationRepo.CreateAsync(CustomerLocation);
    public async Task<OutputResponse> UpdateCustomerLocation(CustomerLocationMaster CustomerLocation) => await customerLocationRepo.UpdateAsync(CustomerLocation);
    public async Task<OutputResponse> DeleteCustomerLocation(int id) => await customerLocationRepo.DeleteAsync(id);
    public async Task<CustomerLocationPDTO> GetCustomerLocationPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await customerLocationRepo.GetAllWithPagination(pagination);
        return new CustomerLocationPDTO(items, totalCount);
    }


    #endregion

    #region Vat
    public async Task<List<VatMaster>> GetAllVat() => await vatRepo.GetAllAsync();
    public async Task<VatMaster> GetVatById(int id) => await vatRepo.GetAsync(id);
    public async Task<OutputResponse> CreateVat(VatMaster vat) => await vatRepo.CreateAsync(vat);
    public async Task<OutputResponse> UpdateVat(VatMaster vat) => await vatRepo.UpdateAsync(vat);
    public async Task<OutputResponse> DeleteVat(int id) => await vatRepo.DeleteAsync(id);
    public async Task<List<VATCategoryDetails>> GetVATCategoryDetails(string code) => await vatRepo.GetVatCategoryDetails(code);
    public async Task<VatPDTO> GetVatPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await vatRepo.GetAllWithPagination(pagination);
        return new VatPDTO(items, totalCount);
    }
    #endregion

    #region Brand
    public async Task<List<BrandMaster>> GetAllBrand() => await brandRepo.GetAllAsync();
    public async Task<BrandMaster> GetBrandById(int id) => await brandRepo.GetAsync(id);
    public async Task<OutputResponse> CreateBrand(BrandMaster Brand) => await brandRepo.CreateAsync(Brand);
    public async Task<OutputResponse> UpdateBrand(BrandMaster Brand) => await brandRepo.UpdateAsync(Brand);
    public async Task<OutputResponse> DeleteBrand(int id) => await brandRepo.DeleteAsync(id);
    public async Task<BrandPDTO> GetBrandPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await brandRepo.GetAllWithPagination(pagination);
        return new BrandPDTO(items, totalCount);
    }
    #endregion

    #region Currency
    public async Task<List<CurrencyMaster>> GetAllCurrency() => await currencyRepo.GetAllAsync();
    public async Task<CurrencyMaster> GetCurrencyById(int id) => await currencyRepo.GetAsync(id);
    public async Task<OutputResponse> CreateCurrency(CurrencyMaster currency) => await currencyRepo.CreateAsync(currency);
    public async Task<OutputResponse> UpdateCurrency(CurrencyMaster currency) => await currencyRepo.UpdateAsync(currency);
    public async Task<OutputResponse> DeleteCurrency(int id) => await currencyRepo.DeleteAsync(id);
    public async Task<CurrencyPDTO> GetCurrencyPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await currencyRepo.GetAllWithPagination(pagination);
        return new CurrencyPDTO(items, totalCount);
    }
    #endregion

    #region Purchaser

    public async Task<List<PurchaserMaster>> GetAllPurchaser() => await purchaseRepo.GetAllAsync();
    public async Task<PurchaserMaster> GetPurchaserById(int id) => await purchaseRepo.GetAsync(id);
    public async Task<OutputResponse> CreatePurchaser(PurchaserMaster purchaser) => await purchaseRepo.CreateAsync(purchaser);
    public async Task<OutputResponse> UpdatePurchaser(PurchaserMaster purchaser) => await purchaseRepo.UpdateAsync(purchaser);
    public async Task<OutputResponse> DeletePurchaser(int id) => await purchaseRepo.DeleteAsync(id);
    public async Task<PurchaserPDTO> GetPurchaserPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await purchaseRepo.GetAllWithPagination(pagination);
        return new PurchaserPDTO(items, totalCount);
    }


    #endregion

    #region Project
    public async Task<List<ProjectMaster>> GetAllProject() => await projectRepo.GetAllAsync();
    public async Task<ProjectMaster> GetProjectById(int id) => await projectRepo.GetAsync(id);
    public async Task<OutputResponse> CreateProject(ProjectMaster project) => await projectRepo.CreateAsync(project);
    public async Task<OutputResponse> UpdateProject(ProjectMaster project) => await projectRepo.UpdateAsync(project);
    public async Task<OutputResponse> DeleteProject(int id) => await projectRepo.DeleteAsync(id);

    public async Task<ProjectPDTO> GetProjectPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await projectRepo.GetAllWithPagination(pagination);
        return new ProjectPDTO(items, totalCount);
    }

    #endregion

    #region Customer
    public async Task<List<CustomerMaster>> GetAllCustomers() => await customerMasterRepo.GetAllAsync();
    public async Task<CustomerMaster> GetCustomerById(int id) => await customerMasterRepo.GetAsync(id);
    public async Task<OutputResponse> CreateCustomer(CustomerMaster customer) => await customerMasterRepo.CreateAsync(customer);
    public async Task<OutputResponse> UpdateCustomer(CustomerMaster customer) => await customerMasterRepo.UpdateAsync(customer);
    public async Task<OutputResponse> DeleteCustomer(int id) => await customerMasterRepo.DeleteAsync(id);
    public async Task<CustomerPDTO> GetCustomerPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await customerMasterRepo.GetAllWithPagination(pagination);
        return new CustomerPDTO(items, totalCount);
    }
    public async Task<string> GetNextCustomerCode() => await customerMasterRepo.GetNextCustomerCodeAsync();
    #endregion

    #region Supplier
    public async Task<List<SupplierMaster>> GetAllSuppliers() => await supplierMasterRepo.GetAllAsync();
    public async Task<SupplierMaster> GetSupplierById(int id) => await supplierMasterRepo.GetAsync(id);
    public async Task<OutputResponse> CreateSupplier(SupplierMaster supplier) => await supplierMasterRepo.CreateAsync(supplier);
    public async Task<OutputResponse> UpdateSupplier(SupplierMaster supplier) => await supplierMasterRepo.UpdateAsync(supplier);
    public async Task<OutputResponse> DeleteSupplier(int id) => await supplierMasterRepo.DeleteAsync(id);
    public async Task<SupplierPDTO> GetSupplierPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await supplierMasterRepo.GetAllWithPagination(pagination);
        return new SupplierPDTO(items, totalCount);
    }
    public async Task<string> GetNextSupplierCode() => await supplierMasterRepo.GetNextSupplierCodeAsync();
    #endregion

    #region AdminUser
    public Task<List<AdminUser>> GetAllAdminUsers() => throw new NotImplementedException();
    public Task<AdminUser> GetAdminUserById(int id) => throw new NotImplementedException();
    public Task<AdminUser> CreateAdminUser([Body] AdminUser user) => throw new NotImplementedException();
    public Task<AdminUser> UpdateAdminUser([Body] AdminUser user) => throw new NotImplementedException();
    public Task<AdminUser> DeleteAdminUser(int id) => throw new NotImplementedException();
    #endregion

    #region Item
    public async Task<List<ItemMaster>> GetAllItems() => await itemRepo.GetAllAsync();
    public async Task<ItemMaster> GetItemById(int id) => await itemRepo.GetAsync(id);
    public async Task<OutputResponse> CreateItem(ItemMaster item) => await itemRepo.CreateAsync(item);
    public async Task<OutputResponse> UpdateItem(ItemMaster item) => await itemRepo.UpdateAsync(item);
    public async Task<OutputResponse> DeleteItem(int id) => await itemRepo.DeleteAsync(id);
    public async Task<ItemMasterPDTO> GetItemPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await itemRepo.GetAllWithPagination(pagination);
        return new ItemMasterPDTO(items, totalCount);
    }
    public async Task<ItemMultiUnit> GetItemMultiUnit(int itemId, int unitId, string barcode) => await itemRepo.GetItemMultiUnit(itemId, unitId, barcode);
    public async Task<ItemMultiUnitPDTO> GetItemMultiUnitPDetails(PaginationModel pagination) => await itemRepo.GetItemMultiUnitPDTO(pagination);
    public async Task<string> GetNextItemCode(int? categoryId, int? subCategoryId, string? catCode, string? subCatCode)
        => await itemRepo.GetNextItemCodeAsync(categoryId, subCategoryId, catCode, subCatCode);
    public async Task<ItemDialogPDTO> GetItemDialogDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await itemRepo.GetItemsForDialogWithStock(pagination);
        return new ItemDialogPDTO(items, totalCount);
    }
    public async Task<ItemMultiUnitDialogPDTO> GetMultiUnitDialogDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await itemRepo.GetMultiUnitItemsForDialogWithStock(pagination);
        return new ItemMultiUnitDialogPDTO(items, totalCount);
    }
    public async Task<List<ItemMultiUnit>> GetItemMultiUnits(int itemId) => await itemRepo.GetItemMultiUnits(itemId);
    public async Task<ItemStockInfoDTO> GetItemStockInfo(int itemId) => await itemRepo.GetItemStockInfoAsync(itemId);
    public async Task<string> GetUnitName(int unitId)
    {
        var unit = await unitRepo.GetAsync(unitId);
        return unit?.Name ?? "";
    }
    public async Task<string> GetNextQuotationCode() => await quotationRepo.GetNextQuotationNoAsync();
    public async Task<List<BarcodeData>> GetBarcodeItems(int? categoryId = null) => await itemRepo.GetBarcodeItemsAsync(categoryId);
    #endregion

    #region CustomerOPEntry
    public async Task<List<CustomerOPEntry>> GetAllCustomerOPEntry() => await customerOPEntryRepo.GetAllAsync();

    public async Task<CustomerOPEntryDTO> GetCustomerOPEntryById(int id) => await customerOPEntryRepo.GetFullAsync(id);

    public async Task<OutputResponse> CreateCustomerOPEntry(CustomerOPEntryDTO entry)
    {
        var master = new CustomerOPEntry
        {
            CustomerOPEntryId = entry.CustomerOPEntryId,
            DocNo = entry.DocNo,
            EntryDate = entry.EntryDate,
            EnteredBy = entry.EnteredBy,
            Remarks = entry.Remarks,
            FK_BranchId = entry.FK_BranchId,
            FK_UserId = entry.FK_UserId
        };
        return await customerOPEntryRepo.CreateAsync(master, entry.EntryDetails);
    }

    public async Task<OutputResponse> UpdateCustomerOPEntry(CustomerOPEntry value, List<CustomerOPEntryDetails> details) => await customerOPEntryRepo.UpdateAsync(value, details);
    public async Task<OutputResponse> DeleteCustomerOPEntry(int id) => await customerOPEntryRepo.DeleteAsync(id);

    public async Task<CustomerOpEntryPDTO> GetCustomerOPEntryPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await customerOPEntryRepo.GetAllWithPagination(pagination);
        return new CustomerOpEntryPDTO(items, totalCount);
    }
    public async Task<string> GetNewCustomerOPEntryDocNo() => await customerOPEntryRepo.GetNewDocNo();
    #endregion

    #region SupplierOPEntry
    public async Task<List<SupplierOPEntry>> GetAllSupplierOPEntry() => await supplierOPEntryRepo.GetAllAsync();

    public async Task<SupplierOPEntryDTO> GetSupplierOPEntryById(int id) => await supplierOPEntryRepo.GetFullAsync(id);

    public async Task<OutputResponse> CreateSupplierOPEntry(SupplierOPEntryDTO entry)
    {
        var master = new SupplierOPEntry
        {
            SupplierOPEntryId = entry.SupplierOPEntryId,
            DocNo = entry.DocNo,
            EntryDate = entry.EntryDate,
            EnteredBy = entry.EnteredBy,
            Remarks = entry.Remarks,
            FK_BranchId = entry.FK_BranchId,
            FK_UserId = entry.FK_UserId
        };

        if (master.SupplierOPEntryId > 0)
        {
            return await supplierOPEntryRepo.UpdateAsync(master, entry.EntryDetails);
        }
        else
        {
            return await supplierOPEntryRepo.CreateAsync(master, entry.EntryDetails);
        }
    }

    public async Task<OutputResponse> DeleteSupplierOPEntry(int id) => await supplierOPEntryRepo.DeleteAsync(id);

    public async Task<SupplierOpEntryPDTO> GetSupplierOPEntryPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await supplierOPEntryRepo.GetAllWithPagination(pagination);
        return new SupplierOpEntryPDTO(items, totalCount);
    }
    public async Task<string> GetNewSupplierOPEntryDocNo() => await supplierOPEntryRepo.GetNewDocNo();
    #endregion

    #region StoreMaster
    public async Task<List<StoreMaster>> GetAllStores() => await storeMasterRepo.GetAllAsync();
    public async Task<StoreMaster> GetStoreById(int id) => await storeMasterRepo.GetAsync(id);
    public async Task<OutputResponse> CreateStore(StoreMaster store) => await storeMasterRepo.CreateAsync(store);
    public async Task<OutputResponse> UpdateStore(StoreMaster store) => await storeMasterRepo.UpdateAsync(store);
    public async Task<OutputResponse> DeleteStore(int id) => await storeMasterRepo.DeleteAsync(id);
    public async Task<bool> CheckIfDefaultStoreExists(int excludeId = 0, int? branchId = null) => await storeMasterRepo.CheckIfDefaultStoreExists(excludeId, branchId);
    #endregion

    #region StockAdjustment
    public async Task<string> GetNextStockAdjustmentCode() => await stockAdjustmentRepo.GetVoucherNo(AppData.BranchId);
    public async Task<int> GetNewStockAdjustmentDocNo(int branchId) => await stockAdjustmentRepo.GetNextVchrNo(branchId);
    public async Task<StockAdjustmentHdr> GetStockAdjustmentById(int id) => await stockAdjustmentRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateStockAdjustment(StockAdjustmentHdr hdr) => await stockAdjustmentRepo.CreateAsync(hdr);
    public async Task<OutputResponse> UpdateStockAdjustment(StockAdjustmentHdr hdr) => await stockAdjustmentRepo.UpdateAsync(hdr);
    public async Task<OutputResponse> DeleteStockAdjustment(int id) => await stockAdjustmentRepo.DeleteStockAdjustment(id);
    public async Task<TransactionPDTO> GetStockAdjustmentPDetails(PaginationModel pagination) => await stockAdjustmentRepo.GetAllAsync(pagination);
    public async Task<decimal> GetItemCurrentStock(int itemId, int? storeId, int? excludeVoucherId = 0) => await stockAdjustmentRepo.GetItemCurrentStock(itemId, storeId, excludeVoucherId);
    #endregion

    #region CompanyDetails
    public async Task<CompanyDetails?> GetCompanyDetailsByBranch(int branchId) => await companyDetailsRepo.GetByBranchIdAsync(branchId);
    public async Task<bool> CheckCompanyDetailsExists(int branchId) => await companyDetailsRepo.ExistsForBranchAsync(branchId);
    public async Task<OutputResponse> CreateCompanyDetails(CompanyDetails details) => await companyDetailsRepo.CreateAsync(details);
    public async Task<OutputResponse> UpdateCompanyDetails(CompanyDetails details) => await companyDetailsRepo.UpdateAsync(details);
    #endregion

    #region PermanentSettings
    public async Task<List<PermanentSettings>> GetAllPermanentSettings() => await permanentSettingsRepo.GetAllSettings();
    public async Task<OutputResponse> UpdatePermanentSettings(List<PermanentSettings> settings) => await permanentSettingsRepo.UpdateSettings(settings);
    #endregion


    #region VehicleMaster
    public async Task<List<VehicleMaster>> GetAllVehicles() => await vehicleRepo.GetAllAsync();
    public async Task<VehicleMaster> GetVehicleById(int id) => await vehicleRepo.GetAsync(id);
    public async Task<OutputResponse> CreateVehicle(VehicleMaster vehicle) => await vehicleRepo.CreateAsync(vehicle);
    public async Task<OutputResponse> UpdateVehicle(VehicleMaster vehicle) => await vehicleRepo.UpdateAsync(vehicle);
    public async Task<OutputResponse> DeleteVehicle(int id) => await vehicleRepo.DeleteAsync(id);
    public async Task<VehiclePDTO> GetVehiclePDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await vehicleRepo.GetAllWithPagination(pagination);
        return new VehiclePDTO(items, totalCount);
    }
    #endregion

    #region RepairMaster
    public async Task<List<RepairMaster>> GetAllRepairs() => await repairRepo.GetAllAsync();
    public async Task<RepairMaster> GetRepairById(int id) => await repairRepo.GetAsync(id);
    public async Task<OutputResponse> CreateRepair(RepairMaster repair) => await repairRepo.CreateAsync(repair);
    public async Task<OutputResponse> UpdateRepair(RepairMaster repair) => await repairRepo.UpdateAsync(repair);
    public async Task<OutputResponse> DeleteRepair(int id) => await repairRepo.DeleteAsync(id);
    public async Task<RepairPDTO> GetRepairPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await repairRepo.GetAllWithPagination(pagination);
        return new RepairPDTO(items, totalCount);
    }
    #endregion

    #region TechnicianMaster
    public async Task<List<TechnicianMaster>> GetAllTechnicians() => await technicianRepo.GetAllAsync();
    public async Task<TechnicianMaster> GetTechnicianById(int id) => await technicianRepo.GetAsync(id);
    public async Task<OutputResponse> CreateTechnician(TechnicianMaster technician) => await technicianRepo.CreateAsync(technician);
    public async Task<OutputResponse> UpdateTechnician(TechnicianMaster technician) => await technicianRepo.UpdateAsync(technician);
    public async Task<OutputResponse> DeleteTechnician(int id) => await technicianRepo.DeleteAsync(id);
    public async Task<TechnicianPDTO> GetTechnicianPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await technicianRepo.GetAllWithPagination(pagination);
        return new TechnicianPDTO(items, totalCount);
    }
    #endregion

    #endregion

    #region User Management
    public async Task<List<UserDetails>> GetAllUsers() => await userRepo.GetAllAsync();
    public async Task<UserDetails> GetUserById(int id) => await userRepo.GetAsync(id);
    public async Task<OutputResponse> CreateUser(UserDetails user) => await userRepo.CreateAsync(user);
    public async Task<OutputResponse> UpdateUser(UserDetails user) => await userRepo.UpdateAsync(user);
    public async Task<OutputResponse> DeleteUser(int id) => await userRepo.DeleteAsync(id);
    public async Task<List<BranchMaster>> GetUserBranches(string userName) => await userRepo.GetBranchesForUser(userName);
    public async Task<UserPDTO> GetUserPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await userRepo.GetAllWithPagination(pagination);
        return new UserPDTO(items, totalCount);
    }
    public async Task<OutputResponse> SaveUserBranches(List<UserBranch> userBranches) => await userRepo.SaveUserBranches(userBranches);
    #endregion

    #region Admin
    public Task<List<LoginCompany>> GetAllAdminCompanies() => throw new NotImplementedException();
    public Task<LoginCompanyPDTO> GetPAdminCompanies(PaginationModel pagination) => throw new NotImplementedException();
    public Task<LoginCompany> GetAdminCompanyById(int id) => throw new NotImplementedException();
    public Task<LoginCompany> CreateAdminCompany([Body] ExLCompany company) => throw new NotImplementedException();
    public Task<LoginCompany> UpdateAdminCompany([Body] ExLCompany company) => throw new NotImplementedException();
    public Task<LoginCompany> DeleteAdminCompany(int id) => throw new NotImplementedException();
    #endregion

    #region Workshop - JobOrder
    public async Task<string> GetNextJobOrderCode(int branchId) => await jobOrderRepo.GetVoucherNo(branchId);
    public async Task<int> GetNewJobOrderDocNo(int branchId) => await jobOrderRepo.GetNextVchrNo(branchId);
    public async Task<JobOrderHdr> GetJobOrderById(int id) => await jobOrderRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateJobOrder(JobOrderHdr hdr) => await jobOrderRepo.CreateAsync(hdr);
    public async Task<OutputResponse> UpdateJobOrder(JobOrderHdr hdr) => await jobOrderRepo.UpdateAsync(hdr);
    public async Task<OutputResponse> DeleteJobOrder(int id) => await jobOrderRepo.DeleteAsync(id);
    public async Task<TransactionPDTO> GetJobOrderPDetails(PaginationModel pagination) => await jobOrderRepo.GetAllWithPagination(pagination);
    #endregion

    #region Workshop - JobCard
    public async Task<string> GetNextJobCardCode(int branchId) => await jobCardRepo.GetVoucherNo(branchId);
    public async Task<int> GetNewJobCardDocNo(int branchId) => await jobCardRepo.GetNextVchrNo(branchId);
    public async Task<JobCardHdr> GetJobCardById(int id) => await jobCardRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateJobCard(JobCardHdr hdr) => await jobCardRepo.CreateAsync(hdr);
    public async Task<OutputResponse> UpdateJobCard(JobCardHdr hdr) => await jobCardRepo.UpdateAsync(hdr);
    public async Task<OutputResponse> DeleteJobCard(int id) => await jobCardRepo.DeleteAsync(id);
    public async Task<TransactionPDTO> GetJobCardPDetails(PaginationModel pagination) => await jobCardRepo.GetAllWithPagination(pagination);
    public async Task<List<JobCardHistoryDTO>> GetJobCardHistory(string plateNo) => await jobCardRepo.GetJobCardHistory(plateNo);
    #endregion

    #region General
    public async Task<ChartEntity> GetChartValues(int branchId, string format) => await generalRepo.GetChartValues(branchId, format);
    public async Task<string> GetArabicText(string sourceText)
    {
        GoogleTrans gtrans = new();
        // Use the injected 'http' client
        var result = await gtrans.Translate(http, sourceText);
        return result;
    }
    public async Task<List<CommonModel>> GetCombofill(string head, int BranchId) => await generalRepo.GetComboFill(head, BranchId);
    public async Task<List<ExMasters>> GetMasters(string type, int BranchId) => await generalRepo.GetMasters(type, BranchId);
    public async Task<decimal> GetValueMethod(string head, int val) => await generalRepo.GetValueMethod(head, val);


    #region Image Handling
    public async Task<string> UploadImage(byte[] imageBytes, string fileName, string entityType, int entityId, string basePath, string clientFolder)
    {
        using var stream = new MemoryStream(imageBytes);
        return await commonImageRepo.UploadImageAsync(stream, fileName, entityType, entityId, basePath, clientFolder);
    }

    public async Task<string> GetImageUrl(string basePath, string imagePath)
    {
        // Sanitize inputs
        basePath = basePath?.Trim('"').Replace("\\", "/") ?? "";
        imagePath = imagePath?.Trim('"').Replace("\\", "/") ?? "";

        // For ViewServiceMSSql (direct repo access), we need to get settings from PermanentSettingsData
        // since this is used in MAUI/Desktop apps where settings are already loaded
        var mode = PermanentSettingsData.ImageStorageMode.ToString();
        string? region = null, bucket = null, accessKey = null, secretKey = null;

        if (PermanentSettingsData.ImageStorageMode == 1) // AWS S3
        {
            region = PermanentSettingsData.ImageAwsRegion;
            bucket = PermanentSettingsData.ImageAwsBucket;
            accessKey = PermanentSettingsData.ImageAwsAccessKey;
            secretKey = PermanentSettingsData.ImageAwsSecretKey;
        }

        // Use Stream instead of Path to support both Local and AWS
        using var stream = await commonImageRepo.GetImageStreamAsync(basePath, imagePath, mode, region, bucket, accessKey, secretKey);

        if (stream == null)
        {
            // Fallback for placeholder
            return "_content/ZakiSAASERP.Shared/images/default.png";
        }

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var extension = Path.GetExtension(imagePath).TrimStart('.').ToLower();
        if (string.IsNullOrEmpty(extension)) extension = "png";

        return $"data:image/{extension};base64,{Convert.ToBase64String(bytes)}";
    }

    public async Task<bool> DeleteImage(string basePath, string imagePath, string clientFolder)
    {
        // For ViewServiceMSSql (direct repo access), we can get mode and AWS settings from PermanentSettingsData
        // since this is used in MAUI/Desktop apps where settings are already loaded
        var mode = PermanentSettingsData.ImageStorageMode.ToString();
        string? region = null, bucket = null, accessKey = null, secretKey = null;

        if (PermanentSettingsData.ImageStorageMode == 1) // AWS S3
        {
            region = PermanentSettingsData.ImageAwsRegion;
            bucket = PermanentSettingsData.ImageAwsBucket;
            accessKey = PermanentSettingsData.ImageAwsAccessKey;
            secretKey = PermanentSettingsData.ImageAwsSecretKey;
        }

        return await commonImageRepo.DeleteImageAsync(basePath, imagePath, clientFolder, mode, region, bucket, accessKey, secretKey);
    }
    #endregion
    #endregion

    #region Transactions

    #region Sales

    #region Sales Quotation
    public async Task<TransactionPDTO> GetSalesQuotationPDetails(PaginationModel pagination) => await quotationRepo.GetAllWithPagination(pagination);
    public async Task<OutputResponse> CreateSalesQuotation(SalesQuotationDTO quotation) => await quotationRepo.CreateAsync(quotation);
    public async Task<OutputResponse> DeleteSalesQuotation(int id) => await quotationRepo.DeleteAsync(id);
    public async Task<SalesQuotationDTO> GetSalesQuotationById(int id) => await quotationRepo.GetAsync(id);
    public async Task<OutputResponse> UpdateSalesQuotation(SalesQuotationDTO quotation) => await quotationRepo.UpdateAsync(quotation);
    public async Task<int> GetNewSalesQuotationDocNo(int BranchId) => await quotationRepo.GetNextVchrNo(BranchId);
    // public async Task<string> GetNextQuotationCode() => await quotationRepo.GetNextQuotationNoAsync();
    #endregion

    #region Sales Invoice
    public async Task<TransactionPDTO> GetSalesInvoicePDetails(PaginationModel pagination) => await invoiceRepo.GetAllWithPagination(pagination);
    public async Task<OutputResponse> CreateSalesInvoice(SalesInvoiceDTO invoice) => await invoiceRepo.CreateAsync(invoice);
    public async Task<OutputResponse> DeleteSalesInvoice(int id) => await invoiceRepo.DeleteAsync(id);
    public async Task<SalesInvoiceDTO> GetSalesInvoiceById(int id) => await invoiceRepo.GetAsync(id);
    public async Task<OutputResponse> UpdateSalesInvoice(SalesInvoiceDTO invoice) => await invoiceRepo.UpdateAsync(invoice);
    public async Task<int> GetNewSalesInvoiceDocNo(int BranchId) => await invoiceRepo.GetNextVchrNo(BranchId);
    public async Task<string> GetNextSalesInvoiceCode() => await invoiceRepo.GetNextInvoiceNoAsync();

    #region Sales Invoice Hold
    public async Task<OutputResponse> CreateSalesInvoiceHold(SalesInvoiceHoldDTO invoiceHold) => await invoiceHoldRepo.CreateAsync(invoiceHold);
    public async Task<SalesInvoiceHoldDTO> GetSalesInvoiceHoldById(int id) => await invoiceHoldRepo.GetAsync(id);
    public async Task<OutputResponse> DeleteSalesInvoiceHold(int id) => await invoiceHoldRepo.DeleteAsync(id);
    public async Task<OutputResponse> DeleteAllSalesInvoiceHold(int branchId, string? terminal = null) => await invoiceHoldRepo.DeleteAllAsync(branchId, terminal);
    public async Task<List<SalesInvoiceHoldHdr>> GetAllSalesInvoiceHold(int branchId, string? terminal = null) => await invoiceHoldRepo.GetAllHeldAsync(branchId, terminal);
    public async Task<TransactionPDTO> GetSalesInvoiceHoldPDetails(PaginationModel pagination) => await invoiceHoldRepo.GetAllWithPagination(pagination);
    #endregion
    #endregion

    #region Sales Invoice Return
    public async Task<TransactionPDTO> GetSalesInvoiceRtnPDetails(PaginationModel pagination) => await salesInvoiceRtnRepo.GetAllWithPagination(pagination);
    public async Task<OutputResponse> CreateSalesInvoiceRtn(SalesInvoiceRtnDTO invoiceRtn) => await salesInvoiceRtnRepo.CreateAsync(invoiceRtn);
    public async Task<OutputResponse> DeleteSalesInvoiceRtn(int id) => await salesInvoiceRtnRepo.DeleteAsync(id);
    public async Task<SalesInvoiceRtnDTO> GetSalesInvoiceRtnById(int id) => await salesInvoiceRtnRepo.GetAsync(id);
    public async Task<OutputResponse> UpdateSalesInvoiceRtn(SalesInvoiceRtnDTO invoiceRtn) => await salesInvoiceRtnRepo.UpdateAsync(invoiceRtn);
    public async Task<int> GetNewSalesInvoiceRtnDocNo(int BranchId) => await salesInvoiceRtnRepo.GetNextVchrNo(BranchId);
    public async Task<string> GetNextSalesInvoiceRtnCode() => await salesInvoiceRtnRepo.GetNextSalesInvoiceRtnNoAsync();
    #endregion

    #region Proforma Invoice
    public async Task<TransactionPDTO> GetPProformaInvoices(PaginationModel pagination) => await proformaInvoiceRepo.GetAllWithPagination(pagination);
    public async Task<OutputResponse> CreateProformaInvoice(ProformaInvoiceDTO proformaInvoice) => await proformaInvoiceRepo.CreateAsync(proformaInvoice);
    public async Task<OutputResponse> DeleteProformaInvoice(int id) => await proformaInvoiceRepo.DeleteAsync(id);
    public async Task<ProformaInvoiceDTO> GetProformaInvoiceById(int id) => await proformaInvoiceRepo.GetAsync(id);
    public async Task<OutputResponse> UpdateProformaInvoice(ProformaInvoiceDTO proformaInvoice) => await proformaInvoiceRepo.UpdateAsync(proformaInvoice);
    public async Task<string> GetNewProformaInvoiceNo(int BranchId) => (await proformaInvoiceRepo.GetNextVchrNo(BranchId)).ToString();
    public async Task<string> GetNextProformaInvoiceCode() => await proformaInvoiceRepo.GetNextProformaInvoiceNoAsync();
    public async Task<ProformaInvoiceDTO> CreateProformaFromQuotationAsync(int quotationId) => await proformaInvoiceRepo.CreateFromQuotationAsync(quotationId);
    #endregion

    #region Sales Order
    public async Task<TransactionPDTO> GetSalesOrderPDetails(PaginationModel pagination) => await orderRepo.GetAllWithPagination(pagination);
    public async Task<OutputResponse> CreateSalesOrder(SalesOrderDTO order) => await orderRepo.CreateAsync(order);
    public async Task<OutputResponse> DeleteSalesOrder(int id) => await orderRepo.DeleteAsync(id);
    public async Task<SalesOrderDTO> GetSalesOrderById(int id) => await orderRepo.GetAsync(id);
    public async Task<OutputResponse> UpdateSalesOrder(SalesOrderDTO order) => await orderRepo.UpdateAsync(order);
    public async Task<int> GetNewSalesOrderDocNo(int BranchId) => await orderRepo.GetNextVchrNo(BranchId);
    public async Task<string> GetNextSalesOrderCode() => await orderRepo.GetNextSalesOrderNoAsync();
    #endregion

    #region Delivery Note
    public async Task<TransactionPDTO> GetDeliveryNotePDetails(PaginationModel pagination) => await deliveryNoteRepo.GetAllWithPagination(pagination);
    public async Task<OutputResponse> CreateDeliveryNote(DeliveryNoteDTO deliveryNote) => await deliveryNoteRepo.CreateAsync(deliveryNote);
    public async Task<OutputResponse> DeleteDeliveryNote(int id) => await deliveryNoteRepo.DeleteAsync(id);
    public async Task<DeliveryNoteDTO> GetDeliveryNoteById(int id) => await deliveryNoteRepo.GetAsync(id);
    public async Task<OutputResponse> UpdateDeliveryNote(DeliveryNoteDTO deliveryNote) => await deliveryNoteRepo.UpdateAsync(deliveryNote);
    public async Task<int> GetNewDeliveryNoteDocNo(int BranchId) => await deliveryNoteRepo.GetNextVchrNo(BranchId);
    public async Task<string> GetNextDeliveryNoteCode() => await deliveryNoteRepo.GetNextDeliveryNoteNoAsync();
    public async Task<List<PendingDeliveryNoteDTO>> GetPendingDeliveryNotes(int customerId) => await deliveryNoteRepo.GetPendingDeliveryNotes(customerId);
    #endregion

    #region Delivery Note Return
    public async Task<TransactionPDTO> GetDeliveryNoteRtnPDetails(PaginationModel pagination) => await deliveryNoteRtnRepo.GetAllWithPagination(pagination);
    public async Task<OutputResponse> CreateDeliveryNoteRtn(DeliveryNoteRtnDTO deliveryNoteRtn) => await deliveryNoteRtnRepo.CreateAsync(deliveryNoteRtn);
    public async Task<OutputResponse> DeleteDeliveryNoteRtn(int id) => await deliveryNoteRtnRepo.DeleteAsync(id);
    public async Task<DeliveryNoteRtnDTO> GetDeliveryNoteRtnById(int id) => await deliveryNoteRtnRepo.GetAsync(id);
    public async Task<OutputResponse> UpdateDeliveryNoteRtn(DeliveryNoteRtnDTO deliveryNoteRtn) => await deliveryNoteRtnRepo.UpdateAsync(deliveryNoteRtn);
    public async Task<int> GetNewDeliveryNoteRtnDocNo(int BranchId) => await deliveryNoteRtnRepo.GetNextVchrNo(BranchId);
    #endregion

    #endregion

    #region Purchase

    #region Enquiry
    public async Task<string> GetNextEnquiryNo(int branchId) => await enquiryRepo.GetNextEnquiryNoAsync("", branchId);
    #region InternalMaterialTransfer
    public async Task<string> GetNextInternalMaterialTransferCode() => await internalMaterialTransferRepo.GetVoucherNo(AppData.BranchId);
    public async Task<int> GetNewInternalMaterialTransferNo(int branchId) => await internalMaterialTransferRepo.GetNextVchrNo(branchId);
    public async Task<InternalMaterialTransferHdr> GetInternalMaterialTransferById(int id) => await internalMaterialTransferRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateInternalMaterialTransfer(InternalMaterialTransferHdr hdr) => await internalMaterialTransferRepo.CreateAsync(hdr);
    public async Task<OutputResponse> UpdateInternalMaterialTransfer(InternalMaterialTransferHdr hdr) => await internalMaterialTransferRepo.UpdateAsync(hdr);
    public async Task<OutputResponse> DeleteInternalMaterialTransfer(int id) => await internalMaterialTransferRepo.DeleteAsync(id);
    public async Task<TransactionPDTO> GetInternalMaterialTransferPDetails(PaginationModel pmodel) => await internalMaterialTransferRepo.GetAllAsync(pmodel);
    #endregion

    public async Task<OutputResponse> CreateEnquiry(RFQHdr enquiry)
    {
        var msg = await enquiryRepo.SaveAsync(enquiry);
        return new OutputResponse(0, msg);
    }
    public async Task<OutputResponse> UpdateEnquiry(RFQHdr enquiry)
    {
        var msg = await enquiryRepo.UpdateAsync(enquiry);
        return new OutputResponse(0, msg);
    }
    public async Task<OutputResponse> DeleteEnquiry(int id)
    {
        var res = await enquiryRepo.DeleteAsync(id);
        return new OutputResponse(0, res ? "Deleted Successfully" : "Failed to Delete");
    }
    public async Task<RFQHdr> GetEnquiryById(int id) => await enquiryRepo.GetByIdAsync(id);
    public async Task<List<RFQHdr>> GetAllEnquiries() => await enquiryRepo.GetAllAsync("");
    #endregion

    #region Purchase Invoice
    public async Task<PurchaseDTO> GetPurchaseById(int id) => await purchaseInvoiceRepo.GetAsync(id);
    public async Task<OutputResponse> CreatePurchase(PurchaseDTO purchase) => await purchaseInvoiceRepo.CreateAsync(purchase);
    public async Task<TransactionPDTO> GetPurchasePDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await purchaseInvoiceRepo.GetAllWithPagination(pagination);
        return new TransactionPDTO(items, totalCount);
    }
    public async Task<string> GetNextPurchaseNo() => await purchaseInvoiceRepo.GetNextPurchaseNoAsync();
    public async Task<OutputResponse> DeletePurchase(int id) => await purchaseInvoiceRepo.DeleteAsync(id);
    public async Task<OutputResponse> UpdatePurchase(PurchaseDTO purchase) => await purchaseInvoiceRepo.UpdateAsync(purchase);
    public async Task<int> GetPurchaseNextVchrNo(int branchId) => await purchaseInvoiceRepo.GetNextVchrNo(branchId);
    #endregion

    public async Task<TransactionPDTO> GetPurchaseOrderPDetails(PaginationModel pagination) => await purchaseOrderRepo.GetAllWithPagination(MapPagination(pagination));
    public async Task<OutputResponse> CreatePurchaseOrder(PurchaseOrderDTO purchaseOrder) => await purchaseOrderRepo.CreateAsync(purchaseOrder);
    public async Task<OutputResponse> DeletePurchaseOrder(int id) => await purchaseOrderRepo.DeleteAsync(id);
    public async Task<PurchaseOrderDTO> GetPurchaseOrderById(int id) => await purchaseOrderRepo.GetAsync(id);
    public async Task<OutputResponse> UpdatePurchaseOrder(PurchaseOrderDTO purchaseOrder) => await purchaseOrderRepo.UpdateAsync(purchaseOrder);
    public async Task<int> GetNewPurchaseOrderDocNo(int BranchId) => await purchaseOrderRepo.GetNextVchrNo(BranchId);
    public async Task<string> GetNextPurchaseOrderCode() => await purchaseOrderRepo.GetNextPurchaseOrderNoAsync();

    public async Task<TransactionPDTO> GetGoodsReceiptPDetails(PaginationModel pagination) => await goodsReceiptRepo.GetAllWithPagination(MapPagination(pagination));
    public async Task<OutputResponse> CreateGoodsReceipt(GoodsReceiptDTO goodsReceipt)
    {
        var res = await goodsReceiptRepo.Create(goodsReceipt);
        return new OutputResponse(res.Id, res.Message);
    }
    public async Task<OutputResponse> DeleteGoodsReceipt(int id)
    {
        var res = await goodsReceiptRepo.Delete(id);
        return new OutputResponse(res.Id, res.Message);
    }
    public async Task<GoodsReceiptDTO> GetGoodsReceiptById(int id) => await goodsReceiptRepo.GetById(id);
    public async Task<OutputResponse> UpdateGoodsReceipt(GoodsReceiptDTO goodsReceipt)
    {
        var res = await goodsReceiptRepo.Update(goodsReceipt);
        return new OutputResponse(res.Id, res.Message);
    }
    public async Task<string> GetNewGoodsReceiptDocNo(int BranchId) => await goodsReceiptRepo.GetNewDocNo(BranchId);
    public async Task<string> GetNextGoodsReceiptNo() => await goodsReceiptRepo.GetNextGoodsReceiptNoAsync();
    //public async Task<string> GetNewCustomerReceiptDocNo(int BranchId) => await customerReceiptRepo.GetNewDocNo(BranchId);
    //public async Task<int> GetNewCustomerReceiptVoucherNo(int BranchId) => await customerReceiptRepo.GetNextVchrNo(BranchId);

    #endregion

    #region Material Transfer Request
    public async Task<string> GetNextMaterialTransferRequestCode() => await materialTransferRequestRepo.GetVoucherNo(AppData.BranchId);
    public async Task<int> GetNewMaterialTransferRequestNo(int branchId) => await materialTransferRequestRepo.GetNextVchrNo(branchId);
    public async Task<MaterialTransferRequestHdr> GetMaterialTransferRequestById(int id) => await materialTransferRequestRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateMaterialTransferRequest(MaterialTransferRequestHdr hdr) => await materialTransferRequestRepo.CreateAsync(hdr);
    public async Task<OutputResponse> UpdateMaterialTransferRequest(MaterialTransferRequestHdr hdr) => await materialTransferRequestRepo.UpdateAsync(hdr);
    public async Task<OutputResponse> DeleteMaterialTransferRequest(int id) => await materialTransferRequestRepo.DeleteMaterialTransferRequest(id);
    public async Task<TransactionPDTO> GetMaterialTransferRequestPDetails(PaginationModel pagination) => await materialTransferRequestRepo.GetAllAsync(pagination);

    // Approval Methods
    public async Task<List<MaterialTransferRequestHdr>> GetMaterialTransferApprovals(MTRApprovalFilter filter) => await materialTransferRequestRepo.GetApprovalList(filter);
    public async Task<OutputResponse> ApproveMaterialTransferRequest(int id, int userId, List<MaterialTransferRequestDtls> details) => await materialTransferRequestRepo.ApproveMaterialTransferRequest(id, userId, details);
    public async Task<OutputResponse> RejectMaterialTransferRequest(int id, int userId, string? rejectionReason) => await materialTransferRequestRepo.RejectMaterialTransferRequest(id, userId, rejectionReason);
    #endregion

    #region Damage Stock
    public async Task<DamageStockDTO> GetDamageStockById(int id) => await damageStockRepo.GetById(id);
    public async Task<OutputResponse> CreateDamageStock(DamageStockDTO damageStock)
    {
        var res = await damageStockRepo.Create(damageStock);
        return new OutputResponse(res.Id, res.Message);
    }
    public async Task<TransactionPDTO> GetDamageStockPDetails(PaginationModel pagination) => await damageStockRepo.GetAllWithPagination(MapPagination(pagination));
    public async Task<string> GetNewDamageStockDocNo(int BranchId) => await damageStockRepo.GetNextVoucherNo(BranchId);
    public async Task<OutputResponse> DeleteDamageStock(int id)
    {
        var res = await damageStockRepo.Delete(id);
        return new OutputResponse(res.Id, res.Message);
    }
    public async Task<OutputResponse> UpdateDamageStock(DamageStockDTO damageStock)
    {
        var res = await damageStockRepo.Update(damageStock);
        return new OutputResponse(res.Id, res.Message);
    }
    #endregion



    #region Material Transfer IN
    public async Task<string> GetNextMaterialTransferInCode() => await materialTransferInRepo.GetVoucherNo(AppData.BranchId);
    public async Task<int> GetNewMaterialTransferInNo(int branchId) => await materialTransferInRepo.GetNextVchrNo(branchId);
    public async Task<MaterialTransferInHdr> GetMaterialTransferInById(int id) => await materialTransferInRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateMaterialTransferIn(MaterialTransferInHdr hdr) => await materialTransferInRepo.CreateAsync(hdr);
    public async Task<OutputResponse> UpdateMaterialTransferIn(MaterialTransferInHdr hdr) => await materialTransferInRepo.UpdateAsync(hdr);
    public async Task<OutputResponse> DeleteMaterialTransferIn(int id) => await materialTransferInRepo.DeleteMaterialTransferIn(id);
    public async Task<TransactionPDTO> GetMaterialTransferInPDetails(PaginationModel pagination) => await materialTransferInRepo.GetAllAsync(pagination);
    #endregion

    #region Material Transfer OUT
    public async Task<string> GetNextMaterialTransferOutCode() => await materialTransferOutRepo.GetVoucherNo(AppData.BranchId);
    public async Task<int> GetNewMaterialTransferOutNo(int branchId) => await materialTransferOutRepo.GetNextVchrNo(branchId);
    public async Task<MaterialTransferOutHdr> GetMaterialTransferOutById(int id) => await materialTransferOutRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateMaterialTransferOut(MaterialTransferOutHdr hdr) => await materialTransferOutRepo.CreateAsync(hdr);
    public async Task<OutputResponse> UpdateMaterialTransferOut(MaterialTransferOutHdr hdr) => await materialTransferOutRepo.UpdateAsync(hdr);
    public async Task<OutputResponse> DeleteMaterialTransferOut(int id) => await materialTransferOutRepo.DeleteMaterialTransferOut(id);
    public async Task<TransactionPDTO> GetMaterialTransferOutPDetails(PaginationModel pagination) => await materialTransferOutRepo.GetAllAsync(pagination);
    #endregion

    #region Purchase Return
    public async Task<string> GetNextPurchaseReturnCode() => await purchaseReturnRepo.GetNextPurchaseReturnNoAsync();
    public async Task<TransactionPDTO> GetPurchaseReturnPDetails(PaginationModel pagination) => await purchaseReturnRepo.GetAllWithPagination(pagination);
    public async Task<PurchaseReturnDTO> GetPurchaseReturnById(int id) => await purchaseReturnRepo.GetAsync(id);
    public async Task<OutputResponse> CreatePurchaseReturn(PurchaseReturnDTO entity) => await purchaseReturnRepo.CreateAsync(entity);
    public async Task<OutputResponse> UpdatePurchaseReturn(PurchaseReturnDTO entity) => await purchaseReturnRepo.UpdateAsync(entity);
    public async Task<OutputResponse> DeletePurchaseReturn(int id) => await purchaseReturnRepo.DeleteAsync(id);
    public async Task<int> GetNewPurchaseReturnDocNo(int branchId) => await purchaseReturnRepo.GetNextVchrNo(branchId);
    public async Task<List<TransactionDtls>> GetPurchaseReturnBalance(int purchaseId) => await purchaseReturnRepo.GetPurchaseReturnBalance(purchaseId);
    #endregion

    #region Supplier Payment
    public async Task<SupplierPaymentPDTO> GetSupplierPaymentPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await supplierPaymentRepo.GetAllWithPagination(pagination);
        return new SupplierPaymentPDTO(items, totalCount);
    }
    public async Task<SupplierPaymentDTO> GetSupplierPaymentById(int id) => await supplierPaymentRepo.GetAsync(id);
    public async Task<OutputResponse> CreateSupplierPayment(SupplierPaymentDTO entity) => await supplierPaymentRepo.CreateAsync(entity);
    public async Task<OutputResponse> UpdateSupplierPayment(SupplierPaymentDTO entity) => await supplierPaymentRepo.UpdateAsync(entity);
    public async Task<OutputResponse> DeleteSupplierPayment(int id) => await supplierPaymentRepo.DeleteAsync(id);
    public async Task<string> GetNewSupplierPaymentNo(int branchId) => await supplierPaymentRepo.GetNextVoucherNoAsync(branchId);
    public async Task<List<SupplierPaymentDtls>> GetPendingPurchaseInvoices(int supplierId) => await supplierPaymentRepo.GetPendingPurchaseInvoices(supplierId);
    #endregion



    #endregion

    #region Accounts
    #region Expense Entry
    public async Task<List<ExpenseEntryHdr>> GetAllExpenseEntries() => await expenseEntryRepo.GetAllAsync();
    public async Task<ExpenseEntryDTO> GetExpenseEntryById(int id) => await expenseEntryRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateExpenseEntry(ExpenseEntryDTO entry) => await expenseEntryRepo.CreateAsync(entry);
    public async Task<OutputResponse> UpdateExpenseEntry(ExpenseEntryDTO entry) => await expenseEntryRepo.UpdateAsync(entry);
    public async Task<OutputResponse> DeleteExpenseEntry(int id) => await expenseEntryRepo.DeleteAsync(id);
    public async Task<string> GetNextExpenseNo() => await expenseEntryRepo.GetNextExpenseNoAsync();
    public async Task<TransactionPDTO> GetExpenseEntryPDetails(PaginationModel pagination)
    {
        var data = await expenseEntryRepo.GetAllWithPagination(pagination);
        var transactions = data.Item1.Select(e => new ExMasters
        {
            Id = e.ExpenseEntryId,
            Code = e.ExpenseNo,
            IsSelected = true,
            Amount = e.NetTotal,
            Description = e.EntryDate.ToString("dd-MM-yyyy")
        }).ToList();
        return new TransactionPDTO(transactions, data.Item2);
    }
    #endregion

    #region Journal Entry
    public async Task<List<Models.Transactions.Accounts.JournalEntryHdr>> GetAllJournalEntries() => await journalEntryRepo.GetAllAsync();
    public async Task<Models.Transactions.Accounts.JournalEntryDTO> GetJournalEntryById(int id) => await journalEntryRepo.GetByIdAsync(id);
    public async Task<OutputResponse> CreateJournalEntry(Models.Transactions.Accounts.JournalEntryDTO entry) => await journalEntryRepo.CreateAsync(entry);
    public async Task<OutputResponse> UpdateJournalEntry(Models.Transactions.Accounts.JournalEntryDTO entry) => await journalEntryRepo.UpdateAsync(entry);
    public async Task<OutputResponse> DeleteJournalEntry(int id) => await journalEntryRepo.DeleteAsync(id);
    public async Task<string> GetNextJournalNo() => await journalEntryRepo.GetNextJournalNoAsync();
    #endregion

    #region Account Opening Entry
    public async Task<List<AccountOpeningEntryHdr>> GetAllAccountOpeningEntry() => await accountOpeningEntryRepo.GetAllAsync();
    public async Task<AccountOpeningEntryDTO> GetAccountOpeningEntryById(int id) => await accountOpeningEntryRepo.GetFullAsync(id);
    public async Task<TransactionPDTO> GetAccountOpeningEntryPDetails(PaginationModel pagination)
    {
        var data = await accountOpeningEntryRepo.GetAllWithPagination(pagination);
        var transactions = data.Item1.Select(e => new ExMasters
        {
            Id = e.AccountOpeningEntryId,
            Code = e.VoucherNo,
            IsSelected = true,
            Amount = e.DebitTotal ?? 0,
            Description = e.EntryDate?.ToString("dd-MM-yyyy") ?? ""
        }).ToList();
        return new TransactionPDTO(transactions, data.Item2);
    }
    public async Task<string> GetAccountOpeningEntryNewDocNo(int branchId) => await accountOpeningEntryRepo.GetNewDocNo(branchId);
    public async Task<AccountOpeningEntryDTO> CreateAccountOpeningEntry(AccountOpeningEntryDTO dto)
    {
        var result = await accountOpeningEntryRepo.CreateAsync(dto.Header, dto.Details);
        return new AccountOpeningEntryDTO { Id = result.Id, Message = result.Message };
    }
    public async Task<AccountOpeningEntryDTO> UpdateAccountOpeningEntry(AccountOpeningEntryDTO entry)
    {
        var result = await accountOpeningEntryRepo.UpdateAsync(entry.Header, entry.Details);
        return new AccountOpeningEntryDTO { Id = result.Id, Message = result.Message };
    }
    public async Task<AccountOpeningEntryDTO> DeleteAccountOpeningEntry(int id)
    {
        var result = await accountOpeningEntryRepo.DeleteAsync(id);
        return new AccountOpeningEntryDTO { Id = result.Id, Message = result.Message };
    }
    public async Task<List<AccountOpeningEntryDtls>> GetExistingOpeningBalances(int branchId) => await accountOpeningEntryRepo.GetExistingOpeningBalances(branchId);
    #endregion

    #endregion

    #region OpeningStockEntry
    public async Task<List<OpeningStockEntry>> GetAllOpeningStockEntry() => await openingStockEntryRepo.GetAllAsync();
    public async Task<OpeningStockEntryDTO> GetOpeningStockEntryFull(int id) => await openingStockEntryRepo.GetFullAsync(id);
    public async Task<List<OpeningStockEntryItemForUpdate>> GetOpeningStockEntryExistingStock(int itemId) => await openingStockEntryRepo.GetExistingStockForItem(itemId);
    public async Task<OutputResponse> CreateOpeningStockEntry(OpeningStockEntryDTO dto)
    {
        var master = new OpeningStockEntry
        {
            OpeningStockEntryId = dto.OpeningStockEntryId,
            DocumentNo = dto.DocumentNo,
            EntryDate = dto.EntryDate,
            EnteredBy = dto.EnteredBy,
            Remarks = dto.Remarks,
            FK_BranchId = dto.FK_BranchId,
            FK_UserId = dto.FK_UserId
        };
        return await openingStockEntryRepo.CreateAsync(master, dto.EntryDetails);
    }
    public async Task<OutputResponse> UpdateOpeningStockEntry(OpeningStockEntryDTO dto)
    {
        var master = new OpeningStockEntry
        {
            OpeningStockEntryId = dto.OpeningStockEntryId,
            DocumentNo = dto.DocumentNo,
            EntryDate = dto.EntryDate,
            EnteredBy = dto.EnteredBy,
            Remarks = dto.Remarks,
            FK_BranchId = dto.FK_BranchId,
            FK_UserId = dto.FK_UserId
        };
        return await openingStockEntryRepo.UpdateAsync(master, dto.EntryDetails);
    }
    public async Task<OutputResponse> DeleteOpeningStockEntry(int id) => await openingStockEntryRepo.DeleteAsync(id);
    public async Task<OpeningStockEntryPDTO> GetOpeningStockEntryPDetails(PaginationModel pagination)
    {
        var (items, totalCount) = await openingStockEntryRepo.GetAllWithPagination(pagination);
        return new OpeningStockEntryPDTO(items, totalCount);
    }
    public async Task<string> GetNewOpeningStockEntryDocNo() => await openingStockEntryRepo.GetNewDocNo();
    #endregion

    #region Menu Visibility
    public async Task<MenuVisibilityDTO> GetMenuConfiguration() => await menuVisibilityRepo.GetMenuConfigurationAsync();

    public async Task<List<ERPType>> GetERPTypes() => await menuVisibilityRepo.GetAllERPTypesAsync();

    public async Task<List<MenuVisibilityItem>> GetERPTypePreset(int erpTypeId) => await menuVisibilityRepo.GetERPTypePresetAsync(erpTypeId);

    public async Task<OutputResponse> UpdateClientERPType(int erpTypeId, string modifiedBy) => await menuVisibilityRepo.UpdateClientERPTypeAsync(erpTypeId, modifiedBy);

    public async Task<OutputResponse> SaveCustomMenuVisibility(List<MenuVisibilityItem> items) => await menuVisibilityRepo.SaveCustomMenuVisibilityAsync(items);

    public async Task<List<MenuDetails>> GetVisibleMenus() => await menuVisibilityRepo.GetVisibleMenusAsync();

    public async Task<List<MenuDetails>> GetAllMenuDetails() => await menuVisibilityRepo.GetAllMenuDetailsAsync();
    #endregion

    #region Print Design
    public async Task<PrintDesignDTO> GetPrintDesignAsync(PrintDesignReq req) => await printDesignRepo.GetAsync(req);
    public async Task<OutputResponse> CreatePrintDesignAsync([Body] PrintDesignDTO dto) => await printDesignRepo.CreateAsync(dto);
    public async Task<OutputResponse> UpdatePrintDesignAsync([Body] PrintDesignDTO dto) => await printDesignRepo.UpdateAsync(dto);
    public async Task<OutputResponse> DeletePrintDesignAsync(int id) => await printDesignRepo.DeleteAsync(id);
    public async Task<TransactionPDTO> GetPrintDesignPDetails(PaginationModel pagination) => await printDesignRepo.GetAllWithPagination(pagination);
    #endregion

    #region Barcode Design
    public async Task<BarcodeDesignDTO> GetBarcodeDesignAsync(int id) => await barcodeDesignRepo.GetAsync(id);
    public async Task<BarcodeDesignDTO> GetBarcodeDesignAsync() => await barcodeDesignRepo.GetAsync();
    public async Task<OutputResponse> CreateBarcodeDesignAsync([Body] BarcodeDesignDTO dto) => await barcodeDesignRepo.CreateAsync(dto);
    public async Task<OutputResponse> UpdateBarcodeDesignAsync([Body] BarcodeDesignDTO dto) => await barcodeDesignRepo.UpdateAsync(dto);
    public async Task<OutputResponse> DeleteBarcodeDesignAsync(int id) => await barcodeDesignRepo.DeleteAsync(id);
    public async Task<TransactionPDTO> GetBarcodeDesignPDetails(PaginationModel pagination) => await barcodeDesignRepo.GetAllWithPagination(pagination);
    public async Task<List<BarcodeDesignHdr>> GetAllBarcodeDesigns() => await barcodeDesignRepo.GetAllDesignsAsync();
    #endregion

    #region Reports

    #region Inventory Report
    public async Task<StockReportResponse> GetStockReport(StockReportRequest req) => await inventReportRepo.GetStockReportAsync(req);
    public async Task<StockTransferReportResponse> GetStockTransferReport(StockTransferReportRequest req) => await inventReportRepo.GetStockTransferReportAsync(req);
    public async Task<ItemTransactionHistoryReportResponse> GetItemTransactionHistoryReport(ItemTransactionHistoryReportRequest req)
        => await inventReportRepo.GetItemTransactionHistoryReportAsync(req);
    public async Task<StockAdjustmentReportResponse> GetStockAdjustmentReportAsync(StockAdjustmentReportRequest req) => await inventReportRepo.GetStockAdjustmentReportAsync(req);
    #endregion

    #region Sales Report
    public async Task<SalesReportResponse> GetSalesReport(SalesReportRequest req) => await salesReportRepo.GetSalesReportAsync(req);
    public async Task<SalesMonitorResponse> GetSalesMonitorReportAsync(SalesMonitorRequest req) => await salesReportRepo.GetSalesMonitorReportAsync(req);
    public async Task<List<SalesMonitorReceiptItem>> GetReceiptsByInvoiceAsync(int invoiceId) => await salesReportRepo.GetReceiptsByInvoiceAsync(invoiceId);

    public async Task<SalesDetailsReportResponse> GetSalesDetailsReport(SalesDetailsReportRequest req) => await salesReportRepo.GetSalesDetailsAsync(req);
    public async Task<CustomerWiseSalesReportResponse> GetCustomerWiseSalesReport(CustomerWiseSalesReportRequest req) => await salesReportRepo.GetCustomerWiseSalesReportAsync(req);
    public async Task<CategoryWiseSalesReportResponse> GetCategoryWiseReport(CategoryWiseSalesReportRequest req) => await salesReportRepo.GetCategoryWiseSalesReportAsync(req);
    public async Task<SalesQuotationRegisterResponse> GetSalesQuotationRegisterAsync(SalesQuotationRegisterRequest req) => await salesReportRepo.GetSalesQuotationRegisterAsync(req);
    public async Task<CustomerWiseQuotationReportResponse> GetCustomerWiseQuotationReportAsync(CustomerWiseQuotationReportRequest req) => await salesReportRepo.GetCustomerWiseQuotationReportAsync(req);
    public async Task<DeliveryNoteRegisterResponse> GetDeliveryNoteRegisterAsync(DeliveryNoteRegisterRequest req) => await salesReportRepo.GetDeliveryNoteRegisterAsync(req);
    public async Task<DateWiseProfitReportResponse> GetDateWiseProfitReport(DateWiseProfitReportRequest req) => await salesReportRepo.GetDateWiseProfitReportAsync(req);
    public async Task<ProformaInvoiceReportResponse> GetProformaInvoiceReport(ProformaInvoiceReportRequest req) => await salesReportRepo.GetProformaInvoiceReportAsync(req);
    public async Task<SalesOrderReportResponse> GetSalesOrderReport(SalesOrderReportRequest req) => await salesReportRepo.GetSalesOrderReportAsync(req);
    public async Task<EnquiryRegistrationReportResponse> GetEnquiryRegistrationReport(EnquiryRegistrationReportRequest req) => await salesReportRepo.GetEnquiryRegistrationReportAsync(req);
    public async Task<VatReportResponse> GetVatReport(VatReportRequest req) =>
    await salesReportRepo.GetVatReportAsync(req);

    #endregion

    #region Purchase Report
    public async Task<PurchaseReportResponse> GetPurchaseReport(PurchaseReportRequest req) => await purchaseReportRepo.GetPurchaseReportAsync(req);
    public async Task<PurchaseDetailsReportResponse> GetPurchaseDetailsReport(PurchaseDetailsReportRequest req) => await purchaseReportRepo.GetPurchaseDetailsAsync(req);
    public async Task<PurchaseOrderReportResponse> GetPurchaseOrderReport(PurchaseOrderReportRequest req) => await purchaseReportRepo.GetPurchaseOrderReport(req);
    public async Task<PurchaseMonitorResponse> GetPurchaseMonitorReportAsync(PurchaseMonitorRequest req) => await purchaseReportRepo.GetPurchaseMonitorReportAsync(req);
    public async Task<List<PurchaseMonitorPaymentItem>> GetPaymentsByPurchaseAsync(int purchaseId) => await purchaseReportRepo.GetPaymentsByPurchaseAsync(purchaseId);
    #endregion

    #region Transaction Monitor
    public async Task<TransactionMonitorResponse> GetTransactionMonitorReportAsync(TransactionMonitorRequest req) => await transactionReportRepo.GetTransactionMonitorReportAsync(req);
    #endregion

    #region Customer Report
    public async Task<CustomerStatementResponse> GetCustomerStatementReport(CustomerStatementRequest req) => await customerReportRepo.GetCustomerStatementAsync(req);
    public async Task<List<CustomerSummaryDTO>> GetCustomerSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null) => await customerReportRepo.GetCustomerSummaryAsync(fromDate, toDate, branchId, customerId);

    #endregion

    #region Account Report
    public async Task<JournalEntryReportResponse> GetJournalEntryReport(JournalEntryReportRequest req) => await accountReportRepo.GetJournalEntryReport(req);
    #endregion
    public async Task<List<SupplierSummaryDTO>> GetSupplierSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        => await supplierReportRepo.GetSupplierSummaryAsync(fromDate, toDate, branchId, supplierId);

    public async Task<List<SupplierListDTO>> GetSupplierListAsync()
        => await supplierReportRepo.GetSupplierListAsync();
    public Task<ChartOfAccountReportResponse> GetChartOfAccountReport(ChartOfAccountReportRequest req)
        => accountReportRepo.GetChartOfAccountReport(req);
    #endregion


    private PaginationModel MapPagination(PaginationModel pagination)
    {
        return new PaginationModel(
            pagination.page,
            pagination.pageSize,
            pagination.sortColumn ?? "",
            pagination.sortDirection ?? "",
            pagination.search ?? "",
            BranchId: pagination.BranchId
        );
    }

    #region Zatca
    public async Task<int> GetFailedCount() => await zatcaRepo.GetFailedCount();
    public async Task<PendingIcvData> GetPendingICVData(ICVReqDTO entry) => await zatcaRepo.GetPendingIcvDataAsync(entry);
    public async Task<IcvPendingPDTO> GetPendingICVList(IcvPendReqDTO entry) => await zatcaRepo.GetPendingIcvListAsync(entry);
    public async Task ZatcaReUploadAll(IcvReuploadAll dto)
    {
       
        await zatcaRepo.ReUploadAll(dto.icvs,  dto.Company);
    }
    public async Task<OutputResponse> ZatcaReUploadSelected(IcvReupload dto)
    {
        return await zatcaRepo.ReUploadSelected(dto.icv,  dto.Company);
    }
    public async Task<CSRResponse> GenerateCSR(CSRRequest request) => await zatcaRepo.GenerateCSR(request);
    public async Task<CSRResponse> GetCsr(int id) => await zatcaRepo.GetCSR(id);
    public async Task<OutputResponse> AddNewCSR(CSRResponse request) => await zatcaRepo.AddNewCSR(request);
    public async Task<ZatcaInvoiceDTO> GetZatcaPendingInvoicesAsync(PaginationModel pmodel) => await zatcaInvoiceRepo.GetAllZatcaInvoiceWithPagination(pmodel);
    public async Task UploadPendingSimplifiedInvoices(int BranchId) => await zatcaUploadService.UploadPendingSimplifiedInvoices(BranchId);
    #endregion


    #region Customer Receipt
    public async Task<List<ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptHdr>> GetAllCustomerReceipts() => await customerReceiptRepo.GetAllAsync();
    public async Task<ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO> GetCustomerReceiptById(int id) => (await customerReceiptRepo.GetByIdAsync(id))!;
    public async Task<OutputResponse> CreateCustomerReceipt(ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt) => await customerReceiptRepo.CreateAsync(receipt);
    public async Task<OutputResponse> UpdateCustomerReceipt(ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt) => await customerReceiptRepo.UpdateAsync(receipt);
    public async Task<OutputResponse> DeleteCustomerReceipt(int id) => await customerReceiptRepo.DeleteAsync(id);
    public async Task<string> GetNextCustomerReceiptNo() => await customerReceiptRepo.GetNextReceiptNoAsync();
    public async Task<List<ZakiSAASERP.Models.DTO.OutstandingInvoiceDTO>> GetCustomerOutstandingInvoices(int customerId, int branchId) => await customerReceiptRepo.GetCustomerOutstandingInvoicesAsync(customerId, branchId);
    public async Task<decimal> GetCustomerBalance(int customerId, int branchId) => await customerReceiptRepo.GetCustomerBalanceAsync(customerId, branchId);
    #endregion

    public async Task<List<ZakiSAASERP.Models.Masters.CustomerMaster>> GetAllCustomer() => await customerMasterRepo.GetAllAsync();

    #region Accounts
    public async Task<List<ZakiSAASERP.Models.Masters.AccountMaster>> GetCashAndBankAccounts() => await accountMasterRepo.GetCashAndBankAccountsAsync();
    #endregion

    #region User Privilege
    public async Task<UserPrivilegeDTO> GetUserprivilege(int id) => await userPrevilegeRepo.GetAsync(id);
    public async Task<OutputResponse> NewUserprivilege(UserPrivilegeDTO entity) => await userPrevilegeRepo.CreateAsync(entity);
    public async Task<OutputResponse> UpdateUserprivilege(UserPrivilegeDTO entity) => await userPrevilegeRepo.UpdateAsync(entity);
    public async Task<OutputResponse> DeleteUserprivilege(int id) => await userPrevilegeRepo.DeleteAsync(id);
    public async Task<List<WebUserPrivilege>> GetWebUserprivilege(PrivilegeDTO dto)
    {
        return await userPrevilegeRepo.GetWebUserPrivileges(dto.id);
    }
    public async Task<TransactionPDTO> GetUserPrivilegePDTO(PaginationModel pagination) => await userPrevilegeRepo.GetAllWithPagination(pagination);
    public async Task<List<UserPrivilegeHdr>> GetAllUserPrivilegeHdrs() => await userPrevilegeRepo.GetAllAsync();
    public Task<List<UserPrivilegeHdr>> GetUserprivileges() => userPrevilegeRepo.GetAllAsync();

    public async Task<List<CustomerListDTO>> GetCustomerListAsync()
        => await customerReportRepo.GetCustomerListAsync();

    public async Task<TrialBalanceReportResponse> GetTrialBalanceReport(TrialBalanceReportRequest req)
        => await accountReportRepo.GetTrialBalanceReport(req);

    public async Task<ExpenseReportResponse> GetExpenseReport(ExpenseReportRequest req)
        => await accountReportRepo.GetExpenseReport(req);

    public async Task<ProfitAndLossReportResponse> GetProfitAndLossReport(ProfitAndLossReportRequest req)
        => await accountReportRepo.GetProfitAndLossReport(req);

    public async Task<BalanceSheetReportResponse> GetBalanceSheetReport(BalanceSheetReportRequest req)
        => await accountReportRepo.GetBalanceSheetReport(req);

    #endregion

    #region Monitoring Reports
    public Task<List<CustomerPendingInvoiceDTO>> GetCustomerPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null) =>
        customerReportRepo.GetCustomerPendingInvoiceAsync(fromDate, toDate, branchId, customerId);
    public Task<string> GetAiAgentResponse(int id, string question) => throw new NotImplementedException();
    public Task Ragindexing() => throw new NotImplementedException();
    public async Task<LedgerReportResponse> GetLedgerReport(LedgerReportRequest req) => await accountReportRepo.GetLedgerReport(req);
    public Task<ApiResponse<Stream>> GetSalesReportPDF(SalesReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetSalesDetailsReportPDF(SalesDetailsReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetCustomerWiseSalesReportPDF(CustomerWiseSalesReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetSalesQuotationRegisterReportPDF(SalesQuotationRegisterRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetCustomerWiseQuotationReportPDF(CustomerWiseQuotationReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetDeliveryNoteRegisterReportPDF(DeliveryNoteRegisterRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetDateWiseProfitReportPDF(DateWiseProfitReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetProformaInvoiceReportPDF(ProformaInvoiceReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetSalesOrderReportPDF(SalesOrderReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetPurchaseSummaryServerReportPDF(PurchaseReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetPurchaseDetailsServerReportPDF(PurchaseDetailsReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetPurchaseOrderServerReportPDF(PurchaseOrderReportRequest req) => throw new NotImplementedException();
    public async Task<PurchaseReturnReportResponse> GetPurchaseReturnReport(PurchaseReturnReportRequest req) => await purchaseReportRepo.GetPurchaseReturnReportAsync(req);
    public Task<ApiResponse<Stream>> GetPurchaseReturnSummaryServerReportPDF(PurchaseReturnReportRequest req) => throw new NotImplementedException();
    public async Task<PurchaseReturnDetailsReportResponse> GetPurchaseReturnDetailsReport(PurchaseReturnDetailsReportRequest req) => await purchaseReportRepo.GetPurchaseReturnDetailsAsync(req);
    public Task<ApiResponse<Stream>> GetPurchaseReturnDetailsServerReportPDF(PurchaseReturnDetailsReportRequest req) => throw new NotImplementedException();
    public Task<ApiResponse<Stream>> GetSalesReturnReportPDF(SalesReportRequest req) => throw new NotImplementedException();
    public async Task<SalesReturnReportResponse> GetSalesReturnReport(SalesReturnReportRequest req) => await salesReportRepo.GetSalesReturnReportAsync(req);
    public Task<ApiResponse<Stream>> GetSalesReturnReportPDF(SalesReturnReportRequest req) => throw new NotImplementedException();
    public async Task<SalesReturnDetailsReportResponse> GetSalesReturnDetailsReport(SalesReturnDetailsReportRequest req) => await salesReportRepo.GetSalesReturnDetailsAsync(req);
    public Task<ApiResponse<Stream>> GetSalesReturnDetailsReportPDF(SalesReturnDetailsReportRequest req) => throw new NotImplementedException();

    public Task<DateTime?> GetOldestUnpaidInvoiceDate(int customerId)
    {
        throw new NotImplementedException();
    }

    public Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region ProductImport
    public async Task<ProductImportResultDTO> ImportProductsFromCsvAsync(List<ProductImportRowDTO> rows)
        => await productImportRepo.ImportProductsAsync(rows);

    public async Task<byte[]> GetProductImportSampleCsvAsync()
        => await Task.FromResult(productImportRepo.GenerateSampleCsv());
    #endregion

    #region ProductExport
    public async Task<List<ProductExportRowDTO>> GetProductsForExportAsync()
        => await productExportRepo.GetProductsForExportAsync();

    public async Task<byte[]> ExportProductsToCsvAsync()
        => await productExportRepo.ExportProductsToCsvAsync();
    #endregion
}