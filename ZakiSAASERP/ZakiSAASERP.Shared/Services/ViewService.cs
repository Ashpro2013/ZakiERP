using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Transactions.Inventory;
using ZakiSAASERP.Models.Transactions.Purchase;
using ZakiSAASERP.Models.Transactions.Workshop;
using ZakiSAASERP.Models.User;
using ZakiSAASERP.Models.ViewModel;
using Refit;
namespace ZakiSAASERP.Shared.Services;

public class ViewService(IApiAccessor service) : IViewService
{

    #region mastersa

    #region Account Head
    public async Task<OutputResponse> CreateAccountHead(AccountHeadMaster accountHead) => await service.Api.CreateAccountHead(accountHead);
    public async Task<OutputResponse> DeleteAccountHead(int id) => await service.Api.DeleteAccountHead(id);
    public async Task<List<AccountHeadMaster>> GetAllAccountHeads() => await service.Api.GetAllAccountHeads();
    public async Task<AccountHeadMaster> GetAccountHeadById(int id) => await service.Api.GetAccountHeadById(id);
    public async Task<OutputResponse> UpdateAccountHead(AccountHeadMaster accountHead) => await service.Api.UpdateAccountHead(accountHead);
    public async Task<OutputResponse> GetAccountHeadNextCode(int typeId) => await service.Api.GetAccountHeadNextCode(typeId);
    #endregion

    #region AccountGroup
    public async Task<OutputResponse> CreateAccountGroup(AccountGroup accountGroup) => await service.Api.CreateAccountGroup(accountGroup);
    public async Task<OutputResponse> DeleteAccountGroup(int id) => await service.Api.DeleteAccountGroup(id);
    public async Task<List<AccountGroup>> GetAllAccountGroups() => await service.Api.GetAllAccountGroups();
    public async Task<AccountGroup> GetAccountGroupById(int id) => await service.Api.GetAccountGroupById(id);
    public async Task<OutputResponse> UpdateAccountGroup(AccountGroup accountGroup) => await service.Api.UpdateAccountGroup(accountGroup);
    public async Task<OutputResponse> GetAccountGroupNextCode(int accountHeadId) => await service.Api.GetAccountGroupNextCode(accountHeadId);
    #endregion

    #region AccountSubGroup
    public async Task<OutputResponse> CreateAccountSubGroup(AccountSubGroup accountSubGroup) => await service.Api.CreateAccountSubGroup(accountSubGroup);
    public async Task<OutputResponse> DeleteAccountSubGroup(int id) => await service.Api.DeleteAccountSubGroup(id);
    public async Task<List<AccountSubGroup>> GetAllAccountSubGroups() => await service.Api.GetAllAccountSubGroups();
    public async Task<AccountSubGroup> GetAccountSubGroupById(int id) => await service.Api.GetAccountSubGroupById(id);
    public async Task<OutputResponse> UpdateAccountSubGroup(AccountSubGroup accountSubGroup) => await service.Api.UpdateAccountSubGroup(accountSubGroup);
    public async Task<OutputResponse> GetAccountSubGroupNextCode(int accountGroupId) => await service.Api.GetAccountSubGroupNextCode(accountGroupId);
    #endregion

    #region Account
    public Task<List<AccountMaster>> GetAllAccounts() => service.Api.GetAllAccounts();
    public Task<List<AccountType>> GetAllAccountTypes() => service.Api.GetAllAccountTypes();
    public async Task<AccountMaster> GetAccountMasterById(int id) => await service.Api.GetAccountById(id);
    public async Task<OutputResponse> CreateAccountMaster(AccountMaster accountMaster) => await service.Api.CreateAccount(accountMaster);
    public async Task<OutputResponse> UpdateAccountMaster(AccountMaster accountMaster) => await service.Api.UpdateAccount(accountMaster);
    public async Task<OutputResponse> DeleteAccountMaster(int id) => await service.Api.DeleteAccountMaster(id);
    public async Task<OutputResponse> GetAccountMasterNextCode(int subGroupId) => await service.Api.GetAccountMasterNextCode(subGroupId);
    #endregion

    #region Branch
    public async Task<OutputResponse> CreateBranch(BranchMaster branch) => await service.Api.CreateBranch(branch);
    public async Task<OutputResponse> DeleteBranch(int id) => await service.Api.DeleteBranch(id);
    public async Task<List<BranchMaster>> GetAllBranches() => await service.Api.GetAllBranches();
    public async Task<BranchMaster> GetBranchById(int id) => await service.Api.GetBranchById(id);
    public async Task<OutputResponse> UpdateBranch(BranchMaster branch) => await service.Api.UpdateBranch(branch);
    #endregion

    #region FinancialYear
    public async Task<OutputResponse> CreateFinancialYear(FinancialYear financialYear) => await service.Api.CreateFinancialYear(financialYear);
    public async Task<OutputResponse> DeleteFinancialYear(int id) => await service.Api.DeleteFinancialYear(id);
    public async Task<List<FinancialYear>> GetAllFinancialYears() => await service.Api.GetAllFinancialYears();
    public async Task<FinancialYear> GetFinancialYearById(int id) => await service.Api.GetFinancialYearById(id);
    public async Task<OutputResponse> UpdateFinancialYear(FinancialYear financialYear) => await service.Api.UpdateFinancialYear(financialYear);
    public async Task<int> GetMaxRefNo() => await service.Api.GetMaxRefNo();
    #endregion

    #region User
    public async Task<LoginResponse> Login(LoginRequest request) => await service.Api.Login(request);
    public async Task<PreLoginDatas> GetPreLoginDetails(string tenant, string username) => await service.Api.GetPreLoginDetails(tenant, username);
    #endregion

    #region Unit
    public async Task<OutputResponse> CreateUnit(UnitMaster unit)
    {
        var data = await service.Api.CreateUnit(unit);
        return data;
    }
    public async Task<OutputResponse> DeleteUnit(int id) => await service.Api.DeleteUnit(id);
    public async Task<List<UnitMaster>> GetAllUnits() => await service.Api.GetAllUnits();
    public async Task<UnitMaster> GetUnitById(int id) => await service.Api.GetUnitById(id);
    public async Task<OutputResponse> UpdateUnit(UnitMaster unit) => await service.Api.UpdateUnit(unit);
    public async Task<UnitPDTO> GetUnitPDetails(PaginationModel pagination) => await service.Api.GetPUnits(pagination);

    // General
    public async Task<decimal> GetItemCurrentStock(int itemId, int storeId, int excludeVoucherId = 0) => await service.Api.GetItemCurrentStock(itemId, storeId, excludeVoucherId);
    #endregion

    #region User Management
    public async Task<List<ZakiSAASERP.Models.User.UserDetails>> GetAllUsers() => await service.Api.GetAllUsers();
    public async Task<ZakiSAASERP.Models.User.UserDetails> GetUserById(int id) => await service.Api.GetUserById(id);
    public async Task<OutputResponse> CreateUser(ZakiSAASERP.Models.User.UserDetails user) => await service.Api.CreateUser(user);
    public async Task<OutputResponse> UpdateUser(ZakiSAASERP.Models.User.UserDetails user) => await service.Api.UpdateUser(user);
    public async Task<OutputResponse> DeleteUser(int id) => await service.Api.DeleteUser(id);
    public async Task<UserPDTO> GetUserPDetails(PaginationModel pagination) => await service.Api.GetPUsers(pagination);
    public async Task<List<ZakiSAASERP.Models.Masters.BranchMaster>> GetUserBranches(string userName) => await service.Api.GetUserBranches(userName);
    public async Task<OutputResponse> SaveUserBranches(List<ZakiSAASERP.Models.User.UserBranch> userBranches) => await service.Api.SaveUserBranches(userBranches);
    #endregion

    #region Pagination
    public async Task<AccountHeadPDTO> GetAccountHeadPDetails(PaginationModel pagination) => await service.Api.GetPAccountHeads(pagination);
    public async Task<AccountPDTO> GetAccountPDetails(PaginationModel pagination) => await service.Api.GetPAccounts(pagination);
    public async Task<BranchPDTO> GetBranchPDetails(PaginationModel pagination) => await service.Api.GetPBranches(pagination);
    public async Task<FinancialYearPDTO> GetFinancialYearPDetails(PaginationModel pagination) => await service.Api.GetPFinancialYears(pagination);
    public async Task<SalesManPDTO> GetSalesManPDetails(PaginationModel pagination) => await service.Api.GetPSalesMen(pagination);
    public async Task<CategoryPDTO> GetCategoryPDetails(PaginationModel pagination) => await service.Api.GetPCategories(pagination);
    public async Task<SubCategoryPDTO> GetSubCategoryPDetails(PaginationModel pagination) => await service.Api.GetPSubCategories(pagination);
    public async Task<CustomerGroupPDTO> GetCustomerGroupPDetails(PaginationModel pagination) => await service.Api.GetPCustomerGroups(pagination);
    public async Task<CountryPDTO> GetCountryPDetails(PaginationModel pagination) => await service.Api.GetPCountries(pagination);
    public async Task<CustomerLocationPDTO> GetCustomerLocationPDetails(PaginationModel pagination) => await service.Api.GetPCustomerLocations(pagination);
    public async Task<VatPDTO> GetVatPDetails(PaginationModel pagination) => await service.Api.GetPVats(pagination);
    public async Task<BrandPDTO> GetBrandPDetails(PaginationModel pagination) => await service.Api.GetPBrands(pagination);
    public async Task<CurrencyPDTO> GetCurrencyPDetails(PaginationModel pagination) => await service.Api.GetPCurrencies(pagination);
    public async Task<PurchaserPDTO> GetPurchaserPDetails(PaginationModel pagination) => await service.Api.GetPPurchasers(pagination);

    public async Task<ProjectPDTO> GetProjectPDetails(PaginationModel pagination) => await service.Api.GetPProjects(pagination);
    public async Task<AccountGroupPDTO> GetAccountGroupPDetails(PaginationModel pagination) => await service.Api.GetPAccountGroups(pagination);
    public async Task<AccountSubGroupPDTO> GetAccountSubGroupPDetails(PaginationModel pagination) => await service.Api.GetPAccountSubGroups(pagination);



    #endregion

    #region SalesMan
    public async Task<List<SalesManMaster>> GetAllSalesMan() => await service.Api.GetAllSalesMan();
    public async Task<SalesManMaster> GetSalesManById(int id) => await service.Api.GetSalesManById(id);
    public async Task<OutputResponse> CreateSalesMan(SalesManMaster salesMan) => await service.Api.CreateSalesMan(salesMan);
    public async Task<OutputResponse> UpdateSalesMan(SalesManMaster salesMan) => await service.Api.UpdateSalesMan(salesMan);
    public async Task<OutputResponse> DeleteSalesMan(int id) => await service.Api.DeleteSalesMan(id);
    #endregion

    #region Category
    public async Task<List<CategoryMaster>> GetAllCategory() => await service.Api.GetAllCategory();
    public async Task<CategoryMaster> GetCategoryById(int id) => await service.Api.GetCategoryById(id);
    public async Task<OutputResponse> CreateCategory(CategoryMaster Category) => await service.Api.CreateCategory(Category);
    public async Task<OutputResponse> UpdateCategory(CategoryMaster Category) => await service.Api.UpdateCategory(Category);
    public async Task<OutputResponse> DeleteCategory(int id) => await service.Api.DeleteCategory(id);
    public async Task<string> GetNextCategoryCode() => await service.Api.GetNextCategoryCode();
    public async Task<string> UploadImage(byte[] imageBytes, string fileName, string entityType, int entityId, string basePath, string clientFolder)
    {
        using var stream = new MemoryStream(imageBytes);
        var streamPart = new StreamPart(stream, fileName, "application/octet-stream");
        return await service.Api.UploadImage(streamPart, entityType, entityId, basePath, clientFolder);
    }
    public async Task<string> GetImageUrl(string basePath, string imagePath)
    {
        // Sanitize inputs
        basePath = basePath?.Trim('"').Replace("\\", "/") ?? "";
        imagePath = imagePath?.Trim('"').Replace("\\", "/") ?? "";

        // Use Base64 encoding to strictly avoid URL invalid character issues (like ':' or '\' or ' ')
        var b64Base = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(basePath));
        var b64Image = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(imagePath));

        // Use static settings loaded at startup
        var extraParams = "";
        if (PermanentSettingsData.IsLoaded)
        {
            extraParams = $"&mode={PermanentSettingsData.ImageStorageMode}";
            if (PermanentSettingsData.ImageStorageMode == 1) // AWS S3
            {
                var b64Region = !string.IsNullOrEmpty(PermanentSettingsData.ImageAwsRegion)
                    ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(PermanentSettingsData.ImageAwsRegion))
                    : "";
                var b64Bucket = !string.IsNullOrEmpty(PermanentSettingsData.ImageAwsBucket)
                    ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(PermanentSettingsData.ImageAwsBucket))
                    : "";
                var b64AccessKey = !string.IsNullOrEmpty(PermanentSettingsData.ImageAwsAccessKey)
                    ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(PermanentSettingsData.ImageAwsAccessKey))
                    : "";
                var b64SecretKey = !string.IsNullOrEmpty(PermanentSettingsData.ImageAwsSecretKey)
                    ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(PermanentSettingsData.ImageAwsSecretKey))
                    : "";
                extraParams += $"&region={b64Region}&bucket={b64Bucket}&accessKey={b64AccessKey}&secretKey={b64SecretKey}";
            }
        }

        return await Task.FromResult($"/api/image/fetch?basePath={b64Base}&imagePath={b64Image}{extraParams}");
    }

    public async Task<bool> DeleteImage(string basePath, string imagePath, string clientFolder)
    {
        try
        {
            // Sanitize & Encode
            basePath = basePath?.Trim('"').Replace("\\", "/") ?? "";
            imagePath = imagePath?.Trim('"').Replace("\\", "/") ?? "";
            clientFolder = clientFolder?.Trim('"') ?? "";

            var b64Base = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(basePath));
            var b64Image = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(imagePath));
            var b64Client = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(clientFolder));

            await service.Api.DeleteImage(b64Base, b64Image, b64Client);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    #endregion

    #region RepairMaster
    public async Task<List<RepairMaster>> GetAllRepairs() => await service.Api.GetAllRepairs();
    public async Task<RepairMaster> GetRepairById(int id) => await service.Api.GetRepairById(id);
    public async Task<RepairPDTO> GetRepairPDetails(PaginationModel pagination) => await service.Api.GetPRepairs(pagination);
    public async Task<OutputResponse> CreateRepair(RepairMaster repair) => await service.Api.CreateRepair(repair);
    public async Task<OutputResponse> UpdateRepair(RepairMaster repair) => await service.Api.UpdateRepair(repair);
    public async Task<OutputResponse> DeleteRepair(int id) => await service.Api.DeleteRepair(id);
    #endregion

    #region TechnicianMaster
    public async Task<List<TechnicianMaster>> GetAllTechnicians() => await service.Api.GetAllTechnicians();
    public async Task<TechnicianMaster> GetTechnicianById(int id) => await service.Api.GetTechnicianById(id);
    public async Task<TechnicianPDTO> GetTechnicianPDetails(PaginationModel pagination) => await service.Api.GetPTechnicians(pagination);
    public async Task<OutputResponse> CreateTechnician(TechnicianMaster technician) => await service.Api.CreateTechnician(technician);
    public async Task<OutputResponse> UpdateTechnician(TechnicianMaster technician) => await service.Api.UpdateTechnician(technician);
    public async Task<OutputResponse> DeleteTechnician(int id) => await service.Api.DeleteTechnician(id);
    #endregion

    #region SubCategory
    public async Task<List<SubCategoryMaster>> GetAllSubCategory() => await service.Api.GetAllSubCategory();
    public async Task<SubCategoryMaster> GetSubCategoryById(int id) => await service.Api.GetSubCategoryById(id);
    public async Task<OutputResponse> CreateSubCategory(SubCategoryMaster SubCategory) => await service.Api.CreateSubCategory(SubCategory);
    public async Task<OutputResponse> UpdateSubCategory(SubCategoryMaster SubCategory) => await service.Api.UpdateSubCategory(SubCategory);
    public async Task<OutputResponse> DeleteSubCategory(int id) => await service.Api.DeleteSubCategory(id);
    public async Task<string> GetNextSubCategoryCode(int categoryId, string? categoryCode) => await service.Api.GetNextSubCategoryCode(categoryId, categoryCode);
    #endregion

    #region Vehicle
    public async Task<List<VehicleMaster>> GetAllVehicles() => await service.Api.GetAllVehicles();
    public async Task<VehicleMaster> GetVehicleById(int id) => await service.Api.GetVehicleById(id);
    public async Task<OutputResponse> CreateVehicle(VehicleMaster vehicle) => await service.Api.CreateVehicle(vehicle);
    public async Task<OutputResponse> UpdateVehicle(VehicleMaster vehicle) => await service.Api.UpdateVehicle(vehicle);
    public async Task<OutputResponse> DeleteVehicle(int id) => await service.Api.DeleteVehicle(id);
    public async Task<VehiclePDTO> GetVehiclePDetails(PaginationModel pagination) => await service.Api.GetPVehicles(pagination);
    #endregion

    #region Vat
    public async Task<List<VatMaster>> GetAllVat() => await service.Api.GetAllVat();
    public async Task<VatMaster> GetVatById(int id) => await service.Api.GetVatById(id);
    public async Task<OutputResponse> CreateVat(VatMaster vat) => await service.Api.CreateVat(vat);
    public async Task<OutputResponse> UpdateVat(VatMaster vat) => await service.Api.UpdateVat(vat);
    public async Task<OutputResponse> DeleteVat(int id) => await service.Api.DeleteVat(id);
    #endregion

    #region VatCategoryDetails
    public async Task<List<VATCategoryDetails>> GetVATCategoryDetails(string code) => await service.Api.GetVATCategoryDetails(code);
    #endregion VatCategoryDetails

    #region CustomerGroup
    public async Task<List<CustomerGroupMaster>> GetAllCustomerGroup() => await service.Api.GetAllCustomerGroup();
    public async Task<CustomerGroupMaster> GetCustomerGroupById(int id) => await service.Api.GetCustomerGroupById(id);
    public async Task<OutputResponse> CreateCustomerGroup(CustomerGroupMaster CustomerGroup) => await service.Api.CreateCustomerGroup(CustomerGroup);
    public async Task<OutputResponse> UpdateCustomerGroup(CustomerGroupMaster CustomerGroup) => await service.Api.UpdateCustomerGroup(CustomerGroup);
    public async Task<OutputResponse> DeleteCustomerGroup(int id) => await service.Api.DeleteCustomerGroup(id);
    #endregion

    #region Country
    public async Task<List<CountryMaster>> GetAllCountry() => await service.Api.GetAllCountry();
    public async Task<CountryMaster> GetCountryById(int id) => await service.Api.GetCountryById(id);
    public async Task<OutputResponse> CreateCountry(CountryMaster Country) => await service.Api.CreateCountry(Country);
    public async Task<OutputResponse> UpdateCountry(CountryMaster Country) => await service.Api.UpdateCountry(Country);
    public async Task<OutputResponse> DeleteCountry(int id) => await service.Api.DeleteCountry(id);
    #endregion

    #region CustomerLocation
    public async Task<List<CustomerLocationMaster>> GetAllCustomerLocation() => await service.Api.GetAllCustomerLocation();
    public async Task<CustomerLocationMaster> GetCustomerLocationById(int id) => await service.Api.GetCustomerLocationById(id);
    public async Task<OutputResponse> CreateCustomerLocation(CustomerLocationMaster CustomerLocation) => await service.Api.CreateCustomerLocation(CustomerLocation);
    public async Task<OutputResponse> UpdateCustomerLocation(CustomerLocationMaster CustomerLocation) => await service.Api.UpdateCustomerLocation(CustomerLocation);
    public async Task<OutputResponse> DeleteCustomerLocation(int id) => await service.Api.DeleteCustomerLocation(id);
    #endregion

    #region CompanyDetails
    public async Task<CompanyDetails?> GetCompanyDetailsByBranch(int branchId) => await service.Api.GetCompanyDetailsByBranch(branchId);
    public async Task<bool> CheckCompanyDetailsExists(int branchId) => await service.Api.CheckCompanyDetailsExists(branchId);
    public async Task<OutputResponse> CreateCompanyDetails(CompanyDetails details) => await service.Api.CreateCompanyDetails(details);
    public async Task<OutputResponse> UpdateCompanyDetails(CompanyDetails details) => await service.Api.UpdateCompanyDetails(details);
    #endregion

    #region Brand
    public async Task<List<BrandMaster>> GetAllBrand() => await service.Api.GetAllBrand();
    public async Task<BrandMaster> GetBrandById(int id) => await service.Api.GetBrandById(id);
    public async Task<OutputResponse> CreateBrand(BrandMaster Brand) => await service.Api.CreateBrand(Brand);
    public async Task<OutputResponse> UpdateBrand(BrandMaster Brand) => await service.Api.UpdateBrand(Brand);
    public async Task<OutputResponse> DeleteBrand(int id) => await service.Api.DeleteBrand(id);
    #endregion

    #region Currency
    public async Task<List<CurrencyMaster>> GetAllCurrency() => await service.Api.GetAllCurrency();
    public async Task<CurrencyMaster> GetCurrencyById(int id) => await service.Api.GetCurrencyById(id);
    public async Task<OutputResponse> CreateCurrency(CurrencyMaster Currency) => await service.Api.CreateCurrency(Currency);
    public async Task<OutputResponse> UpdateCurrency(CurrencyMaster Currency) => await service.Api.UpdateCurrency(Currency);
    public async Task<OutputResponse> DeleteCurrency(int id) => await service.Api.DeleteCurrency(id);
    #endregion

    #region Purchaser
    public async Task<List<PurchaserMaster>> GetAllPurchaser() => await service.Api.GetAllPurchaser();
    public async Task<PurchaserMaster> GetPurchaserById(int id) => await service.Api.GetPurchaserById(id);
    public async Task<OutputResponse> CreatePurchaser(PurchaserMaster Purchaser) => await service.Api.CreatePurchaser(Purchaser);
    public async Task<OutputResponse> UpdatePurchaser(PurchaserMaster Purchaser) => await service.Api.UpdatePurchaser(Purchaser);
    public async Task<OutputResponse> DeletePurchaser(int id) => await service.Api.DeletePurchaser(id);
    #endregion

    #region Project
    public async Task<List<ProjectMaster>> GetAllProject() => await service.Api.GetAllProject();
    public async Task<ProjectMaster> GetProjectById(int id) => await service.Api.GetProjectById(id);
    public async Task<OutputResponse> CreateProject(ProjectMaster project) => await service.Api.CreateProject(project);
    public async Task<OutputResponse> UpdateProject(ProjectMaster project) => await service.Api.UpdateProject(project);
    public async Task<OutputResponse> DeleteProject(int id) => await service.Api.DeleteProject(id);
    #endregion

    #region Customer
    public async Task<List<CustomerMaster>> GetAllCustomers() => await service.Api.GetAllCustomers();
    public async Task<CustomerMaster> GetCustomerById(int id) => await service.Api.GetCustomerById(id);
    public async Task<OutputResponse> CreateCustomer(CustomerMaster customer) => await service.Api.CreateCustomer(customer);
    public async Task<OutputResponse> UpdateCustomer(CustomerMaster customer) => await service.Api.UpdateCustomer(customer);
    public async Task<OutputResponse> DeleteCustomer(int id) => await service.Api.DeleteCustomer(id);
    public async Task<CustomerPDTO> GetCustomerPDetails(PaginationModel pagination) => await service.Api.GetPCustomers(pagination);
    public async Task<string> GetNextCustomerCode() => await service.Api.GetNextCustomerCode();
    public async Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId) => await service.Api.GetCustomerOutstandingBalance(customerId);
    #endregion

    #region Supplier
    public async Task<List<SupplierMaster>> GetAllSuppliers() => await service.Api.GetAllSuppliers();
    public async Task<SupplierMaster> GetSupplierById(int id) => await service.Api.GetSupplierById(id);
    public async Task<OutputResponse> CreateSupplier(SupplierMaster supplier) => await service.Api.CreateSupplier(supplier);
    public async Task<OutputResponse> UpdateSupplier(SupplierMaster supplier) => await service.Api.UpdateSupplier(supplier);
    public async Task<OutputResponse> DeleteSupplier(int id) => await service.Api.DeleteSupplier(id);
    public async Task<SupplierPDTO> GetSupplierPDetails(PaginationModel pagination) => await service.Api.GetPSuppliers(pagination);
    public async Task<string> GetNextSupplierCode() => await service.Api.GetNextSupplierCode();
    #endregion

    #region Item
    public async Task<List<ItemMaster>> GetAllItems() => await service.Api.GetAllItems();
    public async Task<ItemMaster> GetItemById(int id) => await service.Api.GetItemById(id);
    public async Task<OutputResponse> CreateItem(ItemMaster item) => await service.Api.CreateItem(item);
    public async Task<OutputResponse> UpdateItem(ItemMaster item) => await service.Api.UpdateItem(item);
    public async Task<OutputResponse> DeleteItem(int id) => await service.Api.DeleteItem(id);
    public async Task<ItemMasterPDTO> GetItemPDetails(PaginationModel pagination) => await service.Api.GetItemPDetails(pagination);
    public async Task<ItemMultiUnit> GetItemMultiUnit(int itemId, int unitId, string barcode) => await service.Api.GetItemMultiUnit(itemId, unitId, barcode);
    public async Task<ItemMultiUnitPDTO> GetItemMultiUnitPDetails(PaginationModel pagination) => await service.Api.GetItemMultiUnitPDetails(pagination);
    public async Task<string> GetNextItemCode(int? categoryId, int? subCategoryId, string? catCode, string? subCatCode)
        => await service.Api.GetNextItemCode(categoryId, subCategoryId, catCode, subCatCode);
    public async Task<string> GetNextQuotationCode() => await service.Api.GetNextQuotationCode();
    public async Task<ItemDialogPDTO> GetItemDialogDetails(PaginationModel pagination) => await service.Api.GetItemDialogDetails(pagination);
    public async Task<ItemMultiUnitDialogPDTO> GetMultiUnitDialogDetails(PaginationModel pagination) => await service.Api.GetMultiUnitDialogDetails(pagination);
    public async Task<List<ItemMultiUnit>> GetItemMultiUnits(int itemId) => await service.Api.GetItemMultiUnits(itemId);
    public async Task<ItemStockInfoDTO> GetItemStockInfo(int itemId) => await service.Api.GetItemStockInfo(itemId);
    public async Task<string> GetUnitName(int unitId)
    {
        var unit = await service.Api.GetUnitById(unitId);
        return unit?.Name ?? "";
    }
    public async Task<List<BarcodeData>> GetBarcodeItems(int? categoryId = null) => await service.Api.GetBarcodeItems(categoryId);
    #endregion

    #region CustomerOPEntry
    public async Task<List<CustomerOPEntry>> GetAllCustomerOPEntry() => await service.Api.GetAllCustomerOPEntry();
    public async Task<CustomerOPEntryDTO> GetCustomerOPEntryById(int id) => await service.Api.GetCustomerOPEntryById(id);
    public async Task<OutputResponse> CreateCustomerOPEntry(CustomerOPEntryDTO entry) => await service.Api.CreateCustomerOPEntry(entry);

    public async Task<CustomerOpEntryPDTO> GetCustomerOPEntryPDetails(PaginationModel pagination) => await service.Api.GetCustomerOPEntryPDetails(pagination);
    public async Task<string> GetNewCustomerOPEntryDocNo() => await service.Api.GetNewCustomerOPEntryDocNo();
    #endregion

    #region SupplierOPEntry
    public async Task<List<SupplierOPEntry>> GetAllSupplierOPEntry() => await service.Api.GetAllSupplierOPEntry();
    public async Task<SupplierOPEntryDTO> GetSupplierOPEntryById(int id) => await service.Api.GetSupplierOPEntryById(id);
    public async Task<OutputResponse> CreateSupplierOPEntry(SupplierOPEntryDTO entry) => await service.Api.CreateSupplierOPEntry(entry);
    public async Task<SupplierOpEntryPDTO> GetSupplierOPEntryPDetails(PaginationModel pagination) => await service.Api.GetSupplierOPEntryPDetails(pagination);
    public async Task<string> GetNewSupplierOPEntryDocNo() => await service.Api.GetNewSupplierOPEntryDocNo();
    #endregion

    #region StoreMaster
    public async Task<List<StoreMaster>> GetAllStores() => await service.Api.GetAllStores();
    public async Task<StoreMaster> GetStoreById(int id) => await service.Api.GetStoreById(id);
    public async Task<OutputResponse> CreateStore(StoreMaster store) => await service.Api.CreateStore(store);
    public async Task<OutputResponse> UpdateStore(StoreMaster store) => await service.Api.UpdateStore(store);
    public async Task<OutputResponse> DeleteStore(int id) => await service.Api.DeleteStore(id);
    public async Task<bool> CheckIfDefaultStoreExists(int excludeId = 0, int? branchId = null) => await service.Api.CheckIfDefaultStoreExists(excludeId, branchId);
    #endregion

    #region OpeningStockEntry
    public async Task<List<OpeningStockEntry>> GetAllOpeningStockEntry() => await service.Api.GetAllOpeningStockEntry();
    public async Task<OpeningStockEntryDTO> GetOpeningStockEntryFull(int id) => await service.Api.GetOpeningStockEntryFull(id);
    public async Task<List<OpeningStockEntryItemForUpdate>> GetOpeningStockEntryExistingStock(int itemId) => await service.Api.GetOpeningStockEntryExistingStock(itemId);
    public async Task<OutputResponse> CreateOpeningStockEntry(OpeningStockEntryDTO dto) => await service.Api.CreateOpeningStockEntry(dto);
    public async Task<OutputResponse> UpdateOpeningStockEntry(OpeningStockEntryDTO entry) => await service.Api.UpdateOpeningStockEntry(entry);
    public async Task<OutputResponse> DeleteOpeningStockEntry(int id) => await service.Api.DeleteOpeningStockEntry(id);
    public async Task<OpeningStockEntryPDTO> GetOpeningStockEntryPDetails(PaginationModel pagination) => await service.Api.GetOpeningStockEntryPDetails(pagination);
    public async Task<string> GetNewOpeningStockEntryDocNo() => await service.Api.GetOpeningStockEntryNewDocNo();
    #endregion

    #endregion

    #region PermanentSettings
    public async Task<List<PermanentSettings>> GetAllPermanentSettings() => await service.Api.GetAllPermanentSettings();
    public async Task<OutputResponse> UpdatePermanentSettings(List<PermanentSettings> settings) => await service.Api.UpdatePermanentSettings(settings);
    #endregion

    #region Admin

    #region Subscription
    public async Task<List<LoginCompany>> GetAllAdminCompanies() => await service.Api.GetAllAdminCompanies();
    public async Task<LoginCompanyPDTO> GetPAdminCompanies(PaginationModel pagination) => await service.Api.GetPLoginCompany(pagination);
    public async Task<LoginCompany> GetAdminCompanyById(int id) => await service.Api.GetAdminCompanyById(id);
    public async Task<LoginCompany> CreateAdminCompany([Body] ExLCompany company) => await service.Api.CreateAdminCompany(company);
    public async Task<LoginCompany> UpdateAdminCompany([Body] ExLCompany company) => await service.Api.UpdateAdminCompany(company);
    public async Task<LoginCompany> DeleteAdminCompany(int id) => await service.Api.DeleteAdminCompany(id);
    #endregion

    #region Admin User
    public async Task<List<AdminUser>> GetAllAdminUsers() => await service.Api.GetAllAdminUsers();
    public async Task<AdminUser> GetAdminUserById(int id) => await service.Api.GetAdminUserById(id);
    public async Task<AdminUser> CreateAdminUser([Body] AdminUser user) => await service.Api.CreateAdminUser(user);
    public async Task<AdminUser> UpdateAdminUser([Body] AdminUser user) => await service.Api.UpdateAdminUser(user);
    public async Task<AdminUser> DeleteAdminUser(int id) => await service.Api.DeleteAdminUser(id);
    #endregion
    #endregion

    #region General
    public async Task<ChartEntity> GetChartValues(int branchId, string format) => await service.Api.GetChartValues(branchId, format);
    public async Task<string> GetArabicText(string sourceText) => await service.Api.GetArabicText(sourceText);
    public async Task<List<CommonModel>> GetCombofill(string head, int BranchId) => await service.Api.GetCombofill(head, BranchId);
    public async Task<List<ExMasters>> GetMasters(string type, int BranchId) => await service.Api.GetMasters(type, BranchId);
    public async Task<decimal> GetValueMethod(string head, int val) => await service.Api.GetValueMethod(head, val);
    #endregion

    #region JobOrder
    public async Task<string> GetNextJobOrderCode(int branchId) => await service.Api.GetNextJobOrderCode(branchId);
    public async Task<int> GetNewJobOrderDocNo(int branchId) => await service.Api.GetNewJobOrderDocNo(branchId);
    public async Task<JobOrderHdr> GetJobOrderById(int id) => await service.Api.GetJobOrderById(id);
    public async Task<OutputResponse> CreateJobOrder(JobOrderHdr hdr) => await service.Api.CreateJobOrder(hdr);
    public async Task<OutputResponse> UpdateJobOrder(JobOrderHdr hdr) => await service.Api.UpdateJobOrder(hdr);
    public async Task<OutputResponse> DeleteJobOrder(int id) => await service.Api.DeleteJobOrder(id);
    public async Task<TransactionPDTO> GetJobOrderPDetails(PaginationModel pagination) => await service.Api.GetJobOrderPDetails(pagination);
    #endregion

    #region JobCard
    public async Task<string> GetNextJobCardCode(int branchId) => await service.Api.GetNextJobCardCode(branchId);
    public async Task<int> GetNewJobCardDocNo(int branchId) => await service.Api.GetNewJobCardDocNo(branchId);
    public async Task<JobCardHdr> GetJobCardById(int id) => await service.Api.GetJobCardById(id);
    public async Task<OutputResponse> CreateJobCard(JobCardHdr hdr) => await service.Api.CreateJobCard(hdr);
    public async Task<OutputResponse> UpdateJobCard(JobCardHdr hdr) => await service.Api.UpdateJobCard(hdr);
    public async Task<OutputResponse> DeleteJobCard(int id) => await service.Api.DeleteJobCard(id);
    public async Task<TransactionPDTO> GetJobCardPDetails(PaginationModel pagination) => await service.Api.GetJobCardPDetails(pagination);
    public async Task<List<JobCardHistoryDTO>> GetJobCardHistory(string plateNo) => await service.Api.GetJobCardHistory(plateNo);
    #endregion

    #region Account Report
    public async Task<JournalEntryReportResponse> GetJournalEntryReport(JournalEntryReportRequest req) => await service.Api.GetJournalEntryReport(req);
    public async Task<TrialBalanceReportResponse> GetTrialBalanceReport(TrialBalanceReportRequest req) => await service.Api.GetTrialBalanceReport(req);
    public async Task<ExpenseReportResponse> GetExpenseReport(ExpenseReportRequest req) => await service.Api.GetExpenseReport(req);
    public async Task<ProfitAndLossReportResponse> GetProfitAndLossReport(ProfitAndLossReportRequest req) => await service.Api.GetProfitAndLossReport(req);
    public async Task<BalanceSheetReportResponse> GetBalanceSheetReport(BalanceSheetReportRequest req) => await service.Api.GetBalanceSheetReport(req);
    public async Task<LedgerReportResponse> GetLedgerReport(LedgerReportRequest req) => await service.Api.GetLedgerReport(req);
    public async Task<ChartOfAccountReportResponse> GetChartOfAccountReport(ChartOfAccountReportRequest req) => await service.Api.GetChartOfAccountReport(req);
    #endregion

    #region Transactions

    #region Inventory
    #region StockAdjustment
    public async Task<string> GetNextStockAdjustmentCode() => await service.Api.GetNextStockAdjustmentCode();
    public async Task<int> GetNewStockAdjustmentDocNo(int branchId) => await service.Api.GetNewStockAdjustmentNo(branchId);
    public async Task<StockAdjustmentHdr> GetStockAdjustmentById(int id) => await service.Api.GetStockAdjustmentById(id);
    public async Task<OutputResponse> CreateStockAdjustment(StockAdjustmentHdr hdr) => await service.Api.CreateStockAdjustment(hdr);
    public async Task<OutputResponse> UpdateStockAdjustment(StockAdjustmentHdr hdr) => await service.Api.UpdateStockAdjustment(hdr);
    public async Task<OutputResponse> DeleteStockAdjustment(int id) => await service.Api.DeleteStockAdjustment(id);
    public async Task<TransactionPDTO> GetStockAdjustmentPDetails(PaginationModel pagination) => await service.Api.GetStockAdjustmentPDetails(pagination);
    public async Task<decimal> GetItemCurrentStock(int itemId, int? storeId, int? excludeVoucherId = 0) => await service.Api.GetItemCurrentStock(itemId, storeId, excludeVoucherId);
    #endregion

    #region MaterialTransferRequest
    public async Task<string> GetNextMaterialTransferRequestCode() => await service.Api.GetNextMaterialTransferRequestCode();

    public async Task<int> GetNewMaterialTransferRequestNo(int branchId) => await service.Api.GetNewMaterialTransferRequestNo(branchId);

    public async Task<MaterialTransferRequestHdr> GetMaterialTransferRequestById(int id) => await service.Api.GetMaterialTransferRequestById(id);

    public async Task<OutputResponse> CreateMaterialTransferRequest(MaterialTransferRequestHdr hdr) => await service.Api.CreateMaterialTransferRequest(hdr);

    public async Task<OutputResponse> UpdateMaterialTransferRequest(MaterialTransferRequestHdr hdr) => await service.Api.UpdateMaterialTransferRequest(hdr);

    public async Task<OutputResponse> DeleteMaterialTransferRequest(int id) => await service.Api.DeleteMaterialTransferRequest(id);

    public async Task<TransactionPDTO> GetMaterialTransferRequestPDetails(PaginationModel pmodel) => await service.Api.GetMaterialTransferRequestPDetails(pmodel);

    // Approval Methods
    public async Task<List<MaterialTransferRequestHdr>> GetMaterialTransferApprovals(MTRApprovalFilter filter) =>
        await service.Api.GetMaterialTransferApprovalList(filter);

    public async Task<OutputResponse> ApproveMaterialTransferRequest(int id, int userId, List<MaterialTransferRequestDtls> details)
    {
        var request = new { Id = id, UserId = userId, Details = details };
        return await service.Api.ApproveMaterialTransferRequest(request);
    }

    public async Task<OutputResponse> RejectMaterialTransferRequest(int id, int userId, string? rejectionReason)
    {
        var request = new { Id = id, UserId = userId, RejectionReason = rejectionReason };
        return await service.Api.RejectMaterialTransferRequest(request);
    }
    #endregion

    #region MaterialTransferIn
    public async Task<string> GetNextMaterialTransferInCode() => await service.Api.GetNextMaterialTransferInCode();
    public async Task<int> GetNewMaterialTransferInNo(int branchId) => await service.Api.GetNewMaterialTransferInNo(branchId);
    public async Task<MaterialTransferInHdr> GetMaterialTransferInById(int id) => await service.Api.GetMaterialTransferInById(id);
    public async Task<OutputResponse> CreateMaterialTransferIn(MaterialTransferInHdr hdr) => await service.Api.CreateMaterialTransferIn(hdr);
    public async Task<OutputResponse> UpdateMaterialTransferIn(MaterialTransferInHdr hdr) => await service.Api.UpdateMaterialTransferIn(hdr);
    public async Task<OutputResponse> DeleteMaterialTransferIn(int id) => await service.Api.DeleteMaterialTransferIn(id);
    public async Task<TransactionPDTO> GetMaterialTransferInPDetails(PaginationModel pmodel) => await service.Api.GetMaterialTransferInPDetails(pmodel);
    #endregion

    #region MaterialTransferOut
    public async Task<string> GetNextMaterialTransferOutCode() => await service.Api.GetNextMaterialTransferOutCode();
    public async Task<int> GetNewMaterialTransferOutNo(int branchId) => await service.Api.GetNewMaterialTransferOutNo(branchId);
    public async Task<MaterialTransferOutHdr> GetMaterialTransferOutById(int id) => await service.Api.GetMaterialTransferOutById(id);
    public async Task<OutputResponse> CreateMaterialTransferOut(MaterialTransferOutHdr hdr) => await service.Api.CreateMaterialTransferOut(hdr);
    public async Task<OutputResponse> UpdateMaterialTransferOut(MaterialTransferOutHdr hdr) => await service.Api.UpdateMaterialTransferOut(hdr);
    public async Task<OutputResponse> DeleteMaterialTransferOut(int id) => await service.Api.DeleteMaterialTransferOut(id);
    public async Task<TransactionPDTO> GetMaterialTransferOutPDetails(PaginationModel pmodel) => await service.Api.GetMaterialTransferOutPDetails(pmodel);
    #endregion

    #region InternalMaterialTransfer
    public async Task<string> GetNextInternalMaterialTransferCode() => await service.Api.GetNextInternalMaterialTransferCode();
    public async Task<int> GetNewInternalMaterialTransferNo(int branchId) => await service.Api.GetNewInternalMaterialTransferNo(branchId);
    public async Task<InternalMaterialTransferHdr> GetInternalMaterialTransferById(int id) => await service.Api.GetInternalMaterialTransferById(id);
    public async Task<OutputResponse> CreateInternalMaterialTransfer(InternalMaterialTransferHdr hdr) => await service.Api.CreateInternalMaterialTransfer(hdr);
    public async Task<OutputResponse> UpdateInternalMaterialTransfer(InternalMaterialTransferHdr hdr) => await service.Api.UpdateInternalMaterialTransfer(hdr);
    public async Task<OutputResponse> DeleteInternalMaterialTransfer(int id) => await service.Api.DeleteInternalMaterialTransfer(id);
    public async Task<TransactionPDTO> GetInternalMaterialTransferPDetails(PaginationModel pmodel) => await service.Api.GetInternalMaterialTransferPDetails(pmodel);
    #endregion
    #endregion

    #region Sales

    #region Sales Quotation
    public async Task<TransactionPDTO> GetSalesQuotationPDetails(PaginationModel pagination) => await service.Api.GetPSalesQuotations(pagination);
    public async Task<SalesQuotationDTO> GetSalesQuotationById(int id) => await service.Api.GetSalesQuotationById(id);
    public async Task<OutputResponse> CreateSalesQuotation(SalesQuotationDTO quotation) => await service.Api.CreateSalesQuotation(quotation);
    public async Task<OutputResponse> DeleteSalesQuotation(int id) => await service.Api.DeleteSalesQuotation(id);
    public async Task<OutputResponse> UpdateSalesQuotation(SalesQuotationDTO quotation) => await service.Api.UpdateSalesQuotation(quotation);
    public async Task<int> GetNewSalesQuotationDocNo(int BranchId) => await service.Api.GetNewSalesQuotationNo(BranchId);
    #endregion

    #region Sales Invoice
    public async Task<TransactionPDTO> GetSalesInvoicePDetails(PaginationModel pagination) => await service.Api.GetPSalesInvoices(pagination);
    public async Task<SalesInvoiceDTO> GetSalesInvoiceById(int id) => await service.Api.GetSalesInvoiceById(id);
    public async Task<OutputResponse> CreateSalesInvoice(SalesInvoiceDTO invoice) => await service.Api.CreateSalesInvoice(invoice);
    public async Task<OutputResponse> DeleteSalesInvoice(int id) => await service.Api.DeleteSalesInvoice(id);
    public async Task<OutputResponse> UpdateSalesInvoice(SalesInvoiceDTO invoice) => await service.Api.UpdateSalesInvoice(invoice);
    public async Task<int> GetNewSalesInvoiceDocNo(int BranchId) => await service.Api.GetNewSalesInvoiceNo(BranchId);
    public async Task<string> GetNextSalesInvoiceCode() => await service.Api.GetNextInvoiceCode();
    public async Task<DateTime?> GetOldestUnpaidInvoiceDate(int customerId) => await service.Api.GetOldestUnpaidInvoiceDate(customerId);
    #endregion

    #region Sales Invoice Hold
    public async Task<OutputResponse> CreateSalesInvoiceHold(SalesInvoiceHoldDTO invoiceHold) => await service.Api.CreateSalesInvoiceHold(invoiceHold);
    public async Task<SalesInvoiceHoldDTO> GetSalesInvoiceHoldById(int id) => await service.Api.GetSalesInvoiceHoldById(id);
    public async Task<OutputResponse> DeleteSalesInvoiceHold(int id) => await service.Api.DeleteSalesInvoiceHold(id);
    public async Task<OutputResponse> DeleteAllSalesInvoiceHold(int branchId, string? terminal = null) => await service.Api.DeleteAllSalesInvoiceHold(branchId, terminal);
    public async Task<List<SalesInvoiceHoldHdr>> GetAllSalesInvoiceHold(int branchId, string? terminal = null) => await service.Api.GetAllSalesInvoiceHold(branchId, terminal);
    public async Task<TransactionPDTO> GetSalesInvoiceHoldPDetails(PaginationModel pagination) => await service.Api.GetPSalesInvoiceHolds(pagination);
    #endregion

    #region Sales Invoice Return
    public async Task<TransactionPDTO> GetSalesInvoiceRtnPDetails(PaginationModel pagination) => await service.Api.GetPSalesInvoiceRtns(pagination);
    public async Task<SalesInvoiceRtnDTO> GetSalesInvoiceRtnById(int id) => await service.Api.GetSalesInvoiceRtnById(id);
    public async Task<OutputResponse> CreateSalesInvoiceRtn(SalesInvoiceRtnDTO invoiceRtn) => await service.Api.CreateSalesInvoiceRtn(invoiceRtn);
    public async Task<OutputResponse> DeleteSalesInvoiceRtn(int id) => await service.Api.DeleteSalesInvoiceRtn(id);
    public async Task<OutputResponse> UpdateSalesInvoiceRtn(SalesInvoiceRtnDTO invoiceRtn) => await service.Api.UpdateSalesInvoiceRtn(invoiceRtn);
    public async Task<int> GetNewSalesInvoiceRtnDocNo(int BranchId) => await service.Api.GetNewSalesInvoiceRtnNo(BranchId);
    public async Task<string> GetNextSalesInvoiceRtnCode() => await service.Api.GetNextSalesInvoiceRtnCode();
    #endregion

    #region Proforma Invoice
    public async Task<TransactionPDTO> GetPProformaInvoices(PaginationModel pagination) => await service.Api.GetPProformaInvoices(pagination);
    public async Task<ProformaInvoiceDTO> GetProformaInvoiceById(int id) => await service.Api.GetProformaInvoiceById(id);
    public async Task<OutputResponse> CreateProformaInvoice(ProformaInvoiceDTO proformaInvoice) => await service.Api.CreateProformaInvoice(proformaInvoice);
    public async Task<OutputResponse> DeleteProformaInvoice(int id) => await service.Api.DeleteProformaInvoice(id);
    public async Task<OutputResponse> UpdateProformaInvoice(ProformaInvoiceDTO proformaInvoice) => await service.Api.UpdateProformaInvoice(proformaInvoice);
    public async Task<string> GetNewProformaInvoiceNo(int BranchId) => await service.Api.GetNewProformaInvoiceNo(BranchId);
    public async Task<string> GetNextProformaInvoiceCode() => await service.Api.GetNextProformaInvoiceCode();
    #endregion

    #region Sales Order
    public async Task<TransactionPDTO> GetSalesOrderPDetails(PaginationModel pagination) => await service.Api.GetPSalesOrders(pagination);
    public async Task<SalesOrderDTO> GetSalesOrderById(int id) => await service.Api.GetSalesOrderById(id);
    public async Task<OutputResponse> CreateSalesOrder(SalesOrderDTO order) => await service.Api.CreateSalesOrder(order);
    public async Task<OutputResponse> DeleteSalesOrder(int id) => await service.Api.DeleteSalesOrder(id);
    public async Task<OutputResponse> UpdateSalesOrder(SalesOrderDTO salesOrder) => await service.Api.UpdateSalesOrder(salesOrder);
    public async Task<string> GetNextSalesOrderCode() => await service.Api.GetNextSalesOrderCode();
    public async Task<int> GetNewSalesOrderDocNo(int BranchId)
    {
        var val = await service.Api.GetNewSalesOrderNo(BranchId);
        return val;
    }
    #endregion

    #region Delivery Note
    public async Task<TransactionPDTO> GetDeliveryNotePDetails(PaginationModel pagination) => await service.Api.GetPDeliveryNotes(pagination);
    public async Task<DeliveryNoteDTO> GetDeliveryNoteById(int id) => await service.Api.GetDeliveryNoteById(id);
    public async Task<OutputResponse> CreateDeliveryNote(DeliveryNoteDTO deliveryNote) => await service.Api.CreateDeliveryNote(deliveryNote);
    public async Task<OutputResponse> DeleteDeliveryNote(int id) => await service.Api.DeleteDeliveryNote(id);
    public async Task<OutputResponse> UpdateDeliveryNote(DeliveryNoteDTO deliveryNote) => await service.Api.UpdateDeliveryNote(deliveryNote);
    public async Task<string> GetNextDeliveryNoteCode() => await service.Api.GetNextDeliveryNoteCode();
    public async Task<List<PendingDeliveryNoteDTO>> GetPendingDeliveryNotes(int customerId) => await service.Api.GetPendingDeliveryNotes(customerId);
    public async Task<int> GetNewDeliveryNoteDocNo(int BranchId)
    {
        var val = await service.Api.GetNewDeliveryNoteNo(BranchId);
        return val;
    }
    #endregion

    #region Delivery Note Return
    public async Task<TransactionPDTO> GetDeliveryNoteRtnPDetails(PaginationModel pagination) => await service.Api.GetPDeliveryNoteRtns(pagination);
    public async Task<DeliveryNoteRtnDTO> GetDeliveryNoteRtnById(int id) => await service.Api.GetDeliveryNoteRtnById(id);
    public async Task<OutputResponse> CreateDeliveryNoteRtn(DeliveryNoteRtnDTO deliveryNoteRtn) => await service.Api.CreateDeliveryNoteRtn(deliveryNoteRtn);
    public async Task<OutputResponse> DeleteDeliveryNoteRtn(int id) => await service.Api.DeleteDeliveryNoteRtn(id);
    public async Task<OutputResponse> UpdateDeliveryNoteRtn(DeliveryNoteRtnDTO deliveryNoteRtn) => await service.Api.UpdateDeliveryNoteRtn(deliveryNoteRtn);
    public async Task<int> GetNewDeliveryNoteRtnDocNo(int BranchId) => await service.Api.GetNewDeliveryNoteRtnNo(BranchId);
    #endregion

    #endregion

    #region Purchase
    #region Enquiry
    public async Task<string> GetNextEnquiryNo(int branchId) => await service.Api.GetNextEnquiryNo(branchId);

    public async Task<OutputResponse> CreateEnquiry(RFQHdr enquiry)
    {
        var res = await service.Api.CreateEnquiry(enquiry);
        return new OutputResponse(0, res);
    }

    public async Task<OutputResponse> UpdateEnquiry(RFQHdr enquiry)
    {
        var res = await service.Api.UpdateEnquiry(enquiry);
        return new OutputResponse(0, res);
    }

    public async Task<OutputResponse> DeleteEnquiry(int id)
    {
        var res = await service.Api.DeleteEnquiry(id);
        return new OutputResponse(0, res ? "Deleted Successfully" : "Failed to Delete");
    }

    public async Task<RFQHdr> GetEnquiryById(int id) => await service.Api.GetEnquiryById(id);

    public async Task<List<RFQHdr>> GetAllEnquiries() => await service.Api.GetAllEnquiries();
    #endregion

    #region Purchase Order
    public async Task<TransactionPDTO> GetPurchaseOrderPDetails(PaginationModel pagination) => await service.Api.GetPPurchaseOrders(pagination);
    public async Task<PurchaseOrderDTO> GetPurchaseOrderById(int id) => await service.Api.GetPurchaseOrderById(id);
    public async Task<OutputResponse> CreatePurchaseOrder(PurchaseOrderDTO order) => await service.Api.CreatePurchaseOrder(order);
    public async Task<OutputResponse> DeletePurchaseOrder(int id) => await service.Api.DeletePurchaseOrder(id);
    public async Task<OutputResponse> UpdatePurchaseOrder(PurchaseOrderDTO purchaseOrder) => await service.Api.UpdatePurchaseOrder(purchaseOrder);
    public async Task<string> GetNextPurchaseOrderCode() => await service.Api.GetNextPurchaseOrderCode();
    public async Task<int> GetNewPurchaseOrderDocNo(int BranchId)
    {
        var val = await service.Api.GetNewPurchaseOrderNo(BranchId);
        return val;
    }
    #endregion

    #region Purchase Invoice
    public async Task<PurchaseDTO> GetPurchaseById(int id) => await service.Api.GetPurchaseById(id);
    public async Task<OutputResponse> CreatePurchase(PurchaseDTO purchase) => await service.Api.CreatePurchase(purchase);
    public async Task<TransactionPDTO> GetPurchasePDetails(PaginationModel pagination) => await service.Api.GetPPurchases(pagination);
    public async Task<string> GetNextPurchaseNo() => await service.Api.GetNextPurchaseCode();
    public async Task<OutputResponse> DeletePurchase(int id) => await service.Api.DeletePurchase(id);
    public async Task<OutputResponse> UpdatePurchase(PurchaseDTO purchase) => await service.Api.UpdatePurchase(purchase);
    public async Task<int> GetPurchaseNextVchrNo(int branchId) => await service.Api.GetNewPurchaseNo(branchId);
    #endregion

    #region Purchase Return
    public async Task<string> GetNextPurchaseReturnCode() => await service.Api.GetNextPurchaseReturnCode();
    public async Task<TransactionPDTO> GetPurchaseReturnPDetails(PaginationModel pagination) => await service.Api.GetPPurchaseReturns(pagination);
    public async Task<PurchaseReturnDTO> GetPurchaseReturnById(int id) => await service.Api.GetPurchaseReturnById(id);
    public async Task<OutputResponse> CreatePurchaseReturn(PurchaseReturnDTO entity) => await service.Api.CreatePurchaseReturn(entity);
    public async Task<OutputResponse> UpdatePurchaseReturn(PurchaseReturnDTO entity) => await service.Api.UpdatePurchaseReturn(entity);
    public async Task<OutputResponse> DeletePurchaseReturn(int id) => await service.Api.DeletePurchaseReturn(id);
    public async Task<int> GetNewPurchaseReturnDocNo(int branchId) => await service.Api.GetNewPurchaseReturnNo(branchId);
    public async Task<List<TransactionDtls>> GetPurchaseReturnBalance(int purchaseId) => await service.Api.GetPurchaseReturnBalance(purchaseId);
    #endregion

    #region Supplier Payment
    public async Task<string> GetNewSupplierPaymentNo(int branchId) => await service.Api.GetNewSupplierPaymentNo(branchId);
    public async Task<SupplierPaymentDTO> GetSupplierPaymentById(int id) => await service.Api.GetSupplierPaymentById(id);
    public async Task<SupplierPaymentPDTO> GetSupplierPaymentPDetails(PaginationModel pagination) => await service.Api.GetPSupplierPayments(pagination);
    public async Task<OutputResponse> CreateSupplierPayment(SupplierPaymentDTO supplierPayment) => await service.Api.CreateSupplierPayment(supplierPayment);
    public async Task<OutputResponse> UpdateSupplierPayment(SupplierPaymentDTO supplierPayment) => await service.Api.UpdateSupplierPayment(supplierPayment);
    public async Task<OutputResponse> DeleteSupplierPayment(int id) => await service.Api.DeleteSupplierPayment(id);
    public async Task<List<SupplierPaymentDtls>> GetPendingPurchaseInvoices(int supplierId) => await service.Api.GetPendingPurchaseInvoices(supplierId);
    public async Task<decimal> GetSupplierBalance(int supplierId, int branchId) => await service.Api.GetSupplierBalance(supplierId, branchId);
    #endregion
    #endregion

    #region Menu Visibility
    public async Task<ZakiSAASERP.Models.Settings.MenuVisibilityDTO> GetMenuConfiguration() => await service.Api.GetMenuConfiguration();

    public async Task<List<ZakiSAASERP.Models.Settings.ERPType>> GetERPTypes() => await service.Api.GetERPTypes();

    public async Task<List<ZakiSAASERP.Models.Settings.MenuVisibilityItem>> GetERPTypePreset(int erpTypeId) => await service.Api.GetERPTypePreset(erpTypeId);

    public async Task<OutputResponse> UpdateClientERPType(int erpTypeId, string modifiedBy) => await service.Api.UpdateClientERPType(erpTypeId, modifiedBy);

    public async Task<OutputResponse> SaveCustomMenuVisibility(List<ZakiSAASERP.Models.Settings.MenuVisibilityItem> items) => await service.Api.SaveCustomMenuVisibility(items);

    public async Task<List<ZakiSAASERP.Models.User.MenuDetails>> GetVisibleMenus() => await service.Api.GetVisibleMenus();

    public async Task<List<ZakiSAASERP.Models.User.MenuDetails>> GetAllMenuDetails() => await service.Api.GetAllMenuDetails();
    #endregion

    #region Goods Receipt
    public async Task<TransactionPDTO> GetGoodsReceiptPDetails(PaginationModel pagination) => await service.Api.GetPGoodsReceipts(pagination);
    public async Task<GoodsReceiptDTO> GetGoodsReceiptById(int id) => await service.Api.GetGoodsReceiptById(id);
    public async Task<OutputResponse> CreateGoodsReceipt(GoodsReceiptDTO order) => await service.Api.CreateGoodsReceipt(order);
    public async Task<OutputResponse> DeleteGoodsReceipt(int id) => await service.Api.DeleteGoodsReceipt(id);
    public async Task<OutputResponse> UpdateGoodsReceipt(GoodsReceiptDTO order) => await service.Api.UpdateGoodsReceipt(order);
    public async Task<string> GetNewGoodsReceiptDocNo(int BranchId)
    {
        var val = await service.Api.GetNewGoodsReceiptNo(BranchId);
        return val;
    }
    public async Task<string> GetNextGoodsReceiptNo() => await service.Api.GetNextGoodsReceiptCode();
    #endregion

    #region Damage Stock
    public async Task<string> GetNewDamageStockDocNo(int BranchId) => await service.Api.GetNextDamageStockDocNo(BranchId);
    public async Task<DamageStockDTO> GetDamageStockById(int id) => await service.Api.GetDamageStockById(id);
    public async Task<TransactionPDTO> GetDamageStockPDetails(PaginationModel pagination) => await service.Api.GetPDamageStocks(pagination);
    public async Task<OutputResponse> CreateDamageStock(DamageStockDTO damageStock) => await service.Api.CreateDamageStock(damageStock);
    public async Task<OutputResponse> UpdateDamageStock(DamageStockDTO damageStock) => await service.Api.UpdateDamageStock(damageStock);
    public async Task<OutputResponse> DeleteDamageStock(int id) => await service.Api.DeleteDamageStock(id);
    #endregion




    #endregion

    #region Print Design
    public async Task<PrintDesignDTO> GetPrintDesignAsync(PrintDesignReq req) => await service.Api.GetPrintDesignAsync(req);
    public async Task<OutputResponse> CreatePrintDesignAsync([Body] PrintDesignDTO dto) => await service.Api.CreatePrintDesignAsync(dto);
    public async Task<OutputResponse> UpdatePrintDesignAsync([Body] PrintDesignDTO dto) => await service.Api.UpdatePrintDesignAsync(dto);
    public async Task<OutputResponse> DeletePrintDesignAsync(int id) => await service.Api.DeletePrintDesignAsync(id);
    public async Task<TransactionPDTO> GetPrintDesignPDetails(PaginationModel pagination) => await service.Api.GetPrintDesignPDetails(pagination);
    #endregion

    #region Barcode Design
    public async Task<BarcodeDesignDTO> GetBarcodeDesignAsync(int id) => await service.Api.GetBarcodeDesignAsync(id);
    public async Task<BarcodeDesignDTO> GetBarcodeDesignAsync() => await service.Api.GetBarcodeDesignAsync();
    public async Task<OutputResponse> CreateBarcodeDesignAsync([Body] BarcodeDesignDTO dto) => await service.Api.CreateBarcodeDesignAsync(dto);
    public async Task<OutputResponse> UpdateBarcodeDesignAsync([Body] BarcodeDesignDTO dto) => await service.Api.UpdateBarcodeDesignAsync(dto);
    public async Task<OutputResponse> DeleteBarcodeDesignAsync(int id) => await service.Api.DeleteBarcodeDesignAsync(id);
    public async Task<TransactionPDTO> GetBarcodeDesignPDetails([Body] PaginationModel pagination) => await service.Api.GetBarcodeDesignPDetails(pagination);
    public async Task<List<BarcodeDesignHdr>> GetAllBarcodeDesigns() => await service.Api.GetAllBarcodeDesigns();
    #endregion

    #region Proforma Invoice
    public async Task<ProformaInvoiceDTO> CreateProformaFromQuotationAsync(int quotationId) => await service.Api.CreateProformaFromQuotationAsync(quotationId);
    #endregion

    #region Journal Entry
    public async Task<List<ZakiSAASERP.Models.Transactions.Accounts.JournalEntryHdr>> GetAllJournalEntries() => await service.Api.GetAllJournalEntries();
    public async Task<ZakiSAASERP.Models.Transactions.Accounts.JournalEntryDTO> GetJournalEntryById(int id) => await service.Api.GetJournalEntryById(id);
    public async Task<OutputResponse> CreateJournalEntry(ZakiSAASERP.Models.Transactions.Accounts.JournalEntryDTO entry) => await service.Api.CreateJournalEntry(entry);
    public async Task<OutputResponse> UpdateJournalEntry(ZakiSAASERP.Models.Transactions.Accounts.JournalEntryDTO entry) => await service.Api.UpdateJournalEntry(entry);
    public async Task<OutputResponse> DeleteJournalEntry(int id) => await service.Api.DeleteJournalEntry(id);
    public async Task<string> GetNextJournalNo() => await service.Api.GetNextJournalNo();
    #endregion

    #region Account Opening Entry
    public async Task<List<AccountOpeningEntryHdr>> GetAllAccountOpeningEntry() => await service.Api.GetAllAccountOpeningEntry();
    public async Task<AccountOpeningEntryDTO> GetAccountOpeningEntryById(int id) => await service.Api.GetAccountOpeningEntryById(id);
    public async Task<TransactionPDTO> GetAccountOpeningEntryPDetails(PaginationModel pagination) => await service.Api.GetAccountOpeningEntryPDetails(pagination);
    public async Task<string> GetAccountOpeningEntryNewDocNo(int branchId) => await service.Api.GetAccountOpeningEntryNewDocNo(branchId);
    public async Task<AccountOpeningEntryDTO> CreateAccountOpeningEntry(AccountOpeningEntryDTO dto) => await service.Api.CreateAccountOpeningEntry(dto);
    public async Task<AccountOpeningEntryDTO> UpdateAccountOpeningEntry(AccountOpeningEntryDTO entry) => await service.Api.UpdateAccountOpeningEntry(entry);
    public async Task<AccountOpeningEntryDTO> DeleteAccountOpeningEntry(int id) => await service.Api.DeleteAccountOpeningEntry(id);
    public async Task<List<AccountOpeningEntryDtls>> GetExistingOpeningBalances(int branchId) => await service.Api.GetExistingOpeningBalances(branchId);
    #endregion

    #region Expense Entry
    public async Task<List<ZakiSAASERP.Models.Transactions.Accounts.ExpenseEntryHdr>> GetAllExpenseEntries() => await service.Api.GetAllExpenseEntries();
    public async Task<ZakiSAASERP.Models.Transactions.Accounts.ExpenseEntryDTO> GetExpenseEntryById(int id) => await service.Api.GetExpenseEntryById(id);
    public async Task<OutputResponse> CreateExpenseEntry(ZakiSAASERP.Models.Transactions.Accounts.ExpenseEntryDTO entry) => await service.Api.CreateExpenseEntry(entry);
    public async Task<OutputResponse> UpdateExpenseEntry(ZakiSAASERP.Models.Transactions.Accounts.ExpenseEntryDTO entry) => await service.Api.UpdateExpenseEntry(entry);
    public async Task<OutputResponse> DeleteExpenseEntry(int id) => await service.Api.DeleteExpenseEntry(id);
    public async Task<string> GetNextExpenseNo() => await service.Api.GetNextExpenseNo();
    public async Task<TransactionPDTO> GetExpenseEntryPDetails(PaginationModel pagination) => await service.Api.GetExpenseEntryPDetails(pagination);
    #endregion

    #region Customer Receipt
    public async Task<List<ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptHdr>> GetAllCustomerReceipts() => await service.Api.GetAllCustomerReceipts();
    public async Task<ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO> GetCustomerReceiptById(int id) => await service.Api.GetCustomerReceiptById(id);
    public async Task<OutputResponse> CreateCustomerReceipt(ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt) => await service.Api.CreateCustomerReceipt(receipt);
    public async Task<OutputResponse> UpdateCustomerReceipt(ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt) => await service.Api.UpdateCustomerReceipt(receipt);
    public async Task<OutputResponse> DeleteCustomerReceipt(int id) => await service.Api.DeleteCustomerReceipt(id);
    public async Task<string> GetNextCustomerReceiptNo() => await service.Api.GetNextCustomerReceiptNo();
    public async Task<List<ZakiSAASERP.Models.DTO.OutstandingInvoiceDTO>> GetCustomerOutstandingInvoices(int customerId, int branchId) => await service.Api.GetCustomerOutstandingInvoices(customerId, branchId);
    public async Task<decimal> GetCustomerBalance(int customerId, int branchId) => await service.Api.GetCustomerBalance(customerId, branchId);
    public async Task<List<ZakiSAASERP.Models.Masters.CustomerMaster>> GetAllCustomer() => await service.Api.GetAllCustomers();
    #endregion

    #region Accounts
    public async Task<List<ZakiSAASERP.Models.Masters.AccountMaster>> GetCashAndBankAccounts() => await service.Api.GetCashAndBankAccounts();
    #endregion

    #region Reports

    #region Inventory Report
    public async Task<StockReportResponse> GetStockReport(StockReportRequest req) => await service.Api.GetStockReport(req);
    public async Task<StockTransferReportResponse> GetStockTransferReport(StockTransferReportRequest req) => await service.Api.GetStockTransferReport(req);
    public async Task<ItemTransactionHistoryReportResponse> GetItemTransactionHistoryReport(ItemTransactionHistoryReportRequest req) => await service.Api.GetItemTransactionHistoryReport(req);
    public async Task<StockAdjustmentReportResponse> GetStockAdjustmentReportAsync(StockAdjustmentReportRequest req) => await service.Api.GetStockAdjustmentReportAsync(req);
    #endregion

    #region Sales Report
    public async Task<SalesReportResponse> GetSalesReport(SalesReportRequest req) => await service.Api.GetSalesReport(req);
    public async Task<ApiResponse<Stream>> GetSalesReportPDF(SalesReportRequest req) => await service.Api.GetSalesReportPDF(req);
    public async Task<SalesReturnReportResponse> GetSalesReturnReport(SalesReturnReportRequest req) => await service.Api.GetSalesReturnReport(req);
    public async Task<ApiResponse<Stream>> GetSalesReturnReportPDF(SalesReturnReportRequest req) => await service.Api.GetSalesReturnReportPDF(req);
    public async Task<SalesReturnDetailsReportResponse> GetSalesReturnDetailsReport(SalesReturnDetailsReportRequest req) => await service.Api.GetSalesReturnDetailsReport(req);
    public async Task<ApiResponse<Stream>> GetSalesReturnDetailsReportPDF(SalesReturnDetailsReportRequest req) => await service.Api.GetSalesReturnDetailsReportPDF(req);
    public async Task<SalesMonitorResponse> GetSalesMonitorReportAsync(SalesMonitorRequest req) => await service.Api.GetSalesMonitorReportAsync(req);
    public async Task<List<SalesMonitorReceiptItem>> GetReceiptsByInvoiceAsync(int invoiceId) => await service.Api.GetReceiptsByInvoiceAsync(invoiceId);
    public async Task<SalesDetailsReportResponse> GetSalesDetailsReport(SalesDetailsReportRequest req) => await service.Api.GetSalesDetailsReport(req);
    public async Task<ApiResponse<Stream>> GetSalesDetailsReportPDF(SalesDetailsReportRequest req) => await service.Api.GetSalesDetailsReportPDF(req);
    public async Task<CustomerWiseSalesReportResponse> GetCustomerWiseSalesReport(CustomerWiseSalesReportRequest req) => await service.Api.GetCustomerWiseSalesReport(req);
    public async Task<ApiResponse<Stream>> GetCustomerWiseSalesReportPDF(CustomerWiseSalesReportRequest req) => await service.Api.GetCustomerWiseSalesReportPDF(req);
    public async Task<CategoryWiseSalesReportResponse> GetCategoryWiseReport(CategoryWiseSalesReportRequest req) => await service.Api.GetCategoryWiseReport(req);
    public async Task<SalesQuotationRegisterResponse> GetSalesQuotationRegisterAsync(SalesQuotationRegisterRequest req) => await service.Api.GetSalesQuotationRegisterAsync(req);
    public async Task<ApiResponse<Stream>> GetSalesQuotationRegisterReportPDF(SalesQuotationRegisterRequest req) => await service.Api.GetSalesQuotationRegisterReportPDF(req);
    public async Task<CustomerWiseQuotationReportResponse> GetCustomerWiseQuotationReportAsync(CustomerWiseQuotationReportRequest req) => await service.Api.GetCustomerWiseQuotationReportAsync(req);
    public async Task<ApiResponse<Stream>> GetCustomerWiseQuotationReportPDF(CustomerWiseQuotationReportRequest req) => await service.Api.GetCustomerWiseQuotationReportPDF(req);
    public async Task<DeliveryNoteRegisterResponse> GetDeliveryNoteRegisterAsync(DeliveryNoteRegisterRequest req) => await service.Api.GetDeliveryNoteRegisterAsync(req);
    public async Task<ApiResponse<Stream>> GetDeliveryNoteRegisterReportPDF(DeliveryNoteRegisterRequest req) => await service.Api.GetDeliveryNoteRegisterReportPDF(req);
    public async Task<DateWiseProfitReportResponse> GetDateWiseProfitReport(DateWiseProfitReportRequest req) => await service.Api.GetDateWiseProfitReport(req);
    public async Task<ApiResponse<Stream>> GetDateWiseProfitReportPDF(DateWiseProfitReportRequest req) => await service.Api.GetDateWiseProfitReportPDF(req);
    public async Task<ProformaInvoiceReportResponse> GetProformaInvoiceReport(ProformaInvoiceReportRequest req) => await service.Api.GetProformaInvoiceReport(req);
    public async Task<ApiResponse<Stream>> GetProformaInvoiceReportPDF(ProformaInvoiceReportRequest req) => await service.Api.GetProformaInvoiceReportPDF(req);
    public async Task<SalesOrderReportResponse> GetSalesOrderReport(SalesOrderReportRequest req) => await service.Api.GetSalesOrderReport(req);
    public async Task<ApiResponse<Stream>> GetSalesOrderReportPDF(SalesOrderReportRequest req) => await service.Api.GetSalesOrderReportPDF(req);
    public async Task<EnquiryRegistrationReportResponse> GetEnquiryRegistrationReport(EnquiryRegistrationReportRequest req) => await service.Api.GetEnquiryRegistrationReport(req);
    public async Task<VatReportResponse> GetVatReport(VatReportRequest req) => await service.Api.GetVatReport(req);
    #endregion

    #region Customer Report
    public async Task<CustomerStatementResponse> GetCustomerStatementReport(CustomerStatementRequest req) => await service.Api.GetCustomerStatementReport(req);
    public async Task<List<CustomerSummaryDTO>> GetCustomerSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null) => await service.Api.GetCustomerSummaryAsync(fromDate, toDate, branchId, customerId);
    public async Task<List<CustomerPendingInvoiceDTO>> GetCustomerPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null) => await service.Api.GetCustomerPendingInvoiceAsync(fromDate, toDate, branchId, customerId);
    public async Task<List<CustomerListDTO>> GetCustomerListAsync() => await service.Api.GetCustomerListAsync();
    #endregion

    #region Purchase Report
    public async Task<PurchaseReportResponse> GetPurchaseReport(PurchaseReportRequest req) => await service.Api.GetPurchaseReport(req);
    public async Task<PurchaseDetailsReportResponse> GetPurchaseDetailsReport(PurchaseDetailsReportRequest req) => await service.Api.GetPurchaseDetailsReport(req);
    public async Task<PurchaseOrderReportResponse> GetPurchaseOrderReport(PurchaseOrderReportRequest req) => await service.Api.GetPurchaseOrderReport(req);
    public async Task<ApiResponse<Stream>> GetPurchaseSummaryServerReportPDF(PurchaseReportRequest req) => await service.Api.GetPurchaseSummaryServerReportPDF(req);
    public async Task<ApiResponse<Stream>> GetPurchaseDetailsServerReportPDF(PurchaseDetailsReportRequest req) => await service.Api.GetPurchaseDetailsServerReportPDF(req);
    public async Task<ApiResponse<Stream>> GetPurchaseOrderServerReportPDF(PurchaseOrderReportRequest req) => await service.Api.GetPurchaseOrderServerReportPDF(req);
    public async Task<PurchaseMonitorResponse> GetPurchaseMonitorReportAsync(PurchaseMonitorRequest req) => await service.Api.GetPurchaseMonitorReportAsync(req);
    public async Task<List<PurchaseMonitorPaymentItem>> GetPaymentsByPurchaseAsync(int purchaseId) => await service.Api.GetPaymentsByPurchaseAsync(purchaseId);
    public async Task<TransactionMonitorResponse> GetTransactionMonitorReportAsync(TransactionMonitorRequest req) => await service.Api.GetTransactionMonitorReportAsync(req);
    public async Task<PurchaseReturnReportResponse> GetPurchaseReturnReport(PurchaseReturnReportRequest req) => await service.Api.GetPurchaseReturnReport(req);
    public async Task<ApiResponse<Stream>> GetPurchaseReturnSummaryServerReportPDF(PurchaseReturnReportRequest req) => await service.Api.GetPurchaseReturnSummaryServerReportPDF(req);
    public async Task<PurchaseReturnDetailsReportResponse> GetPurchaseReturnDetailsReport(PurchaseReturnDetailsReportRequest req) => await service.Api.GetPurchaseReturnDetailsReport(req);
    public async Task<ApiResponse<Stream>> GetPurchaseReturnDetailsServerReportPDF(PurchaseReturnDetailsReportRequest req) => await service.Api.GetPurchaseReturnDetailsServerReportPDF(req);
    #endregion


    #region Account Report
    public async Task<List<SupplierStatementDTO>> GetSupplierStatementAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        => await service.Api.GetSupplierStatementAsync(fromDate, toDate, branchId, supplierId);
    public async Task<List<SupplierSummaryDTO>> GetSupplierSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        => await service.Api.GetSupplierSummaryAsync(fromDate, toDate, branchId, supplierId);

    public async Task<List<SupplierPendingInvoiceDTO>> GetSupplierPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null)
        => await service.Api.GetSupplierPendingInvoiceAsync(fromDate, toDate, branchId, supplierId);

    public async Task<List<SupplierPaymentReportDTO>> GetSupplierPaymentReportAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId, string? billType)
        => await service.Api.GetSupplierPaymentReportAsync(fromDate, toDate, branchId, supplierId, billType);

    public async Task<List<SupplierListDTO>> GetSupplierListAsync()
        => await service.Api.GetSupplierListAsync();
    #endregion
    #endregion

    #region Zatca
    public async Task<int> GetFailedCount() => await service.Api.GetFailedCount();
    public async Task<PendingIcvData> GetPendingICVData(ICVReqDTO entry) => await service.Api.GetPendingICVData(entry);
    public async Task<IcvPendingPDTO> GetPendingICVList(IcvPendReqDTO entry) => await service.Api.GetPendingICVList(entry);
    public async Task ZatcaReUploadAll(IcvReuploadAll dto) => await service.Api.ZatcaReUploadAll(dto);
    public async Task<OutputResponse> ZatcaReUploadSelected(IcvReupload dto) => await service.Api.ZatcaReUploadSelected(dto);
    public async Task<CSRResponse> GenerateCSR(CSRRequest request) => await service.Api.GenerateCSR(request);
    public async Task<CSRResponse> GetCsr(int id) => await service.Api.GetCsr(id);
    public async Task<OutputResponse> AddNewCSR(CSRResponse request) => await service.Api.AddNewCSR(request);
    public async Task<ZatcaInvoiceDTO> GetZatcaPendingInvoicesAsync(PaginationModel pmodel) => await service.Api.GetZatcaPendingInvoicesAsync(pmodel);
    public async Task UploadPendingSimplifiedInvoices([Query] int BranchId) => await service.Api.UploadPendingSimplifiedInvoices(BranchId);
    #endregion

    #region User Privilege
    public async Task<UserPrivilegeDTO> GetUserprivilege(int id) => await service.Api.GetUserprivilege(id);
    public async Task<List<WebUserPrivilege>> GetWebUserprivilege(PrivilegeDTO dto) => await service.Api.GetWebUserprivilege(dto);
    public async Task<OutputResponse> NewUserprivilege(UserPrivilegeDTO entity) => await service.Api.NewUserprivilege(entity);
    public async Task<OutputResponse> UpdateUserprivilege(UserPrivilegeDTO entity) => await service.Api.UpdateUserprivilege(entity);
    public async Task<OutputResponse> DeleteUserprivilege(int id) => await service.Api.DeleteUserprivilege(id);
    public async Task<TransactionPDTO> GetUserPrivilegePDTO(PaginationModel pagination) => await service.Api.GetUserPrivilegePDTO(pagination);
    public async Task<List<UserPrivilegeHdr>> GetUserprivileges() => await service.Api.GetUserprivileges();
    #endregion

    #region ProductImport
    public async Task<ProductImportResultDTO> ImportProductsFromCsvAsync(List<ProductImportRowDTO> rows)
        => await service.Api.ImportProductsFromCsvAsync(rows);

    public async Task<byte[]> GetProductImportSampleCsvAsync()
        => await service.Api.GetProductImportSampleCsvAsync();
    #endregion

    #region ProductExport
    public async Task<List<ProductExportRowDTO>> GetProductsForExportAsync()
        => await service.Api.GetProductsForExportAsync();

    public async Task<byte[]> ExportProductsToCsvAsync()
        => await service.Api.ExportProductsToCsvAsync();
    #endregion

}
