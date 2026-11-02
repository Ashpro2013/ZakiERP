using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Sales;

public partial class CustomerWiseSalesReportsView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    CustomerWiseSalesReportResponse? result;

    // Filters
    int BranchId = 0;
    int SalesManId = 0;
    int CustomerId = 0;
    string CustomerName = "";
    string InvoiceType = "All";
    string BillType = "All";
    bool IsLoading = false;
    // Data Sources
    List<CommonModel> branchList = new();
    List<CommonModel> salesmanList = new();
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
            salesmanList = await api.GetCombofill("SalesMan", BranchId);
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
        SalesManId = 0;
        CustomerId = 0;
        CustomerName = "";
        InvoiceType = "All";
        BillType = "All";
        pdfData = null;
    }

    async Task PartySearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
        var result = await dialog.Result;
        if (!result!.Canceled && result.Data is CustomerMaster selectedCustomer)
        {
            CustomerId = selectedCustomer.CustomerId;
            CustomerName = selectedCustomer!.Name!;
            StateHasChanged();
        }
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            IsLoading = true;
            pdfData = null;
            var req = new CustomerWiseSalesReportRequest(
                BranchId,
                FromDate.ToDateTime(),
                ToDate.ToDateTime(),
                CustomerId == 0 ? null : CustomerId,
                SalesManId == 0 ? null : SalesManId,
                InvoiceType,
                BillType
            );

            bool Iswindows = AppData.IsWindows;
            if (Iswindows)
            {
                result = await api.GetCustomerWiseSalesReport(req);

                if (result is not null && result.Items.Any())
                {
                    await PrintPreviewLogic(IsPreview);
                }
                else
                {
                    await Dialog.DialogMethod(Localizer("No records found"));
                }
            }
            else
            {
                var data = await api.GetCustomerWiseSalesReportPDF(req);
                if (data is not null && data.IsSuccessStatusCode)
                {
                    using var stream = data.Content;
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    if (IsPreview)
                        pdfData = memoryStream.ToArray();
                    else
                        await downloader.DownloadFromStream(memoryStream.ToArray(), "CustomerWiseSalesReport.pdf");
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
        }
    }

    async Task PrintPreviewLogic(bool IsPreview)
    {
        var reportHeader = new CustomerWiseSalesReportHdr(
            $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
            DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")
        );

        Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
        Dictionary<string, object> ReportDtls = new() { { "Details", result!.Items } };

        var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Customer_Wise_Sales_Report"));
        if (pdto.hdr == null || !pdto.dtls.Any())
        {
            await Dialog.DialogMethod("Print design 'Customer Wise Sales Report' not found.");
            return;
        }

        SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
        pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);
    }

    async Task ReportGenerate() => await ReportPdfGenerate(true);
    async Task PDFExportMethod() => await ReportPdfGenerate();

    async Task ExcelExportMethod()
    {
        try
        {
            var req = new CustomerWiseSalesReportRequest(
               BranchId,
               FromDate.ToDateTime(),
               ToDate.ToDateTime(),
               CustomerId == 0 ? null : CustomerId,
               SalesManId == 0 ? null : SalesManId,
               InvoiceType,
               BillType
           );

            if (result == null)
            {
                result = await api.GetCustomerWiseSalesReport(req);
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
