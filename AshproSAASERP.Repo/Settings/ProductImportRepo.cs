using System.Globalization;
using System.Text;
using AshproSAASERP.Interface;
using AshproSAASERP.Interface.Settings;
using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.Masters;
using AshproSAASERP.Repo.CodeGenerationServices;
using Microsoft.EntityFrameworkCore;

namespace AshproSAASERP.Repo.Settings;

public class ProductImportRepo(IDbContextProvider contextProvider, CodeSequenceService codeService) : IProductImportRepo
{
    public byte[] GenerateSampleCsv()
    {
        var csvHeader = "Product Name,Arabic Name,Barcode,Category,SubCategory,Brand,Unit,Vat Category,SalesRate,Sales Inclusive Rate,Purchase Rate\r\n";
        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(csvHeader);
        return preamble.Concat(bytes).ToArray();
    }

    public List<ProductImportRowDTO> ParseCsv(string csvContent, out List<string> parseErrors)
    {
        parseErrors = new List<string>();
        var rows = new List<ProductImportRowDTO>();

        if (string.IsNullOrWhiteSpace(csvContent))
        {
            parseErrors.Add("CSV content is empty.");
            return rows;
        }

        var lines = SplitCsvLines(csvContent);
        if (lines.Count < 2)
        {
            parseErrors.Add("CSV must contain a header row and at least one data row.");
            return rows;
        }

        var headerTokens = ParseCsvLine(lines[0]);
        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < headerTokens.Count; i++)
        {
            var clean = headerTokens[i].Trim().Replace(" ", "").Replace("_", "").ToLowerInvariant();
            if (!colMap.ContainsKey(clean))
            {
                colMap[clean] = i;
            }
        }

        int GetColIndex(params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                var clean = alias.Trim().Replace(" ", "").Replace("_", "").ToLowerInvariant();
                if (colMap.TryGetValue(clean, out int idx))
                    return idx;
            }
            return -1;
        }

        int pNameIdx = GetColIndex("ProductName", "ItemName", "Name", "Product");
        int pArabicNameIdx = GetColIndex("ArabicName", "ProductArabicName", "ItemNameArabic", "ItemArabicName", "Arabic", "الاسمالعربي", "الاسم");
        int barcodeIdx = GetColIndex("Barcode", "BarCode", "ItemBarcode");
        int catIdx = GetColIndex("Category", "CategoryName", "Cat");
        int subCatIdx = GetColIndex("SubCategory", "SubCategoryName", "SubCat");
        int brandIdx = GetColIndex("Brand", "BrandName");
        int unitIdx = GetColIndex("Unit", "UnitName", "BaseUnit");
        int vatIdx = GetColIndex("VatCategory", "Vat", "Tax", "TaxCategory");
        int sRateIdx = GetColIndex("SalesRate", "SaleRate", "Rate", "Price");
        int sIncRateIdx = GetColIndex("SalesInclusiveRate", "InclusiveRate", "InclusivePrice", "IncRate");
        int pRateIdx = GetColIndex("PurchaseRate", "Cost", "StandardCost", "BuyRate");

        // Fallbacks if not matched by aliases
        if (pArabicNameIdx != -1)
        {
            if (pNameIdx == -1 && headerTokens.Count > 0) pNameIdx = 0;
            if (barcodeIdx == -1 && headerTokens.Count > 2) barcodeIdx = 2;
            if (catIdx == -1 && headerTokens.Count > 3) catIdx = 3;
            if (subCatIdx == -1 && headerTokens.Count > 4) subCatIdx = 4;
            if (brandIdx == -1 && headerTokens.Count > 5) brandIdx = 5;
            if (unitIdx == -1 && headerTokens.Count > 6) unitIdx = 6;
            if (vatIdx == -1 && headerTokens.Count > 7) vatIdx = 7;
            if (sRateIdx == -1 && headerTokens.Count > 8) sRateIdx = 8;
            if (sIncRateIdx == -1 && headerTokens.Count > 9) sIncRateIdx = 9;
            if (pRateIdx == -1 && headerTokens.Count > 10) pRateIdx = 10;
        }
        else
        {
            if (pNameIdx == -1 && headerTokens.Count > 0) pNameIdx = 0;
            if (barcodeIdx == -1 && headerTokens.Count > 1) barcodeIdx = 1;
            if (catIdx == -1 && headerTokens.Count > 2) catIdx = 2;
            if (subCatIdx == -1 && headerTokens.Count > 3) subCatIdx = 3;
            if (brandIdx == -1 && headerTokens.Count > 4) brandIdx = 4;
            if (unitIdx == -1 && headerTokens.Count > 5) unitIdx = 5;
            if (vatIdx == -1 && headerTokens.Count > 6) vatIdx = 6;
            if (sRateIdx == -1 && headerTokens.Count > 7) sRateIdx = 7;
            if (sIncRateIdx == -1 && headerTokens.Count > 8) sIncRateIdx = 8;
            if (pRateIdx == -1 && headerTokens.Count > 9) pRateIdx = 9;
        }

        List<VatMaster> dbVats = new();
        decimal defaultVatPercent = 15m;
        try
        {
            using var db = contextProvider.CreateDbContext();
            dbVats = db.VatMasters.AsNoTracking().ToList();
            var def = dbVats.FirstOrDefault(v => v.IsDefault) ?? dbVats.FirstOrDefault();
            if (def?.Percentage != null)
            {
                defaultVatPercent = def.Percentage.Value;
            }
        }
        catch { }

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var tokens = ParseCsvLine(line);
            if (tokens.All(string.IsNullOrWhiteSpace)) continue;

            string GetToken(int idx) => (idx >= 0 && idx < tokens.Count) ? tokens[idx].Trim() : string.Empty;

            decimal? ParseDecimal(string val)
            {
                if (string.IsNullOrWhiteSpace(val)) return null;
                val = val.Replace("$", "").Replace("SAR", "").Replace("SR", "").Replace(",", "").Trim();
                if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
                if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.CurrentCulture, out var d2)) return d2;
                return null;
            }

            var pName = GetToken(pNameIdx);
            var pArabicName = GetToken(pArabicNameIdx);
            var barcode = GetToken(barcodeIdx);
            var category = GetToken(catIdx);
            var subCategory = GetToken(subCatIdx);
            var brand = GetToken(brandIdx);
            var unit = GetToken(unitIdx);
            var vatCategory = GetToken(vatIdx);
            var salesRate = ParseDecimal(GetToken(sRateIdx));
            var salesIncRate = ParseDecimal(GetToken(sIncRateIdx));
            var purchaseRate = ParseDecimal(GetToken(pRateIdx));

            // Determine VAT percentage for rate calculation
            decimal vatPct = defaultVatPercent;
            if (!string.IsNullOrWhiteSpace(vatCategory))
            {
                var matchedVat = dbVats.FirstOrDefault(v =>
                    v.Name.Equals(vatCategory, StringComparison.OrdinalIgnoreCase) ||
                    v.Code.Equals(vatCategory, StringComparison.OrdinalIgnoreCase));

                if (matchedVat != null && matchedVat.Percentage.HasValue)
                {
                    vatPct = matchedVat.Percentage.Value;
                }
                else
                {
                    var cleanVat = vatCategory.Replace("%", "").Replace("VAT", "").Replace("Tax", "").Trim();
                    if (decimal.TryParse(cleanVat, NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
                    {
                        vatPct = p;
                    }
                    else if (decimal.TryParse(cleanVat, NumberStyles.Any, CultureInfo.CurrentCulture, out var p2))
                    {
                        vatPct = p2;
                    }
                }
            }

            // If inclusive rate not given or 0 then calculate with vat percentage and sales rate
            if (salesRate.HasValue && salesRate.Value > 0 && (!salesIncRate.HasValue || salesIncRate.Value == 0))
            {
                salesIncRate = Math.Round(salesRate.Value * (1 + (vatPct / 100m)), 2);
            }
            else if (salesIncRate.HasValue && salesIncRate.Value > 0 && (!salesRate.HasValue || salesRate.Value == 0))
            {
                salesRate = Math.Round(salesIncRate.Value / (1 + (vatPct / 100m)), 2);
            }

            var rowDto = new ProductImportRowDTO
            {
                RowNumber = i,
                ProductName = pName,
                ArabicName = string.IsNullOrWhiteSpace(pArabicName) ? null : pArabicName,
                Barcode = barcode,
                Category = category,
                SubCategory = subCategory,
                Brand = brand,
                Unit = unit,
                VatCategory = vatCategory,
                SalesRate = salesRate,
                SalesInclusiveRate = salesIncRate,
                PurchaseRate = purchaseRate,
                IsValid = !string.IsNullOrWhiteSpace(pName),
                Status = string.IsNullOrWhiteSpace(pName) ? "Invalid: Missing Product Name" : "Ready"
            };

            rows.Add(rowDto);
        }

        return rows;
    }

    public async Task<ProductImportResultDTO> ImportProductsAsync(List<ProductImportRowDTO> rows)
    {
        var result = new ProductImportResultDTO
        {
            TotalRows = rows.Count
        };

        if (rows == null || rows.Count == 0)
        {
            result.Message = "No rows to import.";
            result.IsSuccess = false;
            return result;
        }

        await using var db = contextProvider.CreateDbContext();

        // 1. Preload master records
        var categories = await db.CategoryMasters.ToListAsync();
        var subCategories = await db.SubCategoryMasters.ToListAsync();
        var brands = await db.BrandMasters.ToListAsync();
        var units = await db.UnitMasters.ToListAsync();
        var vats = await db.VatMasters.ToListAsync();

        // Preload existing names and barcodes
        var existingNames = new HashSet<string>(
            await db.ItemMasters.Select(x => x.ItemName!.Trim().ToLower()).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);

        var existingBarcodes = new HashSet<string>(
            await db.ItemMasters.Where(x => !string.IsNullOrEmpty(x.Barcode)).Select(x => x.Barcode!.Trim()).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);

        var currentBatchNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentBatchBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Default VAT: default or first
        var defaultVat = vats.FirstOrDefault(v => v.IsDefault) ?? vats.FirstOrDefault();
        if (defaultVat == null)
        {
            defaultVat = new VatMaster { Name = "VAT 15%", Code = "VAT15", Percentage = 15, IsDefault = true };
            db.VatMasters.Add(defaultVat);
            await db.SaveChangesAsync();
            vats.Add(defaultVat);
        }

        // Default Unit: EACH or first
        var defaultUnit = units.FirstOrDefault(u => u.Name.Equals("EACH", StringComparison.OrdinalIgnoreCase)) ?? units.FirstOrDefault();
        if (defaultUnit == null)
        {
            defaultUnit = new UnitMaster { Name = "EACH", Code = "EACH" };
            db.UnitMasters.Add(defaultUnit);
            await db.SaveChangesAsync();
            units.Add(defaultUnit);
        }

        // Default Category: GENERAL or first
        var defaultCategory = categories.FirstOrDefault(c => c.Name.Equals("GENERAL", StringComparison.OrdinalIgnoreCase)) ?? categories.FirstOrDefault();
        if (defaultCategory == null)
        {
            defaultCategory = new CategoryMaster { Name = "GENERAL", Code = "GEN" };
            db.CategoryMasters.Add(defaultCategory);
            await db.SaveChangesAsync();
            categories.Add(defaultCategory);
        }

        foreach (var row in rows)
        {
            try
            {
                var prodName = row.ProductName?.Trim();
                if (string.IsNullOrWhiteSpace(prodName))
                {
                    result.ErrorCount++;
                    result.Messages.Add($"Row {row.RowNumber}: Product Name is empty. Skipped.");
                    continue;
                }

                var barcode = string.IsNullOrWhiteSpace(row.Barcode) ? null : row.Barcode.Trim();

                // Check existing product by Name or Barcode
                if (existingNames.Contains(prodName) || currentBatchNames.Contains(prodName))
                {
                    result.SkippedCount++;
                    result.Messages.Add($"Row {row.RowNumber}: Product '{prodName}' already exists. Skipped.");
                    continue;
                }

                if (!string.IsNullOrEmpty(barcode) && (existingBarcodes.Contains(barcode) || currentBatchBarcodes.Contains(barcode)))
                {
                    result.SkippedCount++;
                    result.Messages.Add($"Row {row.RowNumber}: Barcode '{barcode}' already exists. Skipped.");
                    continue;
                }

                // 2. Category lookup or create
                CategoryMaster? category = null;
                var catName = row.Category?.Trim();
                if (!string.IsNullOrWhiteSpace(catName))
                {
                    category = categories.FirstOrDefault(c => c.Name.Equals(catName, StringComparison.OrdinalIgnoreCase));
                    if (category == null)
                    {
                        string catCode = await GetNextCategoryCodeAsync(db);
                        category = new CategoryMaster
                        {
                            Name = catName,
                            Code = catCode,
                            IsFixedAsset = false
                        };
                        db.CategoryMasters.Add(category);
                        await db.SaveChangesAsync();
                        if (await codeService.IsAutoCodeEnabledAsync(CodeKeys.Category, db))
                        {
                            await codeService.ConsumeCodeAsync(CodeKeys.Category, db);
                        }
                        categories.Add(category);
                    }
                }
                else
                {
                    category = defaultCategory;
                }

                // 3. SubCategory lookup or create
                SubCategoryMaster? subCategory = null;
                var subCatName = row.SubCategory?.Trim();
                if (!string.IsNullOrWhiteSpace(subCatName))
                {
                    subCategory = subCategories.FirstOrDefault(s =>
                        s.Name!.Equals(subCatName, StringComparison.OrdinalIgnoreCase) &&
                        (category == null || s.FK_CategoryId == category.CategoryId));

                    if (subCategory == null)
                    {
                        string subCatCode = await GetNextSubCategoryCodeAsync(category?.CategoryId, category?.Code, db);
                        subCategory = new SubCategoryMaster
                        {
                            Name = subCatName,
                            FK_CategoryId = category?.CategoryId,
                            Code = subCatCode,
                            IsFixedAsset = false
                        };
                        db.SubCategoryMasters.Add(subCategory);
                        await db.SaveChangesAsync();
                        await codeService.ConsumeSubCategoryCodeAsync(category?.CategoryId, db);
                        subCategories.Add(subCategory);
                    }
                }

                // 4. Brand lookup or create
                BrandMaster? brand = null;
                var brName = row.Brand?.Trim();
                if (!string.IsNullOrWhiteSpace(brName))
                {
                    brand = brands.FirstOrDefault(b => b.Name.Equals(brName, StringComparison.OrdinalIgnoreCase));
                    if (brand == null)
                    {
                        string brCode = await GetNextBrandCodeAsync(db);
                        brand = new BrandMaster
                        {
                            Name = brName,
                            Code = brCode
                        };
                        db.BrandMasters.Add(brand);
                        await db.SaveChangesAsync();
                        brands.Add(brand);
                    }
                }

                // 5. Unit lookup or create
                UnitMaster? unit = null;
                var uName = row.Unit?.Trim();
                if (!string.IsNullOrWhiteSpace(uName))
                {
                    unit = units.FirstOrDefault(u => u.Name.Equals(uName, StringComparison.OrdinalIgnoreCase) || u.Code.Equals(uName, StringComparison.OrdinalIgnoreCase));
                    if (unit == null)
                    {
                        string uCode = uName.Length > 10 ? uName.Substring(0, 10).ToUpper() : uName.ToUpper();
                        if (units.Any(u => u.Code.Equals(uCode, StringComparison.OrdinalIgnoreCase)))
                        {
                            uCode = (uCode + (units.Count + 1)).Length > 20 ? (uCode + (units.Count + 1)).Substring(0, 20) : (uCode + (units.Count + 1));
                        }
                        unit = new UnitMaster
                        {
                            Name = uName,
                            Code = uCode
                        };
                        db.UnitMasters.Add(unit);
                        await db.SaveChangesAsync();
                        units.Add(unit);
                    }
                }
                else
                {
                    unit = defaultUnit;
                }

                // 6. Vat Category lookup or create
                VatMaster? vat = null;
                var vatInput = row.VatCategory?.Trim();
                if (!string.IsNullOrWhiteSpace(vatInput))
                {
                    vat = vats.FirstOrDefault(v =>
                        v.Name.Equals(vatInput, StringComparison.OrdinalIgnoreCase) ||
                        v.Code.Equals(vatInput, StringComparison.OrdinalIgnoreCase));

                    if (vat == null && decimal.TryParse(vatInput.Replace("%", "").Trim(), out var parsedPct))
                    {
                        vat = vats.FirstOrDefault(v => v.Percentage == parsedPct);
                        if (vat == null)
                        {
                            vat = new VatMaster
                            {
                                Name = $"VAT {parsedPct}%",
                                Code = $"VAT{parsedPct}",
                                Percentage = parsedPct,
                                IsDefault = false
                            };
                            db.VatMasters.Add(vat);
                            await db.SaveChangesAsync();
                            vats.Add(vat);
                        }
                    }
                }

                if (vat == null)
                {
                    vat = defaultVat;
                }

                // 7. Rates resolution
                decimal vatPercent = vat.Percentage ?? 0;
                decimal? salesRate = row.SalesRate;
                decimal? incRate = row.SalesInclusiveRate;
                decimal? purchaseRate = row.PurchaseRate;

                if (salesRate.HasValue && salesRate.Value > 0 && (!incRate.HasValue || incRate.Value == 0))
                {
                    incRate = Math.Round(salesRate.Value * (1 + (vatPercent / 100m)), 2);
                }
                else if (incRate.HasValue && incRate.Value > 0 && (!salesRate.HasValue || salesRate.Value == 0))
                {
                    salesRate = Math.Round(incRate.Value / (1 + (vatPercent / 100m)), 2);
                }

                row.SalesRate = salesRate;
                row.SalesInclusiveRate = incRate;

                // 8. Generate Item Code
                string itemCode = await codeService.GetNextItemCodeAsync(
                    category?.CategoryId, subCategory?.SubCategoryId,
                    category?.Code, subCategory?.Code, db);

                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    itemCode = await GenerateFallbackItemCodeAsync(db);
                }

                string finalBarcode = string.IsNullOrWhiteSpace(barcode) ? itemCode : barcode;

                // 9. Create ItemMaster
                var newItem = new ItemMaster
                {
                    ItemCode = itemCode,
                    ItemName = prodName,
                    ItemNameArabic = !string.IsNullOrWhiteSpace(row.ArabicName) ? row.ArabicName.Trim() : null,
                    Barcode = finalBarcode,
                    FK_CategoryId = category?.CategoryId,
                    FK_SubCategoryId = subCategory?.SubCategoryId,
                    FK_BrandId = brand?.BrandId,
                    FK_UnitId = unit.UnitId,
                    FK_VatId = vat.VatId,
                    VatCategory = vatPercent == 0 ? "Z" : "S",
                    SalesRate = salesRate,
                    InclusiveRate = incRate,
                    PurchaseRate = purchaseRate,
                    StandardCost = purchaseRate,
                    StockMode = "Inventory",
                    IsActive = true,
                    IsSaleable = true
                };

                // 10. Add MultiUnit for base unit
                var baseMultiUnit = new ItemMultiUnit
                {
                    FK_UnitId = unit.UnitId,
                    MultiUnitName = $"{newItem.ItemName}-{unit.Name}",
                    MultiUnitArabicName = !string.IsNullOrWhiteSpace(newItem.ItemNameArabic) ? $"{newItem.ItemNameArabic}-{unit.Name}" : null,
                    QTYPerUnit = 1,
                    Rate = newItem.SalesRate,
                    IncRate = newItem.InclusiveRate,
                    Cost = newItem.PurchaseRate ?? newItem.StandardCost,
                    Barcode = newItem.Barcode
                };
                newItem.MultiUnits.Add(baseMultiUnit);

                // Add to DB
                await db.ItemMasters.AddAsync(newItem);
                await db.SaveChangesAsync();

                // Consume Item Code Sequence
                await codeService.ConsumeItemCodeAsync(category?.CategoryId, subCategory?.SubCategoryId, db);

                // Record in cache & batch trackers
                existingNames.Add(prodName);
                currentBatchNames.Add(prodName);
                if (!string.IsNullOrEmpty(finalBarcode))
                {
                    existingBarcodes.Add(finalBarcode);
                    currentBatchBarcodes.Add(finalBarcode);
                }

                result.ImportedCount++;
                result.Messages.Add($"Row {row.RowNumber}: Successfully imported '{prodName}' (Code: {itemCode}, Barcode: {finalBarcode}).");
            }
            catch (Exception ex)
            {
                result.ErrorCount++;
                result.Messages.Add($"Row {row.RowNumber}: Error importing '{row.ProductName}': {ex.Message}");
            }
        }

        result.IsSuccess = result.ImportedCount > 0;
        result.Message = $"Import finished: {result.ImportedCount} imported, {result.SkippedCount} skipped, {result.ErrorCount} failed.";
        return result;
    }

    private async Task<string> GetNextCategoryCodeAsync(TenantDBContext db)
    {
        if (await codeService.IsAutoCodeEnabledAsync(CodeKeys.Category, db))
        {
            var code = await codeService.GetNextUniqueCodeAsync(CodeKeys.Category, async c => await db.CategoryMasters.AnyAsync(x => x.Code == c));
            if (!string.IsNullOrWhiteSpace(code)) return code;
        }
        int count = await db.CategoryMasters.CountAsync() + 1;
        return $"CAT-{count:D3}";
    }

    private async Task<string> GetNextSubCategoryCodeAsync(int? categoryId, string? catCode, TenantDBContext db)
    {
        if (await codeService.IsAutoCodeEnabledAsync(CodeKeys.SubCategory, db))
        {
            var code = await codeService.GetNextSubCategoryCodeAsync(categoryId, catCode, db);
            if (!string.IsNullOrWhiteSpace(code)) return code;
        }
        int count = await db.SubCategoryMasters.CountAsync() + 1;
        return $"SUB-{count:D3}";
    }

    private async Task<string> GetNextBrandCodeAsync(TenantDBContext db)
    {
        int count = await db.BrandMasters.CountAsync() + 1;
        return $"BR-{count:D3}";
    }

    private async Task<string> GenerateFallbackItemCodeAsync(TenantDBContext db)
    {
        int count = await db.ItemMasters.CountAsync() + 1;
        string code;
        do
        {
            code = $"ITM-{count:D5}";
            count++;
        } while (await db.ItemMasters.AnyAsync(x => x.ItemCode == code));
        return code;
    }

    private static List<string> SplitCsvLines(string text)
    {
        var lines = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                sb.Append(c);
            }
            else if ((c == '\r' || c == '\n') && !inQuotes)
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }
                lines.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        if (sb.Length > 0)
        {
            lines.Add(sb.ToString());
        }
        return lines;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var tokens = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                tokens.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        tokens.Add(sb.ToString());
        return tokens;
    }
}
