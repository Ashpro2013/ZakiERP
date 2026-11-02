using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Shared.Helpers;
using ZakiSAASERP.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ZakiSAASERP.Shared.Pages.Settings;

public partial class ProductExportView : ITabAware
{
    [Parameter] public int TabId { get; set; }

    private List<ProductExportRowDTO> products = new();
    private bool isLoading = false;
    private string loadingText = "";
    private string searchString = "";

    private IEnumerable<ProductExportRowDTO> FilteredRows
    {
        get
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return products;

            return products.Where(x =>
                (x.ProductName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.ArabicName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Barcode?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Category?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.SubCategory?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Brand?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Unit?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.VatCategory?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            isLoading = true;
            loadingText = AppData.IsArabic ? "جاري تحميل المنتجات..." : "Loading products...";
            StateHasChanged();

            products = await productExportRepo.GetProductsForExportAsync();

            if (!products.Any())
            {
                snackbar.Add(AppData.IsArabic ? "لم يتم العثور على أي منتجات." : "No products found in the database.", Severity.Info);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error loading products: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task ExportToCsvAsync()
    {
        if (!products.Any()) return;

        try
        {
            isLoading = true;
            loadingText = AppData.IsArabic ? "جاري إنشاء ملف CSV..." : "Generating CSV file...";
            StateHasChanged();

            var exportList = FilteredRows.ToList();
            byte[] csvBytes = productExportRepo.GenerateCsv(exportList);
            var fileName = $"Products_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            await downloader.DownloadFromStream(csvBytes, fileName);

            snackbar.Add(AppData.IsArabic
                ? $"تم تصدير {exportList.Count} منتج بنجاح إلى ملف {fileName}."
                : $"Successfully exported {exportList.Count} product(s) to {fileName}.", Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Export error: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}
