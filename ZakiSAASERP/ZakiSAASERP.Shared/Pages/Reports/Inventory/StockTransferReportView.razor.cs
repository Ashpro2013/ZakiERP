using System.Text.Json;
namespace ZakiSAASERP.Shared.Pages.Reports.Inventory;

public partial class StockTransferReportView
{
    #region Variables
    byte[]? pdfData;
    StockTransferReportResponse? result;
    List<CommonModel>? branchList = new();
    StockTransferReportRequest req = new();
    PrintDesignDTO pdto;
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        req.FromDate = DateTime.Now.Date;
        req.ToDate = DateTime.Now.Date;
        branchList = await ViewService.GetCombofill("Branch", 0);
        pdto = await ViewService.GetPrintDesignAsync(new PrintDesignReq(0, "Stock_Transfer_Report"));
        await InvokeAsync(StateHasChanged);
    }
    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            result = await ViewService.GetStockTransferReport(req);
            if (result == null || result.Items == null || !result.Items.Any())
            {
                await Dialog.DialogMethod(Localizer("No records found")); return;
            }
            var reportHeader = new StockReportHdr(
                $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                DateTime.Now.ToString("HH:mm:ss"),
                (result.TotalAmount ?? 0).ToString("N2")
            );
            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            var obj = result.Items.Cast<object>().ToList();
            Dictionary<string, object> ReportDtls = new() { { "Details", obj } };
            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                await Dialog.DialogMethod("Print design 'Stock_Transfer_Report' not found.");
                return;
            }
            SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
            pdfData = await pHelper.PrintReportMethod(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);
        }
        catch (Exception ex) { await Dialog.DialogMethod(ex.Message); }
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
            result = await ViewService.GetStockTransferReport(req);
        }
        if (result is not null && result.Items is not null && result.Items.Any())
        {

            var jsonData = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
        }
        else
        {
            await Dialog.DialogMethod(Localizer("No records found"));
        }
    }
    void ClearMethod()
    {
        req = new StockTransferReportRequest { FromDate = DateTime.Now.Date, ToDate = DateTime.Now.Date };
        pdfData = null;
        result = null;
    }
    #endregion
}

