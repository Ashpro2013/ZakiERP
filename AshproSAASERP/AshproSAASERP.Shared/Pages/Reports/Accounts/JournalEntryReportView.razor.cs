using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Accounts;

public partial class JournalEntryReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    JournalEntryReportResponse result;

    // Filters
    int BranchId = 0;
    int AccountId = 0;
    string AccountName = "";

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
        AccountId = 0;
        AccountName = "";
        pdfData = null;
        result = null;
    }

    async Task AccountSearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        // Using AccountSearchDialog assuming it exists, or similar
        var dialog = await DialogService.ShowAsync<AccountSearchDialog>(Localizer("AccountSearch"), options);
        var dialogResult = await dialog.Result;
        if (!dialogResult!.Canceled && dialogResult.Data is AccountMaster selectedAccount)
        {
            AccountId = selectedAccount.AccountId;
            AccountName = selectedAccount.Name;
            StateHasChanged();
        }
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            var req = new JournalEntryReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate,
                AccountId = AccountId == 0 ? null : AccountId
            };

            result = await api.GetJournalEntryReport(req);

            if (result is not null && result.Items.Any())
            {
                var reportHeader = new JournalEntryReportHdr(
                    $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    (result.TotalDebit ?? 0).ToString("N2"),
                    (result.TotalCredit ?? 0).ToString("N2")
                );

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Journal_Entry_Report"));
                if (pdto.hdr == null || !pdto.dtls.Any())
                {
                    Dialog.DialogMethod("Print design 'Journal_Entry_Report' not found. Please create design from designer.");
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
            var req = new JournalEntryReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate,
                AccountId = AccountId == 0 ? null : AccountId
            };

            if (result == null)
            {
                result = await api.GetJournalEntryReport(req);
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
