using AshproSAASERP.Models.Company;
using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.Transactions.Inventory;
using AshproSAASERP.Models.User;
namespace AshproSAASERP.Models.DTO;

#region Common
public record PaginationModel(int page, int pageSize, string? sortColumn, string? sortDirection, string? search, int? FilterId = null, int BranchId = 1, int? StoreId = null, string? Terminal = null)
{
    public PaginationModel() : this(1, 10, null, null, null) { }
}
public class PaginationResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
}

#endregion

#region Masters
public record UnitPDTO(List<UnitMaster> PUnits, int TotalCount);
public record AccountHeadPDTO(List<AccountHeadMaster> PAccountHeads, int TotalCount);
public record AccountPDTO(List<AccountMaster> PAccounts, int TotalCount);
public record BranchPDTO(List<BranchMaster> PBranches, int TotalCount);
public record FinancialYearPDTO(List<FinancialYear> PFinancialYears, int TotalCount);
public record SalesManPDTO(List<SalesManMaster> PSalesMen, int TotalCount);
public record CategoryPDTO(List<CategoryMaster> PCategories, int TotalCount);
public record SubCategoryPDTO(List<SubCategoryMaster> PSubCategories, int TotalCount);
public record CustomerGroupPDTO(List<CustomerGroupMaster> PCustomerGroups, int TotalCount);
public record CountryPDTO(List<CountryMaster> PCountries, int TotalCount);
public record CustomerLocationPDTO(List<CustomerLocationMaster> PCustomerLocations, int TotalCount);
public record VehiclePDTO(IEnumerable<VehicleMaster> Items, int TotalCount);
public record RepairPDTO(IEnumerable<RepairMaster> Items, int TotalCount);
public record TechnicianPDTO(IEnumerable<TechnicianMaster> Items, int TotalCount);

public record VatPDTO(List<VatMaster> PVats, int TotalCount);
public record BrandPDTO(List<BrandMaster> PBrands, int TotalCount);
public record CurrencyPDTO(List<CurrencyMaster> PCurrencies, int TotalCount);
public record PurchaserPDTO(List<PurchaserMaster> PPurchasers, int TotalCount);
public record LoginCompanyPDTO(List<LoginCompany> PLCompanies, int TotalCount);
public record UserPDTO(List<UserDetails> PUsers, int TotalCount);
public record CustomerPDTO(List<CustomerMaster> PCustomers, int TotalCount);
public record SupplierPDTO(List<SupplierMaster> PSuppliers, int TotalCount);
public record ItemMasterPDTO(List<ItemMaster> PItems, int TotalCount);
public record CustomerOpEntryPDTO(List<CustomerOPEntry> PCustomerOpEntries, int TotalCount);
public record SupplierOpEntryPDTO(List<SupplierOPEntry> PSupplierOpEntries, int TotalCount);
public record StorePDTO(List<StoreMaster> PStoreEntries, int TotalCount);
public record OpeningStockEntryPDTO(List<OpeningStockEntry> POpeningStockEntries, int TotalCount);
public record ProjectPDTO(List<ProjectMaster> PProjects, int TotalCount);
public record AccountGroupPDTO(List<AccountGroup> PAccountGroups, int TotalCount);
public record AccountSubGroupPDTO(List<AccountSubGroup> PAccountSubGroups, int TotalCount);
public record AccountsPDTO(List<AccountMaster> PAccountsEntries, int TotalCount);
public record ItemMultiUnitPDTO(List<ItemMultiUnit> PItemMultiUnits, int TotalCount);
public record ItemDialogPDTO(List<ItemDialogDTO> PItems, int TotalCount);
public record ItemMultiUnitDialogPDTO(List<ItemMultiUnitDialogDTO> PItems, int TotalCount);

#endregion

#region Transactions


#region Sales
public record TransactionPDTO(List<ExMasters> PTransactions, int TotalCount);
public record SupplierPaymentPDTO(List<AshproSAASERP.Models.Transactions.Purchase.SupplierPaymentHdr> PTransactions, int TotalCount);
#endregion

public record DamageStockPDTO(List<DamageStockHdr> PDamageStocks, int TotalCount);
#endregion



