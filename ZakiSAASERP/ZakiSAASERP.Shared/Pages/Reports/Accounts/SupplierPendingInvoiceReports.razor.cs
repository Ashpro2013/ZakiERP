namespace ZakiSAASERP.Shared.Pages.Reports.Accounts;

using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Enums;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Services;
using FileDownloader;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static ZakiSAASERP.Shared.Services.ArabicLocalization;

public partial class SupplierPendingInvoiceReports : ComponentBase
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
    private int? supplierId = null;
    private string supplierName = string.Empty;


    // Injected in Razor via @inject Downloader downloader

    private List<CommonModel> branchList = new();
    private List<SupplierPendingInvoiceDTO> reportData = new();

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

        try
        {
            reportData = await api.GetSupplierPendingInvoiceAsync(fromDate.Value, toDate.Value, branchId, supplierId);

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
            var pdto = await api.GetPrintDesignAsync(new(0, ReportTypes.Supplier_Pending_Invoice.ToString()));

            if (pdto == null || pdto.hdr == null)
            {
                // Fallback or Error
                Snackbar.Add("Print design 'Supplier_Pending_Invoice' not found. Using default structure may require setup.", Severity.Warning);
                // Ideally we shouldn't continue if design is missing, or we need to pass a default one.
                return;
            }

            // Fix for missing ReturnAmount in DB Design
            if (pdto.dtls is not null && !pdto.dtls.Any(x => x.ControlName == "ReturnAmount" && x.Location == "Detail"))
            {
                var paidCol = pdto.dtls.FirstOrDefault(x => x.ControlName == "PaidAmount" && x.Location == "Detail");
                if (paidCol is not null)
                {
                    var newCol = new PrintDesignDtls
                    {
                        ControlName = "ReturnAmount",
                        FieldText = "Returned",
                        Width = paidCol.Width,
                        PositionX = paidCol.PositionX + paidCol.Width,
                        Alignment = paidCol.Alignment,
                        IsBold = paidCol.IsBold,
                        FontSize = paidCol.FontSize,
                        Font = paidCol.Font,
                        Location = "Detail",
                        ControlType = "Field",
                        Height = paidCol.Height,
                        PIndex = paidCol.PIndex + 1
                    };

                    // Shift subsequent columns
                    foreach (var col in pdto.dtls.Where(x => x.Location == "Detail" && x.PositionX > paidCol.PositionX))
                    {
                        col.PositionX += newCol.Width;
                    }
                    pdto.dtls.Add(newCol);
                }
            }

            // Prepare Header Data
            var header = new SupplierStatementHdr(
                Date: DateTime.Now.ToString("dd/MM/yyyy"),
                FromDate: fromDate.HasValue ? fromDate.Value.ToString("dd/MM/yyyy") : "",
                ToDate: toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "",
                SupplierName: (supplierId.HasValue && !string.IsNullOrEmpty(supplierName)) ? supplierName : "All Suppliers",
                TotalDebit: reportData.Sum(x => x.PaidAmount).ToString("N2"),
                TotalReturned: reportData.Sum(x => x.ReturnAmount).ToString("N2"),
                TotalDiscount: reportData.Sum(x => x.Discount).ToString("N2"),
                TotalCredit: reportData.Sum(x => x.NetAmount).ToString("N2"),
                ClosingBalance: (reportData.Sum(x => x.Balance)).ToString("N2")
            );

            // Convert Header to Dictionary
            Dictionary<string, object?> ReportHdr = header.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(header));

            // Prepare details with Total Row
            var printList = reportData.ToList(); // shallow copy
            printList.Add(new SupplierPendingInvoiceDTO
            {
                SL = null,
                Date = null,
                VoucherNo = "TOTAL",
                VoucherType = "",
                SupplierName = "",
                Narration = "",
                PaidAmount = reportData.Sum(x => x.PaidAmount),
                ReturnAmount = reportData.Sum(x => x.ReturnAmount),
                Discount = reportData.Sum(x => x.Discount),
                NetAmount = reportData.Sum(x => x.NetAmount),
                Balance = reportData.Sum(x => x.Balance)
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
    public record SupplierStatementHdr(string Date, string FromDate, string ToDate, string SupplierName, string TotalDebit, string TotalReturned, string TotalDiscount, string TotalCredit, string ClosingBalance);
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
                InvoiceAmount = x.NetAmount,
                PaidAmount = x.PaidAmount,
                Returned = x.ReturnAmount,
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
