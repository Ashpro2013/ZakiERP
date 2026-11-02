using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Purchase;

public partial class PurchaseDetailsView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[] pdfData;
    PurchaseDetailsReportResponse result;
    bool IsLoading = false;
    // Filters
    int BranchId = 0;
    int SupplierId = 0;
    string SupplierName = "";
    string ItemName = "";

    // Data Sources
    List<CommonModel> branchList = new();
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
        SupplierId = 0;
        SupplierName = "";
        ItemName = "";
        pdfData = null;
        result = null;
    }

    async Task SupplierSearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<SupplierSearchDialog>(Localizer("SupplierSearch"), options);
        var dialogResult = await dialog.Result;
        if (!dialogResult!.Canceled && dialogResult.Data is SupplierMaster selectedSupplier)
        {
            SupplierId = selectedSupplier.SupplierId;
            SupplierName = selectedSupplier.Name;
            StateHasChanged();
        }
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            IsLoading = true;
            var req = new PurchaseDetailsReportRequest(
                BranchId,
                FromDate.ToDateTime(),
                ToDate.ToDateTime(),
                SupplierId == 0 ? null : SupplierId,
                string.IsNullOrEmpty(ItemName) ? null : ItemName
            );

            if (!AppData.IsWindows)
            {
                var response = await api.GetPurchaseDetailsServerReportPDF(req);

                if (response.IsSuccessStatusCode && response.Content is not null)
                {
                    using var ms = new MemoryStream();
                    await response.Content.CopyToAsync(ms);
                    var data = ms.ToArray();

                    if (IsPreview)
                    {
                        pdfData = data;
                    }
                    else
                    {
                        await downloader.DownloadFromStream(data.ToArray(), "PurchaseDetails.pdf");
                    }
                }
                else
                {
                    await Dialog.DialogMethod(Localizer("No records found"));
                }
            }
            else
            {
                result = await api.GetPurchaseDetailsReport(req);

                if (result is not null && result.Reports.Any())
                {
                    // Create header object with totals formatted
                    var ReportHdr = new Dictionary<string, object?>
                    {
                        { "DateRange", $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}" },
                        { "PrintDate", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") },
                        { "TotalQuantity", (result.TotalQuantity ?? 0).ToString("N2") },
                        { "TotalAmount", (result.TotalAmount ?? 0).ToString("N2") }
                    };

                    var obj = result.Reports.Cast<object>().ToList();
                    Dictionary<string, object> ReportDtls = new() { { "Details", obj } };

                    var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Purchase_Details_Report"));
                    if (pdto.hdr == null || !pdto.dtls.Any())
                    {
                        await Dialog.DialogMethod("Print design 'Purchase_Details_Report' not found.");
                        return;
                    }

                    SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                    pdfData = await pHelper.PrintReportMethod(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
                }
                else
                {
                    await Dialog.DialogMethod(Localizer("No records found"));
                }
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
            var req = new PurchaseDetailsReportRequest(
               BranchId,
               FromDate.ToDateTime(),
               ToDate.ToDateTime(),
               SupplierId == 0 ? null : SupplierId,
               string.IsNullOrEmpty(ItemName) ? null : ItemName
           );

            if (result == null)
            {
                result = await api.GetPurchaseDetailsReport(req);
            }

            if (result is not null && result.Reports.Any())
            {
                var report = result.Reports;
                var jsonData = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
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
    }
    #endregion
}
