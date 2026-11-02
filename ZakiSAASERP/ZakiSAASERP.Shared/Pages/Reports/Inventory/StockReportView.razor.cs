using System.Text.Json;
namespace ZakiSAASERP.Shared.Pages.Reports.Inventory;

public partial class StockReportView
{
    #region Variables
    byte[]? pdfData;
    StockReportResponse? result;
    List<CommonModel>? branchList = new();
    List<CommonModel>? storeList = new();
    List<CommonModel>? categoryList = new();
    List<CommonModel>? brandList = new();
    StockReportRequest req = new();
    PrintDesignDTO pdto;
    bool IsLoading = false;
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        req.Date = DateTime.Now.Date;
        branchList = await api.GetCombofill("Branch", 0);
        storeList = await api.GetCombofill("Store", 0);
        categoryList = await api.GetCombofill("Category", 0);
        brandList = await api.GetCombofill("Brand", 0);
        pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Stock_Report"));
        await InvokeAsync(StateHasChanged);
    }
    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            pdfData = null;
            IsLoading = true;
            result = await api.GetStockReport(req);
            if (result == null || !result.reports.Any()) { Dialog.DialogMethod(Localizer("No records found")); return; }
            var report = result.reports;
            var reportHeader = new StockReportHdr(req.Date.ToDateTime().ToString("dd-MM-yyyy"), DateTime.Now.ToString("HH:mm:ss"), result.TotalStockValue.ToString("N2"));
            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", report } };
            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                Dialog.DialogMethod("Print design 'Stock_Report' not found.");
                return;
            }
            SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
            pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);
        }
        catch (Exception ex) { Dialog.DialogMethod(ex.Message); }
        finally { IsLoading = false; }
    }
    async Task ReportGenerate()
    {
        pdfData = null;
        await ReportPdfGenerate(true);
    }
    async Task PDFExportMethod() => await ReportPdfGenerate();
    async Task ExcelExportMethod()
    {
        if (result == null)
        {
            result = await api.GetStockReport(req);
        }
        var report = result.reports;
        var jsonData = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
    }
    #endregion
}
