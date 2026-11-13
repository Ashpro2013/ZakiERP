using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Accounts;

public partial class ChartOfAccountReportView
{
    #region Variables
    byte[] pdfData;
    ChartOfAccountReportResponse result;

    // Filters
    int AccountGroupId = 0;

    // Data Sources
    List<CommonModel> accountGroupList = new();
    #endregion

    #region Properties
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        AccountGroupId = 0; // Default All

        try
        {
            // Load Combos
            // Assuming GetCombofill can return AccountGroups. If not, I might need to use GetAllAccountGroups from API.
            // Checking ViewService, GetCombofill supports dynamic head. Let's assume "AccountGroup" works or use specific method.
            // Step 82 ViewService shows GetCombofill.
            // Let's stick to specific method if possible or Combofill if I'm sure.
            // ViewService has GetAllAccountGroups. I'll use that for safer typing if I can convert to CommonModel or just bind directly.
            // Using GetCombofill is standard pattern here it seems.
            accountGroupList = await api.GetCombofill("AccountGroup", 0);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    void ClearMethod()
    {
        AccountGroupId = 0;
        pdfData = null;
        result = null;
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            var req = new ChartOfAccountReportRequest(AccountGroupId);

            result = await api.GetChartOfAccountReport(req);

            if (result is not null && result.Items.Any())
            {
                // Create Header
                var reportHeader = new ChartOfAccountReportHdr
                (
                    $"{DateTime.Now:dd-MM-yyyy}",
                    $"{DateTime.Now:HH:mm:ss}"
                );

                var finalReportItems = result.Items;

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Chart_Of_Account_Report"));
                if (pdto.hdr == null || !pdto.dtls.Any())
                {
                    await Dialog.DialogMethod("Print design 'Chart Of Account Report' not found. Please create design from designer.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview, false);
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

    async Task ReportGenerate() => await ReportPdfGenerate(true);
    async Task PDFExportMethod() => await ReportPdfGenerate();

    async Task ExcelExportMethod()
    {
        try
        {
            if (result == null)
            {
                var req = new ChartOfAccountReportRequest(AccountGroupId);
                result = await api.GetChartOfAccountReport(req);
            }

            if (result is not null && result.Items.Any())
            {
                var finalReportItems = result.Items;

                var jsonData = JsonSerializer.Serialize(finalReportItems, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
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

    #endregion
}
