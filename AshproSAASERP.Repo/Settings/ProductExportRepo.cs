using System.Globalization;
using System.Text;
using AshproSAASERP.Interface;
using AshproSAASERP.Interface.Settings;
using AshproSAASERP.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace AshproSAASERP.Repo.Settings;

public class ProductExportRepo(IDbContextProvider contextProvider) : IProductExportRepo
{
    public async Task<List<ProductExportRowDTO>> GetProductsForExportAsync()
    {
        using var db = contextProvider.CreateDbContext();

        var items = await db.ItemMasters
            .AsNoTracking()
            .Where(x => x.IsActive != false)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Unit)
            .Include(x => x.Vat)
            .Include(x => x.MultiUnits)
            .OrderBy(x => x.ItemId)
            .ToListAsync();

        var defaultVatPercent = 15m;
        try
        {
            var defVat = await db.VatMasters.AsNoTracking().FirstOrDefaultAsync(v => v.IsDefault)
                         ?? await db.VatMasters.AsNoTracking().FirstOrDefaultAsync();
            if (defVat?.Percentage != null)
            {
                defaultVatPercent = defVat.Percentage.Value;
            }
        }
        catch { }

        var result = new List<ProductExportRowDTO>();
        int sl = 1;

        foreach (var item in items)
        {
            // Barcode resolution: ItemMaster.Barcode or first MultiUnit barcode
            var barcode = item.Barcode;
            if (string.IsNullOrWhiteSpace(barcode) && item.MultiUnits.Any())
            {
                barcode = item.MultiUnits.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.Barcode))?.Barcode;
            }

            // Vat resolution
            var vatPercentage = item.Vat?.Percentage ?? defaultVatPercent;
            var vatCategory = !string.IsNullOrWhiteSpace(item.Vat?.Name)
                ? item.Vat.Name
                : (!string.IsNullOrWhiteSpace(item.VatCategory) ? item.VatCategory : "Standard");

            // Sales Inclusive Rate resolution: if not set or 0, calculate with vat percentage and sales rate
            decimal? salesInclusiveRate = item.InclusiveRate;
            if ((salesInclusiveRate == null || salesInclusiveRate == 0) && item.SalesRate.HasValue && item.SalesRate.Value > 0)
            {
                salesInclusiveRate = Math.Round(item.SalesRate.Value * (1 + (vatPercentage / 100m)), 2, MidpointRounding.AwayFromZero);
            }

            result.Add(new ProductExportRowDTO
            {
                SL = sl++,
                ProductName = item.ItemName,
                ArabicName = item.ItemNameArabic,
                Barcode = barcode,
                Category = item.Category?.Name,
                SubCategory = item.SubCategory?.Name,
                Brand = item.Brand?.Name,
                Unit = item.Unit?.Name,
                VatCategory = vatCategory,
                SalesRate = item.SalesRate,
                SalesInclusiveRate = salesInclusiveRate,
                PurchaseRate = item.PurchaseRate
            });
        }

        return result;
    }

    public byte[] GenerateCsv(List<ProductExportRowDTO> products)
    {
        var sb = new StringBuilder();

        // Customer specified header:
        // Product Name,Arabic Name,Barcode,Category,SubCategory,Brand,Unit,Vat Category,SalesRate,Sales Inclusive Rate,Purchase Rate
        sb.AppendLine("Product Name,Arabic Name,Barcode,Category,SubCategory,Brand,Unit,Vat Category,SalesRate,Sales Inclusive Rate,Purchase Rate");

        foreach (var p in products)
        {
            var pName = EscapeCsv(p.ProductName);
            var arabicName = EscapeCsv(p.ArabicName);
            var barcode = EscapeCsv(p.Barcode);
            var cat = EscapeCsv(p.Category);
            var subCat = EscapeCsv(p.SubCategory);
            var brand = EscapeCsv(p.Brand);
            var unit = EscapeCsv(p.Unit);
            var vatCat = EscapeCsv(p.VatCategory);
            var salesRate = p.SalesRate.HasValue ? p.SalesRate.Value.ToString("F2", CultureInfo.InvariantCulture) : "";
            var incRate = p.SalesInclusiveRate.HasValue ? p.SalesInclusiveRate.Value.ToString("F2", CultureInfo.InvariantCulture) : "";
            var purchaseRate = p.PurchaseRate.HasValue ? p.PurchaseRate.Value.ToString("F2", CultureInfo.InvariantCulture) : "";

            sb.AppendLine($"{pName},{arabicName},{barcode},{cat},{subCat},{brand},{unit},{vatCat},{salesRate},{incRate},{purchaseRate}");
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return preamble.Concat(bytes).ToArray();
    }

    public async Task<byte[]> ExportProductsToCsvAsync()
    {
        var products = await GetProductsForExportAsync();
        return GenerateCsv(products);
    }

    private static string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        var containsSpecial = field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r');
        if (containsSpecial)
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
