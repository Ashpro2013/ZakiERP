using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Sales;

public partial class CategoryWiseReportView
{
    #region Variables
    byte[]? pdfData;
    bool IsLoading = false;
    CategoryWiseSalesReportResponse? result;
    List<CommonModel> branchList = new();
    List<CommonModel> categoryList = new();
    CategoryWiseSalesReportRequest req = new();
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Load Combos
            branchList = await api.GetCombofill("Branch", 0);
            var cats = await api.GetAllCategory();
            categoryList = cats.Select(x => new CommonModel { Id = x.CategoryId, Name = x.Name }).ToList();
            ClearMethod();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    void ClearMethod()
    {
        req = new CategoryWiseSalesReportRequest { FromDate = DateTime.Now, ToDate = DateTime.Now, BillType = "All", BranchId = AppData.BranchId, CategoryId = 0, InvoiceType = "All" };
        pdfData = null;
        result = null;
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            pdfData = null;
            IsLoading = true;
            result = await api.GetCategoryWiseReport(req);

            if (result is not null && result.Items.Any())
            {
                var taxable = (result.TotalGross ?? 0) - (result.TotalDiscount ?? 0);
                var reportHeader = new CategoryWiseSalesReportHdr(
                    $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    (result.TotalGross ?? 0).ToString("N2"),
                    (result.TotalDiscount ?? 0).ToString("N2"),
                    taxable.ToString("N2"),
                    (result.TotalVat ?? 0).ToString("N2"),
                    (result.TotalNet ?? 0).ToString("N2")
                );

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Category_Wise_Sales_Report"));
                if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
                {
                    await Dialog.DialogMethod("Print design 'Category Wise Sales Report' not found.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);
            }
            else
            {
                await Dialog.DialogMethod(Localizer("No records found"));
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task ReportGenerate() => await ReportPdfGenerate(true);
    async Task PDFExportMethod() => await ReportPdfGenerate();

    async Task ExcelExportMethod()
    {
        try
        {
            if (result == null)
            {
                result = await api.GetCategoryWiseReport(req);
            }

            if (result is not null && result.Items.Any())
            {
                var report = result.Items;
                var jsonData = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
            }
            else
                await Dialog.DialogMethod(Localizer("No records found"));
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion
}
