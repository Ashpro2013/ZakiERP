using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Transactions;

public partial class TransactionMonitorReport
{



    private MudTable<TransactionMonitorDtls> table = null!;
    private TransactionMonitorResponse? result;

    // Transaction type selection
    private string selectedTransactionType = "Sales Invoice";

    // Filter parameters
    private DateTime? fromDate = DateTime.Now.Date;
    private DateTime? toDate = DateTime.Now.Date;
    private int branchId;
    private int partyId;
    private string partyName = "";
    private string billType = "All";

    private string searchString = "";

    private List<CommonModel> branchList = new();
    private List<CommonModel> customerList = new();
    private List<CommonModel> supplierList = new();

    private bool IsLoading = false;
    [Parameter] public int TabId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        branchId = AppData.BranchId;
        await LoadBranches();
        await LoadPartyLists();
    }

    private async Task LoadBranches()
    {
        var branches = await api.GetAllBranches();
        branchList = branches.Select(b => new CommonModel { Id = b.BranchId, Name = b.Name }).ToList();
    }

    private async Task LoadPartyLists()
    {
        // Load customers
        var customers = await api.GetAllCustomers();
        customerList = customers.Select(c => new CommonModel { Id = c.CustomerId, Name = c.Name }).ToList();

        // Load suppliers
        var suppliers = await api.GetAllSuppliers();
        supplierList = suppliers.Select(s => new CommonModel { Id = s.SupplierId, Name = s.Name }).ToList();
    }

    private async Task LoadData()
    {
        if (fromDate == null || toDate == null)
        {
            await Dialog.ShowMessage(Localizer("SelectDate"));
            return;
        }

        IsLoading = true;
        StateHasChanged();

        try
        {
            var request = new TransactionMonitorRequest(
                BranchId: branchId,
                FromDate: fromDate.Value,
                ToDate: toDate.Value,
                TransactionType: selectedTransactionType,
                PartyId: partyId > 0 ? partyId : null,
                SearchTerm: searchString ?? ""
            );

            result = await api.GetTransactionMonitorReportAsync(request);
        }
        catch (Exception ex)
        {
            await Dialog.ShowMessage($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task PartySearchMethod()
    {
        // Show customer search (always customer now)
        var selectedCustomer = await Dialog.GetCustomerSearchMethod("");
        if (selectedCustomer is not null)
        {
            partyId = selectedCustomer.CustomerId;
            partyName = selectedCustomer.Name;
        }
    }
    private async Task RowClickEvent(TableRowClickEventArgs<TransactionMonitorDtls> args)
    {
        var item = args.Item;
        if (item == null) return;

        // Navigate based on transaction type
        switch (selectedTransactionType)
        {
            case "Delivery Note":
                await CrudHelper.Navigation(tabService, item.Id, "DeliveryNoteTitle");
                break;
            case "Sales Invoice":
                await CrudHelper.Navigation(tabService, item.Id, "SalesInvoiceTitle");
                break;
            case "Sales Quotation":
                await CrudHelper.Navigation(tabService, item.Id, "SalesQuotTitle");
                break;
            case "Proforma Invoice":
                await CrudHelper.Navigation(tabService, item.Id, "ProformaInvoiceTitle");
                break;
        }
    }

    private async Task ExportExcel()
    {
        if (result == null || !result.Reports.Any())
        {
            await Dialog.ShowMessage(Localizer("NoRecordFound"));
            return;
        }

        try
        {
            IsLoading = true;
            var excelData = result.Reports.Select(x => new
            {
                InvNo = x.InvoiceNo,
                Date = x.Date,
                PartyName = x.PartyName,
                Amount = x.Amount,
                QuotNo = x.QuotNo,
                RefNo = x.RefNo,
                ProformaNo = x.ProformaNo,
                SalesNo = x.SalesNo,
                DeliveryNo = x.DeliveryNo,

            }).ToList();

            var jsonData = JsonSerializer.Serialize(excelData);
            await JS.InvokeVoidAsync("exportJsonToExcel", jsonData);
        }
        catch (Exception ex)
        {
            await Dialog.ShowMessage($"Error exporting: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task ClearMethod()
    {
        fromDate = DateTime.Now.Date;
        toDate = DateTime.Now.Date;
        branchId = AppData.BranchId;
        partyId = 0;
        partyName = "";

        searchString = "";
        result = null;
        selectedTransactionType = "Sales Invoice";
        StateHasChanged();
    }
}
