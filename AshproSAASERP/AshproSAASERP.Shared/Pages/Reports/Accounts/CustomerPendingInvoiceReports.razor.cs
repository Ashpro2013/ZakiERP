namespace AshproSAASERP.Shared.Pages.Reports.Accounts;

using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.Enums;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Shared.Services;
using FileDownloader;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AshproSAASERP.Shared.Services.ArabicLocalization;

public partial class CustomerPendingInvoiceReports : ComponentBase
{
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IViewService api { get; set; } = default!;
    [Inject] private IDialogService Dialog { get; set; } = default!;
    [Inject] private ModelService DialogModel { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private Downloader downloader { get; set; } = default!;

    private DateTime? fromDate = DateTime.Now.AddMonths(-1);
    private DateTime? toDate = DateTime.Now;
    private int branchId;
    private int? customerId = null;
    private string customerName = string.Empty;

    private List<CommonModel> branchList = new();
    private List<CustomerPendingInvoiceDTO> reportData = new();

    [Parameter] public int TabId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadMasters();
    }

    private async Task LoadMasters()
    {
        try
        {
            branchList = await api.GetCombofill("Branch", 0);

            if (branchList.Any())
            {
                branchId = (AppData.BranchId > 0 && branchList.Any(x => x.Id == AppData.BranchId))
                            ? AppData.BranchId
                            : branchList.First().Id;
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading masters: {ex.Message}", Severity.Error);
        }
    }

    private async Task CustomerSearchMethod()
    {
        var result = await DialogModel.GetCustomerSearchMethod("");

        if (result is not null)
        {
            customerId = result.CustomerId;
            customerName = result.Name;
        }
    }

    private void ClearCustomer()
    {
        customerId = null;
        customerName = string.Empty;
    }

    private async Task DetailSearch()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        if (fromDate == null || toDate == null)
        {
            Snackbar.Add(Localizer("DetailsSearchDate"), Severity.Warning);
            return;
        }

        if (customerId == null || customerId == 0)
        {
            Snackbar.Add(Localizer("Please Select Customer"), Severity.Warning);
            return;
        }

        try
        {
            reportData = await api.GetCustomerPendingInvoiceAsync(fromDate.Value, toDate.Value, branchId, customerId);

            if (!reportData.Any())
            {
                Snackbar.Add(Localizer("NoRecordFound"), Severity.Info);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error fetching report: {ex.Message}", Severity.Error);
        }
    }

    private async Task PrintHelperMethod(bool IsPreview)
    {
        await LoadData();

        if (!reportData.Any())
        {
            return;
        }

        try
        {
            var pdto = await api.GetPrintDesignAsync(new(0, ReportTypes.Customer_Pending_Invoice.ToString()));

            if (pdto == null || pdto.hdr == null)
            {
                Snackbar.Add("Print design 'Customer_Pending_Invoice' not found. Please setup print design.", Severity.Warning);
                return;
            }

            // Prepare Header Data
            var header = new CustomerPendingInvoiceHdr(
                Date: DateTime.Now.ToString("dd/MM/yyyy"),
                FromDate: fromDate.HasValue ? fromDate.Value.ToString("dd/MM/yyyy") : "",
                ToDate: toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "",
                CustomerName: (customerId.HasValue && !string.IsNullOrEmpty(customerName)) ? customerName : "All Customers",
                TotalDebit: reportData.Sum(x => x.PaidAmount).ToString("N2"), // Paid
                TotalReturn: reportData.Sum(x => x.ReturnAmount).ToString("N2"),
                TotalDiscount: reportData.Sum(x => x.Discount).ToString("N2"),
                TotalCredit: reportData.Sum(x => x.NetAmount).ToString("N2"), // Invoice Amount
                ClosingBalance: (reportData.Sum(x => x.Balance)).ToString("N2")
            );

            // Convert Header to Dictionary
            Dictionary<string, object?> ReportHdr = header.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(header));

            // Prepare details with Total Row
            var printList = reportData.ToList();
            printList.Add(new CustomerPendingInvoiceDTO
            {
                SL = null,
                Date = null,
                VoucherNo = "TOTAL",
                VoucherType = "",
                CustomerName = "",
                Narration = "",
                PaidAmount = reportData.Sum(x => x.PaidAmount),
                ReturnAmount = reportData.Sum(x => x.ReturnAmount),
                Discount = reportData.Sum(x => x.Discount),
                NetAmount = reportData.Sum(x => x.NetAmount),
                Balance = reportData.Sum(x => x.Balance)
            });

            // Prepare Details Data
            var obj = printList.Cast<object>().ToList();
            Dictionary<string, object> ReportDtls = new() { { "Details", obj } };

            var printHelper = new SyncfusionPrintHelper();
            byte[]? bytes = await printHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);

            if (IsPreview && bytes is not null)
            {
                await DialogModel.InvoicePreviewDialog(bytes);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error printing: {ex.Message}", Severity.Error);
        }
    }

    public record CustomerPendingInvoiceHdr(string Date, string FromDate, string ToDate, string CustomerName, string TotalDebit, string TotalReturn, string TotalDiscount, string TotalCredit, string ClosingBalance);

    private async Task Print()
    {
        await PrintHelperMethod(false);
    }

    private async Task Preview()
    {
        await PrintHelperMethod(true);
    }

    private async Task ExportExcel()
    {
        await LoadData();

        if (!reportData.Any())
        {
            return;
        }

        try
        {
            var excelData = reportData.Select(x => new
            {
                Date = x.Date.HasValue ? x.Date.Value.ToString("dd/MM/yyyy") : "",
                VoucherNo = x.VoucherNo,
                Type = x.VoucherType,
                Customer = x.CustomerName,
                Narration = x.Narration,
                InvoiceAmount = x.NetAmount,
                PaidAmount = x.PaidAmount,
                ReturnAmount = x.ReturnAmount,
                Discount = x.Discount,
                Balance = x.Balance
            }).ToList();

            var jsonData = System.Text.Json.JsonSerializer.Serialize(excelData);
            await JSRuntime.InvokeVoidAsync("exportJsonToExcel", jsonData);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error exporting: {ex.Message}", Severity.Error);
        }
    }
}
