namespace ZakiSAASERP.Shared.Pages.Reports.Accounts;

using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Enums;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Services;
using FileDownloader;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static ZakiSAASERP.Shared.Services.ArabicLocalization;

public partial class SupplierPaymentReports : ComponentBase
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
    private string billType = "All";

    private List<CommonModel> branchList = new();
    private List<SupplierPaymentReportDTO> reportData = new();

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
        billType = "All";
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

        try
        {
            reportData = await api.GetSupplierPaymentReportAsync(fromDate.Value, toDate.Value, branchId, supplierId, billType);

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
            var pdto = await api.GetPrintDesignAsync(new(0, ReportTypes.Supplier_Payment_Report.ToString()));

            if (pdto == null || pdto.hdr == null)
            {
                Snackbar.Add("Print design 'Supplier_Payment_Report' not found. Using default structure may require setup.", Severity.Warning);
                return;
            }

            var header = new SupplierPaymentReportHdr(
                Date: DateTime.Now.ToString("dd/MM/yyyy"),
                FromDate: fromDate.HasValue ? fromDate.Value.ToString("dd/MM/yyyy") : "",
                ToDate: toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "",
                SupplierName: (supplierId.HasValue && !string.IsNullOrEmpty(supplierName)) ? supplierName : "All Suppliers",
                PaymentMode: billType,
                TotalAmount: reportData.Sum(x => x.Amount).ToString("N2")
            );

            Dictionary<string, object?> ReportHdr = header.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(header));

            var printList = reportData.ToList();
            printList.Add(new SupplierPaymentReportDTO
            {
                SL = null, // Will be ignored or can be handled as null if type allows
                Date = null,
                VoucherNo = "",
                VoucherType = "",
                PaymentMode = "",
                SupplierName = "TOTAL",
                SupplierCode = "",
                Narration = "",
                Amount = reportData.Sum(x => x.Amount), // Calculate Total
                ChequeNo = "",
                ChequeDate = null,
                Bank = ""
            });

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

    public record SupplierPaymentReportHdr(string Date, string FromDate, string ToDate, string SupplierName, string PaymentMode, string TotalAmount);
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
                PaymentMode = x.PaymentMode,
                Supplier = x.SupplierName,
                Narration = x.Narration,
                ChequeNo = x.ChequeNo,
                ChequeDate = x.ChequeDate.HasValue ? x.ChequeDate.Value.ToString("dd/MM/yyyy") : "",
                Bank = x.Bank,
                Amount = x.Amount
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
