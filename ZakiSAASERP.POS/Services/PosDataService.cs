using ZakiSAASERP.Interface.Masters;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Interface.Transaction.Sales;
using ZakiSAASERP.Models.Company;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;

namespace ZakiSAASERP.POS.Services;

public class PosDataService(
    ICategoryMasterRepo categoryRepo,
    IItemMasterRepo itemRepo,
    ICustomerMasterRepo customerRepo,
    IGeneralRepo generalRepo,
    ISalesInvoiceRepo invoiceRepo,
    ISalesInvoiceHoldRepo holdRepo,
    IPrintDesignRepo printDesignRepo,
    ICompanyDetailsRepo companyDetailsRepo,
    IPermanentSettingsRepo permanentSettingsRepo,
    ISubCategoryMasterRepo subCategoryRepo,
    IBrandMasterRepo brandRepo,
    IUnitMasterRepo unitRepo,
    IVatMasterRepo vatRepo) : IPosDataService
{
    public async Task InitializeSystemSettingsAsync()
    {
        try
        {
            var settings = await permanentSettingsRepo.GetAllSettings();
            if (settings != null && settings.Any())
            {
                PermanentSettingsData.Load(settings);
            }
        }
        catch { }

        AppData.IsWindows = true;
        AppData.IsMaui = false;
        AppData.BranchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
        AppData.UserId = AppData.UserId ?? 1;
        AppData.FYearId = AppData.FYearId ?? 1;

        try
        {
            var company = await companyDetailsRepo.GetByBranchIdAsync(AppData.BranchId);
            if (company != null)
            {
                AppData.CompanyDetails = company;
            }
            else
            {
                AppData.CompanyDetails = new CompanyDetails
                {
                    Name = "ZAKI ERP STORE",
                    FK_BranchId = AppData.BranchId,
                    VatNo = "300000000000003"
                };
            }
        }
        catch
        {
            AppData.CompanyDetails = new CompanyDetails
            {
                Name = "ZAKI ERP STORE",
                FK_BranchId = AppData.BranchId,
                VatNo = "300000000000003"
            };
        }
    }

    public async Task<List<CommonModel>> GetCustomersAsync(int branchId)
    {
        return await generalRepo.GetComboFill("Customers", branchId);
    }

    public async Task<List<CommonModel>> GetUnitsAsync()
    {
        return await generalRepo.GetComboFill("Unit", 1);
    }

    public async Task<List<CommonModel>> GetSalesmenAsync()
    {
        return await generalRepo.GetComboFill("SalesMan", 1);
    }

    public async Task<List<CategoryMaster>> GetCategoriesAsync()
    {
        return await categoryRepo.GetAllAsync();
    }

    public async Task<List<ExMasters>> GetPosProductsAsync(int branchId)
    {
        return await generalRepo.GetMasters("posproduct", branchId);
    }

    public async Task<ItemMultiUnit?> GetItemMultiUnitByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return null;
        return await itemRepo.GetItemMultiUnit(0, 0, barcode.Trim());
    }

    public async Task<ItemMultiUnit?> GetItemMultiUnitByProductIdAsync(int productId)
    {
        var units = await itemRepo.GetItemMultiUnits(productId);
        return units?.FirstOrDefault();
    }

    public async Task<ItemMaster?> GetItemByIdAsync(int itemId)
    {
        return await itemRepo.GetAsync(itemId);
    }

    public async Task<ItemMaster?> GetItemByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return null;
        return await itemRepo.GetByBarcodeAsync(barcode.Trim());
    }

    public async Task<List<UnitMaster>> GetAllUnitsAsync()
    {
        return await unitRepo.GetAllAsync();
    }

    public async Task<List<SubCategoryMaster>> GetSubCategoriesAsync()
    {
        return await subCategoryRepo.GetAllAsync();
    }

    public async Task<List<BrandMaster>> GetBrandsAsync()
    {
        return await brandRepo.GetAllAsync();
    }

    public async Task<List<VatMaster>> GetVatsAsync()
    {
        return await vatRepo.GetAllAsync();
    }

    public async Task<string> GetNextItemCodeAsync(int? categoryId, int? subCategoryId, string? catCode, string? subCatCode)
    {
        return await itemRepo.GetNextItemCodeAsync(categoryId, subCategoryId, catCode, subCatCode);
    }

    public async Task<OutputResponse> CreateItemAsync(ItemMaster item)
    {
        return await itemRepo.CreateAsync(item);
    }

    public async Task<decimal> GetVatPercentageAsync(int itemId)
    {
        return await generalRepo.GetValueMethod("vatFromItemId", itemId);
    }

    public async Task<string> GetNextInvoiceNoAsync(int branchId, bool isAutoCode)
    {
        if (isAutoCode)
        {
            var code = await invoiceRepo.GetNextInvoiceNoAsync();
            if (!string.IsNullOrWhiteSpace(code)) return code;
        }

        var vchrNo = await invoiceRepo.GetNextVchrNo(branchId);
        return vchrNo.ToString();
    }

    public async Task<OutputResponse> SaveInvoiceAsync(SalesInvoiceDTO dto)
    {
        return await invoiceRepo.CreateAsync(dto);
    }

    public async Task<OutputResponse> HoldInvoiceAsync(SalesInvoiceHoldDTO dto)
    {
        return await holdRepo.CreateAsync(dto);
    }

    public async Task<List<SalesInvoiceHoldHdr>> GetHeldInvoicesAsync(int branchId, string? terminal = null)
    {
        return await holdRepo.GetAllHeldAsync(branchId, terminal);
    }

    public async Task<SalesInvoiceHoldDTO?> GetHeldInvoiceByIdAsync(int id)
    {
        return await holdRepo.GetAsync(id);
    }

    public async Task<OutputResponse> DeleteHeldInvoiceAsync(int id)
    {
        return await holdRepo.DeleteAsync(id);
    }

    public async Task<OutputResponse> DeleteAllHeldInvoicesAsync(int branchId, string? terminal = null)
    {
        return await holdRepo.DeleteAllAsync(branchId, terminal);
    }

    public async Task<PrintDesignDTO?> GetPrintDesignAsync(string designName)
    {
        try
        {
            var req = new PrintDesignReq(null, designName, true);
            var design = await printDesignRepo.GetAsync(req);
            if (design?.hdr != null && design.dtls.Any())
            {
                return design;
            }
        }
        catch { }

        // Fallback default thermal 80mm design if none saved
        return CreateFallbackThermalDesign();
    }

    public async Task<CompanyDetails?> GetCompanyDetailsAsync(int branchId)
    {
        return await companyDetailsRepo.GetByBranchIdAsync(branchId);
    }

    public async Task<List<CustomerMaster>> SearchCustomersAsync(string search, int branchId)
    {
        var pagination = new PaginationModel(1, 50, "Name", "asc", search, BranchId: branchId);
        var (list, _) = await customerRepo.GetAllWithPagination(pagination);
        return list;
    }

    public async Task<List<ItemDialogDTO>> SearchItemsAsync(string search, int branchId)
    {
        var pagination = new PaginationModel(1, 60, "ItemName", "asc", search, BranchId: branchId);
        var (list, _) = await itemRepo.GetItemsForDialogWithStock(pagination);
        return list;
    }

    public async Task<SalesInvoiceDTO?> GetInvoiceByIdAsync(int id)
    {
        return await invoiceRepo.GetAsync(id);
    }

    public async Task<SalesInvoiceDTO?> SearchInvoiceByNoAsync(int branchId, string invoiceNo)
    {
        var pagination = new PaginationModel(1, 10, "SalesInvoiceNo", "desc", invoiceNo, BranchId: branchId);
        var res = await invoiceRepo.GetAllWithPagination(pagination);
        var first = res?.PTransactions?.FirstOrDefault();
        if (first != null && first.Id > 0)
        {
            return await invoiceRepo.GetAsync(first.Id);
        }
        return null;
    }

    public async Task<List<ExMasters>> SearchInvoicesAsync(string search, int branchId)
    {
        var pagination = new PaginationModel(1, 100, "Description", "desc", search, BranchId: branchId);
        var res = await invoiceRepo.GetAllWithPagination(pagination);
        return res?.PTransactions ?? [];
    }

    private static PrintDesignDTO CreateFallbackThermalDesign()
    {
        var hdr = new PrintDesignHdr
        {
            DesignName = "Standard Thermal 80mm",
            VoucherType = "Sales_Invoice_Thermal",
            PageWidth = 280,
            PageHeight = 1000,
            LinesPerPage = 150,
            SpaceBtwnLines = 2,
            PrintType = "Thermal"
        };

        var dtls = new List<PrintDesignDtls>
        {
            // Company Header
            new() { Location = "Company_Section", ControlType = "ValueControl", FieldText = "Name", PositionX = 10, PositionY = 10, Width = 260, Height = 22, FontSize = 12, FStyle = "Bold", Alignment = "Center" },
            new() { Location = "Company_Section", ControlType = "ValueControl", FieldText = "City", PositionX = 10, PositionY = 32, Width = 260, Height = 18, FontSize = 9, Alignment = "Center" },
            new() { Location = "Company_Section", ControlType = "ValueControl", FieldText = "VatNo", PositionX = 10, PositionY = 50, Width = 260, Height = 18, FontSize = 9, Alignment = "Center" },
            new() { Location = "Company_Section", ControlType = "HorizontalDotedLine", PositionX = 10, PositionY = 70, Width = 260, Height = 5 },

            // Invoice Header
            new() { Location = "Invoice_Section", ControlType = "LabelControl", FieldText = "Tax Invoice / فاتورة ضريبية", PositionX = 10, PositionY = 75, Width = 260, Height = 20, FontSize = 10, FStyle = "Bold", Alignment = "Center" },
            new() { Location = "Invoice_Section", ControlType = "LabelControl", FieldText = "Inv No:", PositionX = 10, PositionY = 100, Width = 70, Height = 18, FontSize = 9, FStyle = "Bold" },
            new() { Location = "Invoice_Section", ControlType = "ValueControl", FieldText = "HdrNo", PositionX = 85, PositionY = 100, Width = 185, Height = 18, FontSize = 9 },
            new() { Location = "Invoice_Section", ControlType = "LabelControl", FieldText = "Date:", PositionX = 10, PositionY = 120, Width = 70, Height = 18, FontSize = 9, FStyle = "Bold" },
            new() { Location = "Invoice_Section", ControlType = "ValueControl", FieldText = "InvoiceDate", PositionX = 85, PositionY = 120, Width = 185, Height = 18, FontSize = 9 },
            new() { Location = "Invoice_Section", ControlType = "HorizontalDotedLine", PositionX = 10, PositionY = 142, Width = 260, Height = 5 },

            // Table Details Header
            new() { Location = "Invoice_Details_Header", ControlType = "LabelControl", FieldText = "Item / الوصف", PositionX = 10, PositionY = 148, Width = 130, Height = 18, FontSize = 8, FStyle = "Bold" },
            new() { Location = "Invoice_Details_Header", ControlType = "LabelControl", FieldText = "Qty", PositionX = 142, PositionY = 148, Width = 35, Height = 18, FontSize = 8, FStyle = "Bold", Alignment = "Center" },
            new() { Location = "Invoice_Details_Header", ControlType = "LabelControl", FieldText = "Rate", PositionX = 180, PositionY = 148, Width = 45, Height = 18, FontSize = 8, FStyle = "Bold", Alignment = "Right" },
            new() { Location = "Invoice_Details_Header", ControlType = "LabelControl", FieldText = "Total", PositionX = 228, PositionY = 148, Width = 45, Height = 18, FontSize = 8, FStyle = "Bold", Alignment = "Right" },
            new() { Location = "Invoice_Details_Header", ControlType = "HorizontalLine", PositionX = 10, PositionY = 168, Width = 260, Height = 2 },

            // Table Item Detail Row
            new() { Location = "Details_Area", ControlType = "ValueControl", FieldText = "ItemName", PositionX = 10, PositionY = 175, Width = 130, Height = 18, FontSize = 8 },
            new() { Location = "Details_Area", ControlType = "ValueControl", FieldText = "Quantity", PositionX = 142, PositionY = 175, Width = 35, Height = 18, FontSize = 8, Alignment = "Center" },
            new() { Location = "Details_Area", ControlType = "ValueControl", FieldText = "InRate", PositionX = 180, PositionY = 175, Width = 45, Height = 18, FontSize = 8, Alignment = "Right" },
            new() { Location = "Details_Area", ControlType = "ValueControl", FieldText = "LineTotal", PositionX = 228, PositionY = 175, Width = 45, Height = 18, FontSize = 8, Alignment = "Right" },

            // Footer Section
            new() { Location = "Footer_Section", ControlType = "HorizontalLine", PositionX = 10, PositionY = 200, Width = 260, Height = 2 },
            new() { Location = "Footer_Section", ControlType = "LabelControl", FieldText = "Gross / الإجمالي:", PositionX = 10, PositionY = 205, Width = 130, Height = 18, FontSize = 9, FStyle = "Bold" },
            new() { Location = "Footer_Section", ControlType = "ValueControl", FieldText = "GrossAmount", PositionX = 145, PositionY = 205, Width = 125, Height = 18, FontSize = 9, Alignment = "Right" },
            new() { Location = "Footer_Section", ControlType = "LabelControl", FieldText = "Discount / الخصم:", PositionX = 10, PositionY = 225, Width = 130, Height = 18, FontSize = 9 },
            new() { Location = "Footer_Section", ControlType = "ValueControl", FieldText = "LineDsicount", PositionX = 145, PositionY = 225, Width = 125, Height = 18, FontSize = 9, Alignment = "Right" },
            new() { Location = "Footer_Section", ControlType = "LabelControl", FieldText = "VAT Total / الضريبة:", PositionX = 10, PositionY = 245, Width = 130, Height = 18, FontSize = 9 },
            new() { Location = "Footer_Section", ControlType = "ValueControl", FieldText = "TaxTotal", PositionX = 145, PositionY = 245, Width = 125, Height = 18, FontSize = 9, Alignment = "Right" },
            new() { Location = "Footer_Section", ControlType = "HorizontalLine", PositionX = 10, PositionY = 265, Width = 260, Height = 2 },
            new() { Location = "Footer_Section", ControlType = "LabelControl", FieldText = "Net Total / الصافي:", PositionX = 10, PositionY = 270, Width = 130, Height = 22, FontSize = 11, FStyle = "Bold" },
            new() { Location = "Footer_Section", ControlType = "ValueControl", FieldText = "GrandTotal", PositionX = 145, PositionY = 270, Width = 125, Height = 22, FontSize = 12, FStyle = "Bold", Alignment = "Right" },

            // QR Code
            new() { Location = "Footer_Bottom", ControlType = "ValueControl", FieldText = "InvoiceQRCode", PositionX = 85, PositionY = 300, Width = 110, Height = 110, Alignment = "Center" },
            new() { Location = "Footer_Bottom", ControlType = "LabelControl", FieldText = "Thank You For Visiting! / شكراً لزيارتكم", PositionX = 10, PositionY = 415, Width = 260, Height = 20, FontSize = 8, Alignment = "Center" }
        };

        return new PrintDesignDTO(hdr, dtls);
    }
}

