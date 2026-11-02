using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Inventory;

public partial class StockAdjustmentView
{
    #region Variables
    byte[]? pdfData;
    StockAdjustmentReportResponse? result;
    List<CommonModel> branchList = new();
    StockAdjustmentReportRequest req = new();
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Load Combos
            branchList = await api.GetCombofill("Branch", 0);
            ClearMethod();
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    void ClearMethod()
    {
        req = new StockAdjustmentReportRequest { FromDate = DateTime.Now, ToDate = DateTime.Now, BranchId = AppData.BranchId };
        pdfData = null;
        result = null;
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            pdfData = null;
            result = await api.GetStockAdjustmentReportAsync(req);

            if (result is not null && result.Items.Any())
            {
                var reportHeader = new StockAdjustmentReportHdr(
                    $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    (result.TotalQty ?? 0).ToString("N2"),
                    (result.TotalAmount ?? 0).ToString("N2")
                );

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                var obj = result.Items.Cast<object>().ToList();
                Dictionary<string, object> ReportDtls = new() { { "Details", obj } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "StockAdjustment_Report"));
                if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
                {
                    Dialog.DialogMethod("Print design 'StockAdjustment_Report' not found.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintReportMethod(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
            }
            else
            {
                Dialog.DialogMethod(Localizer("No records found"));
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
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
                result = await api.GetStockAdjustmentReportAsync(req);
            }

            if (result is not null && result.Items.Any())
            {
                var report = result.Items;
                report.Add(new StockAdjustmentReportDtls(
                   null,             // SL
                   "",             // Date
                   "",             // VoucherNo
                   "",             // RefNo
                   "",             // ItemCode
                   "TOTAL",        // ItemName
                   null,           // CurrentQty
                   result.TotalQty,// Quantity
                   null,           // NewQty
                   "",             // Unit
                   result.TotalAmount,     // Amount
                   ""              // Remarks
               ));
                var jsonData = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
            }
            else
                Dialog.DialogMethod(Localizer("No records found"));
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion
}
