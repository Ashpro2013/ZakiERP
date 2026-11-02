namespace AshproSAASERP.Shared.Pages.Reports.Accounts;

using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.Enums;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Shared.Services;

public partial class SupplierSummaryReport : ComponentBase
{

    private DateTime? toDate = DateTime.Now;
    private int branchId;
    private string supplierName;
    private int? supplierId = null;
    private decimal? totalbalance = 0;

    // Injected in Razor via @inject Downloader downloader

    private List<CommonModel> branchList = new();
    private List<SupplierSummaryDTO> reportData = new();

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
            reportData = await api.GetSupplierSummaryAsync(effectiveFromDate, toDate.Value, branchId, supplierId);
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
            // Use new Enum value Supplier_Summary
            var pdto = await api.GetPrintDesignAsync(new(0, ReportTypes.Supplier_Summary.ToString()));

            if (pdto == null || pdto.hdr == null)
            {
                snackbar.Add($"Print design '{ReportTypes.Supplier_Summary}' not found.", Severity.Warning);
                return;
            }

            // Prepare Header Data
            // "FromDate" is technically 2000, but for print we might want to hide it or show empty
            // User requested "As On Date", so we emphasize ToDate.
            var header = new SupplierSummaryHdr(
                Date: DateTime.Now.ToString("dd/MM/yyyy"),
                FromDate: "", // Hidden
                ToDate: toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "",
                TotalBalance: reportData.Sum(x => x.ClosingBalance).ToString("N2")
            );



            // Convert Header to Dictionary
            Dictionary<string, object?> ReportHdr = header.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(header));

            // Prepare details with Total Row
            var printList = reportData.ToList(); // shallow copy
            printList.Add(new SupplierSummaryDTO
            {
                SL = null,
                Code = "",
                Name = "TOTAL",
                // Opening removed
                DebitAmount = reportData.Sum(x => x.DebitAmount),
                CreditAmount = reportData.Sum(x => x.CreditAmount),
                ClosingBalance = reportData.Sum(x => x.ClosingBalance)
            });

            // Prepare Details Data (Wrap list in "Details" key)
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
            snackbar.Add($"Error printing: {ex.Message}", Severity.Error);
        }
    }

    // Local Record for Header Data
    public record SupplierSummaryHdr(string Date, string FromDate, string ToDate, string TotalBalance);

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
                Code = x.Code,
                Name = x.Name,
                Opening = x.OpeningBalance,
                Debit = x.DebitAmount,
                Credit = x.CreditAmount,
                Closing = x.ClosingBalance
            }).ToList();

            var jsonData = System.Text.Json.JsonSerializer.Serialize(excelData);
            await JSRuntime.InvokeVoidAsync("exportJsonToExcel", jsonData);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error exporting: {ex.Message}", Severity.Error);
        }
    }
    private void ClearSupplier()
    {
        supplierId = null;
        supplierName = string.Empty;
    }

    private void ClearMethod()
    {
        toDate = DateTime.Now;
        ClearSupplier();
        if (branchList.Any())
        {
            branchId = (AppData.BranchId > 0 && branchList.Any(x => x.Id == AppData.BranchId))
                           ? AppData.BranchId
                           : branchList.First().Id;
        }
        reportData.Clear();
        StateHasChanged();
    }

    private async Task SupplierSearchMethod()
    {
        var result = await DialogModel.GetSupplierSearchMethod("");

        if (result is not null)
        {
            supplierId = result.SupplierId;
            supplierName = result.Name;
            await LoadData();
        }
    }
}
