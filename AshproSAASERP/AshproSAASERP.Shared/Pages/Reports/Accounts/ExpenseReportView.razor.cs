using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Accounts;

public partial class ExpenseReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[] pdfData;
    ExpenseReportResponse result;

    // Filters
    int BranchId = 0;

    // Data Sources
    List<CommonModel> branchList = new();
    #endregion

    #region Properties
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        BranchId = 0; // Default All

        try
        {
            // Load Combos
            branchList = await api.GetCombofill("Branch", BranchId);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    void ClearMethod()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        BranchId = 0;
        pdfData = null;
        result = null;
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            var req = new ExpenseReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate
            };

            result = await api.GetExpenseReport(req);

            if (result is not null && result.Items.Any())
            {
                var reportHeader = new ExpenseReportHdr(
                    $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    (result.TotalAmount ?? 0).ToString("N2"),
                    (result.TotalVat ?? 0).ToString("N2"),
                    (result.TotalNet ?? 0).ToString("N2")
                );

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Expense_Report"));
                if (pdto.hdr == null || !pdto.dtls.Any())
                {
                    Dialog.DialogMethod("Print design 'Expense_Report' not found. Please create design from designer.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
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
            var req = new ExpenseReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate
            };

            if (result == null)
            {
                result = await api.GetExpenseReport(req);
            }

            if (result is not null && result.Items.Any())
            {
                var report = result.Items;
                // Add Total Row
                var jsonData = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
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
    #endregion
}
