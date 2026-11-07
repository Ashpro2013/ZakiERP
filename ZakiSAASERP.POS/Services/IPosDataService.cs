using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;

namespace ZakiSAASERP.POS.Services;

public interface IPosDataService
{
    Task InitializeSystemSettingsAsync();
    Task<List<CommonModel>> GetCustomersAsync(int branchId);
    Task<List<CommonModel>> GetUnitsAsync();
    Task<List<CommonModel>> GetSalesmenAsync();
    Task<List<CategoryMaster>> GetCategoriesAsync();
    Task<List<ExMasters>> GetPosProductsAsync(int branchId);
    Task<ItemMultiUnit?> GetItemMultiUnitByBarcodeAsync(string barcode);
    Task<ItemMultiUnit?> GetItemMultiUnitByProductIdAsync(int productId);
    Task<ItemMaster?> GetItemByIdAsync(int itemId);
    Task<decimal> GetVatPercentageAsync(int itemId);
    Task<string> GetNextInvoiceNoAsync(int branchId, bool isAutoCode);
    Task<OutputResponse> SaveInvoiceAsync(SalesInvoiceDTO dto);
    Task<OutputResponse> HoldInvoiceAsync(SalesInvoiceHoldDTO dto);
    Task<List<SalesInvoiceHoldHdr>> GetHeldInvoicesAsync(int branchId, string? terminal = null);
    Task<SalesInvoiceHoldDTO?> GetHeldInvoiceByIdAsync(int id);
    Task<OutputResponse> DeleteHeldInvoiceAsync(int id);
    Task<OutputResponse> DeleteAllHeldInvoicesAsync(int branchId, string? terminal = null);
    Task<PrintDesignDTO?> GetPrintDesignAsync(string designName);
    Task<CompanyDetails?> GetCompanyDetailsAsync(int branchId);
    Task<List<CustomerMaster>> SearchCustomersAsync(string search, int branchId);
    Task<List<ItemDialogDTO>> SearchItemsAsync(string search, int branchId);
    Task<SalesInvoiceDTO?> GetInvoiceByIdAsync(int id);
    Task<SalesInvoiceDTO?> SearchInvoiceByNoAsync(int branchId, string invoiceNo);
    Task<List<ExMasters>> SearchInvoicesAsync(string search, int branchId);
}

