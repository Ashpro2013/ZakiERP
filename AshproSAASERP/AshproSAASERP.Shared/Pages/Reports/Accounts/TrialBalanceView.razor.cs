using System.Text.Json;
namespace AshproSAASERP.Shared.Pages.Reports.Accounts;

public partial class TrialBalanceView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    TrialBalanceReportResponse? result;
    private List<TrialBalanceReportDtls> reportData = new();

    // Filters
    int BranchId = 0;
    bool ShowZeroBalance = false;

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
            await Dialog.DialogMethod(ex.Message);
        }
    }

    void ClearMethod()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        BranchId = 0;
        ShowZeroBalance = false;
        pdfData = null;
        result = null;
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            var req = new TrialBalanceReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate ?? DateTime.Today,
                ToDate = ToDate ?? DateTime.Today,
                ShowZeroBalance = ShowZeroBalance
            };

            result = await api.GetTrialBalanceReport(req);
            if (result == null || !result.Items.Any())
            {
                await Dialog.DialogMethod(Localizer("No records found"));
                return;
            }
            if (IsPreview)
            {
                reportData = result.Items;
                StateHasChanged();
                return;
            }
            if (result is not null && result.Items.Any())
            {
                var reportHeader = new TrialBalanceReportHdr
                (
                    $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    result.TotalOpening.ToString("N2"),
                    result.TotalTransactionDebit.ToString("N2"),
                    result.TotalTransactionCredit.ToString("N2"),
                    result.TotalClosing.ToString("N2")
                );

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Trial_Balance_Report"));
                if (pdto.hdr == null || !pdto.dtls.Any())
                {
                    await Dialog.DialogMethod("Print design 'Trial Balance Report' not found. Please create design from designer.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, false);
            }
            else
            {
                await Dialog.DialogMethod("No records found");
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task RowClickEvent(TableRowClickEventArgs<TrialBalanceReportDtls> args)
    {
        var item = args.Item;
        if (args.MouseEventArgs.Detail >= 2 && item is not null && !string.IsNullOrEmpty(item.AccountCode))
        {
            var req = new LedgerReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate,
                AccountId = item.Id
            };
            await CrudHelper.Navigation(TabService, 0, "LedgerReport", req);
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
                var req = new TrialBalanceReportRequest
                {
                    BranchId = BranchId,
                    FromDate = FromDate ?? DateTime.Today,
                    ToDate = ToDate ?? DateTime.Today,
                    ShowZeroBalance = ShowZeroBalance
                };
                result = await api.GetTrialBalanceReport(req);
            }

            if (result is not null && result.Items.Any())
            {
                var report = result.Items.ToList(); // Clone or ToList to modify if needed, though Items is List

                // We can add a footer row if needed for Excel, but usually user wants raw data.
                // Or we can construct a new list with footer.

                var excelData = report;
                // Maybe add summary row?
                // For now, raw data export.

                var jsonData = JsonSerializer.Serialize(excelData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
            }
            else
            {
                Dialog.DialogMethod("No records found");
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion
}
