namespace ZakiSAASERP.Shared.Pages.Reports.Accounts;

public partial class CustomerListReport : ComponentBase
{
    // Injected in Razor via @inject Downloader downloader

    private List<CustomerListDTO> reportData = new();

    [Parameter] public int TabId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // tailored for simple list, no masters needed if no drop downs
    }

    private async Task LoadData()
    {
        try
        {
            reportData = await api.GetCustomerListAsync();

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
            var pdto = await api.GetPrintDesignAsync(new(0, "Customer_List"));

            if (pdto == null || pdto.hdr == null)
            {
                Snackbar.Add($"Print design 'Customer_List' not found.", Severity.Warning);
                return;
            }

            // Prepare Header Data
            var header = new CustomerListHdr(
                Date: DateTime.Now.ToString("dd/MM/yyyy")
            );

            // Convert Header to Dictionary
            Dictionary<string, object?> ReportHdr = header.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(header));

            // Prepare Details Data
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
            Snackbar.Add($"Error printing: {ex.Message}", Severity.Error);
        }
    }

    // Local Record for Header Data
    public record CustomerListHdr(string Date);

    private async Task Print()
    {
        await PrintHelperMethod(false);
    }

    private async Task Preview()
    {
        await PrintHelperMethod(true);
    }



    private IEnumerable<string> options { get; set; } = new HashSet<string>()
    {
        "Code", "Name", "NameArabic", "Address", "AddressArabic", "CreditLimit", "CreditDays",
        "Contact", "ContactArabic", "Telephone", "PhoneArabic", "Mobile", "MobileArabic", "Mail",
        "VatNo", "VatNoArabic", "BuildingNo", "BuildingNoArabic", "StreetName", "StreetNameArabic",
        "District", "DistrictArabic", "City", "CityArabic", "PostalCode", "PostalCodeArabic",
        "AdditionalNo", "AdditionalNoArabic", "OtherSellerNo", "OtherSellerNoArabic", "CountrySubentity",
        "CountrySubentityArabic", "IdentificationNo", "IdentificationNoArabic", "Identification",
        "CusBillType", "DiscountPercentage", "Neighbourhood",
        "NeighbourhoodArabic", "GroupVat", "CustomerGroupName", "CountryName", "SalesmanName"
    };

    private IEnumerable<string> selectedOptions { get; set; } = new HashSet<string>()
    {
        "Code", "Name", "Address", "Telephone", "Mail" // Default selection
    };

    private async Task ExportExcel()
    {
        await LoadData();

        if (!reportData.Any())
        {
            return;
        }

        try
        {
            // Reflection to get property values dynamically based on selected columns
            var properties = typeof(CustomerListDTO).GetProperties();

            var excelData = reportData.Select(x =>
            {
                var dict = new Dictionary<string, object>();
                foreach (var col in selectedOptions)
                {
                    var prop = properties.FirstOrDefault(p => p.Name == col);
                    if (prop is not null)
                    {
                        dict.Add(col, prop.GetValue(x) ?? "");
                    }
                }
                return dict;
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
