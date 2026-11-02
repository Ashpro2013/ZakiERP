namespace AshproSAASERP.Shared.Pages.Reports.Accounts;

using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.Enums;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Shared.Services;

public partial class SupplierStatementReport : ComponentBase
{
    private DateTime? fromDate = DateTime.Now.AddMonths(-1);
    private DateTime? toDate = DateTime.Now;
    private int branchId;
    private int? supplierId = null;
    private string supplierName;


    // Injected in Razor via @inject Downloader downloader

    private List<CommonModel> branchList = new();
    private List<SupplierStatementDTO> reportData = new();

    [Parameter] public int TabId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadMasters();
    }

    private async Task LoadMasters()
    {
        try
        {
            // Use standard Combofill for Branches, passing 0 to get all accessible branches
            branchList = await api.GetCombofill("Branch", 0);

            if (branchList.Any())
            {
                // Default to current branch if in list, otherwise first item
                branchId = (AppData.BranchId > 0 && branchList.Any(x => x.Id == AppData.BranchId))
                            ? AppData.BranchId
                            : branchList.First().Id;
            }
            StateHasChanged(); // Ensure UI updates to show Name instead of ID
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading masters: {ex.Message}", Severity.Error);
        }
    }

    private async Task SupplierSearchMethod()
    {
        var result = await DialogModel.GetSupplierSearchMethod("");

        if (result is not null)
        {
            supplierId = result.SupplierId;
            supplierName = result.Name;
        }
    }

    private void ClearSupplier()
    {
        supplierId = null;
        supplierName = string.Empty;
    }

    private void ClearMethod()
    {
        fromDate = DateTime.Now.AddMonths(-1);
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

    private async Task DetailSearch() // Kept for button click
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

        if (supplierId == null || supplierId == 0)
        {
            Snackbar.Add(ArabicLocalization.Localizer("PleaseSelectSupplier"), Severity.Error);
            return;
        }

        try
        {
            reportData = await api.GetSupplierStatementAsync(fromDate.Value, toDate.Value, branchId, supplierId);

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
        // Always reload data to ensure we print what is specified in the filters
        await LoadData();

        if (!reportData.Any())
        {
            // Snackbar already shown in LoadData
            return;
        }

        try
        {
            var pdto = await api.GetPrintDesignAsync(new(0, ReportTypes.Supplier_Statement.ToString()));

            if (pdto == null || pdto.hdr == null)
            {
                Snackbar.Add("Print design 'Supplier_Statement' not found.", Severity.Warning);
                return;
            }

            // Prepare Header Data
            var header = new SupplierStatementHdr(
                Date: DateTime.Now.ToString("dd/MM/yyyy"),
                FromDate: fromDate.HasValue ? fromDate.Value.ToString("dd/MM/yyyy") : "",
                ToDate: toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "",
                SupplierName: supplierName ?? "All Suppliers",
                TotalDebit: reportData.Sum(x => x.Debit).ToString("N2"),
                TotalCredit: reportData.Sum(x => x.Credit).ToString("N2"),
                ClosingBalance: (reportData.LastOrDefault()?.Balance ?? 0).ToString("N2")
            );

            // Convert Header to Dictionary
            Dictionary<string, object?> ReportHdr = header.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(header));

            // Prepare details with Total Row
            var printList = reportData.ToList(); // shallow copy
            printList.Add(new SupplierStatementDTO
            {
                SL = null, // Or empty if formatter handles it
                Date = null,
                VoucherNo = "TOTAL",
                VoucherType = "",
                SupplierName = "",
                Narration = "",
                Debit = reportData.Sum(x => x.Debit),
                Credit = reportData.Sum(x => x.Credit),
                Balance = reportData.LastOrDefault()?.Balance ?? 0
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
            Snackbar.Add($"Error printing: {ex.Message}", Severity.Error);
        }
    }

    // Local Record for Header Data
    public record SupplierStatementHdr(string Date, string FromDate, string ToDate, string SupplierName, string TotalDebit, string TotalCredit, string ClosingBalance);
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
        if (supplierId == null || supplierId == 0)
        {
            Snackbar.Add(ArabicLocalization.Localizer("PleaseSelectSupplier"), Severity.Error);
            return;
        }

        // Always reload data
        await LoadData();

        if (!reportData.Any())
        {
            return;
        }

        try
        {
            // Use existing StockReport logic: helper method uses JS to export JSON to Excel
            // Construct a list of objects exactly as they should appear in Excel
            var excelData = reportData.Select(x => new
            {
                Date = x.Date.HasValue ? x.Date.Value.ToString("dd/MM/yyyy") : "",
                VoucherNo = x.VoucherNo,
                Type = x.VoucherType,
                Supplier = x.SupplierName,
                Narration = x.Narration,
                Debit = x.Debit,
                Credit = x.Credit,
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

    private async Task RowClickEvent(TableRowClickEventArgs<SupplierStatementDTO> args)
    {
        var item = args.Item;
        if (item == null || string.IsNullOrEmpty(item.VoucherType) || item.VoucherId == null || item.VoucherId == 0) return;

        switch (item.VoucherType)
        {
            case "Purchase":
                await CrudHelper.Navigation(TabService, item.VoucherId.Value, "PurchaseTitle");
                break;
            case "SupplierPayment":
                await CrudHelper.Navigation(TabService, item.VoucherId.Value, "SupplierPayment");
                break;
            case "Purchase Return":
                await CrudHelper.Navigation(TabService, item.VoucherId.Value, "Purchase Return");
                break;
            case "SupplierOPEntry": // Matches DB Type
            case "SupplierOpeningEntry": // Match potential variants
            case "OpeningBalance":
            case "Opening Balance":
                await CrudHelper.Navigation(TabService, item.VoucherId.Value, "SupplierOpeningEntry");
                break;
            case "JournalEntry":
                await CrudHelper.Navigation(TabService, item.VoucherId.Value, "JournalEntry");
                break;
            case "ExpenseEntry":
                await CrudHelper.Navigation(TabService, item.VoucherId.Value, "ExpenseEntry");
                break;
        }
    }
}
