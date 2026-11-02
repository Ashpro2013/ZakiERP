namespace ZakiSAASERP.Shared.Pages.Reports.Accounts;

public partial class SupplierListReport : ComponentBase
{
    // Injected in Razor via @inject Downloader downloader

    private List<SupplierListDTO> reportData = new();

    [Parameter] public int TabId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // tailored for simple list, no masters needed if no drop downs
    }

    private async Task LoadData()
    {
        try
        {
            reportData = await api.GetSupplierListAsync();

            if (!reportData.Any())
            {
                Snackbar.Add(Localizer("NoRecordFound"), Severity.Info);
            }
            StateHasChanged();
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
            var pdto = await api.GetPrintDesignAsync(new(0, "Supplier_List"));

            if (pdto == null || pdto.hdr == null)
            {
                Snackbar.Add($"Print design 'Supplier_List' not found.", Severity.Warning);
                return;
            }

            // Prepare Header Data
            var header = new SupplierListHdr(
                Date: DateTime.Now.ToString("dd/MM/yyyy")
            );

            // Convert Header to Dictionary
            Dictionary<string, object?> ReportHdr = header.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(header));

            // Prepare Details Data
            var obj = reportData.Cast<object>().ToList();
            Dictionary<string, object> ReportDtls = new() { { "Details", obj } };

            var printHelper = new SyncfusionPrintHelper();
            byte[]? bytes = await printHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview, true);

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
    public record SupplierListHdr(string Date);

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
                Address = x.Address,
                Telephone = x.Telephone,
                Mail = x.Mail
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
