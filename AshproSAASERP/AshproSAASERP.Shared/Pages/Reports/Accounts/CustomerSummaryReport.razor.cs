namespace AshproSAASERP.Shared.Pages.Reports.Accounts;

using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Shared.Services;

public partial class CustomerSummaryReport : ComponentBase
{

    private DateTime? toDate = DateTime.Now;
    private int branchId;
    private string customerName;
    private int? customerId = null;
    private bool isDetailed = false;
    private decimal? totalbalance = 0;

    // Injected in Razor via @inject Downloader downloader

    private List<CommonModel> branchList = new();
    private List<CustomerSummaryDTO> reportData = new();

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
            snackbar.Add($"Error loading masters: {ex.Message}", Severity.Error);
        }
    }

    private async Task LoadData()
    {
        if (toDate == null)
        {
            snackbar.Add(Localizer("DetailsSearchDate"), Severity.Warning);
            return;
        }

        try
        {
            // Default FromDate to 2000-01-01 for "As On Date" report
            var effectiveFromDate = new DateTime(2000, 1, 1);
            reportData = await api.GetCustomerSummaryAsync(effectiveFromDate, toDate.Value, branchId, customerId);
            totalbalance = reportData.Sum(x => x.ClosingBalance);
            if (!reportData.Any())
            {
                snackbar.Add(Localizer("NoRecordFound"), Severity.Info);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error fetching report: {ex.Message}", Severity.Error);
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

            var designName = "Customer_Summary";
            if (isDetailed)
            {
                designName = "Customer_Summary_Detailed";
            }

            var pdto = await api.GetPrintDesignAsync(new(0, designName));

            if (pdto == null || pdto.hdr == null)
            {
                snackbar.Add($"Print design '{designName}' not found.", Severity.Warning);
                return;
            }


            // Prepare Header Data
            var header = new CustomerSummaryHdr(
                Date: DateTime.Now.ToString("dd/MM/yyyy"),
                FromDate: "",
                ToDate: toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "",
                TotalBalance: reportData.Sum(x => x.ClosingBalance).ToString("N2")
            );

            // Convert Header to Dictionary
            Dictionary<string, object?> ReportHdr = header.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(header));

            var obj = reportData.Cast<object>().ToList();
            Dictionary<string, object> ReportDtls = new() { { "Details", obj } };

            var printHelper = new SyncfusionPrintHelper();
            byte[]? bytes = await printHelper.PrintReportMethod(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);

            if (IsPreview && bytes is not null)
            {
                await DialogModel.InvoicePreviewDialog(bytes);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error printing: {ex.Message}", Severity.Error);
        }
    }

    // Local Record for Header Data
    public record CustomerSummaryHdr(string Date, string FromDate, string ToDate, string TotalBalance);

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
            // Respect isDetailed for Excel
            var excelData = reportData.Select(x => new
            {
                Code = x.Code,
                Name = x.Name,
                Opening = isDetailed ? x.OpeningBalance : 0, // Or null?
                Debit = isDetailed ? x.DebitAmount : 0,
                Credit = isDetailed ? x.CreditAmount : 0,
                Closing = x.ClosingBalance
            });

            // If !isDetailed, maybe we should project to fewer columns?
            // Anonymous type shape is fixed in compilation.
            // Ideally we pass a dictionary or dynamic object to export.
            // For simplicity, I'll export all fields but maybe zeros if not detailed, or just all fields always.
            // User view in Grid is what matters most. Excel usually has all data.
            // I'll export all data as it's "Summary" report, users can hide columns in Excel.

            var exportList = reportData.Select(x => new
            {
                Code = x.Code,
                Name = x.Name,
                Opening = x.OpeningBalance,
                Debit = x.DebitAmount,
                Credit = x.CreditAmount,
                Closing = x.ClosingBalance
            }).ToList();

            var jsonData = System.Text.Json.JsonSerializer.Serialize(exportList);
            await JSRuntime.InvokeVoidAsync("exportJsonToExcel", jsonData);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error exporting: {ex.Message}", Severity.Error);
        }
    }
    private void ClearCustomer()
    {
        customerId = null;
        customerName = string.Empty;
    }

    private async Task CustomerSearchMethod()
    {
        var result = await DialogModel.GetCustomerSearchMethod(); // Presuming this method exists matches SupplierSearchMethod pattern

        if (result is not null)
        {
            customerId = result.CustomerId;
            customerName = result.Name;
            await LoadData();
        }
    }
}
