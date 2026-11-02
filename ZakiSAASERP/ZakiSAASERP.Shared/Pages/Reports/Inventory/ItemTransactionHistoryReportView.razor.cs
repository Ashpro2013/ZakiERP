using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Inventory;

public partial class ItemTransactionHistoryReportView
{
    #region Variables
    byte[]? pdfData;
    ItemTransactionHistoryReportResponse? result;
    List<CommonModel> branchList = new();
    List<CommonModel> storeList = new();
    List<CommonModel> itemList = new();
    CommonModel? selectedItem;
    ItemTransactionHistoryReportRequest req = new();
    PrintDesignDTO pdto;
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        req.FromDate = DateTime.Now.Date;
        req.ToDate = DateTime.Now.Date;
        branchList = await ViewService.GetCombofill("Branch", 0);
        storeList = await ViewService.GetCombofill("Store", 0);
        itemList = await ViewService.GetCombofill("product", AppData.BranchId);
        pdto = await ViewService.GetPrintDesignAsync(new PrintDesignReq(0, "Item_Transaction_History_Report"));
        await InvokeAsync(StateHasChanged);
    }

    private async Task<IEnumerable<CommonModel>> SearchItem(string value, CancellationToken token)
    {
        if (string.IsNullOrEmpty(value))
            return itemList;
        return itemList.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) || x.Code.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            if (selectedItem is not null) req.ItemId = selectedItem.Id;
            else { Dialog.DialogMethod(Localizer("No Item Selected")); return; }

            result = await ViewService.GetItemTransactionHistoryReport(req);
            if (result == null || result.Items == null || !result.Items.Any())
            {
                Dialog.DialogMethod(Localizer("No records found")); return;
            }
            // Dynamic Header
            var reportHeader = new
            {
                DateRange = $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                PrintDate = DateTime.Now.ToString("HH:mm:ss"),
                TotalInQty = (result.TotalInQty ?? 0).ToString("G29"),
                TotalOutQty = (result.TotalOutQty ?? 0).ToString("G29"),
                ItemName = selectedItem?.Name
            };
            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };
            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                Dialog.DialogMethod("Print design 'Item_Transaction_History_Report' not found.");
                return;
            }
            SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
            pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);
        }
        catch (Exception ex) { Dialog.DialogMethod(ex.Message); }
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
            if (selectedItem is not null) req.ItemId = selectedItem.Id; else req.ItemId = null;
            result = await ViewService.GetItemTransactionHistoryReport(req);
        }
        if (result is not null && result.Items is not null && result.Items.Any())
        {

            var jsonData = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
        }
        else
        {
            Dialog.DialogMethod(Localizer("No records found"));
        }
    }
    void ClearMethod()
    {
        req = new ItemTransactionHistoryReportRequest { FromDate = DateTime.Now.Date, ToDate = DateTime.Now.Date };
        selectedItem = null;
        pdfData = null;
        result = null;
    }
    #endregion
}
