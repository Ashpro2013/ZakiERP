using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Models.Zatca;
using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Masters;

public class ItemMasterRepo(IDbContextProvider contextProvider, CodeSequenceService codeService) : IItemMasterRepo
{
    public async Task<OutputResponse> CreateAsync(ItemMaster entity)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            // 1. Check duplicate ItemCode
            if (!string.IsNullOrWhiteSpace(entity.ItemCode))
            {
                var trimmedCode = entity.ItemCode.Trim();
                var existsCode = await db.ItemMasters.AnyAsync(x => x.ItemCode == trimmedCode);
                if (existsCode) return new OutputResponse(0, "Item Code already exists.");
            }

            // 2. Check duplicate ItemName
            if (!string.IsNullOrWhiteSpace(entity.ItemName))
            {
                var trimmedName = entity.ItemName.Trim();
                var existsName = await db.ItemMasters.AnyAsync(x => x.ItemName == trimmedName);
                if (existsName) return new OutputResponse(0, "Item Name already exists.");
            }

            // 3. Check duplicate Barcode within entity's own multi-units
            var multiUnitBarcodes = entity.MultiUnits?
                .Where(mu => !string.IsNullOrWhiteSpace(mu.Barcode))
                .Select(mu => mu.Barcode!.Trim())
                .ToList();

            if (multiUnitBarcodes != null && multiUnitBarcodes.Count != multiUnitBarcodes.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                return new OutputResponse(0, "DuplicateBarcodeInUnits");
            }

            // 4. Check duplicate Barcode across database
            var allBarcodes = new List<string>();
            if (!string.IsNullOrWhiteSpace(entity.Barcode))
                allBarcodes.Add(entity.Barcode.Trim());

            if (multiUnitBarcodes != null)
                allBarcodes.AddRange(multiUnitBarcodes);

            var distinctBarcodes = allBarcodes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (distinctBarcodes.Count > 0)
            {
                try
                {
                    var barcodeExists = await db.ItemMasters.AnyAsync(x => !string.IsNullOrEmpty(x.Barcode) && distinctBarcodes.Contains(x.Barcode)) ||
                                  await db.ItemMultiUnits.AnyAsync(x => !string.IsNullOrEmpty(x.Barcode) && distinctBarcodes.Contains(x.Barcode));
                    if (barcodeExists) return new OutputResponse(0, "Barcode already exists.");
                }
                catch (Exception) { }
            }

            await db.ItemMasters.AddAsync(entity);
            await db.SaveChangesAsync();

            // Consume code sequence after successful save
            await codeService.ConsumeItemCodeAsync(entity.FK_CategoryId, entity.FK_SubCategoryId, db);

            return new OutputResponse(entity.ItemId, "Saved Successfully");
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var existing = await db.ItemMasters
                .Include(x => x.MultiUnits)
                .FirstOrDefaultAsync(x => x.ItemId == id);
            if (existing is null) return new OutputResponse(0, "Item not found.");

            // Delete related MultiUnits first
            if (existing.MultiUnits != null && existing.MultiUnits.Any())
            {
                db.ItemMultiUnits.RemoveRange(existing.MultiUnits);
            }

            db.ItemMasters.Remove(existing);
            await db.SaveChangesAsync();
            return new OutputResponse(id, "Deleted Successfully");
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<List<ItemMaster>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.ItemMasters.AsNoTracking().ToListAsync();
    }
    public async Task<(List<ItemMaster>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.ItemMasters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            query = query.Where(x => x.ItemCode.StartsWith(pagination.search) || x.ItemName.Contains(pagination.search) || x.Barcode.Contains(pagination.search));
        }

        int count = await query.CountAsync();
        var items = await query
           .Skip((pagination.page - 1) * pagination.pageSize)
           .Take(pagination.pageSize)
           .ToListAsync();

        return (items, count);
    }
    public async Task<ItemMaster> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        // Include MultiUnits when fetching
        var item = await db.ItemMasters
            .Include(x => x.MultiUnits).ThenInclude(m => m.Unit)
            .Include(x => x.Unit)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.Brand)
            .Include(x => x.Vat)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ItemId == id);
        return item!;
    }
    public async Task<OutputResponse> UpdateAsync(ItemMaster entity)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var existing = await db.ItemMasters
                .Include(x => x.MultiUnits)
                .FirstOrDefaultAsync(x => x.ItemId == entity.ItemId);
            if (existing is null) return new OutputResponse(0, "Item not found.");

            // 1. Check duplicate ItemCode against other items
            if (!string.IsNullOrWhiteSpace(entity.ItemCode))
            {
                var trimmedCode = entity.ItemCode.Trim();
                var existsCode = await db.ItemMasters.AnyAsync(x => x.ItemId != entity.ItemId && x.ItemCode == trimmedCode);
                if (existsCode) return new OutputResponse(0, "Item Code already exists.");
            }

            // 2. Check duplicate ItemName against other items
            if (!string.IsNullOrWhiteSpace(entity.ItemName))
            {
                var trimmedName = entity.ItemName.Trim();
                var existsName = await db.ItemMasters.AnyAsync(x => x.ItemId != entity.ItemId && x.ItemName == trimmedName);
                if (existsName) return new OutputResponse(0, "Item Name already exists.");
            }

            // 3. Check duplicate Barcode within entity's own multi-units
            var multiUnitBarcodes = entity.MultiUnits?
                .Where(mu => !string.IsNullOrWhiteSpace(mu.Barcode))
                .Select(mu => mu.Barcode!.Trim())
                .ToList();

            if (multiUnitBarcodes != null && multiUnitBarcodes.Count != multiUnitBarcodes.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                return new OutputResponse(0, "DuplicateBarcodeInUnits");
            }

            // 4. Check duplicate Barcode across database (excluding current item and its own multi-units)
            var allBarcodes = new List<string>();
            if (!string.IsNullOrWhiteSpace(entity.Barcode))
                allBarcodes.Add(entity.Barcode.Trim());

            if (multiUnitBarcodes != null)
                allBarcodes.AddRange(multiUnitBarcodes);

            var distinctBarcodes = allBarcodes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (distinctBarcodes.Count > 0)
            {
                var barcodeExists = await db.ItemMasters.AnyAsync(x => x.ItemId != entity.ItemId && !string.IsNullOrEmpty(x.Barcode) && distinctBarcodes.Contains(x.Barcode)) ||
                                    await db.ItemMultiUnits.AnyAsync(x => x.FK_ItemId != entity.ItemId && !string.IsNullOrEmpty(x.Barcode) && distinctBarcodes.Contains(x.Barcode));
                if (barcodeExists) return new OutputResponse(0, "Barcode already exists.");
            }

            // Update parent properties
            db.Entry(existing).CurrentValues.SetValues(entity);

            // Update children (MultiUnits)
            // 1. Delete items not in new list
            foreach (var existingUnit in existing.MultiUnits.ToList())
            {
                if (!entity.MultiUnits.Any(c => c.MultiUnitId == existingUnit.MultiUnitId && c.MultiUnitId != 0))
                {
                    db.ItemMultiUnits.Remove(existingUnit);
                }
            }

            // 2. Add or Update items
            foreach (var unit in entity.MultiUnits)
            {
                if (unit.MultiUnitId == 0)
                {
                    // New unit
                    existing.MultiUnits.Add(unit);
                }
                else
                {
                    // Update existing unit
                    var existingUnit = existing.MultiUnits.FirstOrDefault(u => u.MultiUnitId == unit.MultiUnitId);
                    if (existingUnit != null)
                    {
                        db.Entry(existingUnit).CurrentValues.SetValues(unit);
                    }
                }
            }

            await db.SaveChangesAsync();
            return new OutputResponse(entity.ItemId, "Updated Successfully");
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<ItemMultiUnit> GetItemMultiUnit(int ItemId, int UnitId, string Barcode)
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.ItemMultiUnits.AsNoTracking()
             .FirstOrDefaultAsync(x => string.IsNullOrEmpty(Barcode) ? x.FK_ItemId == ItemId && x.FK_UnitId == UnitId : x.Barcode == Barcode);
    }

    public async Task<ItemMultiUnitPDTO> GetItemMultiUnitPDTO(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();

        var query = from mu in db.ItemMultiUnits.AsNoTracking()
                    join item in db.ItemMasters.AsNoTracking() on mu.FK_ItemId equals item.ItemId
                    join cat in db.CategoryMasters.AsNoTracking() on item.FK_CategoryId equals cat.CategoryId into catGroup
                    from cat in catGroup.DefaultIfEmpty()
                    join unit in db.UnitMasters.AsNoTracking() on mu.FK_UnitId equals unit.UnitId into unitGroup
                    from unit in unitGroup.DefaultIfEmpty()
                    select new { mu, item, cat, unit };

        if (pagination.FilterId.HasValue && pagination.FilterId.Value > 0)
        {
            query = query.Where(x => x.item.FK_CategoryId == pagination.FilterId.Value);
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            if (pagination.sortColumn == "Base")
            {
                query = query.Where(x => x.mu.QTYPerUnit == 1 || x.mu.QTYPerUnit == null);
            }
            else if (pagination.sortColumn == "Multi")
            {
                query = query.Where(x => x.mu.QTYPerUnit > 1);
            }
        }

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.Trim();
            query = query.Where(x => x.mu.MultiUnitName.Contains(term) 
                || x.mu.Barcode.Contains(term) 
                || x.mu.MultiUnitArabicName.Contains(term)
                || (x.item.ItemCode != null && x.item.ItemCode.StartsWith(term))
                || (x.item.ItemName != null && x.item.ItemName.Contains(term))
                || (x.item.Barcode != null && x.item.Barcode.Contains(term)));
        }

        int count = await query.CountAsync();

        var items = await query
           .OrderBy(x => x.item.ItemName)
           .ThenBy(x => x.mu.QTYPerUnit)
           .Skip((pagination.page - 1) * pagination.pageSize)
           .Take(pagination.pageSize)
           .Select(x => new ItemMultiUnit
           {
               MultiUnitId = x.mu.MultiUnitId,
               FK_ItemId = x.mu.FK_ItemId,
               FK_UnitId = x.mu.FK_UnitId,
               QTYPerUnit = x.mu.QTYPerUnit,
               Rate = x.mu.Rate ?? x.item.SalesRate,
               IncRate = x.mu.IncRate ?? x.item.InclusiveRate,
               Barcode = !string.IsNullOrWhiteSpace(x.mu.Barcode) ? x.mu.Barcode : x.item.Barcode,
               Cost = x.mu.Cost ?? x.item.PurchaseRate ?? x.item.StandardCost,
               MultiUnitName = x.mu.MultiUnitName,
               MultiUnitArabicName = x.mu.MultiUnitArabicName,
               ItemCode = x.item.ItemCode,
               ItemName = x.item.ItemName,
               ItemNameArabic = x.item.ItemNameArabic,
               CategoryName = x.cat != null ? x.cat.Name : null,
               UnitName = x.unit != null ? x.unit.Name : (!string.IsNullOrWhiteSpace(x.mu.MultiUnitName) ? x.mu.MultiUnitName : null),
               ItemMaster = x.item
           })
           .ToListAsync();

        return new ItemMultiUnitPDTO(items, count);
    }

    /// <summary>
    /// Gets the next unique item code based on category/subcategory settings
    /// </summary>
    public async Task<string> GetNextItemCodeAsync(int? categoryId, int? subCategoryId, string? catCode, string? subCatCode)
    {
        await using var db = contextProvider.CreateDbContext();
        return await codeService.GetNextItemCodeAsync(categoryId, subCategoryId, catCode, subCatCode, db);
    }

    /// <summary>
    /// Gets items for ItemSearchDialog with current stock from ItemStockBalance (in base unit)
    /// </summary>


    public async Task<(List<ItemDialogDTO>, int)> GetItemsForDialogWithStock(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = from item in db.ItemMasters
                    join stock in db.ItemStockBalances
                        on new { ItemId = item.ItemId, BranchId = pagination.BranchId }
                        equals new { ItemId = stock.FK_ItemId, BranchId = stock.FK_BranchId }
                        into stockGroup
                    from stock in stockGroup.DefaultIfEmpty()
                    select new ItemDialogDTO
                    {
                        ItemId = item.ItemId,
                        Barcode = item.Barcode,
                        ItemCode = item.ItemCode,
                        ItemName = item.ItemName,
                        ItemNameArabic = item.ItemNameArabic,
                        SalesRate = item.SalesRate,
                        PurchaseRate = item.PurchaseRate ?? item.StandardCost,
                        StockMode = item.StockMode,
                        FK_UnitId = item.FK_UnitId,
                        InclusiveRate = item.InclusiveRate,
                        StandardCost = item.StandardCost,
                        CurrentStock = stock != null ? stock.CurrentQty : 0
                    };

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            query = query.Where(x => x.ItemName.Contains(pagination.search)
                || x.ItemCode.Contains(pagination.search)
                || x.Barcode.Contains(pagination.search));
        }

        int count = await query.CountAsync();
        query = query.OrderBy(x => x.ItemName);

        var items = await query
           .Skip((pagination.page - 1) * pagination.pageSize)
           .Take(pagination.pageSize)
           .ToListAsync();

        return (items, count);
    }


    /// <summary>
    /// Gets multi-unit items for ProductSearchDialog with current stock from ItemStockBalance
    /// Stock is converted to each unit based on QTYPerUnit (e.g., 4 pcs / 4 pcs per box = 1 box)
    /// </summary>
    public async Task<(List<ItemMultiUnitDialogDTO>, int)> GetMultiUnitItemsForDialogWithStock(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = from multiUnit in db.ItemMultiUnits                    
                    select new ItemMultiUnitDialogDTO
                    {
                        MultiUnitId = multiUnit.MultiUnitId,
                        FK_ItemId = multiUnit.FK_ItemId,
                        FK_UnitId = multiUnit.FK_UnitId,
                        QTYPerUnit = multiUnit.QTYPerUnit,
                        Rate = multiUnit.Rate,
                        IncRate = multiUnit.IncRate,
                        Barcode = multiUnit.Barcode,
                        Cost = multiUnit.Cost,
                        MultiUnitName = multiUnit.MultiUnitName,
                        MultiUnitArabicName = multiUnit.MultiUnitArabicName
                    };

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            query = query.Where(x => x.MultiUnitName.Contains(pagination.search)
                || x.Barcode.Contains(pagination.search)
                || x.MultiUnitArabicName.Contains(pagination.search));
        }

        int count = await query.CountAsync();

        var items = await query
           .Skip((pagination.page - 1) * pagination.pageSize)
           .Take(pagination.pageSize)
           .ToListAsync();

        return (items, count);
    }

    /// <summary>
    /// Gets all ItemMultiUnit records for a specific item
    /// </summary>
    public async Task<List<ItemMultiUnit>> GetItemMultiUnits(int itemId)
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.ItemMultiUnits
            .Include(x => x.Unit)
            .AsNoTracking()
            .Where(x => x.FK_ItemId == itemId)
            .ToListAsync();
    }

    public async Task<ZeroTaxReason> GetZeroTaxReason(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        var result = await db.ItemMasters.Where(x => x.ItemId == id).Select(x => new
        {
            x.VatCategory,
            x.VatExcCode,
            x.VatExcName
        }).FirstOrDefaultAsync();
        return new ZeroTaxReason { ZatcaTypeCode = result.VatCategory, ExemptionCode = result.VatExcCode, ExemptionTextEN = result.VatExcName };
    }

    public async Task<ItemStockInfoDTO> GetItemStockInfoAsync(int itemId)
    {
        await using var db = contextProvider.CreateDbContext();

        var openingStock = await db.OpeningStockEntryItemForUpdate
            .Where(x => x.FK_ItemId == itemId)
            .SumAsync(x => (decimal?)x.Qty) ?? 0;


        var currentStock = await db.StockTranLogs
            .Where(x => x.FK_ItemId == itemId)
            .SumAsync(x => (decimal?)((x.InQuantity ?? 0) - (x.OutQuantity ?? 0))) ?? 0;

        var storeLogs = await db.StockTranLogs
            .Include(x => x.Store)
            .Include(x => x.Branch)
            .Where(x => x.FK_ItemId == itemId && x.FK_StoreId != null)
            .Select(x => new { x.FK_StoreId, StoreName = x.Store!.Name, BranchName = x.Branch!.Name, x.InQuantity, x.OutQuantity })
            .ToListAsync();

        var storeWiseStock = storeLogs
            .GroupBy(x => new { x.FK_StoreId, x.StoreName, x.BranchName })
            .Select(g => new StoreStockDTO(
                g.Key.StoreName ?? "",
                g.Key.BranchName ?? "",
                g.Sum(x => (x.InQuantity ?? 0) - (x.OutQuantity ?? 0))
            ))
            .Where(x => x.Quantity != 0)
            .ToList();

        return new ItemStockInfoDTO(openingStock, currentStock, storeWiseStock);
    }

    public async Task<List<BarcodeData>> GetBarcodeItemsAsync(int? categoryId = null)
    {
        await using var db = contextProvider.CreateDbContext();

        var query = db.ItemMasters
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Unit)
            .Include(x => x.MultiUnits).ThenInclude(m => m.Unit)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(x => x.FK_CategoryId == categoryId.Value);
        }

        var items = await query.ToListAsync();
        var result = new List<BarcodeData>();

        foreach (var item in items)
        {
            // 1. Primary/Base Barcode
            if (!string.IsNullOrWhiteSpace(item.Barcode))
            {
                result.Add(new BarcodeData
                {
                    ItemId = item.ItemId,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    ItemNameArabic = item.ItemNameArabic,
                    Barcode = item.Barcode.Trim(),
                    SalesRate = item.SalesRate,
                    InclusiveRate = item.InclusiveRate,
                    PurchaseRate = item.PurchaseRate,
                    StandardCost = item.StandardCost,
                    Location = item.Location,
                    StockMode = item.StockMode ?? "Inventory",
                    Model = item.Model,
                    Fitted = item.Fitted,
                    WeightPerCarton = item.WeightPerCarton,
                    LandingCost = item.LandingCost,
                    BrandName = item.Brand?.Name,
                    CategoryName = item.Category?.Name,
                    UnitName = item.Unit?.Name ?? "Base",
                    Quantity = 1,
                    Label = "Base Barcode"
                });
            }

            // 2. MultiUnits Barcodes (MultiBarcode)
            if (item.MultiUnits != null && item.MultiUnits.Count > 0)
            {
                foreach (var mu in item.MultiUnits)
                {
                    if (!string.IsNullOrWhiteSpace(mu.Barcode))
                    {
                        var trimmedMuBarcode = mu.Barcode.Trim();
                        // If base already added with the exact same barcode and unit, avoid duplicate
                        bool isDuplicateOfBase = !string.IsNullOrWhiteSpace(item.Barcode) &&
                            string.Equals(item.Barcode.Trim(), trimmedMuBarcode, StringComparison.OrdinalIgnoreCase) &&
                            (mu.QTYPerUnit == 1 || mu.QTYPerUnit == null);

                        if (!isDuplicateOfBase)
                        {
                            result.Add(new BarcodeData
                            {
                                ItemId = item.ItemId,
                                ItemCode = item.ItemCode,
                                ItemName = item.ItemName,
                                ItemNameArabic = item.ItemNameArabic,
                                Barcode = trimmedMuBarcode,
                                SalesRate = mu.Rate ?? item.SalesRate,
                                InclusiveRate = mu.IncRate ?? item.InclusiveRate,
                                PurchaseRate = mu.Cost ?? item.PurchaseRate,
                                StandardCost = item.StandardCost,
                                Location = item.Location,
                                StockMode = item.StockMode ?? "Inventory",
                                Model = item.Model,
                                Fitted = item.Fitted,
                                WeightPerCarton = item.WeightPerCarton,
                                LandingCost = item.LandingCost,
                                BrandName = item.Brand?.Name,
                                CategoryName = item.Category?.Name,
                                UnitName = !string.IsNullOrWhiteSpace(mu.MultiUnitName) ? mu.MultiUnitName : (mu.Unit?.Name ?? "MultiUnit"),
                                Quantity = 1,
                                Label = "MultiBarcode"
                            });
                        }
                    }
                }
            }
        }

        return result;
    }
}


