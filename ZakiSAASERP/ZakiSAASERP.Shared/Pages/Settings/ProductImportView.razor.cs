using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Shared.Helpers;
using ZakiSAASERP.Shared.Services;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace ZakiSAASERP.Shared.Pages.Settings;

public partial class ProductImportView : ITabAware
{
    [Parameter] public int TabId { get; set; }

    private List<ProductImportRowDTO> parsedRows = new();
    private ProductImportResultDTO? importResult;
    private bool isLoading = false;
    private string loadingText = "";
    private string? selectedFileName;
    private string searchString = "";

    private IEnumerable<ProductImportRowDTO> FilteredRows
    {
        get
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return parsedRows;

            return parsedRows.Where(x =>
                (x.ProductName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.ArabicName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Barcode?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Category?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.SubCategory?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Brand?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Unit?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Status?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }

    private async Task DownloadSampleCsvAsync()
    {
        try
        {
            isLoading = true;
            loadingText = AppData.IsArabic ? "جاري إنشاء وتنزيل النموذج..." : "Generating sample CSV...";
            StateHasChanged();

            byte[] csvBytes = productImportRepo.GenerateSampleCsv();
            await downloader.DownloadFromStream(csvBytes, "Product_Import_Sample.csv");

            snackbar.Add(AppData.IsArabic ? "تم تنزيل النموذج بنجاح." : "Sample CSV downloaded successfully.", Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error downloading sample: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnCsvFileSelected(IBrowserFile file)
    {
        if (file == null) return;

        if (!file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            snackbar.Add(AppData.IsArabic ? "الرجاء اختيار ملف بصيغة CSV صالحة." : "Please select a valid CSV file.", Severity.Warning);
            return;
        }

        try
        {
            isLoading = true;
            loadingText = AppData.IsArabic ? "جاري قراءة وتحليل الملف..." : "Reading and parsing CSV file...";
            selectedFileName = file.Name;
            importResult = null;
            StateHasChanged();

            using var stream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            var rows = productImportRepo.ParseCsv(content, out var errors);

            if (errors.Any() && rows.Count == 0)
            {
                snackbar.Add(string.Join(", ", errors), Severity.Error);
                return;
            }

            parsedRows = rows;

            if (parsedRows.Count == 0)
            {
                snackbar.Add(AppData.IsArabic ? "الملف فارغ أو لا يحتوي على صفوف بيانات." : "File is empty or contains no data rows.", Severity.Warning);
            }
            else
            {
                snackbar.Add(AppData.IsArabic
                    ? $"تمت قراءة {parsedRows.Count} منتج بنجاح."
                    : $"Successfully parsed {parsedRows.Count} product(s) from {file.Name}.", Severity.Info);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error parsing CSV: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private void ClearRows()
    {
        parsedRows.Clear();
        importResult = null;
        selectedFileName = null;
        searchString = "";
        StateHasChanged();
    }

    private async Task ImportProductsAsync()
    {
        if (parsedRows.Count == 0) return;

        var validRows = parsedRows.Where(r => r.IsValid).ToList();
        if (validRows.Count == 0)
        {
            snackbar.Add(AppData.IsArabic ? "لا توجد صفوف صالحة للاستيراد." : "No valid rows found to import.", Severity.Warning);
            return;
        }

        try
        {
            isLoading = true;
            loadingText = AppData.IsArabic
                ? $"جاري استيراد {validRows.Count} منتج إلى قاعدة البيانات..."
                : $"Importing {validRows.Count} product(s) into database...";
            StateHasChanged();

            importResult = await productImportRepo.ImportProductsAsync(validRows);

            if (importResult.IsSuccess)
            {
                snackbar.Add(importResult.Message ?? "Products imported successfully.", Severity.Success);
            }
            else
            {
                snackbar.Add(importResult.Message ?? "Import completed with issues.", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Import failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}
