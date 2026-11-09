using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Transactions.Accounts;
using ZakiSAASERP.Models.Transactions.Inventory;
using ZakiSAASERP.Models.Transactions.Purchase;
using ZakiSAASERP.Models.Transactions.Workshop;
using ZakiSAASERP.Models.User;
using ZakiSAASERP.Models.ViewModel;
using Refit;
using ZakiSAASERP.Models.Settings;
namespace ZakiSAASERP.Shared.Services;

public interface IViewService
{
    #region Masters

    #region Account Head
    Task<List<ZakiSAASERP.Models.Masters.AccountHeadMaster>> GetAllAccountHeads();
    Task<ZakiSAASERP.Models.Masters.AccountHeadMaster> GetAccountHeadById(int id);
    Task<OutputResponse> CreateAccountHead(AccountHeadMaster accountHead);
    Task<OutputResponse> UpdateAccountHead(AccountHeadMaster accountHead);
    Task<OutputResponse> DeleteAccountHead(int id);
    Task<OutputResponse> GetAccountHeadNextCode(int typeId);
    #endregion

    #region AccountGroup
    Task<List<ZakiSAASERP.Models.Masters.AccountGroup>> GetAllAccountGroups();
    Task<ZakiSAASERP.Models.Masters.AccountGroup> GetAccountGroupById(int id);
    Task<OutputResponse> CreateAccountGroup(AccountGroup accountGroup);
    Task<OutputResponse> UpdateAccountGroup(AccountGroup accountGroup);
    Task<OutputResponse> DeleteAccountGroup(int id);
    Task<OutputResponse> GetAccountGroupNextCode(int accountHeadId);
    #endregion

    #region AccountSubGroup
    Task<List<ZakiSAASERP.Models.Masters.AccountSubGroup>> GetAllAccountSubGroups();
    Task<ZakiSAASERP.Models.Masters.AccountSubGroup> GetAccountSubGroupById(int id);
    Task<OutputResponse> CreateAccountSubGroup(AccountSubGroup accountSubGroup);
    Task<OutputResponse> UpdateAccountSubGroup(AccountSubGroup accountSubGroup);
    Task<OutputResponse> DeleteAccountSubGroup(int id);
    Task<OutputResponse> GetAccountSubGroupNextCode(int accountGroupId);
    #endregion

    #region Account
    Task<List<ZakiSAASERP.Models.Masters.AccountMaster>> GetAllAccounts();
    Task<List<ZakiSAASERP.Models.Masters.AccountType>> GetAllAccountTypes();
    Task<ZakiSAASERP.Models.Masters.AccountMaster> GetAccountMasterById(int id);
    Task<OutputResponse> CreateAccountMaster(ZakiSAASERP.Models.Masters.AccountMaster accountMaster);
    Task<OutputResponse> UpdateAccountMaster(ZakiSAASERP.Models.Masters.AccountMaster accountMaster);
    Task<OutputResponse> DeleteAccountMaster(int id);
    Task<OutputResponse> GetAccountMasterNextCode(int subGroupId);
    #endregion

    #region Branch
    Task<List<ZakiSAASERP.Models.Masters.BranchMaster>> GetAllBranches();
    Task<ZakiSAASERP.Models.Masters.BranchMaster> GetBranchById(int id);
    Task<OutputResponse> CreateBranch(BranchMaster branch);
    Task<OutputResponse> UpdateBranch(BranchMaster branch);
    Task<OutputResponse> DeleteBranch(int id);
    #endregion

    #region Financial Year
    Task<List<ZakiSAASERP.Models.Masters.FinancialYear>> GetAllFinancialYears();
    Task<ZakiSAASERP.Models.Masters.FinancialYear> GetFinancialYearById(int id);
    Task<OutputResponse> CreateFinancialYear(FinancialYear financialYear);
    Task<OutputResponse> UpdateFinancialYear(FinancialYear financialYear);
    Task<OutputResponse> DeleteFinancialYear(int id);
    Task<int> GetMaxRefNo();
    #endregion

    #region User
    Task<LoginResponse> Login(LoginRequest request);
    Task<PreLoginDatas> GetPreLoginDetails(string tenant, string username);

    #endregion

    #region Unit
    Task<List<ZakiSAASERP.Models.Masters.UnitMaster>> GetAllUnits();
    Task<ZakiSAASERP.Models.Masters.UnitMaster> GetUnitById(int id);
    Task<OutputResponse> CreateUnit(UnitMaster unit);
    Task<OutputResponse> UpdateUnit(UnitMaster unit);
    Task<OutputResponse> DeleteUnit(int id);
    Task<UnitPDTO> GetUnitPDetails(PaginationModel pagination);
    #endregion

    #region User Management
    Task<List<ZakiSAASERP.Models.User.UserDetails>> GetAllUsers();
    Task<ZakiSAASERP.Models.User.UserDetails> GetUserById(int id);
    Task<OutputResponse> CreateUser(ZakiSAASERP.Models.User.UserDetails user);
    Task<OutputResponse> UpdateUser(ZakiSAASERP.Models.User.UserDetails user);
    Task<OutputResponse> DeleteUser(int id);
    Task<List<ZakiSAASERP.Models.Masters.BranchMaster>> GetUserBranches(string userName);
    Task<UserPDTO> GetUserPDetails(PaginationModel pagination);
    Task<OutputResponse> SaveUserBranches(List<ZakiSAASERP.Models.User.UserBranch> userBranches);
    #endregion

    #region Pagination
    Task<AccountHeadPDTO> GetAccountHeadPDetails(PaginationModel pagination);
    Task<AccountPDTO> GetAccountPDetails(PaginationModel pagination);
    Task<BranchPDTO> GetBranchPDetails(PaginationModel pagination);
    Task<FinancialYearPDTO> GetFinancialYearPDetails(PaginationModel pagination);
    Task<SalesManPDTO> GetSalesManPDetails(PaginationModel pagination);
    Task<CategoryPDTO> GetCategoryPDetails(PaginationModel pagination);
    Task<SubCategoryPDTO> GetSubCategoryPDetails(PaginationModel pagination);
    Task<CustomerGroupPDTO> GetCustomerGroupPDetails(PaginationModel pagination);
    Task<CountryPDTO> GetCountryPDetails(PaginationModel pagination);
    Task<CustomerLocationPDTO> GetCustomerLocationPDetails(PaginationModel pagination);
    Task<VatPDTO> GetVatPDetails(PaginationModel pagination);
    Task<BrandPDTO> GetBrandPDetails(PaginationModel pagination);
    Task<CurrencyPDTO> GetCurrencyPDetails(PaginationModel pagination);
    Task<PurchaserPDTO> GetPurchaserPDetails(PaginationModel pagination);
    Task<ProjectPDTO> GetProjectPDetails(PaginationModel pagination);

    Task<AccountGroupPDTO> GetAccountGroupPDetails(PaginationModel pagination);
    Task<AccountSubGroupPDTO> GetAccountSubGroupPDetails(PaginationModel pagination);



    #endregion

    #region SalesManMaster
    Task<List<SalesManMaster>> GetAllSalesMan();
    Task<SalesManMaster> GetSalesManById(int id);
    Task<OutputResponse> CreateSalesMan(SalesManMaster unit);
    Task<OutputResponse> UpdateSalesMan(SalesManMaster unit);
    Task<OutputResponse> DeleteSalesMan(int id);
    #endregion

    #region Category
    Task<List<CategoryMaster>> GetAllCategory();
    Task<CategoryMaster> GetCategoryById(int id);
    Task<OutputResponse> CreateCategory(CategoryMaster unit);
    Task<OutputResponse> UpdateCategory(CategoryMaster unit);
    Task<OutputResponse> DeleteCategory(int id);
    Task<string> GetNextCategoryCode();
    Task<string> UploadImage(byte[] imageBytes, string fileName, string entityType, int entityId, string basePath, string clientFolder);
    Task<string> GetImageUrl(string basePath, string imagePath);
    Task<bool> DeleteImage(string basePath, string imagePath, string clientFolder);
    #endregion

    #region SubCategory
    Task<List<SubCategoryMaster>> GetAllSubCategory();
    Task<SubCategoryMaster> GetSubCategoryById(int id);
    Task<OutputResponse> CreateSubCategory(SubCategoryMaster unit);
    Task<OutputResponse> UpdateSubCategory(SubCategoryMaster unit);
    Task<OutputResponse> DeleteSubCategory(int id);
    Task<string> GetNextSubCategoryCode(int categoryId, string? categoryCode);
    #endregion

    #region Vehicle
    Task<List<VehicleMaster>> GetAllVehicles();
    Task<VehicleMaster> GetVehicleById(int id);
    Task<OutputResponse> CreateVehicle(VehicleMaster vehicle);
    Task<OutputResponse> UpdateVehicle(VehicleMaster vehicle);
    Task<OutputResponse> DeleteVehicle(int id);
    Task<VehiclePDTO> GetVehiclePDetails(PaginationModel pagination);
    #endregion

    #region RepairMaster
    Task<List<RepairMaster>> GetAllRepairs();
    Task<RepairMaster> GetRepairById(int id);
    Task<OutputResponse> CreateRepair(RepairMaster repair);
    Task<OutputResponse> UpdateRepair(RepairMaster repair);
    Task<OutputResponse> DeleteRepair(int id);
    Task<RepairPDTO> GetRepairPDetails(PaginationModel pagination);
    #endregion

    #region TechnicianMaster
    Task<List<TechnicianMaster>> GetAllTechnicians();
    Task<TechnicianMaster> GetTechnicianById(int id);
    Task<OutputResponse> CreateTechnician(TechnicianMaster technician);
    Task<OutputResponse> UpdateTechnician(TechnicianMaster technician);
    Task<OutputResponse> DeleteTechnician(int id);
    Task<TechnicianPDTO> GetTechnicianPDetails(PaginationModel pagination);
    #endregion

    #region Vat
    Task<List<VatMaster>> GetAllVat();
    Task<VatMaster> GetVatById(int id);
    Task<OutputResponse> CreateVat(VatMaster unit);
    Task<OutputResponse> UpdateVat(VatMaster unit);
    Task<OutputResponse> DeleteVat(int id);
    Task<List<VATCategoryDetails>> GetVATCategoryDetails(string code);
    #endregion

    #region CustomerGroup
    Task<List<CustomerGroupMaster>> GetAllCustomerGroup();
    Task<CustomerGroupMaster> GetCustomerGroupById(int id);
    Task<OutputResponse> CreateCustomerGroup(CustomerGroupMaster customerGroup);
    Task<OutputResponse> UpdateCustomerGroup(CustomerGroupMaster customerGroup);
    Task<OutputResponse> DeleteCustomerGroup(int id);
    #endregion

    #region CountryMaster
    Task<List<CountryMaster>> GetAllCountry();
    Task<CountryMaster> GetCountryById(int id);
    Task<OutputResponse> CreateCountry(CountryMaster countryMaster);
    Task<OutputResponse> UpdateCountry(CountryMaster countryMaster);
    Task<OutputResponse> DeleteCountry(int id);
    #endregion

    #region CustomerLocationMaster
    Task<List<CustomerLocationMaster>> GetAllCustomerLocation();
    Task<CustomerLocationMaster> GetCustomerLocationById(int id);
    Task<OutputResponse> CreateCustomerLocation(CustomerLocationMaster customerLocationMaster);
    Task<OutputResponse> UpdateCustomerLocation(CustomerLocationMaster customerLocationMaster);
    Task<OutputResponse> DeleteCustomerLocation(int id);
    #endregion

    #region CompanyDetails
    Task<CompanyDetails?> GetCompanyDetailsByBranch(int branchId);
    Task<bool> CheckCompanyDetailsExists(int branchId);
    Task<OutputResponse> CreateCompanyDetails(CompanyDetails details);
    Task<OutputResponse> UpdateCompanyDetails(CompanyDetails details);
    #endregion

    #region BrandMaster
    Task<List<BrandMaster>> GetAllBrand();
    Task<BrandMaster> GetBrandById(int id);
    Task<OutputResponse> CreateBrand(BrandMaster unit);
    Task<OutputResponse> UpdateBrand(BrandMaster unit);
    Task<OutputResponse> DeleteBrand(int id);
    #endregion

    #region Currency
    Task<List<CurrencyMaster>> GetAllCurrency();
    Task<CurrencyMaster> GetCurrencyById(int id);
    Task<OutputResponse> CreateCurrency(CurrencyMaster unit);
    Task<OutputResponse> UpdateCurrency(CurrencyMaster unit);
    Task<OutputResponse> DeleteCurrency(int id);
    #endregion

    #region Purchaser
    Task<List<PurchaserMaster>> GetAllPurchaser();
    Task<PurchaserMaster> GetPurchaserById(int id);
    Task<OutputResponse> CreatePurchaser(PurchaserMaster unit);
    Task<OutputResponse> UpdatePurchaser(PurchaserMaster unit);
    Task<OutputResponse> DeletePurchaser(int id);


    #endregion

    #region Project
    Task<List<ProjectMaster>> GetAllProject();
    Task<ProjectMaster> GetProjectById(int id);
    Task<OutputResponse> CreateProject(ProjectMaster unit);
    Task<OutputResponse> UpdateProject(ProjectMaster unit);
    Task<OutputResponse> DeleteProject(int id);
    #endregion

    #region Customer
    Task<List<CustomerMaster>> GetAllCustomers();
    Task<CustomerMaster> GetCustomerById(int id);
    Task<OutputResponse> CreateCustomer(CustomerMaster customer);
    Task<OutputResponse> UpdateCustomer(CustomerMaster customer);
    Task<OutputResponse> DeleteCustomer(int id);
    Task<CustomerPDTO> GetCustomerPDetails(PaginationModel pagination);
    Task<string> GetNextCustomerCode();
    Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId);
    #endregion

    #region Supplier
    Task<List<SupplierMaster>> GetAllSuppliers();
    Task<SupplierMaster> GetSupplierById(int id);
    Task<OutputResponse> CreateSupplier(SupplierMaster supplier);
    Task<OutputResponse> UpdateSupplier(SupplierMaster supplier);
    Task<OutputResponse> DeleteSupplier(int id);
    Task<SupplierPDTO> GetSupplierPDetails(PaginationModel pagination);
    Task<string> GetNextSupplierCode();
    #endregion

    #region Item
    Task<List<ItemMaster>> GetAllItems();
    Task<ItemMaster> GetItemById(int id);
    Task<OutputResponse> CreateItem(ItemMaster item);
    Task<OutputResponse> UpdateItem(ItemMaster item);
    Task<OutputResponse> DeleteItem(int id);
    Task<ItemMasterPDTO> GetItemPDetails(PaginationModel pagination);
    Task<ItemMultiUnit> GetItemMultiUnit(int itemId, int unitId, string barcode);
    Task<ItemMultiUnitPDTO> GetItemMultiUnitPDetails(PaginationModel pagination);
    Task<string> GetNextItemCode(int? categoryId, int? subCategoryId, string? catCode, string? subCatCode);
    Task<string> GetNextQuotationCode();
    Task<ItemDialogPDTO> GetItemDialogDetails(PaginationModel pagination);
    Task<ItemMultiUnitDialogPDTO> GetMultiUnitDialogDetails(PaginationModel pagination);
    Task<List<ItemMultiUnit>> GetItemMultiUnits(int itemId);
    Task<string> GetUnitName(int unitId);
    Task<ItemStockInfoDTO> GetItemStockInfo(int itemId);
    Task<List<BarcodeData>> GetBarcodeItems(int? categoryId = null);
    #endregion

    #region CustomerOPEntry
    Task<List<CustomerOPEntry>> GetAllCustomerOPEntry();
    Task<CustomerOPEntryDTO> GetCustomerOPEntryById(int id);
    Task<OutputResponse> CreateCustomerOPEntry(CustomerOPEntryDTO entry);

    Task<CustomerOpEntryPDTO> GetCustomerOPEntryPDetails(PaginationModel pagination);
    Task<string> GetNewCustomerOPEntryDocNo();
    #endregion

    #region SupplierOPEntry
    Task<List<SupplierOPEntry>> GetAllSupplierOPEntry();
    Task<SupplierOPEntryDTO> GetSupplierOPEntryById(int id);
    Task<OutputResponse> CreateSupplierOPEntry(SupplierOPEntryDTO entry);
    Task<SupplierOpEntryPDTO> GetSupplierOPEntryPDetails(PaginationModel pagination);
    Task<string> GetNewSupplierOPEntryDocNo();
    #endregion

    #endregion

    #region PermanentSettings
    Task<List<PermanentSettings>> GetAllPermanentSettings();
    Task<OutputResponse> UpdatePermanentSettings(List<PermanentSettings> settings);
    #endregion

    #region Admin

    #region Subscription
    Task<List<LoginCompany>> GetAllAdminCompanies();
    Task<LoginCompanyPDTO> GetPAdminCompanies(PaginationModel pagination);
    Task<LoginCompany> GetAdminCompanyById(int id);
    Task<LoginCompany> CreateAdminCompany([Body] ExLCompany company);
    Task<LoginCompany> UpdateAdminCompany([Body] ExLCompany company);
    Task<LoginCompany> DeleteAdminCompany(int id);
    #endregion

    #region Admin User
    Task<List<AdminUser>> GetAllAdminUsers();
    Task<AdminUser> GetAdminUserById(int id);
    Task<AdminUser> CreateAdminUser([Body] AdminUser user);
    Task<AdminUser> UpdateAdminUser([Body] AdminUser user);
    Task<AdminUser> DeleteAdminUser(int id);
    #endregion

    #endregion

    #region General
    Task<ChartEntity> GetChartValues(int branchId, string format);
    Task<string> GetArabicText(string sourceText);
    Task<List<CommonModel>> GetCombofill(string head, int BranchId);
    Task<List<ExMasters>> GetMasters(string head, int BranchId);
    Task<decimal> GetValueMethod(string head, int val);
    #endregion

    #region Inventory
    #region StockAdjustment
    Task<string> GetNextStockAdjustmentCode();
    Task<int> GetNewStockAdjustmentDocNo(int branchId);
    Task<StockAdjustmentHdr> GetStockAdjustmentById(int id);
    Task<OutputResponse> CreateStockAdjustment(StockAdjustmentHdr hdr);
    Task<OutputResponse> UpdateStockAdjustment(StockAdjustmentHdr hdr);
    Task<OutputResponse> DeleteStockAdjustment(int id);
    Task<TransactionPDTO> GetStockAdjustmentPDetails(PaginationModel pagination);
    Task<decimal> GetItemCurrentStock(int itemId, int? storeId, int? excludeVoucherId = 0);
    #endregion

    #region MaterialTransferRequest
    Task<string> GetNextMaterialTransferRequestCode();
    Task<int> GetNewMaterialTransferRequestNo(int branchId);
    Task<MaterialTransferRequestHdr> GetMaterialTransferRequestById(int id);
    Task<OutputResponse> CreateMaterialTransferRequest(MaterialTransferRequestHdr hdr);
    Task<OutputResponse> UpdateMaterialTransferRequest(MaterialTransferRequestHdr hdr);
    Task<OutputResponse> DeleteMaterialTransferRequest(int id);
    Task<TransactionPDTO> GetMaterialTransferRequestPDetails(PaginationModel pmodel);

    // Approval Methods
    Task<List<MaterialTransferRequestHdr>> GetMaterialTransferApprovals(MTRApprovalFilter filter);
    Task<OutputResponse> ApproveMaterialTransferRequest(int id, int userId, List<MaterialTransferRequestDtls> details);
    Task<OutputResponse> RejectMaterialTransferRequest(int id, int userId, string? rejectionReason);
    #endregion

    #region MaterialTransferIn
    Task<string> GetNextMaterialTransferInCode();
    Task<int> GetNewMaterialTransferInNo(int branchId);
    Task<MaterialTransferInHdr> GetMaterialTransferInById(int id);
    Task<OutputResponse> CreateMaterialTransferIn(MaterialTransferInHdr hdr);
    Task<OutputResponse> UpdateMaterialTransferIn(MaterialTransferInHdr hdr);
    Task<OutputResponse> DeleteMaterialTransferIn(int id);
    Task<TransactionPDTO> GetMaterialTransferInPDetails(PaginationModel pmodel);
    #endregion

    #region MaterialTransferOut
    Task<string> GetNextMaterialTransferOutCode();
    Task<int> GetNewMaterialTransferOutNo(int branchId);
    Task<MaterialTransferOutHdr> GetMaterialTransferOutById(int id);
    Task<OutputResponse> CreateMaterialTransferOut(MaterialTransferOutHdr hdr);
    Task<OutputResponse> UpdateMaterialTransferOut(MaterialTransferOutHdr hdr);
    Task<OutputResponse> DeleteMaterialTransferOut(int id);
    Task<TransactionPDTO> GetMaterialTransferOutPDetails(PaginationModel pmodel);
    #endregion

    #region InternalMaterialTransfer
    Task<string> GetNextInternalMaterialTransferCode();
    Task<int> GetNewInternalMaterialTransferNo(int branchId);
    Task<InternalMaterialTransferHdr> GetInternalMaterialTransferById(int id);
    Task<OutputResponse> CreateInternalMaterialTransfer(InternalMaterialTransferHdr hdr);
    Task<OutputResponse> UpdateInternalMaterialTransfer(InternalMaterialTransferHdr hdr);
    Task<OutputResponse> DeleteInternalMaterialTransfer(int id);
    Task<TransactionPDTO> GetInternalMaterialTransferPDetails(PaginationModel pmodel);

    #endregion

    #endregion

    #region Sales

    #region Quotation
    Task<SalesQuotationDTO> GetSalesQuotationById(int id);
    Task<OutputResponse> CreateSalesQuotation(SalesQuotationDTO salesQuotation);
    Task<TransactionPDTO> GetSalesQuotationPDetails(PaginationModel pagination);
    Task<int> GetNewSalesQuotationDocNo(int BranchId);
    Task<OutputResponse> DeleteSalesQuotation(int id);
    Task<OutputResponse> UpdateSalesQuotation(SalesQuotationDTO salesQuotation);
    #endregion

    #region Sales Invoice
    Task<SalesInvoiceDTO> GetSalesInvoiceById(int id);
    Task<OutputResponse> CreateSalesInvoice(SalesInvoiceDTO salesInvoice);
    Task<TransactionPDTO> GetSalesInvoicePDetails(PaginationModel pagination);
    Task<int> GetNewSalesInvoiceDocNo(int BranchId);
    Task<OutputResponse> DeleteSalesInvoice(int id);
    Task<OutputResponse> UpdateSalesInvoice(SalesInvoiceDTO salesInvoice);
    Task<string> GetNextSalesInvoiceCode();
    Task<DateTime?> GetOldestUnpaidInvoiceDate(int customerId);
    #endregion

    #region Sales Invoice Hold
    Task<OutputResponse> CreateSalesInvoiceHold(SalesInvoiceHoldDTO salesInvoiceHold);
    Task<SalesInvoiceHoldDTO> GetSalesInvoiceHoldById(int id);
    Task<OutputResponse> DeleteSalesInvoiceHold(int id);
    Task<OutputResponse> DeleteAllSalesInvoiceHold(int branchId, string? terminal = null);
    Task<List<SalesInvoiceHoldHdr>> GetAllSalesInvoiceHold(int branchId, string? terminal = null);
    Task<TransactionPDTO> GetSalesInvoiceHoldPDetails(PaginationModel pagination);
    #endregion

    #region Sales Invoice Return
    Task<TransactionPDTO> GetSalesInvoiceRtnPDetails(PaginationModel pagination);
    Task<string> GetNextPurchaseReturnCode();
    Task<TransactionPDTO> GetPurchaseReturnPDetails(PaginationModel pagination);

    Task<SalesInvoiceRtnDTO> GetSalesInvoiceRtnById(int id);
    Task<PurchaseReturnDTO> GetPurchaseReturnById(int id);

    Task<OutputResponse> CreateSalesInvoiceRtn(SalesInvoiceRtnDTO entity);
    Task<OutputResponse> CreatePurchaseReturn(PurchaseReturnDTO entity);

    Task<OutputResponse> UpdateSalesInvoiceRtn(SalesInvoiceRtnDTO entity);
    Task<OutputResponse> UpdatePurchaseReturn(PurchaseReturnDTO entity);

    Task<OutputResponse> DeleteSalesInvoiceRtn(int id);
    Task<OutputResponse> DeletePurchaseReturn(int id);

    Task<int> GetNewSalesInvoiceRtnDocNo(int branchId);
    Task<int> GetNewPurchaseReturnDocNo(int branchId);
    Task<string> GetNextSalesInvoiceRtnCode();
    Task<List<TransactionDtls>> GetPurchaseReturnBalance(int purchaseId);
    #endregion

    #region Order
    Task<SalesOrderDTO> GetSalesOrderById(int id);
    Task<OutputResponse> CreateSalesOrder(SalesOrderDTO salesOrder);
    Task<TransactionPDTO> GetSalesOrderPDetails(PaginationModel pagination);
    Task<int> GetNewSalesOrderDocNo(int BranchId);
    Task<OutputResponse> DeleteSalesOrder(int id);
    Task<OutputResponse> UpdateSalesOrder(SalesOrderDTO salesOrder);
    Task<string> GetNextSalesOrderCode();
    #endregion

    #region Delivery Note
    Task<DeliveryNoteDTO> GetDeliveryNoteById(int id);
    Task<OutputResponse> CreateDeliveryNote(DeliveryNoteDTO deliveryNote);
    Task<TransactionPDTO> GetDeliveryNotePDetails(PaginationModel pagination);
    Task<int> GetNewDeliveryNoteDocNo(int BranchId);
    Task<OutputResponse> DeleteDeliveryNote(int id);
    Task<OutputResponse> UpdateDeliveryNote(DeliveryNoteDTO deliveryNote);
    Task<string> GetNextDeliveryNoteCode();
    Task<List<PendingDeliveryNoteDTO>> GetPendingDeliveryNotes(int customerId);
    #endregion

    #region Delivery Note Return
    Task<DeliveryNoteRtnDTO> GetDeliveryNoteRtnById(int id);
    Task<OutputResponse> CreateDeliveryNoteRtn(DeliveryNoteRtnDTO deliveryNoteRtn);
    Task<TransactionPDTO> GetDeliveryNoteRtnPDetails(PaginationModel pagination);
    Task<int> GetNewDeliveryNoteRtnDocNo(int BranchId);
    Task<OutputResponse> DeleteDeliveryNoteRtn(int id);
    Task<OutputResponse> UpdateDeliveryNoteRtn(DeliveryNoteRtnDTO deliveryNoteRtn);
    #endregion

    #region Proforma Invoice    
    Task<string> GetNewProformaInvoiceNo(int id);
    Task<ProformaInvoiceDTO> GetProformaInvoiceById(int id);
    Task<TransactionPDTO> GetPProformaInvoices(PaginationModel pmodel);
    Task<OutputResponse> CreateProformaInvoice(ProformaInvoiceDTO proformaInvoice);
    Task<OutputResponse> UpdateProformaInvoice(ProformaInvoiceDTO proformaInvoice);
    Task<OutputResponse> DeleteProformaInvoice(int id);
    Task<string> GetNextProformaInvoiceCode();
    #endregion
    #endregion

    #region Purchase

    #region Enquiry
    Task<string> GetNextEnquiryNo(int branchId);
    Task<OutputResponse> CreateEnquiry(RFQHdr enquiry);
    Task<OutputResponse> UpdateEnquiry(RFQHdr enquiry);
    Task<OutputResponse> DeleteEnquiry(int id);
    Task<RFQHdr> GetEnquiryById(int id);
    Task<List<RFQHdr>> GetAllEnquiries();
    #endregion

    #region Purchase Order
    Task<PurchaseOrderDTO> GetPurchaseOrderById(int id);
    Task<OutputResponse> CreatePurchaseOrder(PurchaseOrderDTO purchaseOrder);
    Task<TransactionPDTO> GetPurchaseOrderPDetails(PaginationModel pagination);
    Task<int> GetNewPurchaseOrderDocNo(int BranchId);
    Task<OutputResponse> DeletePurchaseOrder(int id);
    Task<OutputResponse> UpdatePurchaseOrder(PurchaseOrderDTO purchaseOrder);
    Task<string> GetNextPurchaseOrderCode();

    Task<GoodsReceiptDTO> GetGoodsReceiptById(int id);
    Task<OutputResponse> CreateGoodsReceipt(GoodsReceiptDTO goodsReceipt);
    Task<TransactionPDTO> GetGoodsReceiptPDetails(PaginationModel pagination);
    Task<string> GetNewGoodsReceiptDocNo(int BranchId);
    Task<string> GetNextGoodsReceiptNo();
    Task<OutputResponse> DeleteGoodsReceipt(int id);
    Task<OutputResponse> UpdateGoodsReceipt(GoodsReceiptDTO goodsReceipt);
    #endregion

    #region Purchase Invoice
    Task<PurchaseDTO> GetPurchaseById(int id);
    Task<OutputResponse> CreatePurchase(PurchaseDTO purchase);
    Task<TransactionPDTO> GetPurchasePDetails(PaginationModel pagination);
    Task<string> GetNextPurchaseNo();
    Task<OutputResponse> DeletePurchase(int id);
    Task<OutputResponse> UpdatePurchase(PurchaseDTO purchase);
    Task<int> GetPurchaseNextVchrNo(int branchId);
    #endregion

    #region Supplier Payment
    Task<string> GetNewSupplierPaymentNo(int id);
    Task<SupplierPaymentDTO> GetSupplierPaymentById(int id);
    Task<SupplierPaymentPDTO> GetSupplierPaymentPDetails(PaginationModel pagination);
    Task<OutputResponse> CreateSupplierPayment(SupplierPaymentDTO supplierPayment);
    Task<OutputResponse> UpdateSupplierPayment(SupplierPaymentDTO supplierPayment);
    Task<OutputResponse> DeleteSupplierPayment(int id);
    Task<List<SupplierPaymentDtls>> GetPendingPurchaseInvoices(int supplierId);
    Task<decimal> GetSupplierBalance(int supplierId, int branchId);
    #endregion


    #endregion

    #region Damage Stock
    Task<DamageStockDTO> GetDamageStockById(int id);
    Task<OutputResponse> CreateDamageStock(DamageStockDTO damageStock);
    Task<TransactionPDTO> GetDamageStockPDetails(PaginationModel pagination);
    Task<string> GetNewDamageStockDocNo(int BranchId);
    Task<OutputResponse> DeleteDamageStock(int id);
    Task<OutputResponse> UpdateDamageStock(DamageStockDTO damageStock);
    #endregion

    #region StoreMaster
    Task<List<StoreMaster>> GetAllStores();
    Task<StoreMaster> GetStoreById(int id);
    Task<OutputResponse> CreateStore(StoreMaster store);
    Task<OutputResponse> UpdateStore(StoreMaster store);
    Task<OutputResponse> DeleteStore(int id);
    Task<bool> CheckIfDefaultStoreExists(int excludeId = 0, int? branchId = null);
    #endregion

    #region OpeningStockEntry
    Task<List<OpeningStockEntry>> GetAllOpeningStockEntry();
    Task<OpeningStockEntryDTO> GetOpeningStockEntryFull(int id);
    Task<List<OpeningStockEntryItemForUpdate>> GetOpeningStockEntryExistingStock(int itemId);
    Task<OutputResponse> CreateOpeningStockEntry(OpeningStockEntryDTO dto);
    Task<OutputResponse> UpdateOpeningStockEntry(OpeningStockEntryDTO entry);
    Task<OutputResponse> DeleteOpeningStockEntry(int id);
    Task<OpeningStockEntryPDTO> GetOpeningStockEntryPDetails(PaginationModel pagination);
    Task<string> GetNewOpeningStockEntryDocNo();
    #endregion

    #region Menu Visibility
    Task<ZakiSAASERP.Models.Settings.MenuVisibilityDTO> GetMenuConfiguration();
    Task<List<ZakiSAASERP.Models.Settings.ERPType>> GetERPTypes();
    Task<List<ZakiSAASERP.Models.Settings.MenuVisibilityItem>> GetERPTypePreset(int erpTypeId);
    Task<OutputResponse> UpdateClientERPType(int erpTypeId, string modifiedBy);
    Task<OutputResponse> SaveCustomMenuVisibility(List<ZakiSAASERP.Models.Settings.MenuVisibilityItem> items);
    Task<List<ZakiSAASERP.Models.User.MenuDetails>> GetVisibleMenus();
    Task<List<ZakiSAASERP.Models.User.MenuDetails>> GetAllMenuDetails();
    #endregion

    #region Print Design
    Task<PrintDesignDTO> GetPrintDesignAsync(PrintDesignReq req);
    Task<OutputResponse> CreatePrintDesignAsync([Body] PrintDesignDTO dto);
    Task<OutputResponse> UpdatePrintDesignAsync([Body] PrintDesignDTO dto);
    Task<OutputResponse> DeletePrintDesignAsync(int id);
    Task<TransactionPDTO> GetPrintDesignPDetails(PaginationModel pagination);
    #endregion

    #region Barcode Design
    Task<BarcodeDesignDTO> GetBarcodeDesignAsync(int id);
    Task<BarcodeDesignDTO> GetBarcodeDesignAsync();
    Task<OutputResponse> CreateBarcodeDesignAsync([Body] BarcodeDesignDTO dto);
    Task<OutputResponse> UpdateBarcodeDesignAsync([Body] BarcodeDesignDTO dto);
    Task<OutputResponse> DeleteBarcodeDesignAsync(int id);
    Task<TransactionPDTO> GetBarcodeDesignPDetails(PaginationModel pagination);
    Task<List<BarcodeDesignHdr>> GetAllBarcodeDesigns();
    #endregion

    #region Proforma Invoice
    Task<ProformaInvoiceDTO> CreateProformaFromQuotationAsync(int quotationId);
    #endregion

    #region Journal Entry
    Task<List<ZakiSAASERP.Models.Transactions.Accounts.JournalEntryHdr>> GetAllJournalEntries();
    Task<ZakiSAASERP.Models.Transactions.Accounts.JournalEntryDTO> GetJournalEntryById(int id);
    Task<OutputResponse> CreateJournalEntry(ZakiSAASERP.Models.Transactions.Accounts.JournalEntryDTO entry);
    Task<OutputResponse> UpdateJournalEntry(ZakiSAASERP.Models.Transactions.Accounts.JournalEntryDTO entry);
    Task<OutputResponse> DeleteJournalEntry(int id);
    Task<string> GetNextJournalNo();
    #endregion

    #region AccountOpeningEntry
    Task<AccountOpeningEntryDTO> GetAccountOpeningEntryById(int id);
    Task<AccountOpeningEntryDTO> CreateAccountOpeningEntry(AccountOpeningEntryDTO entry);
    Task<AccountOpeningEntryDTO> UpdateAccountOpeningEntry(AccountOpeningEntryDTO entry);
    Task<AccountOpeningEntryDTO> DeleteAccountOpeningEntry(int id);
    Task<TransactionPDTO> GetAccountOpeningEntryPDetails(PaginationModel pagination);
    Task<string> GetAccountOpeningEntryNewDocNo(int branchId);
    Task<List<AccountOpeningEntryDtls>> GetExistingOpeningBalances(int branchId);
    #endregion

    #region ExpenseEntry
    Task<List<ExpenseEntryHdr>> GetAllExpenseEntries();
    Task<ExpenseEntryDTO> GetExpenseEntryById(int id);
    Task<OutputResponse> CreateExpenseEntry(ExpenseEntryDTO entry);
    Task<OutputResponse> UpdateExpenseEntry(ExpenseEntryDTO entry);
    Task<OutputResponse> DeleteExpenseEntry(int id);
    Task<string> GetNextExpenseNo();
    Task<TransactionPDTO> GetExpenseEntryPDetails(PaginationModel pagination);
    #endregion

    #region Reports
    Task<List<SupplierStatementDTO>> GetSupplierStatementAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null);
    Task<List<SupplierPendingInvoiceDTO>> GetSupplierPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null);
    Task<List<SupplierSummaryDTO>> GetSupplierSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null);
    Task<List<SupplierListDTO>> GetSupplierListAsync();
    Task<List<SupplierPaymentReportDTO>> GetSupplierPaymentReportAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId, string? billType);
    #endregion

    #region Customer Receipt
    Task<List<ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptHdr>> GetAllCustomerReceipts();
    Task<ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO> GetCustomerReceiptById(int id);
    Task<OutputResponse> CreateCustomerReceipt(ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt);
    Task<OutputResponse> UpdateCustomerReceipt(ZakiSAASERP.Models.Transactions.Accounts.CustomerReceiptDTO receipt);
    Task<OutputResponse> DeleteCustomerReceipt(int id);
    Task<string> GetNextCustomerReceiptNo();
    Task<List<ZakiSAASERP.Models.DTO.OutstandingInvoiceDTO>> GetCustomerOutstandingInvoices(int customerId, int branchId);
    Task<decimal> GetCustomerBalance(int customerId, int branchId);
    Task<List<ZakiSAASERP.Models.Masters.CustomerMaster>> GetAllCustomer();
    #endregion

    #region Accounts
    Task<List<ZakiSAASERP.Models.Masters.AccountMaster>> GetCashAndBankAccounts();
    #endregion

    #region Inventory Report
    Task<StockReportResponse> GetStockReport(StockReportRequest req);
    Task<StockTransferReportResponse> GetStockTransferReport(StockTransferReportRequest req);
    Task<ItemTransactionHistoryReportResponse> GetItemTransactionHistoryReport(ItemTransactionHistoryReportRequest req);
    Task<StockAdjustmentReportResponse> GetStockAdjustmentReportAsync(StockAdjustmentReportRequest req);
    #endregion

    #region Sales Report
    Task<SalesReportResponse> GetSalesReport(SalesReportRequest req);
    Task<ApiResponse<Stream>> GetSalesReportPDF(SalesReportRequest req);
    Task<SalesReturnReportResponse> GetSalesReturnReport(SalesReturnReportRequest req);
    Task<ApiResponse<Stream>> GetSalesReturnReportPDF(SalesReturnReportRequest req);
    Task<SalesReturnDetailsReportResponse> GetSalesReturnDetailsReport(SalesReturnDetailsReportRequest req);
    Task<ApiResponse<Stream>> GetSalesReturnDetailsReportPDF(SalesReturnDetailsReportRequest req);
    Task<SalesMonitorResponse> GetSalesMonitorReportAsync(SalesMonitorRequest req);
    Task<List<SalesMonitorReceiptItem>> GetReceiptsByInvoiceAsync(int invoiceId);
    Task<SalesDetailsReportResponse> GetSalesDetailsReport(SalesDetailsReportRequest req);
    Task<ApiResponse<Stream>> GetSalesDetailsReportPDF(SalesDetailsReportRequest req);
    Task<CustomerWiseSalesReportResponse> GetCustomerWiseSalesReport(CustomerWiseSalesReportRequest req);
    Task<ApiResponse<Stream>> GetCustomerWiseSalesReportPDF(CustomerWiseSalesReportRequest req);
    Task<CategoryWiseSalesReportResponse> GetCategoryWiseReport(CategoryWiseSalesReportRequest req);
    Task<SalesQuotationRegisterResponse> GetSalesQuotationRegisterAsync(SalesQuotationRegisterRequest req);
    Task<ApiResponse<Stream>> GetSalesQuotationRegisterReportPDF(SalesQuotationRegisterRequest req);
    Task<CustomerWiseQuotationReportResponse> GetCustomerWiseQuotationReportAsync(CustomerWiseQuotationReportRequest req);
    Task<ApiResponse<Stream>> GetCustomerWiseQuotationReportPDF(CustomerWiseQuotationReportRequest req);
    Task<DeliveryNoteRegisterResponse> GetDeliveryNoteRegisterAsync(DeliveryNoteRegisterRequest req);
    Task<ApiResponse<Stream>> GetDeliveryNoteRegisterReportPDF(DeliveryNoteRegisterRequest req);
    Task<DateWiseProfitReportResponse> GetDateWiseProfitReport(DateWiseProfitReportRequest req);
    Task<ApiResponse<Stream>> GetDateWiseProfitReportPDF(DateWiseProfitReportRequest req);
    Task<ProformaInvoiceReportResponse> GetProformaInvoiceReport(ProformaInvoiceReportRequest req);
    Task<ApiResponse<Stream>> GetProformaInvoiceReportPDF(ProformaInvoiceReportRequest req);
    Task<SalesOrderReportResponse> GetSalesOrderReport(SalesOrderReportRequest req);
    Task<ApiResponse<Stream>> GetSalesOrderReportPDF(SalesOrderReportRequest req);
    Task<EnquiryRegistrationReportResponse> GetEnquiryRegistrationReport(EnquiryRegistrationReportRequest req);
    Task<VatReportResponse> GetVatReport(VatReportRequest req);
    #endregion

    #region Purchase Report
    Task<PurchaseReportResponse> GetPurchaseReport(PurchaseReportRequest req);
    Task<PurchaseMonitorResponse> GetPurchaseMonitorReportAsync(PurchaseMonitorRequest req);
    Task<List<PurchaseMonitorPaymentItem>> GetPaymentsByPurchaseAsync(int purchaseId);
    Task<PurchaseDetailsReportResponse> GetPurchaseDetailsReport(PurchaseDetailsReportRequest req);
    Task<PurchaseOrderReportResponse> GetPurchaseOrderReport(PurchaseOrderReportRequest req);
    Task<ApiResponse<Stream>> GetPurchaseSummaryServerReportPDF(PurchaseReportRequest req);
    Task<ApiResponse<Stream>> GetPurchaseDetailsServerReportPDF(PurchaseDetailsReportRequest req);
    Task<ApiResponse<Stream>> GetPurchaseOrderServerReportPDF(PurchaseOrderReportRequest req);
    Task<PurchaseReturnReportResponse> GetPurchaseReturnReport(PurchaseReturnReportRequest req);
    Task<ApiResponse<Stream>> GetPurchaseReturnSummaryServerReportPDF(PurchaseReturnReportRequest req);
    Task<PurchaseReturnDetailsReportResponse> GetPurchaseReturnDetailsReport(PurchaseReturnDetailsReportRequest req);
    Task<ApiResponse<Stream>> GetPurchaseReturnDetailsServerReportPDF(PurchaseReturnDetailsReportRequest req);
    #endregion

    #region Transaction Monitor
    Task<TransactionMonitorResponse> GetTransactionMonitorReportAsync(TransactionMonitorRequest req);
    #endregion

    #region Customer Report
    Task<CustomerStatementResponse> GetCustomerStatementReport(CustomerStatementRequest req);
    Task<List<CustomerSummaryDTO>> GetCustomerSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null);
    Task<List<CustomerPendingInvoiceDTO>> GetCustomerPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null);
    Task<List<CustomerListDTO>> GetCustomerListAsync();

    #endregion

    #region Account Report
    Task<JournalEntryReportResponse> GetJournalEntryReport(JournalEntryReportRequest req);
    Task<TrialBalanceReportResponse> GetTrialBalanceReport(TrialBalanceReportRequest req);
    Task<ExpenseReportResponse> GetExpenseReport(ExpenseReportRequest req);
    Task<ProfitAndLossReportResponse> GetProfitAndLossReport(ProfitAndLossReportRequest req);
    Task<BalanceSheetReportResponse> GetBalanceSheetReport(BalanceSheetReportRequest req);
    Task<LedgerReportResponse> GetLedgerReport(LedgerReportRequest req);
    Task<ChartOfAccountReportResponse> GetChartOfAccountReport(ChartOfAccountReportRequest req);
    #endregion

    #region Zatca
    Task<int> GetFailedCount();
    Task<PendingIcvData> GetPendingICVData(ICVReqDTO entry);
    Task<IcvPendingPDTO> GetPendingICVList(IcvPendReqDTO entry);
    Task ZatcaReUploadAll(IcvReuploadAll dto);
    Task<OutputResponse> ZatcaReUploadSelected(IcvReupload dto);
    Task<CSRResponse> GenerateCSR(CSRRequest request);
    Task<CSRResponse> GetCsr(int id);
    Task<OutputResponse> AddNewCSR(CSRResponse request);
    Task<ZatcaInvoiceDTO> GetZatcaPendingInvoicesAsync(PaginationModel pmodel);
    Task UploadPendingSimplifiedInvoices(int BranchId);
    #endregion

    #region User Privilege
    Task<List<UserPrivilegeHdr>> GetUserprivileges();
    Task<UserPrivilegeDTO> GetUserprivilege(int id);
    Task<List<WebUserPrivilege>> GetWebUserprivilege(PrivilegeDTO dto);
    Task<OutputResponse> NewUserprivilege(UserPrivilegeDTO entity);
    Task<OutputResponse> UpdateUserprivilege(UserPrivilegeDTO entity);
    Task<OutputResponse> DeleteUserprivilege(int id);
    Task<TransactionPDTO> GetUserPrivilegePDTO(PaginationModel pmodel);
    #endregion

    #region JobOrder
    Task<string> GetNextJobOrderCode(int branchId);
    Task<int> GetNewJobOrderDocNo(int branchId);
    Task<JobOrderHdr> GetJobOrderById(int id);
    Task<OutputResponse> CreateJobOrder(JobOrderHdr hdr);
    Task<OutputResponse> UpdateJobOrder(JobOrderHdr hdr);
    Task<OutputResponse> DeleteJobOrder(int id);
    Task<TransactionPDTO> GetJobOrderPDetails(PaginationModel pmodel);
    #endregion

    #region JobCard
    Task<string> GetNextJobCardCode(int branchId);
    Task<int> GetNewJobCardDocNo(int branchId);
    Task<JobCardHdr> GetJobCardById(int id);
    Task<OutputResponse> CreateJobCard(JobCardHdr hdr);
    Task<OutputResponse> UpdateJobCard(JobCardHdr hdr);
    Task<OutputResponse> DeleteJobCard(int id);
    Task<TransactionPDTO> GetJobCardPDetails(PaginationModel pmodel);
    Task<List<JobCardHistoryDTO>> GetJobCardHistory(string plateNo);
    #endregion

    #region ProductImport
    Task<ProductImportResultDTO> ImportProductsFromCsvAsync(List<ProductImportRowDTO> rows);
    Task<byte[]> GetProductImportSampleCsvAsync();
    #endregion

    #region ProductExport
    Task<List<ProductExportRowDTO>> GetProductsForExportAsync();
    Task<byte[]> ExportProductsToCsvAsync();
    #endregion

    #region Shift Closing
    Task<OutputResponse> SaveShiftClosingAsync(ShiftClosingRecord record);
    Task<ShiftClosingReportResponse> GetShiftClosingReportAsync(ShiftClosingReportRequest request);
    Task<ShiftClosingRecord?> GetShiftClosingByIdAsync(int id);
    #endregion

}
