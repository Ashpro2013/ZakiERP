using AshproSAASERP.Shared.Layout;
using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Transactions;

public partial class PurchaseMonitorReport
{
    [CascadingParameter] public MainLayout Layout { get; set; }
    [Inject] ISnackbar Snackbar { get; set; }

    #region Variables
    private DateTime? fromDate = DateTime.Now.AddMonths(-1);
    private DateTime? toDate = DateTime.Now;

    // Filters
    private int branchId = 0;
    private int? supplierId = 0;
    private string supplierName = string.Empty;
    private string billType = "All";

    private string status = "All";

    private bool IsLoading = false;
    private string searchString = "";

    // Data Sources
    private List<CommonModel> branchList = new();

    private MudTable<PurchaseMonitorDtls>? table;
    private PurchaseMonitorResponse? result;
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
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Initialization Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task SupplierSearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<SupplierSearchDialog>("", options);
        var res = await dialog.Result;

        if (!res!.Canceled && res.Data is SupplierMaster selectedSupplier)
        {
            supplierId = selectedSupplier.SupplierId;
            supplierName = selectedSupplier.Name;
        }
    }

    private async Task LoadData()
    {
        if (fromDate == null || toDate == null) return;

        try
        {
            IsLoading = true;

            var req = new PurchaseMonitorRequest(
                branchId,
                fromDate.Value,
                toDate.Value,
                (supplierId == 0 ? null : supplierId),
                billType,
                1,
                0, // 0 = Load All
                status,
                searchString
            );

            result = await api.GetPurchaseMonitorReportAsync(req);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error fetching data: {ex.Message}", Severity.Error);
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
        supplierId = 0;
        supplierName = "";
        billType = "All";
        status = "All";
        result = null;
        searchString = "";
        _shouldLoad = false;
        StateHasChanged();
    }

    private async Task OpenPaymentDetails(PurchaseMonitorDtls item)
    {
        try
        {
            var parameters = new DialogParameters { ["PurchaseId"] = item.Id };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<PurchaseMonitorPaymentDetailsDialog>(Localizer("Payment Details"), parameters, options);
            await dialog.Result;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task RowClickEvent(TableRowClickEventArgs<PurchaseMonitorDtls> args)
    {
        await OpenPaymentDetails(args.Item);
    }

    private async Task ExportExcel()
    {
        try
        {
            IsLoading = true;
            // Fetch all data for export
            var req = new PurchaseMonitorRequest(
                branchId,
                fromDate ?? DateTime.Now.AddMonths(-1),
                toDate ?? DateTime.Now,
                (supplierId == 0 ? null : supplierId),
                billType,
                0, // PageNumber usually doesn't matter if PageSize is 0
                0, // PageSize 0 -> All
                status,
                searchString
            );

            var response = await api.GetPurchaseMonitorReportAsync(req);

            if (response == null || response.Reports == null || !response.Reports.Any())
            {
                Snackbar.Add(Localizer("NoRecordFound"), Severity.Info);
                return;
            }

            var excelData = response.Reports.Select(x => new
            {
                Date = x.Date,
                InvoiceNo = x.InvoiceNo,
                Supplier = x.SupplierName,
                BillType = x.BillType,
                Amount = x.Amount,
                Paid = x.Paid,
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
