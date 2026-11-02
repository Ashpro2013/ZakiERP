using System.Text.Json;
namespace ZakiSAASERP.Shared.Pages.Reports.Sales;

public partial class SalesOrderReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    bool IsLoading = false;
    SalesOrderReportResponse? result;

    // Filters
    int BranchId = 0;
    int CustomerId = 0;
    string CustomerName = "";

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
        CustomerId = 0;
        CustomerName = "";
        pdfData = null;
        result = null;
    }
    async Task CustomerSearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
        var dialogResult = await dialog.Result;
        if (!dialogResult!.Canceled && dialogResult.Data is CustomerMaster selectedCustomer)
        {
            CustomerId = selectedCustomer.CustomerId;
            CustomerName = selectedCustomer.Name;
            StateHasChanged();
        }
    }
    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            IsLoading = true;
            var req = new SalesOrderReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate,
                CustomerId = CustomerId == 0 ? null : CustomerId
            };
            pdfData = null;
            if (AppData.IsWindows)
            {
                result = await api.GetSalesOrderReport(req);

                if (result is not null && result.Items.Any())
                {
                    var reportHeader = new SalesOrderReportHdr(
                        $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                        (result.TotalVat ?? 0).ToString("N2"),
                        (result.TotalNet ?? 0).ToString("N2")
                    );

                    Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                    Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };

                    var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Sales_Order_Register"));
                    if (pdto.hdr == null || !pdto.dtls.Any())
                    {
                        await Dialog.DialogMethod("Print design 'Sales_Order_Register' not found.");
                        return;
                    }

                    SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                    pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
                }
                else
                {
                    await Dialog.DialogMethod(Localizer("No records found"));
                }
            }
            else
            {
                var data = await api.GetSalesOrderReportPDF(req);
                if (data is not null && data.IsSuccessStatusCode)
                {
                    using var stream = data.Content;
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    if (IsPreview)
                        pdfData = memoryStream.ToArray();
                    else
                        await downloader.DownloadFromStream(memoryStream.ToArray(), "SalesOrderReport.pdf");
                }
                else
                {
                    await Dialog.DialogMethod("Failed to generate report");
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
            StateHasChanged();
        }
    }

    async Task ReportGenerate() => await ReportPdfGenerate(true);
    async Task PDFExportMethod() => await ReportPdfGenerate();

    async Task ExcelExportMethod()
    {
        try
        {
            var req = new SalesOrderReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate,
                CustomerId = CustomerId == 0 ? null : CustomerId
            };

            if (result == null)
            {
                result = await api.GetSalesOrderReport(req);
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
