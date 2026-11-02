namespace ZakiSAASERP.Repo.CodeGenerationServices;

/// <summary>
/// Service to handle auto-generation of codes (Item, Customer, Supplier, etc.)
/// Uses CodeSequences table for state and PermanentSettings table for configuration.
/// </summary>
public class CodeSequenceService(IDbContextProvider contextProvider)
{
    #region Settings Helpers

    private async Task<string?> GetSettingValue(TenantDBContext db, string key)
    {
        var setting = await db.PermanentSettings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey == key);
        return setting?.SettingValue;
    }

    private async Task<bool> GetSettingBool(TenantDBContext db, string key, bool defaultValue = false)
    {
        var val = await GetSettingValue(db, key);
        return val == "1" || (val?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false) ? true : defaultValue;
    }

    private async Task<string> GetSettingString(TenantDBContext db, string key, string defaultValue)
    {
        return await GetSettingValue(db, key) ?? defaultValue;
    }

    private async Task<int> GetSettingInt(TenantDBContext db, string key, int defaultValue)
    {
        var val = await GetSettingValue(db, key);
        return int.TryParse(val, out int i) ? i : defaultValue;
    }

    #endregion

    #region General Code Generation

    /// <summary>
    /// Checks if auto-code generation is enabled for the given key
    /// </summary>
    public async Task<bool> IsAutoCodeEnabledAsync(string key, TenantDBContext db)
    {
        return await IsAutoCodeEnabledAsync(db, key);
    }

    private async Task<bool> IsAutoCodeEnabledAsync(TenantDBContext db, string key)
    {
        return key switch
        {
            CodeKeys.Customer => await GetSettingBool(db, "AutoCustomerCode"),
            CodeKeys.Supplier => await GetSettingBool(db, "AutoSupplierCode"),
            CodeKeys.Category => await GetSettingBool(db, "AutoCategoryCode"),
            CodeKeys.SubCategory => await GetSettingBool(db, "AutoSubCategoryCode"),
            CodeKeys.Quotation => await GetSettingBool(db, "AutoQuotationNo"),
            CodeKeys.SalesOrder => await GetSettingBool(db, "AutoSalesOrderNo"),
            CodeKeys.DeliveryNote => await GetSettingBool(db, "AutoDeliveryNoteNo"),
            CodeKeys.PurchaseOrder => await GetSettingBool(db, "AutoPurchaseOrderNo"),
            CodeKeys.ProformaInvoice => await GetSettingBool(db, "AutoProformaInvoiceNo"),
            CodeKeys.SalesInvoice => await GetSettingBool(db, "AutoSalesInvoiceNo"),
            CodeKeys.SalesInvoiceRtn => await GetSettingBool(db, "AutoSalesInvoiceRtnNo"),
            CodeKeys.Purchase => await GetSettingBool(db, "AutoPurchaseNo"),
            CodeKeys.PurchaseReturn => await GetSettingBool(db, "AutoPurchaseReturnNo"),
            CodeKeys.Item => await GetSettingBool(db, "AutoItemCode"),
            CodeKeys.JournalEntry => await GetSettingBool(db, "AutoJournalEntryNo"),
            CodeKeys.ExpenseEntry => await GetSettingBool(db, "AutoExpenseEntryNo"),
            CodeKeys.SupplierPayment => await GetSettingBool(db, "AutoSupplierPaymentNo", true),
            CodeKeys.CustomerReceipt => await GetSettingBool(db, "AutoCustomerReceiptNo"),
            CodeKeys.StockAdjustment => await GetSettingBool(db, "AutoStockAdjustmentNo"),
            CodeKeys.MaterialTransferRequest => await GetSettingBool(db, "AutoMaterialTransferRequestNo"),
            CodeKeys.MaterialTransferIn => await GetSettingBool(db, "AutoMaterialTransferInNo"),
            CodeKeys.MaterialTransferOut => await GetSettingBool(db, "AutoMaterialTransferOutNo"),
            CodeKeys.JobOrder => await GetSettingBool(db, "AutoJobOrderNo"),
            CodeKeys.JobCard => await GetSettingBool(db, "AutoJobCardNo"),
            CodeKeys.InternalMaterialTransfer => await GetSettingBool(db, "AutoInternalMaterialTransferNo"),
            CodeKeys.GoodsReceipt => await GetSettingBool(db, "AutoGoodsReceiptNo"),
            CodeKeys.DeliveryNoteReturn => await GetSettingBool(db, "AutoDeliveryNoteReturnNo"),
            CodeKeys.DamageStock => await GetSettingBool(db, "AutoDamageStockNo"),
            CodeKeys.Enquiry => await GetSettingBool(db, "AutoEnquiryNo"),
            CodeKeys.AccountOpeningEntry => await GetSettingBool(db, "AutoAccountOpeningEntryNo"),
            _ => false
        };
    }

    /// <summary>
    /// Gets the next unique code for standard entities (Customer, Supplier, etc.)
    /// </summary>
    public async Task<string> GetNextUniqueCodeAsync(string key, Func<string, Task<bool>> checkUnique)
    {
        await using var db = contextProvider.CreateDbContext();
        if (!await IsAutoCodeEnabledAsync(db, key)) return string.Empty;

        // 1. Get configuration
        var (prefix, startNo, padding) = await GetConfigAsync(db, key);

        // 2. Get last used number from DB
        var lastSequence = await db.CodeSequences.FindAsync(key);
        int currentNo = lastSequence?.LastNumber ?? (startNo - 1);

        // 3. Loop to find unique
        int nextNo = currentNo + 1;
        string nextCode;
        bool isUnique;

        do
        {
            nextCode = FormatCode(prefix, nextNo, padding);
            if (string.IsNullOrEmpty(nextCode)) return string.Empty;

            isUnique = !await checkUnique(nextCode);
            if (!isUnique) nextNo++;

        } while (!isUnique);

        return nextCode;
    }

    /// <summary>
    /// Consumes the next number for the given key (updates DB)
    /// </summary>
    public async Task ConsumeCodeAsync(string key, TenantDBContext db) => await ConsumeCodeAsync(db, key);
    private async Task ConsumeCodeAsync(TenantDBContext db, string key)
    {
        if (!await IsAutoCodeEnabledAsync(db, key)) return;

        var sequence = await db.CodeSequences.FindAsync(key);
        if (sequence == null)
        {
            var (_, startNo, _) = await GetConfigAsync(db, key);
            sequence = new CodeSequence { CodeKey = key, LastNumber = startNo };
            await db.CodeSequences.AddAsync(sequence);
        }
        else
        {
            sequence.LastNumber++;
        }
        await db.SaveChangesAsync();
    }

    #endregion

    #region Item Code Generation

    public async Task<string> GetNextItemCodeAsync(int? categoryId, int? subCategoryId, string? catCode, string? subCatCode, TenantDBContext db)
    {
        if (!await GetSettingBool(db, "AutoItemCode")) return string.Empty;
        int startNo = await GetSettingInt(db, "ItemCodeStartNumber", 1);
        int padding = await GetSettingInt(db, "ItemCodeZeroPadding", 3);
        string separator = await GetSettingString(db, "CodeSeparator", "-");
        string itemPrefix = await GetSettingString(db, "ItemCodePrefix", "IT");

        bool useCat = await GetSettingBool(db, "CategoryWiseItemCode") && categoryId.HasValue && !string.IsNullOrEmpty(catCode);
        bool useSubCat = await GetSettingBool(db, "SubCategoryWiseItemCode") && subCategoryId.HasValue && !string.IsNullOrEmpty(subCatCode);

        string key;
        string codePrefix;

        if (useCat)
        {
            key = $"{CodeKeys.Item}_Cat_{categoryId}";
            codePrefix = $"{catCode}{separator}{itemPrefix}";

            if (useSubCat)
            {
                key += $"_Sub_{subCategoryId}";
                codePrefix = $"{catCode}{separator}{subCatCode}{separator}{itemPrefix}";
            }
        }
        else
        {
            // Global mode: manual implementation instead of recursion to keep db context logic clean
            var isEnabled = await IsAutoCodeEnabledAsync(db, CodeKeys.Item);
            if (!isEnabled) return string.Empty;

            // ... duplicated logic from GetNextUniqueCodeAsync but using local db ...
            var (prefix, startNoG, paddingG) = await GetConfigAsync(db, CodeKeys.Item);
            var lastSequence = await db.CodeSequences.FindAsync(CodeKeys.Item);
            int currentNoG = lastSequence?.LastNumber ?? (startNoG - 1);
            int nextNoG = currentNoG + 1;
            string nextCodeG;
            bool isUniqueG;
            do
            {
                nextCodeG = FormatCode(prefix, nextNoG, paddingG);
                isUniqueG = !await db.ItemMasters.AnyAsync(x => x.ItemCode == nextCodeG);
                if (!isUniqueG) nextNoG++;
            } while (!isUniqueG);
            return nextCodeG;
        }

        // Get sequence for this category/subcategory combination
        var sequence = await db.CodeSequences.FindAsync(key);
        int currentNo = sequence?.LastNumber ?? (startNo - 1);
        int nextNo = currentNo + 1;

        string nextCode;
        bool isUnique;

        do
        {
            nextCode = $"{codePrefix}{nextNo.ToString().PadLeft(padding, '0')}";
            isUnique = !await db.ItemMasters.AnyAsync(x => x.ItemCode == nextCode);
            if (!isUnique) nextNo++;

        } while (!isUnique);

        return nextCode;
    }

    public async Task ConsumeItemCodeAsync(int? categoryId, int? subCategoryId, TenantDBContext db)
    {
        if (!await GetSettingBool(db, "AutoItemCode")) return;

        bool useCat = await GetSettingBool(db, "CategoryWiseItemCode") && categoryId.HasValue;
        bool useSubCat = await GetSettingBool(db, "SubCategoryWiseItemCode") && subCategoryId.HasValue;

        string key;

        if (useCat)
        {
            key = $"{CodeKeys.Item}_Cat_{categoryId}";
            if (useSubCat)
            {
                key += $"_Sub_{subCategoryId}";
            }

            var sequence = await db.CodeSequences.FindAsync(key);
            if (sequence == null)
            {
                int startNo = await GetSettingInt(db, "ItemCodeStartNumber", 1);
                sequence = new CodeSequence { CodeKey = key, LastNumber = startNo };
                await db.CodeSequences.AddAsync(sequence);
            }
            else
            {
                sequence.LastNumber++;
            }
            await db.SaveChangesAsync();
        }
        else
        {
            await ConsumeCodeAsync(db, CodeKeys.Item);
        }
    }

    #endregion

    #region SubCategory Code Generation

    public async Task<string> GetNextSubCategoryCodeAsync(int? categoryId, string? categoryCode, TenantDBContext db)
    {
        if (!await GetSettingBool(db, "AutoSubCategoryCode")) return string.Empty;

        bool categoryWise = await GetSettingBool(db, "CategoryWiseSubCategoryCode") && categoryId.HasValue && !string.IsNullOrEmpty(categoryCode);

        if (categoryWise)
        {
            int startNo = await GetSettingInt(db, "SubCategoryCodeStartNumber", 1);
            int padding = await GetSettingInt(db, "SubCategoryCodeZeroPadding", 2);
            string prefix = await GetSettingString(db, "SubCategoryCodePrefix", "SUB");

            string key = $"{CodeKeys.SubCategory}_Cat_{categoryId}";

            var sequence = await db.CodeSequences.FindAsync(key);
            int currentNo = sequence?.LastNumber ?? (startNo - 1);
            int nextNo = currentNo + 1;

            string nextCode;
            bool isUnique;

            do
            {
                nextCode = $"{prefix}{nextNo.ToString().PadLeft(padding, '0')}";
                isUnique = !await db.SubCategoryMasters.AnyAsync(x => x.Code == nextCode && x.FK_CategoryId == categoryId);
                if (!isUnique) nextNo++;

            } while (!isUnique);

            return nextCode;
        }
        else
        {
            // Duplicated logic from GetNextUniqueCodeAsync
            var (prefix, startNo, padding) = await GetConfigAsync(db, CodeKeys.SubCategory);
            var lastSequence = await db.CodeSequences.FindAsync(CodeKeys.SubCategory);
            int currentNo = lastSequence?.LastNumber ?? (startNo - 1);
            int nextNo = currentNo + 1;
            string nextCode;
            bool isUnique;
            do
            {
                nextCode = FormatCode(prefix, nextNo, padding);
                isUnique = !await db.SubCategoryMasters.AnyAsync(x => x.Code == nextCode);
                if (!isUnique) nextNo++;
            } while (!isUnique);
            return nextCode;
        }
    }

    public async Task ConsumeSubCategoryCodeAsync(int? categoryId, TenantDBContext db)
    {
        if (!await GetSettingBool(db, "AutoSubCategoryCode")) return;

        bool categoryWise = await GetSettingBool(db, "CategoryWiseSubCategoryCode") && categoryId.HasValue;

        if (categoryWise)
        {
            string key = $"{CodeKeys.SubCategory}_Cat_{categoryId}";
            var sequence = await db.CodeSequences.FindAsync(key);

            if (sequence == null)
            {
                int startNo = await GetSettingInt(db, "SubCategoryCodeStartNumber", 1);
                sequence = new CodeSequence { CodeKey = key, LastNumber = startNo };
                await db.CodeSequences.AddAsync(sequence);
            }
            else
            {
                sequence.LastNumber++;
            }
            await db.SaveChangesAsync();
        }
        else
        {
            await ConsumeCodeAsync(db, CodeKeys.SubCategory);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<(string Prefix, int StartNo, int Padding)> GetConfigAsync(TenantDBContext db, string key)
    {
        return key switch
        {
            CodeKeys.Customer => (
                await GetSettingString(db, "CustomerCodePrefix", "CUS"),
                await GetSettingInt(db, "CustomerCodeStartNumber", 1),
                await GetSettingInt(db, "CustomerCodeZeroPadding", 3)),

            CodeKeys.Supplier => (
                await GetSettingString(db, "SupplierCodePrefix", "SUP"),
                await GetSettingInt(db, "SupplierCodeStartNumber", 1),
                await GetSettingInt(db, "SupplierCodeZeroPadding", 3)),

            CodeKeys.Category => (
                await GetSettingString(db, "CategoryCodePrefix", "CAT"),
                await GetSettingInt(db, "CategoryCodeStartNumber", 1),
                await GetSettingInt(db, "CategoryCodeZeroPadding", 2)),

            CodeKeys.SubCategory => (
                await GetSettingString(db, "SubCategoryCodePrefix", "SUB"),
                await GetSettingInt(db, "SubCategoryCodeStartNumber", 1),
                await GetSettingInt(db, "SubCategoryCodeZeroPadding", 2)),

            CodeKeys.Quotation => (
                await GetSettingString(db, "QuotationNoPrefix", "QT"),
                await GetSettingInt(db, "QuotationNoStartNumber", 1),
                await GetSettingInt(db, "QuotationNoZeroPadding", 4)),

            CodeKeys.SalesOrder => (
                await GetSettingString(db, "SalesOrderNoPrefix", "SO"),
                await GetSettingInt(db, "SalesOrderNoStartNumber", 1),
                await GetSettingInt(db, "SalesOrderNoZeroPadding", 4)),

            CodeKeys.DeliveryNote => (
                await GetSettingString(db, "DeliveryNoteNoPrefix", "DN"),
                await GetSettingInt(db, "DeliveryNoteNoStartNumber", 1),
                await GetSettingInt(db, "DeliveryNoteNoZeroPadding", 4)),

            CodeKeys.PurchaseOrder => (
                await GetSettingString(db, "PurchaseOrderNoPrefix", "PO"),
                await GetSettingInt(db, "PurchaseOrderNoStartNumber", 1),
                await GetSettingInt(db, "PurchaseOrderNoZeroPadding", 4)),

            CodeKeys.ProformaInvoice => (
                await GetSettingString(db, "ProformaInvoiceNoPrefix", "PI"),
                await GetSettingInt(db, "ProformaInvoiceNoStartNumber", 1),
                await GetSettingInt(db, "ProformaInvoiceNoZeroPadding", 4)),

            CodeKeys.SalesInvoice => (
                await GetSettingString(db, "SalesInvoiceNoPrefix", "SIN"),
                await GetSettingInt(db, "SalesInvoiceNoStartNumber", 1),
                await GetSettingInt(db, "SalesInvoiceNoZeroPadding", 4)),

            CodeKeys.SalesInvoiceRtn => (
                await GetSettingString(db, "SalesInvoiceRtnNoPrefix", "SR"),
                await GetSettingInt(db, "SalesInvoiceRtnNoStartNumber", 1),
                await GetSettingInt(db, "SalesInvoiceRtnNoZeroPadding", 4)),

            CodeKeys.Purchase => (
                await GetSettingString(db, "PurchaseNoPrefix", "P"),
                await GetSettingInt(db, "PurchaseNoStartNumber", 1),
                await GetSettingInt(db, "PurchaseNoZeroPadding", 4)),

            CodeKeys.PurchaseReturn => (
                await GetSettingString(db, "PurchaseReturnNoPrefix", "PR"),
                await GetSettingInt(db, "PurchaseReturnNoStartNumber", 1),
                await GetSettingInt(db, "PurchaseReturnNoZeroPadding", 4)),

            CodeKeys.JournalEntry => (
                await GetSettingString(db, "JournalEntryNoPrefix", "JV"),
                await GetSettingInt(db, "JournalEntryNoStartNumber", 1),
                await GetSettingInt(db, "JournalEntryNoZeroPadding", 4)),

            CodeKeys.ExpenseEntry => (
                await GetSettingString(db, "ExpenseEntryNoPrefix", "EXP"),
                await GetSettingInt(db, "ExpenseEntryNoStartNumber", 1),
                await GetSettingInt(db, "ExpenseEntryNoZeroPadding", 4)),


            CodeKeys.SupplierPayment => (
                await GetSettingString(db, "SupplierPaymentNoPrefix", "SP"),
                await GetSettingInt(db, "SupplierPaymentNoStartNumber", 1),
                await GetSettingInt(db, "SupplierPaymentNoZeroPadding", 4)),


            CodeKeys.CustomerReceipt => (
                await GetSettingString(db, "CustomerReceiptNoPrefix", "CR"),
                await GetSettingInt(db, "CustomerReceiptNoStartNumber", 1),
                await GetSettingInt(db, "CustomerReceiptNoZeroPadding", 6)),

            CodeKeys.StockAdjustment => (
                await GetSettingString(db, "StockAdjustmentNoPrefix", "SA"),
                await GetSettingInt(db, "StockAdjustmentNoStartNumber", 1),
                await GetSettingInt(db, "StockAdjustmentNoZeroPadding", 4)),

            CodeKeys.MaterialTransferRequest => (
                await GetSettingString(db, "MaterialTransferRequestNoPrefix", "MTR"),
                await GetSettingInt(db, "MaterialTransferRequestNoStartNumber", 1),
                await GetSettingInt(db, "MaterialTransferRequestNoZeroPadding", 4)),

            CodeKeys.MaterialTransferIn => (
                await GetSettingString(db, "MaterialTransferInNoPrefix", "MTI"),
                await GetSettingInt(db, "MaterialTransferInNoStartNumber", 1),
                await GetSettingInt(db, "MaterialTransferInNoZeroPadding", 4)),

            CodeKeys.MaterialTransferOut => (
                await GetSettingString(db, "MaterialTransferOutNoPrefix", "MTO"),
                await GetSettingInt(db, "MaterialTransferOutNoStartNumber", 1),
                await GetSettingInt(db, "MaterialTransferOutNoZeroPadding", 4)),

            CodeKeys.JobOrder => (
                await GetSettingString(db, "JobOrderNoPrefix", "JO"),
                await GetSettingInt(db, "JobOrderNoStartNumber", 1),
                await GetSettingInt(db, "JobOrderNoZeroPadding", 4)),

            CodeKeys.JobCard => (
                await GetSettingString(db, "JobCardNoPrefix", "JC"),
                await GetSettingInt(db, "JobCardNoStartNumber", 1),
                await GetSettingInt(db, "JobCardNoZeroPadding", 4)),

            CodeKeys.InternalMaterialTransfer => (
                await GetSettingString(db, "InternalMaterialTransferNoPrefix", "IMT"),
                await GetSettingInt(db, "InternalMaterialTransferNoStartNumber", 1),
                await GetSettingInt(db, "InternalMaterialTransferNoZeroPadding", 4)),

            CodeKeys.GoodsReceipt => (
                await GetSettingString(db, "GoodsReceiptNoPrefix", "GR"),
                await GetSettingInt(db, "GoodsReceiptNoStartNumber", 1),
                await GetSettingInt(db, "GoodsReceiptNoZeroPadding", 4)),

            CodeKeys.DeliveryNoteReturn => (
                await GetSettingString(db, "DeliveryNoteReturnNoPrefix", "DNR"),
                await GetSettingInt(db, "DeliveryNoteReturnNoStartNumber", 1),
                await GetSettingInt(db, "DeliveryNoteReturnNoZeroPadding", 4)),

            CodeKeys.DamageStock => (
                await GetSettingString(db, "DamageStockNoPrefix", "DS"),
                await GetSettingInt(db, "DamageStockNoStartNumber", 1),
                await GetSettingInt(db, "DamageStockNoZeroPadding", 4)),

            CodeKeys.Enquiry => (
                await GetSettingString(db, "EnquiryNoPrefix", "ENQ"),
                await GetSettingInt(db, "EnquiryNoStartNumber", 1),
                await GetSettingInt(db, "EnquiryNoZeroPadding", 4)),

            CodeKeys.AccountOpeningEntry => (
                await GetSettingString(db, "AccountOpeningEntryNoPrefix", "AOE"),
                await GetSettingInt(db, "AccountOpeningEntryNoStartNumber", 1),
                await GetSettingInt(db, "AccountOpeningEntryNoZeroPadding", 4)),

            _ => (string.Empty, 1, 3)
        };
    }

    private string FormatCode(string prefix, int number, int padding)
    {
        return $"{prefix}{number.ToString().PadLeft(padding, '0')}";
    }

    #endregion
}
