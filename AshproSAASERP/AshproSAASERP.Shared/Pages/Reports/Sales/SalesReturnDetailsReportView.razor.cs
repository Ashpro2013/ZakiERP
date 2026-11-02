using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Sales;

public partial class SalesReturnDetailsReportView : ComponentBase, ITabAware
{
    [Parameter] public int TabId { get; set; }
    private DateTime? FromDate { get; set; } = DateTime.Now;
    private DateTime? ToDate { get; set; } = DateTime.Now;
    private int BranchId { get; set; }
    private int CustomerId { get; set; }
    private string CustomerName { get; set; } = string.Empty;
    private int SalesManId { get; set; }
    private string InvoiceType { get; set; } = "All";
    private string BillType { get; set; } = "All";
    private string ItemName { get; set; } = string.Empty;

    List<CommonModel> branchList = new();
    List<CommonModel> salesmanList = new();
    SalesReturnDetailsReportResponse? result;
    bool IsLoading = false;
    bool IsLoaded = false;
    string searchString = "";
    byte[]? pdfData;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            branchList = await api.GetCombofill("Branch", BranchId);
            salesmanList = await api.GetCombofill("SalesMan", BranchId);
            if (branchList.Any())
            {
                BranchId = branchList.First().Id;
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    async Task PartySearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
        var resultDialog = await dialog.Result;
        if (!resultDialog!.Canceled && resultDialog.Data is CustomerMaster selectedCustomer)
        {
            CustomerId = selectedCustomer.CustomerId;
            CustomerName = selectedCustomer.Name!;
            StateHasChanged();
        }
    }

    async Task PreviewMethod() => await FetchData();
    async Task FetchData()
    {
        try
        {
            IsLoading = true;
            pdfData = null;
            if (FromDate == null || ToDate == null)
            {
                await Dialog.DialogMethod(Localizer("Please select date range"));
                return;
            }

            var req = new SalesReturnDetailsReportRequest(
                BranchId,
                FromDate.Value,
                ToDate.Value,
                CustomerId == 0 ? null : CustomerId,
                SalesManId == 0 ? null : SalesManId,
                InvoiceType,
                BillType,
                ItemName
            );

            result = await api.GetSalesReturnDetailsReport(req);

            if (result == null || result.Reports == null || !result.Reports.Any())
            {
                await Dialog.DialogMethod(Localizer("No records found"));
                IsLoaded = false;
            }
            else
            {
                IsLoaded = true;
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

    private async Task GenerateReport() => await ReportPdfGenerate();
    private async Task PrintMethod()
    {
        // Server Side Print
        try
        {
            IsLoading = true;
            var req = new SalesReturnDetailsReportRequest(
                BranchId,
                FromDate!.Value,
                ToDate!.Value,
                CustomerId == 0 ? null : CustomerId,
                SalesManId == 0 ? null : SalesManId,
                InvoiceType,
                BillType,
                ItemName
            );

            var response = await api.GetSalesReturnDetailsReportPDF(req);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                using var stream = response.Content;
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                pdfData = memoryStream.ToArray();
            }
            else
            {
                await Dialog.DialogMethod(Localizer("Failed to generate report"));
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

    private async Task ReportPdfGenerate() => await PrintMethod();
    private async Task DownloadExcel()
    {
        try
        {
            if (FromDate == null || ToDate == null) return;



            if (result == null)
            {
                var req = new SalesReturnDetailsReportRequest(
                 BranchId,
                 FromDate!.Value,
                 ToDate!.Value,
                 CustomerId == 0 ? null : CustomerId,
                 SalesManId == 0 ? null : SalesManId,
                 InvoiceType,
                 BillType,
                 ItemName
             );

                result = await api.GetSalesReturnDetailsReport(req);
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

    private void ClearMethod()
    {
        FromDate = DateTime.Now;
        ToDate = DateTime.Now;
        CustomerId = 0;
        CustomerName = "";
        SalesManId = 0;
        InvoiceType = "All";
        BillType = "All";
        ItemName = "";
        result = null;
        pdfData = null;
        IsLoaded = false;
    }

    private bool FilterFunc1(SalesReturnDetailsReportDtls element) => FilterFunc(element, searchString);

    private bool FilterFunc(SalesReturnDetailsReportDtls element, string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return true;
        if ($"{element.InvoiceNo} {element.CustomerName} {element.ItemName}".Contains(searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private void RowClickEvent(TableRowClickEventArgs<SalesReturnDetailsReportDtls> tableRowClickEventArgs)
    {
        // Handle row click
    }

    private string Localizer(string text)
    {
        // Simple mock localizer
        return text;
    }
}
