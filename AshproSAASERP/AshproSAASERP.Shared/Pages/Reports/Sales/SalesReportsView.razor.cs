using AshproSAASERP.Shared.Layout;
using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Sales;

public partial class SalesReportsView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    SalesReportResponse? result;

    // Filters
    int BranchId = 0;
    int SalesManId = 0;
    int CustomerId = 0;
    string CustomerName = "";
    string InvoiceType = "All";
    string BillType = "All";

    // Data Sources
    List<CommonModel> branchList = new();
    List<CommonModel> salesmanList = new();
    bool IsLoading = false;
    bool IsLoaded = false;
    string searchString = "";
    [CascadingParameter] public MainLayout? Layout { get; set; }
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
        IsLoaded = false;
        result = null;
    }
    async Task PartySearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
        var res = await dialog.Result;
        if (!res!.Canceled && res.Data is CustomerMaster selectedCustomer)
        {
            CustomerId = selectedCustomer.CustomerId;
            CustomerName = selectedCustomer.Name!;
            StateHasChanged();
        }
    }

    async Task LoadData()
    {
        await FetchData();
        if (result is not null && result.Reports.Any())
        {
            IsLoaded = true;
        }
        else
        {
            IsLoaded = false;
        }
    }

    async Task FetchData()
    {
        try
        {
            IsLoading = true;
            pdfData = null;
            // Reset checking IsLoaded here? No, let LoadData handle visibility.

            var req = new SalesReportRequest(
               BranchId,
               FromDate.ToDateTime(),
               ToDate.ToDateTime(),
               CustomerId == 0 ? null : CustomerId,
               SalesManId == 0 ? null : SalesManId,
               InvoiceType,
               BillType
           );

            result = await api.GetSalesReport(req);

            if (result == null || !result.Reports.Any())
            {
                await Dialog.DialogMethod(Localizer("No records found"));
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

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            IsLoading = true;
            if (AppData.IsWindows)
            {
                if (result == null || !result.Reports.Any())
                {
                    await FetchData();
                    if (result == null || !result.Reports.Any()) return;
                }



                var report = result.Reports;

                // Re-construct list for print
                var printReports = new List<SalesReportDtls>(result.Reports);

                var reportHeader = new SalesReportHdr(
                    $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    (result.TotalGross ?? 0).ToString("N2"),
                    (result.TotalDiscount ?? 0).ToString("N2"),
                    (result.TotalAmount ?? 0).ToString("N2"),
                    (result.TotalVat ?? 0).ToString("N2"),
                    (result.TotalNet ?? 0).ToString("N2")
                );


                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));

                Dictionary<string, object> ReportDtls = new() { { "Details", printReports } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Sales_Report"));
                if (pdto==null || pdto.hdr == null || !pdto.dtls.Any())
                {
                    await Dialog.DialogMethod("Print design 'Sales_Report' not found.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
                IsLoaded = false;
            }
            else
            {
                IsLoading = true;
                var req = new SalesReportRequest(
                BranchId,
                FromDate.ToDateTime(),
                ToDate.ToDateTime(),
                CustomerId == 0 ? null : CustomerId,
                SalesManId == 0 ? null : SalesManId,
                InvoiceType,
                BillType);
                var data = await api.GetSalesReportPDF(req);
                if (data is not null)
                {
                    using var stream = data.Content;

                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    if (IsPreview)
                        pdfData = memoryStream.ToArray();
                    else
                        await downloader.DownloadFromStream(memoryStream.ToArray(), "SalesReport.pdf");
                }
                else
                {
                    await Dialog.DialogMethod(Localizer("Failed to generate report"));
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

    async Task ReportGenerate() => await LoadData();
    async Task PreviewMethod() => await ReportPdfGenerate(true);
    async Task PrintMethod() => await ReportPdfGenerate(false);
    async Task PDFExportMethod() => await ReportPdfGenerate();

    async Task ExcelExportMethod()
    {
        try
        {
            var req = new SalesReportRequest(
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
                result = await api.GetSalesReport(req);
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
                Dialog.DialogMethod(Localizer("No records found"));
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task RowClickEvent(TableRowClickEventArgs<SalesReportDtls> args)
    {
        if (args.MouseEventArgs.Detail >= 2 && args.Item.Id.HasValue && args.Item.Id > 0 && !string.IsNullOrEmpty(args.Item.InvType))
        {
            if (args.Item.InvType.Equals("Sales", StringComparison.OrdinalIgnoreCase))
            {
                await CrudHelper.Navigation(tabService, args.Item.Id.Value, "SalesInvoiceTitle");
            }
            else if (args.Item.InvType.Contains("Return", StringComparison.OrdinalIgnoreCase))
            {
                await CrudHelper.Navigation(tabService, args.Item.Id.Value, "SalesInvoiceRtnTitle");
            }
        }
    }

    private bool FilterFunc1(SalesReportDtls element) => FilterFunc(element, searchString);

    private bool FilterFunc(SalesReportDtls element, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return true;
        if (element.InvoiceNo.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (element.CustomerName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (element.SalesmanName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if ($"{element.Date} {element.BillType} {element.InvType}".Contains(searchString))
            return true;
        return false;
    }
    #endregion
}
