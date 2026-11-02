using AshproSAASERP.Shared.Layout;
using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Transactions;

public partial class SalesMonitorReport
{
    [CascadingParameter] public MainLayout Layout { get; set; }
    [Inject] ISnackbar Snackbar { get; set; }

    #region Variables
    private DateTime? fromDate = DateTime.Now.AddMonths(-1);
    private DateTime? toDate = DateTime.Now;

    // Filters
    private int branchId = 0;
    private int salesManId = 0;
    private int? customerId = 0;
    private string customerName = string.Empty;
    private string billType = "All";

    private string status = "All";

    private bool IsLoading = false;
    private string searchString = "";

    // Data Sources
    private List<CommonModel> branchList = new();
    private List<CommonModel> salesmanList = new();

    private MudTable<SalesMonitorDtls>? table;
    private SalesMonitorResponse? result;
    private bool _shouldLoad = false;
    [Parameter] public int TabId { get; set; }
    #endregion

    protected override async Task OnInitializedAsync()
    {
        try
        {
            fromDate = DateTime.Now.Date.AddDays(-30);
            toDate = DateTime.Now.Date;
            branchId = AppData.BranchId > 0 ? AppData.BranchId : 0;

            // Load Combos
            branchList = await api.GetCombofill("Branch", branchId);
            salesmanList = await api.GetCombofill("SalesMan", branchId);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Initialization Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task CustomerSearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>("", options);
        var res = await dialog.Result;

        if (!res!.Canceled && res.Data is CustomerMaster selectedCustomer)
        {
            customerId = selectedCustomer.CustomerId;
            customerName = selectedCustomer.Name;
        }
    }

    private async Task LoadData()
    {
        if (fromDate == null || toDate == null) return;

        try
        {
            IsLoading = true;

            var req = new SalesMonitorRequest(
                branchId,
                fromDate.Value,
                toDate.Value,
                (customerId == 0 ? null : customerId),
                (salesManId == 0 ? null : salesManId),
                "Sales",
                billType,
                1,
                0, // 0 = Load All
                status,
                searchString
            );

            result = await api.GetSalesMonitorReportAsync(req);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error fetching report: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void ClearMethod()
    {
        fromDate = DateTime.Now.Date.AddDays(-30);
        toDate = DateTime.Now.Date;
        branchId = AppData.BranchId > 0 ? AppData.BranchId : 0;
        salesManId = 0;
        customerId = 0;
        customerName = "";
        billType = "All";
        status = "All";
        result = null;
        searchString = "";
        _shouldLoad = false;
        StateHasChanged();
    }

    private async Task OpenReceiptDetails(SalesMonitorDtls item)
    {
        try
        {
            var parameters = new DialogParameters { ["InvoiceId"] = item.Id };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<SalesMonitorReceiptDetailsDialog>(Localizer("Receipt Details"), parameters, options);
            await dialog.Result;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task RowClickEvent(TableRowClickEventArgs<SalesMonitorDtls> args)
    {
        await OpenReceiptDetails(args.Item);
    }

    private async Task ExportExcel()
    {
        try
        {
            IsLoading = true;
            // Fetch all data for export
            var req = new SalesMonitorRequest(
                branchId,
                fromDate ?? DateTime.Now.AddMonths(-1),
                toDate ?? DateTime.Now,
                (customerId == 0 ? null : customerId),
                (salesManId == 0 ? null : salesManId),
                "Sales",
                billType,
                0, // PageNumber usually doesn't matter if PageSize is 0
                0, // PageSize 0 -> All
                status,
                searchString
            );

            var response = await api.GetSalesMonitorReportAsync(req);

            if (response == null || response.Reports == null || !response.Reports.Any())
            {
                Snackbar.Add(Localizer("NoRecordFound"), Severity.Info);
                return;
            }

            var excelData = response.Reports.Select(x => new
            {
                Date = x.Date,
                InvoiceNo = x.InvoiceNo,
                Customer = x.CustomerName,
                Salesman = x.SalesmanName,
                BillType = x.BillType,
                Amount = x.Amount,
                Received = x.Received,
                Discount = x.Discount,
                Return = x.Return,
                Balance = x.Balance,
                Status = x.Status
            }).ToList();

            var jsonData = JsonSerializer.Serialize(excelData);
            await JS.InvokeVoidAsync("exportJsonToExcel", jsonData);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error exporting: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
}
