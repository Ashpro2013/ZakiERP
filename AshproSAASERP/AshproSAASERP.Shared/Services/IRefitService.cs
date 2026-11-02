using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.Models.Others;
using AshproSAASERP.Models.Transactions.Accounts;
using AshproSAASERP.Models.Transactions.Inventory;
using AshproSAASERP.Models.Transactions.Purchase;
using AshproSAASERP.Models.Transactions.Workshop;
using AshproSAASERP.Models.User;
using AshproSAASERP.Models.ViewModel;
using Refit;

namespace AshproSAASERP.Shared.Services;

public interface IRefitService
{
    #region Workshop
    #region JobOrder
    [Get("/api/joborder/nextcode/{branchId}")]
    Task<string> GetNextJobOrderCode(int branchId);

    [Get("/api/joborder/NextNo/{branchId}")]
    Task<int> GetNewJobOrderDocNo(int branchId);

    [Get("/api/joborder/{id}")]
    Task<JobOrderHdr> GetJobOrderById(int id);

    [Post("/api/joborder")]
    Task<OutputResponse> CreateJobOrder([Body] JobOrderHdr hdr);

    [Put("/api/joborder")]
    Task<OutputResponse> UpdateJobOrder([Body] JobOrderHdr hdr);

    [Delete("/api/joborder/{id}")]
    Task<OutputResponse> DeleteJobOrder(int id);

    [Post("/api/joborder/get-all")]
    Task<TransactionPDTO> GetJobOrderPDetails([Body] PaginationModel pmodel);
    #endregion

    #region JobCard
    [Get("/api/jobcard/nextcode/{branchId}")]
    Task<string> GetNextJobCardCode(int branchId);

    [Get("/api/jobcard/NextNo/{branchId}")]
    Task<int> GetNewJobCardDocNo(int branchId);

    [Get("/api/jobcard/{id}")]
    Task<JobCardHdr> GetJobCardById(int id);

    [Post("/api/jobcard")]
    Task<OutputResponse> CreateJobCard([Body] JobCardHdr hdr);

    [Put("/api/jobcard")]
    Task<OutputResponse> UpdateJobCard([Body] JobCardHdr hdr);

    [Delete("/api/jobcard/{id}")]
    Task<OutputResponse> DeleteJobCard(int id);

    [Post("/api/jobcard/get-all")]
    Task<TransactionPDTO> GetJobCardPDetails([Body] PaginationModel pmodel);

    [Get("/api/jobcard/history/{plateNo}")]
    Task<List<JobCardHistoryDTO>> GetJobCardHistory(string plateNo);
    #endregion
    #endregion

    #region Masters

    #region Account Head
    [Get("/api/accounthead")]
    Task<List<Models.Masters.AccountHeadMaster>> GetAllAccountHeads();
    [Get("/api/accounthead/{id}")]
    Task<Models.Masters.AccountHeadMaster> GetAccountHeadById(int id);
    [Get("/api/accounthead/pagination")]
    Task<AccountHeadPDTO> GetPAccountHeads([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/accounthead")]
    Task<OutputResponse> CreateAccountHead([Body] Models.Masters.AccountHeadMaster accountHead);
    [Put("/api/accounthead")]
    Task<OutputResponse> UpdateAccountHead([Body] Models.Masters.AccountHeadMaster accountHead);
    [Delete("/api/accounthead/{id}")]
    Task<OutputResponse> DeleteAccountHead(int id);
    [Get("/api/accounthead/nextcode/{typeId}")]
    Task<OutputResponse> GetAccountHeadNextCode(int typeId);
    #endregion

    #region AccountGroup
    [Get("/api/accountGroup")]
    Task<List<Models.Masters.AccountGroup>> GetAllAccountGroups();
    [Get("/api/accountGroup/{id}")]
    Task<Models.Masters.AccountGroup> GetAccountGroupById(int id);
    [Get("/api/accountGroup/pagination")]
    Task<AccountGroupPDTO> GetPAccountGroups([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/accountGroup")]
    Task<OutputResponse> CreateAccountGroup([Body] Models.Masters.AccountGroup accountGroup);
    [Put("/api/accountGroup")]
    Task<OutputResponse> UpdateAccountGroup([Body] Models.Masters.AccountGroup accountGroup);
    [Delete("/api/accountGroup/{id}")]
    Task<OutputResponse> DeleteAccountGroup(int id);
    [Get("/api/accountGroup/nextcode/{accountHeadId}")]
    Task<OutputResponse> GetAccountGroupNextCode(int accountHeadId);
    #endregion

    #region AccountSubGroup
    [Get("/api/accountSubGroup/pagination")]
    Task<AccountSubGroupPDTO> GetPAccountSubGroups([AliasAs("pmodel")] PaginationModel pmodel);

    [Get("/api/accountSubGroup")]
    Task<List<AccountSubGroup>> GetAllAccountSubGroups();

    [Get("/api/accountSubGroup/{id}")]
    Task<AccountSubGroup> GetAccountSubGroupById(int id);

    [Post("/api/accountSubGroup")]
    Task<OutputResponse> CreateAccountSubGroup([Body] AccountSubGroup accountSubGroup);

    [Put("/api/accountSubGroup")]
    Task<OutputResponse> UpdateAccountSubGroup([Body] AccountSubGroup accountSubGroup);

    [Delete("/api/accountSubGroup/{id}")]
    Task<OutputResponse> DeleteAccountSubGroup(int id);

    [Get("/api/accountSubGroup/nextcode/{accountGroupId}")]
    Task<OutputResponse> GetAccountSubGroupNextCode(int accountGroupId);
    #endregion

    #region AccountType
    [Get("/api/accountType")]
    Task<List<AccountType>> GetAllAccountTypes();
    #endregion

    #region Account
    [Get("/api/account")]
    Task<List<Models.Masters.AccountMaster>> GetAllAccounts();
    [Get("/api/account/{id}")]
    Task<Models.Masters.AccountMaster> GetAccountById(int id);
    [Get("/api/account/pagination")]
    Task<AccountPDTO> GetPAccounts([AliasAs("pmodel")] PaginationModel pagination);
    [Post("/api/account")]
    Task<OutputResponse> CreateAccount([Body] Models.Masters.AccountMaster account);
    [Put("/api/account")]
    Task<OutputResponse> UpdateAccount([Body] Models.Masters.AccountMaster account);
    [Delete("/api/account/{id}")]
    Task<OutputResponse> DeleteAccountMaster(int id);

    [Get("/api/account/nextcode/{subGroupId}")]
    Task<OutputResponse> GetAccountMasterNextCode(int subGroupId);
    #endregion

    #region Branch
    [Get("/api/branch")]
    Task<List<Models.Masters.BranchMaster>> GetAllBranches();
    [Get("/api/branch/{id}")]
    Task<Models.Masters.BranchMaster> GetBranchById(int id);
    [Get("/api/branch/pagination")]
    Task<BranchPDTO> GetPBranches([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/branch")]
    Task<OutputResponse> CreateBranch([Body] Models.Masters.BranchMaster branch);
    [Put("/api/branch")]
    Task<OutputResponse> UpdateBranch([Body] Models.Masters.BranchMaster branch);
    [Delete("/api/branch/{id}")]
    Task<OutputResponse> DeleteBranch(int id);
    #endregion

    #region Financial Year
    [Get("/api/financialyear")]
    Task<List<Models.Masters.FinancialYear>> GetAllFinancialYears();
    [Get("/api/financialyear/{id}")]
    Task<Models.Masters.FinancialYear> GetFinancialYearById(int id);
    [Get("/api/financialyear/pagination")]
    Task<FinancialYearPDTO> GetPFinancialYears([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/financialyear")]
    Task<OutputResponse> CreateFinancialYear([Body] Models.Masters.FinancialYear financialYear);
    [Put("/api/financialyear")]
    Task<OutputResponse> UpdateFinancialYear([Body] Models.Masters.FinancialYear financialYear);
    [Delete("/api/financialyear/{id}")]
    Task<OutputResponse> DeleteFinancialYear(int id);
    [Get("/api/financialyear/maxrefno")]
    Task<int> GetMaxRefNo();
    #endregion

    #region User
    [Post("/api/user/login")]
    Task<LoginResponse> Login([Body] LoginRequest request);
    [Get("/api/user/prelogin")]
    Task<PreLoginDatas> GetPreLoginDetails([AliasAs("tenant")] string tenant, [AliasAs("username")] string username);
    #endregion

    #region Unit
    [Get("/api/unit")]
    Task<List<Models.Masters.UnitMaster>> GetAllUnits();
    [Get("/api/unit/{id}")]
    Task<Models.Masters.UnitMaster> GetUnitById(int id);
    [Post("/api/unit")]
    Task<OutputResponse> CreateUnit([Body] UnitMaster unit);
    [Put("/api/unit")]
    Task<OutputResponse> UpdateUnit([Body] UnitMaster unit);
    [Delete("/api/unit/{id}")]
    Task<OutputResponse> DeleteUnit(int id);
    [Get("/api/unit/pagination")]
    Task<UnitPDTO> GetPUnits([AliasAs("pmodel")] PaginationModel pmodel);
    #endregion

    #region User
    [Get("/api/userdetails")]
    Task<List<Models.User.UserDetails>> GetAllUsers();
    [Get("/api/userdetails/{id}")]
    Task<Models.User.UserDetails> GetUserById(int id);
    [Post("/api/userdetails")]
    Task<OutputResponse> CreateUser([Body] Models.User.UserDetails user);
    [Put("/api/userdetails")]
    Task<OutputResponse> UpdateUser([Body] Models.User.UserDetails user);
    [Delete("/api/userdetails/{id}")]
    Task<OutputResponse> DeleteUser(int id);
    [Get("/api/userdetails/pagination")]
    Task<UserPDTO> GetPUsers([AliasAs("pmodel")] PaginationModel pmodel);

    [Get("/api/userdetails/branches/{username}")]
    Task<List<Models.Masters.BranchMaster>> GetUserBranches(string username);

    [Post("/api/userdetails/savebranches")]
    Task<OutputResponse> SaveUserBranches([Body] List<Models.User.UserBranch> userBranches);
    #endregion

    #region SalesMan

    [Get("/api/salesMan")]
    Task<List<SalesManMaster>> GetAllSalesMan();
    [Get("/api/salesMan/{id}")]
    Task<SalesManMaster> GetSalesManById(int id);
    [Get("/api/salesMan/pagination")]
    Task<SalesManPDTO> GetPSalesMen([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/salesMan")]
    Task<OutputResponse> CreateSalesMan([Body] SalesManMaster salesMan);
    [Put("/api/salesMan")]
    Task<OutputResponse> UpdateSalesMan([Body] SalesManMaster salesMan);
    [Delete("/api/salesMan/{id}")]
    Task<OutputResponse> DeleteSalesMan(int id);
    #endregion

    #region Category

    [Get("/api/category")]
    Task<List<CategoryMaster>> GetAllCategory();
    [Get("/api/category/{id}")]
    Task<CategoryMaster> GetCategoryById(int id);
    [Get("/api/category/pagination")]
    Task<CategoryPDTO> GetPCategories([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/category")]
    Task<OutputResponse> CreateCategory([Body] CategoryMaster category);
    [Put("/api/category")]
    Task<OutputResponse> UpdateCategory([Body] CategoryMaster category);
    [Delete("/api/category/{id}")]
    Task<OutputResponse> DeleteCategory(int id);
    [Get("/api/category/nextcode")]
    Task<string> GetNextCategoryCode();

    [Multipart]
    [Post("/api/image/upload")]
    Task<string> UploadImage([AliasAs("file")] StreamPart file, [AliasAs("entityType")] string entityType, [AliasAs("entityId")] int id, [AliasAs("basePath")] string basePath, [AliasAs("clientFolder")] string clientFolder);

    [Delete("/api/image/delete")]
    Task DeleteImage([AliasAs("basePath")] string basePath, [AliasAs("imagePath")] string imagePath, [AliasAs("clientFolder")] string clientFolder);
    #endregion

    #region Vehicle
    [Get("/api/vehicle")]
    Task<List<VehicleMaster>> GetAllVehicles();
    [Get("/api/vehicle/{id}")]
    Task<VehicleMaster> GetVehicleById(int id);
    [Get("/api/vehicle/pagination")]
    Task<VehiclePDTO> GetPVehicles([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/vehicle")]
    Task<OutputResponse> CreateVehicle([Body] VehicleMaster vehicle);
    [Put("/api/vehicle")]
    Task<OutputResponse> UpdateVehicle([Body] VehicleMaster vehicle);
    [Delete("/api/vehicle/{id}")]
    Task<OutputResponse> DeleteVehicle(int id);
    #endregion

    #region RepairMaster
    [Get("/api/repair")]
    Task<List<RepairMaster>> GetAllRepairs();
    [Get("/api/repair/{id}")]
    Task<RepairMaster> GetRepairById(int id);
    [Get("/api/repair/pagination")]
    Task<RepairPDTO> GetPRepairs([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/repair")]
    Task<OutputResponse> CreateRepair([Body] RepairMaster repair);
    [Put("/api/repair")]
    Task<OutputResponse> UpdateRepair([Body] RepairMaster repair);
    [Delete("/api/repair/{id}")]
    Task<OutputResponse> DeleteRepair(int id);
    #endregion

    #region TechnicianMaster
    [Get("/api/technician")]
    Task<List<Models.Masters.TechnicianMaster>> GetAllTechnicians();
    [Get("/api/technician/{id}")]
    Task<Models.Masters.TechnicianMaster> GetTechnicianById(int id);
    [Get("/api/technician/pagination")]
    Task<TechnicianPDTO> GetPTechnicians([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/technician")]
    Task<OutputResponse> CreateTechnician([Body] Models.Masters.TechnicianMaster entity);
    [Put("/api/technician")]
    Task<OutputResponse> UpdateTechnician([Body] Models.Masters.TechnicianMaster entity);
    [Delete("/api/technician/{id}")]
    Task<OutputResponse> DeleteTechnician(int id);
    #endregion

    #region SubCategory

    [Get("/api/subCategory")]
    Task<List<SubCategoryMaster>> GetAllSubCategory();
    [Get("/api/subCategory/{id}")]
    Task<SubCategoryMaster> GetSubCategoryById(int id);
    [Get("/api/subCategory/pagination")]
    Task<SubCategoryPDTO> GetPSubCategories([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/subCategory")]
    Task<OutputResponse> CreateSubCategory([Body] SubCategoryMaster subCategory);
    [Put("/api/subCategory")]
    Task<OutputResponse> UpdateSubCategory([Body] SubCategoryMaster subCategory);
    [Delete("/api/subCategory/{id}")]
    Task<OutputResponse> DeleteSubCategory(int id);
    [Get("/api/subCategory/nextcode/{categoryId}")]
    Task<string> GetNextSubCategoryCode(int categoryId, string? categoryCode);
    #endregion

    #region Vat
    [Get("/api/vat")]
    Task<List<VatMaster>> GetAllVat();
    [Get("/api/vat/{id}")]
    Task<VatMaster> GetVatById(int id);
    [Get("/api/vat/pagination")]
    Task<VatPDTO> GetPVats([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/vat")]
    Task<OutputResponse> CreateVat([Body] VatMaster vat);
    [Put("/api/vat")]
    Task<OutputResponse> UpdateVat([Body] VatMaster vat);
    [Delete("/api/vat/{id}")]
    Task<OutputResponse> DeleteVat(int id);
    [Get("/api/vat/categorydetails/{code}")]
    Task<List<VATCategoryDetails>> GetVATCategoryDetails(string code);
    #endregion

    #region CustomerGroup
    [Get("/api/customerGroup")]
    Task<List<CustomerGroupMaster>> GetAllCustomerGroup();
    [Get("/api/customerGroup/{id}")]
    Task<CustomerGroupMaster> GetCustomerGroupById(int id);
    [Get("/api/customerGroup/pagination")]
    Task<CustomerGroupPDTO> GetPCustomerGroups([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/customerGroup")]
    Task<OutputResponse> CreateCustomerGroup([Body] CustomerGroupMaster customerGroup);
    [Put("/api/customerGroup")]
    Task<OutputResponse> UpdateCustomerGroup([Body] CustomerGroupMaster customerGroup);
    [Delete("/api/customerGroup/{id}")]
    Task<OutputResponse> DeleteCustomerGroup(int id);
    #endregion

    #region CountryMaster
    [Get("/api/country")]
    Task<List<CountryMaster>> GetAllCountry();
    [Get("/api/country/{id}")]
    Task<CountryMaster> GetCountryById(int id);
    [Get("/api/country/pagination")]
    Task<CountryPDTO> GetPCountries([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/country")]
    Task<OutputResponse> CreateCountry([Body] CountryMaster countryMaster);
    [Put("/api/country")]
    Task<OutputResponse> UpdateCountry([Body] CountryMaster countryMaster);
    [Delete("/api/country/{id}")]
    Task<OutputResponse> DeleteCountry(int id);
    #endregion

    #region CustomerLocationMaster
    [Get("/api/customerLocation")]
    Task<List<CustomerLocationMaster>> GetAllCustomerLocation();
    [Get("/api/customerLocation/{id}")]
    Task<CustomerLocationMaster> GetCustomerLocationById(int id);
    [Get("/api/customerLocation/pagination")]
    Task<CustomerLocationPDTO> GetPCustomerLocations([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/customerLocation")]
    Task<OutputResponse> CreateCustomerLocation([Body] CustomerLocationMaster customerLocation);
    [Put("/api/customerLocation")]
    Task<OutputResponse> UpdateCustomerLocation([Body] CustomerLocationMaster customerLocation);
    [Delete("/api/customerLocation/{id}")]
    Task<OutputResponse> DeleteCustomerLocation(int id);
    #endregion

    #region CompanyDetails
    [Get("/api/companydetails/branch/{branchId}")]
    Task<CompanyDetails?> GetCompanyDetailsByBranch(int branchId);
    [Get("/api/companydetails/exists/{branchId}")]
    Task<bool> CheckCompanyDetailsExists(int branchId);
    [Post("/api/companydetails")]
    Task<OutputResponse> CreateCompanyDetails([Body] CompanyDetails details);
    [Put("/api/companydetails")]
    Task<OutputResponse> UpdateCompanyDetails([Body] CompanyDetails details);
    #endregion

    #region Brand
    [Get("/api/brand")]
    Task<List<BrandMaster>> GetAllBrand();
    [Get("/api/brand/{id}")]
    Task<BrandMaster> GetBrandById(int id);
    [Get("/api/brand/pagination")]
    Task<BrandPDTO> GetPBrands([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/brand")]
    Task<OutputResponse> CreateBrand([Body] BrandMaster Brand);
    [Put("/api/brand")]
    Task<OutputResponse> UpdateBrand([Body] BrandMaster Brand);
    [Delete("/api/brand/{id}")]
    Task<OutputResponse> DeleteBrand(int id);
    #endregion

    #region Currency
    [Get("/api/currency")]
    Task<List<CurrencyMaster>> GetAllCurrency();
    [Get("/api/currency/{id}")]
    Task<CurrencyMaster> GetCurrencyById(int id);
    [Get("/api/currency/pagination")]
    Task<CurrencyPDTO> GetPCurrencies([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/currency")]
    Task<OutputResponse> CreateCurrency([Body] CurrencyMaster currency);
    [Put("/api/currency")]
    Task<OutputResponse> UpdateCurrency([Body] CurrencyMaster currency);
    [Delete("/api/currency/{id}")]
    Task<OutputResponse> DeleteCurrency(int id);
    #endregion

    #region Purchaser
    [Get("/api/purchaser")]
    Task<List<PurchaserMaster>> GetAllPurchaser();
    [Get("/api/purchaser/{id}")]
    Task<PurchaserMaster> GetPurchaserById(int id);
    [Get("/api/purchaser/pagination")]
    Task<PurchaserPDTO> GetPPurchasers([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/purchaser")]
    Task<OutputResponse> CreatePurchaser([Body] PurchaserMaster purchaser);
    [Put("/api/purchaser")]
    Task<OutputResponse> UpdatePurchaser([Body] PurchaserMaster purchaser);
    [Delete("/api/purchaser/{id}")]
    Task<OutputResponse> DeletePurchaser(int id);
    #endregion

    #region Project

    [Get("/api/project")]
    Task<List<ProjectMaster>> GetAllProject();
    [Get("/api/project/{id}")]
    Task<ProjectMaster> GetProjectById(int id);
    [Get("/api/project/pagination")]
    Task<ProjectPDTO> GetPProjects([AliasAs("pmodel")] PaginationModel pmodel);

    [Post("/api/project")]
    Task<OutputResponse> CreateProject([Body] ProjectMaster project);
    [Put("/api/project")]
    Task<OutputResponse> UpdateProject([Body] ProjectMaster project);
    [Delete("/api/project/{id}")]
    Task<OutputResponse> DeleteProject(int id);
    #endregion

    #region Customer
    [Get("/api/customer")]
    Task<List<CustomerMaster>> GetAllCustomers();
    [Get("/api/customer/{id}")]
    Task<CustomerMaster> GetCustomerById(int id);
    [Get("/api/customer/pagination")]
    Task<CustomerPDTO> GetPCustomers([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/customer")]
    Task<OutputResponse> CreateCustomer([Body] CustomerMaster customer);
    [Put("/api/customer")]
    Task<OutputResponse> UpdateCustomer([Body] CustomerMaster customer);
    [Delete("/api/customer/{id}")]
    Task<OutputResponse> DeleteCustomer(int id);
    [Get("/api/customer/nextcode")]
    Task<string> GetNextCustomerCode();
    [Get("/api/customer/outstanding-balance/{customerId}")]
    Task<decimal> GetCustomerOutstandingBalance(int customerId);
    #endregion

    #region Supplier
    [Get("/api/supplier")]
    Task<List<SupplierMaster>> GetAllSuppliers();
    [Get("/api/supplier/{id}")]
    Task<SupplierMaster> GetSupplierById(int id);
    [Get("/api/supplier/pagination")]
    Task<SupplierPDTO> GetPSuppliers([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/supplier")]
    Task<OutputResponse> CreateSupplier([Body] SupplierMaster supplier);
    [Put("/api/supplier")]
    Task<OutputResponse> UpdateSupplier([Body] SupplierMaster supplier);
    [Delete("/api/supplier/{id}")]
    Task<OutputResponse> DeleteSupplier(int id);
    [Get("/api/supplier/nextcode")]
    Task<string> GetNextSupplierCode();
    #endregion

    #region Item
    [Get("/api/item")]
    Task<List<ItemMaster>> GetAllItems();
    [Get("/api/item/{id}")]
    Task<ItemMaster> GetItemById(int id);
    [Get("/api/item/pagination")]
    Task<ItemMasterPDTO> GetItemPDetails([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/item")]
    Task<OutputResponse> CreateItem([Body] ItemMaster item);
    [Put("/api/item")]
    Task<OutputResponse> UpdateItem([Body] ItemMaster item);
    [Delete("/api/item/{id}")]
    Task<OutputResponse> DeleteItem(int id);
    [Get("/api/item/multiunit")]
    Task<ItemMultiUnit> GetItemMultiUnit([Query] int itemId, [Query] int unitId, [Query] string? barcode);
    [Get("/api/item/multipagination")]
    Task<ItemMultiUnitPDTO> GetItemMultiUnitPDetails([AliasAs("pmodel")] PaginationModel pmodel);

    [Get("/api/item/nextcode")]
    Task<string> GetNextItemCode([Query] int? categoryId, [Query] int? subCategoryId, [Query] string? catCode, [Query] string? subCatCode);

    [Get("/api/item/dialogpagination")]
    Task<ItemDialogPDTO> GetItemDialogDetails([AliasAs("pmodel")] PaginationModel pmodel);

    [Get("/api/item/multidialogpagination")]
    Task<ItemMultiUnitDialogPDTO> GetMultiUnitDialogDetails([AliasAs("pmodel")] PaginationModel pmodel);

    [Get("/api/item/multiunitsByItem/{itemId}")]
    Task<List<ItemMultiUnit>> GetItemMultiUnits(int itemId);

    [Get("/api/item/stockinfo/{itemId}")]
    Task<ItemStockInfoDTO> GetItemStockInfo(int itemId);

    [Get("/api/item/barcodeitems")]
    Task<List<BarcodeData>> GetBarcodeItems([Query] int? categoryId = null);
    #endregion

    #region CustomerOPEntry
    [Get("/api/customeropentry")]
    Task<List<CustomerOPEntry>> GetAllCustomerOPEntry();
    [Get("/api/customeropentry/{id}")]
    Task<CustomerOPEntryDTO> GetCustomerOPEntryById(int id);
    [Post("/api/customeropentry")]
    Task<OutputResponse> CreateCustomerOPEntry([Body] CustomerOPEntryDTO entry);

    [Get("/api/customeropentry/pagination")]
    Task<CustomerOpEntryPDTO> GetCustomerOPEntryPDetails([AliasAs("pmodel")] PaginationModel pmodel);
    [Get("/api/customeropentry/newdocno")]
    Task<string> GetNewCustomerOPEntryDocNo();
    #endregion

    #region SupplierOPEntry
    [Get("/api/supplieropentry")]
    Task<List<SupplierOPEntry>> GetAllSupplierOPEntry();
    [Get("/api/supplieropentry/{id}")]
    Task<SupplierOPEntryDTO> GetSupplierOPEntryById(int id);
    [Post("/api/supplieropentry")]
    Task<OutputResponse> CreateSupplierOPEntry([Body] SupplierOPEntryDTO entry);
    [Get("/api/supplieropentry/pagination")]
    Task<SupplierOpEntryPDTO> GetSupplierOPEntryPDetails([AliasAs("pmodel")] PaginationModel pmodel);
    [Get("/api/supplieropentry/newdocno")]
    Task<string> GetNewSupplierOPEntryDocNo();
    #endregion

    #region StoreMaster
    [Get("/api/storemaster")]
    Task<List<StoreMaster>> GetAllStores();
    [Get("/api/storemaster/{id}")]
    Task<StoreMaster> GetStoreById(int id);
    [Post("/api/storemaster")]
    Task<OutputResponse> CreateStore([Body] StoreMaster store);
    [Put("/api/storemaster")]
    Task<OutputResponse> UpdateStore([Body] StoreMaster store);
    [Delete("/api/storemaster/{id}")]
    Task<OutputResponse> DeleteStore(int id);
    [Get("/api/storemaster/checkdefault")]
    Task<bool> CheckIfDefaultStoreExists([AliasAs("excludeId")] int excludeId = 0, [AliasAs("branchId")] int? branchId = null);
    #endregion

    #region OpeningStockEntry
    [Get("/api/openingstockentry")]
    Task<List<OpeningStockEntry>> GetAllOpeningStockEntry();
    [Get("/api/openingstockentry/{id}")]
    Task<OpeningStockEntryDTO> GetOpeningStockEntryFull(int id);
    [Get("/api/openingstockentry/newdocno")]
    Task<string> GetOpeningStockEntryNewDocNo();
    [Get("/api/openingstockentry/existing/{itemId}")]
    Task<List<OpeningStockEntryItemForUpdate>> GetOpeningStockEntryExistingStock(int itemId);
    [Post("/api/openingstockentry")]
    Task<OutputResponse> CreateOpeningStockEntry([Body] OpeningStockEntryDTO dto);
    [Put("/api/openingstockentry")]
    Task<OutputResponse> UpdateOpeningStockEntry([Body] OpeningStockEntryDTO entry);
    [Delete("/api/openingstockentry/{id}")]
    Task<OutputResponse> DeleteOpeningStockEntry(int id);
    [Get("/api/openingstockentry/pagination")]
    Task<OpeningStockEntryPDTO> GetOpeningStockEntryPDetails([AliasAs("pmodel")] PaginationModel pmodel);
    #endregion

    #region AccountOpeningEntry
    [Get("/api/accountopeningentry")]
    Task<List<AccountOpeningEntryHdr>> GetAllAccountOpeningEntry();
    [Get("/api/accountopeningentry/{id}")]
    Task<AccountOpeningEntryDTO> GetAccountOpeningEntryById(int id);
    [Get("/api/accountopeningentry/pagination")]
    Task<TransactionPDTO> GetAccountOpeningEntryPDetails([AliasAs("pmodel")] PaginationModel pmodel);
    [Get("/api/accountopeningentry/newdocno")]
    Task<string> GetAccountOpeningEntryNewDocNo([Query] int branchId);
    [Get("/api/accountopeningentry/balances")]
    Task<List<AccountOpeningEntryDtls>> GetExistingOpeningBalances([Query] int branchId);
    [Post("/api/accountopeningentry")]
    Task<AccountOpeningEntryDTO> CreateAccountOpeningEntry([Body] AccountOpeningEntryDTO dto);
    [Put("/api/accountopeningentry")]
    Task<AccountOpeningEntryDTO> UpdateAccountOpeningEntry([Body] AccountOpeningEntryDTO entry);
    [Delete("/api/accountopeningentry/{id}")]
    Task<AccountOpeningEntryDTO> DeleteAccountOpeningEntry(int id);
    #endregion

    #endregion

    #region PermanentSettings
    [Get("/api/permanentSettings")]
    Task<List<PermanentSettings>> GetAllPermanentSettings();
    [Put("/api/permanentSettings")]
    Task<OutputResponse> UpdatePermanentSettings([Body] List<PermanentSettings> settings);
    #endregion

    #region Menu Visibility
    [Get("/api/menuvisibility/configuration")]
    Task<AshproSAASERP.Models.Settings.MenuVisibilityDTO> GetMenuConfiguration();

    [Get("/api/menuvisibility/erptypes")]
    Task<List<AshproSAASERP.Models.Settings.ERPType>> GetERPTypes();

    [Get("/api/menuvisibility/preset/{erpTypeId}")]
    Task<List<AshproSAASERP.Models.Settings.MenuVisibilityItem>> GetERPTypePreset(int erpTypeId);

    [Put("/api/menuvisibility/erptype/{erpTypeId}")]
    Task<OutputResponse> UpdateClientERPType(int erpTypeId, [Query] string modifiedBy);

    [Post("/api/menuvisibility/custom")]
    Task<OutputResponse> SaveCustomMenuVisibility([Body] List<AshproSAASERP.Models.Settings.MenuVisibilityItem> items);

    [Get("/api/menuvisibility/visible")]
    Task<List<AshproSAASERP.Models.User.MenuDetails>> GetVisibleMenus();

    [Get("/api/menuvisibility/all")]
    Task<List<AshproSAASERP.Models.User.MenuDetails>> GetAllMenuDetails();
    #endregion

    #region General
    [Get("/api/general/chartvalues")]
    Task<AshproSAASERP.Models.ViewModel.ChartEntity> GetChartValues(int branchId, string format);

    [Get("/api/general/arabictext")]
    Task<string> GetArabicText([AliasAs("text")] string sourceText);

    [Get("/api/general/combofill")]
    Task<List<AshproSAASERP.Models.ViewModel.CommonModel>> GetCombofill([AliasAs("head")] string head, [AliasAs("branchId")] int BranchId);

    [Get("/api/general/masters")]
    Task<List<AshproSAASERP.Models.ViewModel.ExMasters>> GetMasters([AliasAs("head")] string type, [AliasAs("branchId")] int BranchId);

    [Get("/api/general/decimalval")]
    Task<decimal> GetValueMethod([AliasAs("head")] string head, [AliasAs("id")] int val);
    #endregion

    #region Admin

    #region Subscription
    [Get("/api/admin")]
    Task<List<LoginCompany>> GetAllAdminCompanies();
    [Get("/api/admin/pagination")]
    Task<LoginCompanyPDTO> GetPLoginCompany([Query] PaginationModel pmodel);
    [Get("/api/admin/{id}")]
    Task<LoginCompany> GetAdminCompanyById(int id);
    [Post("/api/admin")]
    Task<LoginCompany> CreateAdminCompany([Body] ExLCompany company);
    [Put("/api/admin")]
    Task<LoginCompany> UpdateAdminCompany([Body] ExLCompany company);
    [Delete("/api/admin/{id}")]
    Task<LoginCompany> DeleteAdminCompany(int id);
    #endregion

    #region Admin User
    [Get("/api/adminUser")]
    Task<List<AdminUser>> GetAllAdminUsers();
    [Get("/api/adminUser/{id}")]
    Task<AdminUser> GetAdminUserById(int id);
    [Post("/api/adminUser")]
    Task<AdminUser> CreateAdminUser([Body] AdminUser user);
    [Put("/api/adminUser")]
    Task<AdminUser> UpdateAdminUser([Body] AdminUser user);
    [Delete("/api/adminUser/{id}")]
    Task<AdminUser> DeleteAdminUser(int id);
    #endregion

    #endregion

    #region Transactions

    #region Inventory
    #region StockAdjustment
    [Get("/api/stockadjustment/nextcode")]
    Task<string> GetNextStockAdjustmentCode();

    [Get("/api/stockadjustment/NextNo/{branchId}")]
    Task<int> GetNewStockAdjustmentNo(int branchId);

    [Get("/api/stockadjustment/{id}")]
    Task<StockAdjustmentHdr> GetStockAdjustmentById(int id);

    [Post("/api/stockadjustment")]
    Task<OutputResponse> CreateStockAdjustment([Body] StockAdjustmentHdr hdr);

    [Put("/api/stockadjustment")]
    Task<OutputResponse> UpdateStockAdjustment([Body] StockAdjustmentHdr hdr);

    [Delete("/api/stockadjustment/{id}")]
    Task<OutputResponse> DeleteStockAdjustment(int id);

    [Post("/api/stockadjustment/get-all")]
    Task<TransactionPDTO> GetStockAdjustmentPDetails([Body] PaginationModel pmodel);

    [Get("/api/stockadjustment/current-stock")]
    Task<decimal> GetItemCurrentStock([Query] int itemId, [Query] int? storeId, [Query] int? excludeVoucherId = 0);
    #endregion

    #region MaterialTransferRequest
    [Get("/api/materialtransferrequest/nextcode")]
    Task<string> GetNextMaterialTransferRequestCode();

    [Get("/api/materialtransferrequest/NextNo/{branchId}")]
    Task<int> GetNewMaterialTransferRequestNo(int branchId);

    [Get("/api/materialtransferrequest/{id}")]
    Task<MaterialTransferRequestHdr> GetMaterialTransferRequestById(int id);

    [Post("/api/materialtransferrequest")]
    Task<OutputResponse> CreateMaterialTransferRequest([Body] MaterialTransferRequestHdr hdr);

    [Put("/api/materialtransferrequest")]
    Task<OutputResponse> UpdateMaterialTransferRequest([Body] MaterialTransferRequestHdr hdr);

    [Delete("/api/materialtransferrequest/{id}")]
    Task<OutputResponse> DeleteMaterialTransferRequest(int id);

    [Post("/api/materialtransferrequest/get-all")]
    Task<TransactionPDTO> GetMaterialTransferRequestPDetails([Body] PaginationModel pmodel);

    // Approval Endpoints
    [Post("/api/materialtransferrequest/approvallist")]
    Task<List<MaterialTransferRequestHdr>> GetMaterialTransferApprovalList([Body] MTRApprovalFilter filter);

    [Post("/api/materialtransferrequest/approve")]
    Task<OutputResponse> ApproveMaterialTransferRequest([Body] object request);

    [Post("/api/materialtransferrequest/reject")]
    Task<OutputResponse> RejectMaterialTransferRequest([Body] object request);
    #endregion

    #region MaterialTransferIn
    [Get("/api/materialtransferin/nextcode")]
    Task<string> GetNextMaterialTransferInCode();

    [Get("/api/materialtransferin/NextNo/{branchId}")]
    Task<int> GetNewMaterialTransferInNo(int branchId);

    [Get("/api/materialtransferin/{id}")]
    Task<MaterialTransferInHdr> GetMaterialTransferInById(int id);

    [Post("/api/materialtransferin")]
    Task<OutputResponse> CreateMaterialTransferIn([Body] MaterialTransferInHdr hdr);

    [Put("/api/materialtransferin")]
    Task<OutputResponse> UpdateMaterialTransferIn([Body] MaterialTransferInHdr hdr);

    [Delete("/api/materialtransferin/{id}")]
    Task<OutputResponse> DeleteMaterialTransferIn(int id);

    [Post("/api/materialtransferin/get-all")]
    Task<TransactionPDTO> GetMaterialTransferInPDetails([Body] PaginationModel pmodel);
    #endregion

    #region MaterialTransferOut
    [Get("/api/materialtransferout/nextcode")]
    Task<string> GetNextMaterialTransferOutCode();

    [Get("/api/materialtransferout/NextNo/{branchId}")]
    Task<int> GetNewMaterialTransferOutNo(int branchId);

    [Get("/api/materialtransferout/{id}")]
    Task<MaterialTransferOutHdr> GetMaterialTransferOutById(int id);

    [Post("/api/materialtransferout")]
    Task<OutputResponse> CreateMaterialTransferOut([Body] MaterialTransferOutHdr hdr);

    [Put("/api/materialtransferout")]
    Task<OutputResponse> UpdateMaterialTransferOut([Body] MaterialTransferOutHdr hdr);

    [Delete("/api/materialtransferout/{id}")]
    Task<OutputResponse> DeleteMaterialTransferOut(int id);

    [Post("/api/materialtransferout/get-all")]
    Task<TransactionPDTO> GetMaterialTransferOutPDetails([Body] PaginationModel pmodel);
    #endregion

    #region InternalMaterialTransfer
    [Get("/api/internalmaterialtransfer/nextcode")]
    Task<string> GetNextInternalMaterialTransferCode();

    [Get("/api/internalmaterialtransfer/NextNo/{branchId}")]
    Task<int> GetNewInternalMaterialTransferNo(int branchId);

    [Get("/api/internalmaterialtransfer/{id}")]
    Task<InternalMaterialTransferHdr> GetInternalMaterialTransferById(int id);

    [Post("/api/internalmaterialtransfer")]
    Task<OutputResponse> CreateInternalMaterialTransfer([Body] InternalMaterialTransferHdr hdr);

    [Put("/api/internalmaterialtransfer")]
    Task<OutputResponse> UpdateInternalMaterialTransfer([Body] InternalMaterialTransferHdr hdr);

    [Delete("/api/internalmaterialtransfer/{id}")]
    Task<OutputResponse> DeleteInternalMaterialTransfer(int id);

    [Post("/api/internalmaterialtransfer/get-all")]
    Task<TransactionPDTO> GetInternalMaterialTransferPDetails([Body] PaginationModel pmodel);
    #endregion

    #endregion

    #region Sales

    #region Sales Quotation
    [Get("/api/quotation/NextNo/{id}")]
    Task<int> GetNewSalesQuotationNo(int id);
    [Get("/api/quotation/{id}")]
    Task<SalesQuotationDTO> GetSalesQuotationById(int id);
    [Get("/api/quotation/pagination")]
    Task<TransactionPDTO> GetPSalesQuotations([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/quotation")]
    Task<OutputResponse> CreateSalesQuotation([Body] SalesQuotationDTO salesQuotation);
    [Put("/api/quotation")]
    Task<OutputResponse> UpdateSalesQuotation([Body] SalesQuotationDTO salesQuotation);
    [Delete("/api/quotation/{id}")]
    Task<OutputResponse> DeleteSalesQuotation(int id);
    [Get("/api/quotation/nextcode")]
    Task<string> GetNextQuotationCode();
    #endregion

    #region Sales Invoice
    [Get("/api/salesinvoice/NextNo/{id}")]
    Task<int> GetNewSalesInvoiceNo(int id);
    [Get("/api/salesinvoice/{id}")]
    Task<SalesInvoiceDTO> GetSalesInvoiceById(int id);
    [Post("/api/salesinvoice/pagination")]
    Task<TransactionPDTO> GetPSalesInvoices([Body] PaginationModel pmodel);
    [Post("/api/salesinvoice")]
    Task<OutputResponse> CreateSalesInvoice([Body] SalesInvoiceDTO salesInvoice);
    [Put("/api/salesinvoice")]
    Task<OutputResponse> UpdateSalesInvoice([Body] SalesInvoiceDTO salesInvoice);
    [Delete("/api/salesinvoice/{id}")]
    Task<OutputResponse> DeleteSalesInvoice(int id);
    [Get("/api/salesinvoice/nextcode")]
    Task<string> GetNextInvoiceCode();
    [Get("/api/salesinvoice/oldest-unpaid-date/{customerId}")]
    Task<DateTime?> GetOldestUnpaidInvoiceDate(int customerId);
    #endregion

    #region Sales Invoice Hold
    [Get("/api/salesinvoicehold/{id}")]
    Task<SalesInvoiceHoldDTO> GetSalesInvoiceHoldById(int id);
    [Post("/api/salesinvoicehold/pagination")]
    Task<TransactionPDTO> GetPSalesInvoiceHolds([Body] PaginationModel pmodel);
    [Post("/api/salesinvoicehold")]
    Task<OutputResponse> CreateSalesInvoiceHold([Body] SalesInvoiceHoldDTO salesInvoiceHold);
    [Delete("/api/salesinvoicehold/{id}")]
    Task<OutputResponse> DeleteSalesInvoiceHold(int id);
    [Delete("/api/salesinvoicehold/all/{branchId}")]
    Task<OutputResponse> DeleteAllSalesInvoiceHold(int branchId, [Query] string? terminal = null);
    [Get("/api/salesinvoicehold/bybranch/{branchId}")]
    Task<List<SalesInvoiceHoldHdr>> GetAllSalesInvoiceHold(int branchId, [Query] string? terminal = null);
    #endregion

    #region Sales Invoice Return
    [Get("/api/salesinvoicertn/NextNo/{id}")]
    Task<int> GetNewSalesInvoiceRtnNo(int id);
    [Get("/api/salesinvoicertn/{id}")]
    Task<SalesInvoiceRtnDTO> GetSalesInvoiceRtnById(int id);
    [Get("/api/salesinvoicertn/pagination")]
    Task<TransactionPDTO> GetPSalesInvoiceRtns([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/salesinvoicertn")]
    Task<OutputResponse> CreateSalesInvoiceRtn([Body] SalesInvoiceRtnDTO salesInvoiceRtn);
    [Put("/api/salesinvoicertn")]
    Task<OutputResponse> UpdateSalesInvoiceRtn([Body] SalesInvoiceRtnDTO salesInvoiceRtn);
    [Delete("/api/salesinvoicertn/{id}")]
    Task<OutputResponse> DeleteSalesInvoiceRtn(int id);
    [Get("/api/salesinvoicertn/nextcode")]
    Task<string> GetNextSalesInvoiceRtnCode();
    #endregion

    #region Sales Order
    [Get("/api/salesorder/NextNo/{id}")]
    Task<int> GetNewSalesOrderNo(int id);
    [Get("/api/salesorder/{id}")]
    Task<SalesOrderDTO> GetSalesOrderById(int id);
    [Get("/api/salesorder/pagination")]
    Task<TransactionPDTO> GetPSalesOrders([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/salesorder")]
    Task<OutputResponse> CreateSalesOrder([Body] SalesOrderDTO salesOrder);
    [Put("/api/salesorder")]
    Task<OutputResponse> UpdateSalesOrder([Body] SalesOrderDTO salesOrder);
    [Delete("/api/salesorder/{id}")]
    Task<OutputResponse> DeleteSalesOrder(int id);
    [Get("/api/salesorder/nextcode")]
    Task<string> GetNextSalesOrderCode();
    #endregion

    #region Delivery Note
    [Get("/api/deliverynote/NextNo/{id}")]
    Task<int> GetNewDeliveryNoteNo(int id);
    [Get("/api/deliverynote/{id}")]
    Task<DeliveryNoteDTO> GetDeliveryNoteById(int id);
    [Get("/api/deliverynote/pagination")]
    Task<TransactionPDTO> GetPDeliveryNotes([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/deliverynote")]
    Task<OutputResponse> CreateDeliveryNote([Body] DeliveryNoteDTO deliveryNote);
    [Put("/api/deliverynote")]
    Task<OutputResponse> UpdateDeliveryNote([Body] DeliveryNoteDTO deliveryNote);
    [Delete("/api/deliverynote/{id}")]
    Task<OutputResponse> DeleteDeliveryNote(int id);
    [Get("/api/deliverynote/nextcode")]
    Task<string> GetNextDeliveryNoteCode();
    [Get("/api/deliverynote/pending/{customerId}")]
    Task<List<PendingDeliveryNoteDTO>> GetPendingDeliveryNotes(int customerId);
    #endregion

    #region Delivery Note Return
    [Get("/api/dnrtn/nextno/{id}")]
    Task<int> GetNewDeliveryNoteRtnNo(int id);
    [Get("/api/dnrtn/{id}")]
    Task<DeliveryNoteRtnDTO> GetDeliveryNoteRtnById(int id);
    [Get("/api/dnrtn/pagination")]
    Task<TransactionPDTO> GetPDeliveryNoteRtns([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/dnrtn")]
    Task<OutputResponse> CreateDeliveryNoteRtn([Body] DeliveryNoteRtnDTO deliveryNoteRtn);
    [Put("/api/dnrtn")]
    Task<OutputResponse> UpdateDeliveryNoteRtn([Body] DeliveryNoteRtnDTO deliveryNoteRtn);
    [Delete("/api/dnrtn/{id}")]
    Task<OutputResponse> DeleteDeliveryNoteRtn(int id);
    #endregion

    #region Proforma Invoice
    [Get("/api/proformainvoice/NextNo/{id}")]
    Task<string> GetNewProformaInvoiceNo(int id);
    [Get("/api/proformainvoice/{id}")]
    Task<ProformaInvoiceDTO> GetProformaInvoiceById(int id);
    [Get("/api/proformainvoice/pagination")]
    Task<TransactionPDTO> GetPProformaInvoices([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/proformainvoice")]
    Task<OutputResponse> CreateProformaInvoice([Body] ProformaInvoiceDTO proformaInvoice);
    [Put("/api/proformainvoice")]
    Task<OutputResponse> UpdateProformaInvoice([Body] ProformaInvoiceDTO proformaInvoice);
    [Delete("/api/proformainvoice/{id}")]
    Task<OutputResponse> DeleteProformaInvoice(int id);
    [Get("/api/proformainvoice/nextcode")]
    Task<string> GetNextProformaInvoiceCode();
    #endregion

    #endregion

    #region Purchase

    #region Purchase Order
    [Get("/api/purchaseorder/NextNo/{id}")]
    Task<int> GetNewPurchaseOrderNo(int id);
    [Get("/api/purchaseorder/{id}")]
    Task<PurchaseOrderDTO> GetPurchaseOrderById(int id);
    [Get("/api/purchaseorder/pagination")]
    Task<TransactionPDTO> GetPPurchaseOrders([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/purchaseorder")]
    Task<OutputResponse> CreatePurchaseOrder([Body] PurchaseOrderDTO purchaseOrder);
    [Put("/api/purchaseorder")]
    Task<OutputResponse> UpdatePurchaseOrder([Body] PurchaseOrderDTO purchaseOrder);
    [Delete("/api/purchaseorder/{id}")]
    Task<OutputResponse> DeletePurchaseOrder(int id);
    [Get("/api/purchaseorder/nextcode")]
    Task<string> GetNextPurchaseOrderCode();
    #endregion

    #region Purchase
    [Get("/api/purchase/NextNo/{id}")]
    Task<int> GetNewPurchaseNo(int id);

    [Get("/api/purchase/{id}")]
    Task<PurchaseDTO> GetPurchaseById(int id);

    [Get("/api/purchase/pagination")]
    Task<TransactionPDTO> GetPPurchases([AliasAs("pmodel")] PaginationModel pmodel);

    [Post("/api/purchase")]
    Task<OutputResponse> CreatePurchase([Body] PurchaseDTO purchase);

    [Put("/api/purchase")]
    Task<OutputResponse> UpdatePurchase([Body] PurchaseDTO purchase);

    [Delete("/api/purchase/{id}")]
    Task<OutputResponse> DeletePurchase(int id);

    [Get("/api/purchase/nextcode")]
    Task<string> GetNextPurchaseCode();
    #endregion

    #region Goods Receipt
    [Get("/api/goodsreceipt/NextNo/{id}")]
    Task<string> GetNewGoodsReceiptNo(int id);
    [Get("/api/goodsreceipt/nextcode")]
    Task<string> GetNextGoodsReceiptCode();
    [Get("/api/goodsreceipt/{id}")]
    Task<GoodsReceiptDTO> GetGoodsReceiptById(int id);
    [Get("/api/goodsreceipt/pagination")]
    Task<TransactionPDTO> GetPGoodsReceipts([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/goodsreceipt")]
    Task<OutputResponse> CreateGoodsReceipt([Body] GoodsReceiptDTO goodsReceipt);
    [Put("/api/goodsreceipt")]
    Task<OutputResponse> UpdateGoodsReceipt([Body] GoodsReceiptDTO goodsReceipt);
    [Delete("/api/goodsreceipt/{id}")]
    Task<OutputResponse> DeleteGoodsReceipt(int id);
    #endregion

    #region Enquiry
    [Get("/api/enquiry/NextNo/{branchId}")]
    Task<string> GetNextEnquiryNo(int branchId);

    [Get("/api/enquiry/{id}")]
    Task<RFQHdr> GetEnquiryById(int id);

    [Get("/api/enquiry")]
    Task<List<RFQHdr>> GetAllEnquiries();

    [Post("/api/enquiry")]
    Task<string> CreateEnquiry([Body] RFQHdr rfqHdr);

    [Put("/api/enquiry")]
    Task<string> UpdateEnquiry([Body] RFQHdr rfqHdr);

    [Delete("/api/enquiry/{id}")]
    Task<bool> DeleteEnquiry(int id);
    #endregion

    #region Purchase Return
    [Get("/api/purchasereturn/NextNo/{id}")]
    Task<int> GetNewPurchaseReturnNo(int id);
    [Get("/api/purchasereturn/{id}")]
    Task<PurchaseReturnDTO> GetPurchaseReturnById(int id);
    [Get("/api/purchasereturn/pagination")]
    Task<TransactionPDTO> GetPPurchaseReturns([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/purchasereturn")]
    Task<OutputResponse> CreatePurchaseReturn([Body] PurchaseReturnDTO purchaseReturn);
    [Put("/api/purchasereturn")]
    Task<OutputResponse> UpdatePurchaseReturn([Body] PurchaseReturnDTO purchaseReturn);
    [Delete("/api/purchasereturn/{id}")]
    Task<OutputResponse> DeletePurchaseReturn(int id);
    [Get("/api/purchasereturn/nextcode")]
    Task<string> GetNextPurchaseReturnCode();
    [Get("/api/purchasereturn/balance/{purchaseId}")]
    Task<List<TransactionDtls>> GetPurchaseReturnBalance(int purchaseId);
    #endregion

    #region Damage Stock
    [Get("/api/damagestock/NextNo/{id}")]
    Task<string> GetNextDamageStockDocNo(int id);
    [Get("/api/damagestock/{id}")]
    Task<DamageStockDTO> GetDamageStockById(int id);
    [Get("/api/damagestock/pagination")]
    Task<TransactionPDTO> GetPDamageStocks([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/damagestock")]
    Task<OutputResponse> CreateDamageStock([Body] DamageStockDTO damageStock);
    [Put("/api/damagestock")]
    Task<OutputResponse> UpdateDamageStock([Body] DamageStockDTO damageStock);
    [Delete("/api/damagestock/{id}")]
    Task<OutputResponse> DeleteDamageStock(int id);
    #endregion

    #region Supplier Payment
    [Get("/api/supplierpayment/NextNo/{id}")]
    Task<string> GetNewSupplierPaymentNo(int id);
    [Get("/api/supplierpayment/{id}")]
    Task<SupplierPaymentDTO> GetSupplierPaymentById(int id);
    [Get("/api/supplierpayment/pagination")]
    Task<SupplierPaymentPDTO> GetPSupplierPayments([AliasAs("pmodel")] PaginationModel pmodel);
    [Post("/api/supplierpayment")]
    Task<OutputResponse> CreateSupplierPayment([Body] SupplierPaymentDTO supplierPayment);
    [Put("/api/supplierpayment")]
    Task<OutputResponse> UpdateSupplierPayment([Body] SupplierPaymentDTO supplierPayment);
    [Delete("/api/supplierpayment/{id}")]
    Task<OutputResponse> DeleteSupplierPayment(int id);
    [Get("/api/supplierpayment/pendinginvoices/{supplierId}")]
    Task<List<SupplierPaymentDtls>> GetPendingPurchaseInvoices(int supplierId);

    [Get("/api/supplierpayment/supplier/{supplierId}/balance")]
    Task<decimal> GetSupplierBalance(int supplierId, [AliasAs("branchId")] int branchId);
    #endregion



    #endregion


    #region Journal Entry
    [Get("/api/journalentry")]
    Task<List<AshproSAASERP.Models.Transactions.Accounts.JournalEntryHdr>> GetAllJournalEntries();

    [Get("/api/journalentry/{id}")]
    Task<AshproSAASERP.Models.Transactions.Accounts.JournalEntryDTO> GetJournalEntryById(int id);

    [Post("/api/journalentry")]
    Task<OutputResponse> CreateJournalEntry([Body] AshproSAASERP.Models.Transactions.Accounts.JournalEntryDTO entry);

    [Put("/api/journalentry")]
    Task<OutputResponse> UpdateJournalEntry([Body] AshproSAASERP.Models.Transactions.Accounts.JournalEntryDTO entry);

    [Delete("/api/journalentry/{id}")]
    Task<OutputResponse> DeleteJournalEntry(int id);

    [Get("/api/journalentry/nextno")]
    Task<string> GetNextJournalNo();
    #endregion


    #region Expense Entry
    [Get("/api/expenseentry")]
    Task<List<ExpenseEntryHdr>> GetAllExpenseEntries();
    [Get("/api/expenseentry/{id}")]
    Task<ExpenseEntryDTO> GetExpenseEntryById(int id);
    [Post("/api/expenseentry")]
    Task<OutputResponse> CreateExpenseEntry([Body] ExpenseEntryDTO entry);
    [Put("/api/expenseentry")]
    Task<OutputResponse> UpdateExpenseEntry([Body] ExpenseEntryDTO entry);
    [Delete("/api/expenseentry/{id}")]
    Task<OutputResponse> DeleteExpenseEntry(int id);
    [Get("/api/expenseentry/nextcode")]
    Task<string> GetNextExpenseNo();
    [Post("/api/expenseentry/pagination")]
    Task<TransactionPDTO> GetExpenseEntryPDetails([Body] PaginationModel pmodel);


    #endregion

    #region Zatca
    [Post("/api/zatca/pendingcount")]
    Task<PendingIcvData> GetPendingICVData([Body] ICVReqDTO entry);
    [Post("/api/zatca/pendinglist")]
    Task<IcvPendingPDTO> GetPendingICVList([Body] IcvPendReqDTO entry);
    [Post("/api/zatca/all")]
    Task ZatcaReUploadAll([Body] IcvReuploadAll dto);
    [Post("/api/zatca/selected")]
    Task<OutputResponse> ZatcaReUploadSelected([Body] IcvReupload dto);
    [Post("/api/zatca/generatecsr")]
    Task<CSRResponse> GenerateCSR([Body] CSRRequest request);
    [Get("/api/zatca/{id}")]
    Task<CSRResponse> GetCsr(int id);
    [Post("/api/zatca")]
    Task<OutputResponse> AddNewCSR([Body] CSRResponse request);
    [Get("/api/zatca")]
    Task<int> GetFailedCount();
    #endregion

    #region Customer Receipt
    [Get("/api/customerreceipt")]
    Task<List<AshproSAASERP.Models.Transactions.Accounts.CustomerReceiptHdr>> GetAllCustomerReceipts();

    [Get("/api/customerreceipt/{id}")]
    Task<AshproSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO> GetCustomerReceiptById(int id);

    [Post("/api/customerreceipt")]
    Task<OutputResponse> CreateCustomerReceipt([Body] AshproSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt);

    [Put("/api/customerreceipt")]
    Task<OutputResponse> UpdateCustomerReceipt([Body] AshproSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt);

    [Delete("/api/customerreceipt/{id}")]
    Task<OutputResponse> DeleteCustomerReceipt(int id);

    [Get("/api/customerreceipt/nextno")]
    Task<string> GetNextCustomerReceiptNo();

    [Get("/api/customerreceipt/customer/{customerId}/invoices")]
    Task<List<AshproSAASERP.Models.DTO.OutstandingInvoiceDTO>> GetCustomerOutstandingInvoices(int customerId, [AliasAs("branchId")] int branchId);

    [Get("/api/customerreceipt/customer/{customerId}/balance")]
    Task<decimal> GetCustomerBalance(int customerId, [AliasAs("branchId")] int branchId);
    #endregion

    #endregion

    #region Accounts
    [Get("/api/account/cashandbank")]
    Task<List<AshproSAASERP.Models.Masters.AccountMaster>> GetCashAndBankAccounts();
    #endregion

    #region Proforma from Quotation

    [Get("/api/proformainvoice/fromquotation/{quotationId}")]
    Task<ProformaInvoiceDTO> CreateProformaFromQuotationAsync(int quotationId);
    #endregion

    #region Reports
    #region Inventory Report
    [Post("/api/inventoryreport/stockreport")]
    Task<StockReportResponse> GetStockReport([Body] StockReportRequest req);

    [Post("/api/inventoryreport/stocktransferreport")]
    Task<StockTransferReportResponse> GetStockTransferReport([Body] StockTransferReportRequest req);

    [Post("/api/inventoryreport/itemtransactionhistory")]
    Task<ItemTransactionHistoryReportResponse> GetItemTransactionHistoryReport([Body] ItemTransactionHistoryReportRequest req);

    [Post("/api/inventoryreport/stockadjustmentreport")]
    Task<StockAdjustmentReportResponse> GetStockAdjustmentReportAsync([Body] StockAdjustmentReportRequest req);
    #endregion

    #region Sales Report
    [Post("/api/salesreport/")]
    Task<SalesReportResponse> GetSalesReport([Body] SalesReportRequest req);
    [Post("/api/salesreport/salessrvrrpt/")]
    Task<ApiResponse<Stream>> GetSalesReportPDF([Body] SalesReportRequest req);

    [Post("/api/salesreport/salesreturn")]
    Task<SalesReturnReportResponse> GetSalesReturnReport([Body] SalesReturnReportRequest req);

    [Post("/api/salesreport/salesreturnsrvrrpt/")]
    Task<ApiResponse<Stream>> GetSalesReturnReportPDF([Body] SalesReturnReportRequest req);

    [Post("/api/salesreport/salesreturndetails")]
    Task<SalesReturnDetailsReportResponse> GetSalesReturnDetailsReport([Body] SalesReturnDetailsReportRequest req);

    [Post("/api/salesreport/salesreturndetailssrvrrpt/")]
    Task<ApiResponse<Stream>> GetSalesReturnDetailsReportPDF([Body] SalesReturnDetailsReportRequest req);

    [Post("/api/salesreport/salesdetailssrvrrpt/")]
    Task<ApiResponse<Stream>> GetSalesDetailsReportPDF([Body] SalesDetailsReportRequest req);

    [Post("/api/salesreport/monitor")]
    Task<SalesMonitorResponse> GetSalesMonitorReportAsync([Body] SalesMonitorRequest req);

    [Get("/api/salesreport/receipts/{invoiceId}")]
    Task<List<SalesMonitorReceiptItem>> GetReceiptsByInvoiceAsync(int invoiceId);

    [Post("/api/salesreport/salesdetails")]
    Task<SalesDetailsReportResponse> GetSalesDetailsReport([Body] SalesDetailsReportRequest req);

    [Post("/api/salesreport/customerwise")]
    Task<CustomerWiseSalesReportResponse> GetCustomerWiseSalesReport([Body] CustomerWiseSalesReportRequest req);
    [Post("/api/salesreport/custwisesalesrpt")]
    Task<ApiResponse<Stream>> GetCustomerWiseSalesReportPDF([Body] CustomerWiseSalesReportRequest req);

    [Post("/api/salesreport/categorywise")]
    Task<CategoryWiseSalesReportResponse> GetCategoryWiseReport([Body] CategoryWiseSalesReportRequest req);

    [Post("/api/salesreport/salesquotationregister")]
    Task<SalesQuotationRegisterResponse> GetSalesQuotationRegisterAsync([Body] SalesQuotationRegisterRequest req);
    [Post("/api/salesreport/salesquotationregsrvrrpt")]
    Task<ApiResponse<Stream>> GetSalesQuotationRegisterReportPDF([Body] SalesQuotationRegisterRequest req);
    [Post("/api/salesreport/customerquotation")]
    Task<CustomerWiseQuotationReportResponse> GetCustomerWiseQuotationReportAsync([Body] CustomerWiseQuotationReportRequest req);
    [Post("/api/salesreport/custwisequotationrpt")]
    Task<ApiResponse<Stream>> GetCustomerWiseQuotationReportPDF([Body] CustomerWiseQuotationReportRequest req);

    [Post("/api/salesreport/deliverynoteregister")]
    Task<DeliveryNoteRegisterResponse> GetDeliveryNoteRegisterAsync([Body] DeliveryNoteRegisterRequest req);
    [Post("/api/salesreport/deliverynoteregistersrvrrpt")]
    Task<ApiResponse<Stream>> GetDeliveryNoteRegisterReportPDF([Body] DeliveryNoteRegisterRequest req);

    [Post("/api/salesreport/datewiseprofit")]
    Task<DateWiseProfitReportResponse> GetDateWiseProfitReport([Body] DateWiseProfitReportRequest req);
    [Post("/api/salesreport/datewiseprofitsrvrrpt")]
    Task<ApiResponse<Stream>> GetDateWiseProfitReportPDF([Body] DateWiseProfitReportRequest req);

    [Post("/api/salesreport/vatreport")]
    Task<VatReportResponse> GetVatReport([Body] VatReportRequest req);
    #endregion

    #region Purchase Report
    [Post("/api/purchasereport/")]
    Task<PurchaseReportResponse> GetPurchaseReport([Body] PurchaseReportRequest req);

    [Post("/api/purchasereport/purchasedetails")]
    Task<PurchaseDetailsReportResponse> GetPurchaseDetailsReport([Body] PurchaseDetailsReportRequest req);

    [Post("/api/purchasereport/purchaseorder")]
    Task<PurchaseOrderReportResponse> GetPurchaseOrderReport([Body] PurchaseOrderReportRequest req);

    [Post("/api/purchasereport/monitor")]
    Task<PurchaseMonitorResponse> GetPurchaseMonitorReportAsync([Body] PurchaseMonitorRequest req);

    [Get("/api/purchasereport/payments/{purchaseId}")]
    Task<List<PurchaseMonitorPaymentItem>> GetPaymentsByPurchaseAsync(int purchaseId);

    [Post("/api/purchasereport/purchasesummarysrvrrpt")]
    Task<ApiResponse<Stream>> GetPurchaseSummaryServerReportPDF([Body] PurchaseReportRequest req);

    [Post("/api/purchasereport/purchasedetailssrvrrpt")]
    Task<ApiResponse<Stream>> GetPurchaseDetailsServerReportPDF([Body] PurchaseDetailsReportRequest req);

    [Post("/api/purchasereport/purchaseorderregistersrvrrpt")]
    Task<ApiResponse<Stream>> GetPurchaseOrderServerReportPDF([Body] PurchaseOrderReportRequest req);

    [Post("/api/purchasereport/purchasereturnreport")]
    Task<PurchaseReturnReportResponse> GetPurchaseReturnReport([Body] PurchaseReturnReportRequest req);

    [Post("/api/purchasereport/purchasereturnsummarysrvrrpt")]
    Task<ApiResponse<Stream>> GetPurchaseReturnSummaryServerReportPDF([Body] PurchaseReturnReportRequest req);

    [Post("/api/purchasereport/purchasereturndetails")]
    Task<PurchaseReturnDetailsReportResponse> GetPurchaseReturnDetailsReport([Body] PurchaseReturnDetailsReportRequest req);

    [Post("/api/purchasereport/purchasereturndetailssrvrrpt")]
    Task<ApiResponse<Stream>> GetPurchaseReturnDetailsServerReportPDF([Body] PurchaseReturnDetailsReportRequest req);
    #endregion

    #region Proforma Invoice Report
    [Post("/api/salesreport/proformainvoice")]
    Task<ProformaInvoiceReportResponse> GetProformaInvoiceReport([Body] ProformaInvoiceReportRequest req);
    [Post("/api/salesreport/proformainvoicesrvrrpt")]
    Task<ApiResponse<Stream>> GetProformaInvoiceReportPDF([Body] ProformaInvoiceReportRequest req);
    #endregion

    #region Sales Order Report
    [Post("/api/salesreport/salesorderreport")]
    Task<SalesOrderReportResponse> GetSalesOrderReport([Body] SalesOrderReportRequest req);
    [Post("/api/salesreport/salesorderregsrvrrpt")]
    Task<ApiResponse<Stream>> GetSalesOrderReportPDF([Body] SalesOrderReportRequest req);
    #endregion

    #region Enquiry Registration Report
    [Post("/api/salesreport/enquiryregistration")]
    Task<EnquiryRegistrationReportResponse> GetEnquiryRegistrationReport([Body] EnquiryRegistrationReportRequest req);
    #endregion

    #region Account Report
    [Get("/api/supplierreport/statement")]
    Task<List<SupplierStatementDTO>> GetSupplierStatementAsync([Query] DateTime fromDate, [Query] DateTime toDate, [Query] int branchId, [Query] int? supplierId = null);

    [Get("/api/supplierreport/summary")]
    Task<List<SupplierSummaryDTO>> GetSupplierSummaryAsync([Query] DateTime fromDate, [Query] DateTime toDate, [Query] int branchId, int? supplierId = null);

    [Get("/api/supplierreport/supplierpendinginvoice")]
    Task<List<SupplierPendingInvoiceDTO>> GetSupplierPendingInvoiceAsync([Query] DateTime fromDate, [Query] DateTime toDate, [Query] int branchId, int? supplierId = null);

    [Get("/api/supplierreport/supplierpayment")]
    Task<List<SupplierPaymentReportDTO>> GetSupplierPaymentReportAsync([Query] DateTime fromDate, [Query] DateTime toDate, [Query] int branchId, int? supplierId, string? billType);

    [Get("/api/supplierreport/list")]
    Task<List<SupplierListDTO>> GetSupplierListAsync();
    #endregion

    #region Customer Report
    [Post("/api/customerreport/statement")]
    Task<CustomerStatementResponse> GetCustomerStatementReport([Body] CustomerStatementRequest req);

    [Get("/api/customerreport/summary")]
    Task<List<CustomerSummaryDTO>> GetCustomerSummaryAsync([Query] DateTime fromDate, [Query] DateTime toDate, [Query] int branchId, [Query] int? customerId = null);

    [Get("/api/customerreport/list")]
    Task<List<CustomerListDTO>> GetCustomerListAsync();

    [Get("/api/customerreport/customerpendinginvoice")]
    Task<List<CustomerPendingInvoiceDTO>> GetCustomerPendingInvoiceAsync([Query] DateTime fromDate, [Query] DateTime toDate, [Query] int branchId, int? customerId = null);
    #endregion

    #region Account Report
    [Post("/api/accountreport/journalentry")]
    Task<JournalEntryReportResponse> GetJournalEntryReport([Body] JournalEntryReportRequest req);

    [Post("/api/accountreport/trialbalancereport")]
    Task<TrialBalanceReportResponse> GetTrialBalanceReport([Body] TrialBalanceReportRequest req);

    [Post("/api/accountreport/expensereport")]
    Task<ExpenseReportResponse> GetExpenseReport([Body] ExpenseReportRequest req);
    [Post("/api/accountreport/profitandlossreport")]
    Task<ProfitAndLossReportResponse> GetProfitAndLossReport(ProfitAndLossReportRequest req);

    [Post("/api/accountreport/balancesheetreport")]
    Task<BalanceSheetReportResponse> GetBalanceSheetReport([Body] BalanceSheetReportRequest req);

    [Post("/api/accountreport/ledgerreport")]
    Task<LedgerReportResponse> GetLedgerReport([Body] LedgerReportRequest req);

    [Post("/api/accountreport/chartofaccount")]
    Task<ChartOfAccountReportResponse> GetChartOfAccountReport([Body] ChartOfAccountReportRequest req);
    #endregion

    #region Transaction Monitor Report
    [Post("/api/transactionreport/monitor")]
    Task<TransactionMonitorResponse> GetTransactionMonitorReportAsync([Body] TransactionMonitorRequest req);
    #endregion
    #endregion

    #region Settings

    #region Barcode Design
    [Get("/api/barcodedesign/{id}")]
    Task<BarcodeDesignDTO> GetBarcodeDesignAsync(int id);
    [Get("/api/barcodedesign")]
    Task<BarcodeDesignDTO> GetBarcodeDesignAsync();
    [Post("/api/barcodedesign")]
    Task<OutputResponse> CreateBarcodeDesignAsync([Body] BarcodeDesignDTO dto);
    [Put("/api/barcodedesign")]
    Task<OutputResponse> UpdateBarcodeDesignAsync([Body] BarcodeDesignDTO dto);
    [Delete("/api/barcodedesign/{id}")]
    Task<OutputResponse> DeleteBarcodeDesignAsync(int id);
    [Post("/api/barcodedesign/pagination")]
    Task<TransactionPDTO> GetBarcodeDesignPDetails([Body] PaginationModel pagination);

    [Get("/api/barcodedesign/all")]
    Task<List<BarcodeDesignHdr>> GetAllBarcodeDesigns();
    #endregion

    #region Print Design
    [Post("/api/printDesign/design")]
    Task<PrintDesignDTO> GetPrintDesignAsync([Body] PrintDesignReq req);
    [Post("/api/printDesign")]
    Task<OutputResponse> CreatePrintDesignAsync([Body] PrintDesignDTO dto);
    [Put("/api/printDesign")]
    Task<OutputResponse> UpdatePrintDesignAsync([Body] PrintDesignDTO dto);
    [Delete("/api/printDesign/{id}")]
    Task<OutputResponse> DeletePrintDesignAsync(int id);
    [Post("/api/printDesign/pagination")]
    Task<TransactionPDTO> GetPrintDesignPDetails([Body] PaginationModel pmodel);
    #endregion

    #endregion

    #region User Privilege
    [Get("/api/userprivileges")]
    Task<List<UserPrivilegeHdr>> GetUserprivileges();
    [Get("/api/userprivileges/{id}")]
    Task<UserPrivilegeDTO> GetUserprivilege(int id);
    [Get("/api/userprivileges/web")]
    Task<List<WebUserPrivilege>> GetWebUserprivilege([AliasAs("sid")] PrivilegeDTO dto);

    [Post("/api/userprivileges")]
    Task<OutputResponse> NewUserprivilege([Body] UserPrivilegeDTO entity);

    [Put("/api/userprivileges")]
    Task<OutputResponse> UpdateUserprivilege([Body] UserPrivilegeDTO entity);

    [Delete("/api/userprivileges/{id}")]
    Task<OutputResponse> DeleteUserprivilege(int id);
    [Post("/api/userprivileges/pagination")]
    Task<TransactionPDTO> GetUserPrivilegePDTO([Body] PaginationModel pmodel);
    #endregion

    #region Zatca
    [Get("/api/zatcapending")]
    Task<ZatcaInvoiceDTO> GetZatcaPendingInvoicesAsync([Query] PaginationModel pmodel);
    [Get("/api/zatcapending")]
    Task UploadPendingSimplifiedInvoices([Query] int BranchId);
    #endregion

    #region ProductImport
    [Post("/api/productimport/import")]
    Task<ProductImportResultDTO> ImportProductsFromCsvAsync([Body] List<ProductImportRowDTO> rows);

    [Get("/api/productimport/sample")]
    Task<byte[]> GetProductImportSampleCsvAsync();
    #endregion

    #region ProductExport
    [Get("/api/productexport/list")]
    Task<List<ProductExportRowDTO>> GetProductsForExportAsync();

    [Get("/api/productexport/csv")]
    Task<byte[]> ExportProductsToCsvAsync();
    #endregion
}
