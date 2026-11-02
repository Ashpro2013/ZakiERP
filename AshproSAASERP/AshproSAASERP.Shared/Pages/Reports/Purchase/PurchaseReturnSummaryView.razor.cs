using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Purchase;

public partial class PurchaseReturnSummaryView : ComponentBase
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[] pdfData;
    PurchaseReturnReportResponse result;
    bool IsLoading = false;
    // Filters
    int BranchId = 0;
    int? SupplierId = 0;
    string SupplierName = "";
    string BillType = "All";

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
        BillType = "All";
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
            var req = new PurchaseReturnReportRequest(
                BranchId,
                FromDate ?? DateTime.Today,
                ToDate ?? DateTime.Today,
                SupplierId == 0 ? null : SupplierId,
                BillType
            );
            if (!AppData.IsWindows)
            {
                var response = await api.GetPurchaseReturnSummaryServerReportPDF(req);

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
                        await downloader.DownloadFromStream(data.ToArray(), "PurchaseReturnSummary.pdf");
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        await Dialog.DialogMethod(Localizer("Report generation failed. check if design exists."));
                    }
                    else
                    {
                        await Dialog.DialogMethod(Localizer("No records found"));
                    }
                }
            }
            else
            {
                result = await api.GetPurchaseReturnReport(req);
                if (result is not null && result.Reports is not null && result.Reports.Any())
                {
                    var reportHeader = new PurchaseReturnReportHdr(
               $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
               DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
               (result.TotalGross ?? 0).ToString("N2"),
               (result.TotalDiscount ?? 0).ToString("N2"),
               (result.TotalVat ?? 0).ToString("N2"),
               (result.TotalNet ?? 0).ToString("N2")
           );
                    Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                    Dictionary<string, object> ReportDtls = new() { { "Details", result.Reports } };
                    var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Purchase_Return_Report"));
                    if (pdto.hdr == null || !pdto.dtls.Any())
                    {
                        await Dialog.DialogMethod(Localizer("Print design 'Purchase Return Report' not found."));
                        return;
                    }
                    SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                    pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls!, null, IsPreview);
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
            var req = new PurchaseReturnReportRequest(
                BranchId,
                FromDate ?? DateTime.Today,
                ToDate ?? DateTime.Today,
                SupplierId == 0 ? null : SupplierId,
                BillType
            );

            result = await api.GetPurchaseReturnReport(req);

            if (result is not null && result.Reports is not null && result.Reports.Any())
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
