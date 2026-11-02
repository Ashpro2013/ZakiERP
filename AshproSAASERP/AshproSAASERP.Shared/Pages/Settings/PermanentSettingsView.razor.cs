namespace AshproSAASERP.Shared.Pages.Settings;

using AshproSAASERP.Models.Settings;
public partial class PermanentSettingsView
{
    [Parameter] public EventCallback OnClose { get; set; }

    private SettingsModel settings = new();
    private List<PermanentSettings> dbSettings = new();
    private bool isDirty = false;
    private bool menuConfigChanged = false;
    private bool showCloseConfirmation = false;

    // Account Lists
    private List<AccountMaster> cashBankAccounts = new();

    // Printer Lists (Windows Desktop)
    private List<string> installedPrinters = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
        await LoadMenuConfiguration();
        await LoadCashBankAccounts();
        if (AppData.IsWindows)
        {
            await LoadInstalledPrinters();
        }
        if (AppData.IsWindows && string.IsNullOrWhiteSpace(settings.BackupFolder))
        {
            try
            {
                settings.BackupFolder = FormFactor.GetMydocumentsPath();
            }
            catch
            {
                // Ignore if not supported on the current platform
            }
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadInstalledPrinters()
    {
        try
        {
            installedPrinters = FormFactor.GetInstalledPrinters() ?? new();

            if (!string.IsNullOrWhiteSpace(settings.BarcodePrinterName) && !installedPrinters.Contains(settings.BarcodePrinterName))
            {
                installedPrinters.Insert(0, settings.BarcodePrinterName);
            }
            if (!string.IsNullOrWhiteSpace(settings.InvoicePrinterName) && !installedPrinters.Contains(settings.InvoicePrinterName))
            {
                installedPrinters.Insert(0, settings.InvoicePrinterName);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading printers: {ex.Message}", Severity.Warning);
        }
    }

    private async Task RefreshPrinters()
    {
        await LoadInstalledPrinters();
        Snackbar.Add("Printers reloaded successfully", Severity.Info);
        StateHasChanged();
    }

    private async Task LoadCashBankAccounts()
    {
        try
        {
            cashBankAccounts = await api.GetCashAndBankAccounts();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading accounts: {ex.Message}", Severity.Warning);
        }
    }

    #region Menu Configuration
    private List<AshproSAASERP.Models.Settings.ERPType> erpTypes = new();
    private AshproSAASERP.Models.Settings.ERPType? currentERPType;
    private AshproSAASERP.Models.Settings.MenuVisibilityDTO? selectedMenuConfig;
    private int currentERPTypeId = 1;
    private int selectedERPTypeId = 1;
    private bool menuConfigLoading = false;

    private async Task LoadMenuConfiguration()
    {
        try
        {
            menuConfigLoading = true;
            StateHasChanged();

            // Load ERP types
            erpTypes = await api.GetERPTypes();

            // Load current configuration
            var config = await api.GetMenuConfiguration();
            currentERPTypeId = config.ERPTypeId;
            selectedERPTypeId = config.ERPTypeId;
            selectedMenuConfig = config;
            currentERPType = erpTypes.FirstOrDefault(t => t.Id == currentERPTypeId);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading menu configuration: {ex.Message}", Severity.Error);
        }
        finally
        {
            menuConfigLoading = false;
            StateHasChanged();
        }
    }

    private async Task ReloadMenuConfiguration()
    {
        selectedERPTypeId = currentERPTypeId;
        await LoadMenuConfiguration();
    }

    private async Task ApplyERPTypePreset()
    {
        try
        {
            menuConfigLoading = true;
            Snackbar.Add("Applying business type configuration...", Severity.Info);

            var result = await api.UpdateClientERPType(selectedERPTypeId, AppData.UserName ?? "System");

            if (result.Id == 0)
            {
                Snackbar.Add(result.Message ?? "Business type updated successfully!", Severity.Success);
                await LoadMenuConfiguration();
                MarkMenuDirty(); // Flag that menu changed and enable Save button
            }
            else
            {
                Snackbar.Add(result.Message ?? "Failed to update business type", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error applying preset: {ex.Message}", Severity.Error);
        }
        finally
        {
            menuConfigLoading = false;
        }
    }



    private List<AshproSAASERP.Models.Settings.MenuVisibilityItem> GetTopLevelMenus()
    {
        if (selectedMenuConfig == null) return new List<AshproSAASERP.Models.Settings.MenuVisibilityItem>();

        // Exclude Dashboard (single item), but keep Settings as it will have sub-items later
        return selectedMenuConfig.MenuItems
            .Where(m => (m.ParentId == null || m.ParentId == 0) &&
                        m.Name != "Dashboard")
            .OrderBy(m => m.Position)
            .ToList();
    }

    private List<AshproSAASERP.Models.Settings.MenuVisibilityItem> GetChildMenus(int parentId)
    {
        if (selectedMenuConfig == null) return new List<AshproSAASERP.Models.Settings.MenuVisibilityItem>();
        return selectedMenuConfig.MenuItems.Where(m => m.ParentId == parentId).OrderBy(m => m.Position).ToList();
    }
    #endregion


    private void MarkDirty()
    {
        isDirty = true;
    }

    private void MarkMenuDirty()
    {
        isDirty = true;
        menuConfigChanged = true;
    }

    private async Task LoadSettings()
    {
        try
        {
            dbSettings = await api.GetAllPermanentSettings();

            // Item Code Settings
            settings.AutoItemCode = GetBool("AutoItemCode", false);
            settings.ItemCodePrefix = GetValue("ItemCodePrefix", "IT");
            settings.ItemCodeStartNumber = GetInt("ItemCodeStartNumber", 1);
            settings.ItemCodeZeroPadding = GetInt("ItemCodeZeroPadding", 3);
            settings.CategoryWiseItemCode = GetBool("CategoryWiseItemCode", false);
            settings.SubCategoryWiseItemCode = GetBool("SubCategoryWiseItemCode", false);
            settings.CodeSeparator = GetValue("CodeSeparator", "-");

            // Customer Code Settings
            settings.AutoCustomerCode = GetBool("AutoCustomerCode", false);
            settings.CustomerCodePrefix = GetValue("CustomerCodePrefix", "CUS");
            settings.CustomerCodeStartNumber = GetInt("CustomerCodeStartNumber", 1);
            settings.CustomerCodeZeroPadding = GetInt("CustomerCodeZeroPadding", 3);

            // Supplier Code Settings
            settings.AutoSupplierCode = GetBool("AutoSupplierCode", false);
            settings.SupplierCodePrefix = GetValue("SupplierCodePrefix", "SUP");
            settings.SupplierCodeStartNumber = GetInt("SupplierCodeStartNumber", 1);
            settings.SupplierCodeZeroPadding = GetInt("SupplierCodeZeroPadding", 3);

            // Category Code Settings
            settings.AutoCategoryCode = GetBool("AutoCategoryCode", false);
            settings.CategoryCodePrefix = GetValue("CategoryCodePrefix", "CAT");
            settings.CategoryCodeStartNumber = GetInt("CategoryCodeStartNumber", 1);
            settings.CategoryCodeZeroPadding = GetInt("CategoryCodeZeroPadding", 2);

            // SubCategory Code Settings
            settings.AutoSubCategoryCode = GetBool("AutoSubCategoryCode", false);
            settings.SubCategoryCodePrefix = GetValue("SubCategoryCodePrefix", "SUB");
            settings.SubCategoryCodeStartNumber = GetInt("SubCategoryCodeStartNumber", 1);
            settings.SubCategoryCodeZeroPadding = GetInt("SubCategoryCodeZeroPadding", 2);
            settings.CategoryWiseSubCategoryCode = GetBool("CategoryWiseSubCategoryCode", false);

            // Transaction Settings
            settings.AutoQuotationNo = GetBool("AutoQuotationNo", false);
            settings.QuotationNoPrefix = GetValue("QuotationNoPrefix", "QT");
            settings.QuotationNoStartNumber = GetInt("QuotationNoStartNumber", 1);
            settings.QuotationNoZeroPadding = GetInt("QuotationNoZeroPadding", 4);

            settings.AutoSalesOrderNo = GetBool("AutoSalesOrderNo", false);
            settings.SalesOrderNoPrefix = GetValue("SalesOrderNoPrefix", "SO");
            settings.SalesOrderNoStartNumber = GetInt("SalesOrderNoStartNumber", 1);
            settings.SalesOrderNoZeroPadding = GetInt("SalesOrderNoZeroPadding", 4);

            settings.AutoDeliveryNoteNo = GetBool("AutoDeliveryNoteNo", false);
            settings.DeliveryNoteNoPrefix = GetValue("DeliveryNoteNoPrefix", "DN");
            settings.DeliveryNoteNoStartNumber = GetInt("DeliveryNoteNoStartNumber", 1);
            settings.DeliveryNoteNoZeroPadding = GetInt("DeliveryNoteNoZeroPadding", 4);
            settings.DeliveryNoteStockAffect = GetBool("DeliveryNoteStockAffect", false);

            settings.AutoPurchaseOrderNo = GetBool("AutoPurchaseOrderNo", false);
            settings.PurchaseOrderNoPrefix = GetValue("PurchaseOrderNoPrefix", "PO");
            settings.PurchaseOrderNoStartNumber = GetInt("PurchaseOrderNoStartNumber", 1);
            settings.PurchaseOrderNoZeroPadding = GetInt("PurchaseOrderNoZeroPadding", 4);

            settings.AutoProformaInvoiceNo = GetBool("AutoProformaInvoiceNo", false);
            settings.ProformaInvoiceNoPrefix = GetValue("ProformaInvoiceNoPrefix", "PI");
            settings.ProformaInvoiceNoStartNumber = GetInt("ProformaInvoiceNoStartNumber", 1);
            settings.ProformaInvoiceNoZeroPadding = GetInt("ProformaInvoiceNoZeroPadding", 4);

            settings.AutoPurchaseNo = GetBool("AutoPurchaseNo", false);
            settings.PurchaseNoPrefix = GetValue("PurchaseNoPrefix", "PUR");
            settings.PurchaseNoStartNumber = GetInt("PurchaseNoStartNumber", 1);
            settings.PurchaseNoZeroPadding = GetInt("PurchaseNoZeroPadding", 4);

            settings.AutoJournalEntryNo = GetBool("AutoJournalEntryNo", true);
            settings.JournalEntryNoPrefix = GetValue("JournalEntryNoPrefix", "JV");
            settings.JournalEntryNoStartNumber = GetInt("JournalEntryNoStartNumber", 1);
            settings.JournalEntryNoZeroPadding = GetInt("JournalEntryNoZeroPadding", 4);

            settings.AutoStockAdjustmentNo = GetBool("AutoStockAdjustmentNo", false);
            settings.StockAdjustmentNoPrefix = GetValue("StockAdjustmentNoPrefix", "SA");
            settings.StockAdjustmentNoStartNumber = GetInt("StockAdjustmentNoStartNumber", 1);
            settings.StockAdjustmentNoZeroPadding = GetInt("StockAdjustmentNoZeroPadding", 4);

            settings.AutoMaterialTransferRequestNo = GetBool("AutoMaterialTransferRequestNo", false);
            settings.MaterialTransferRequestNoPrefix = GetValue("MaterialTransferRequestNoPrefix", "MTR");
            settings.MaterialTransferRequestNoStartNumber = GetInt("MaterialTransferRequestNoStartNumber", 1);
            settings.MaterialTransferRequestNoZeroPadding = GetInt("MaterialTransferRequestNoZeroPadding", 4);

            settings.AutoMaterialTransferInNo = GetBool("AutoMaterialTransferInNo", false);
            settings.MaterialTransferInNoPrefix = GetValue("MaterialTransferInNoPrefix", "MTI");
            settings.MaterialTransferInNoStartNumber = GetInt("MaterialTransferInNoStartNumber", 1);
            settings.MaterialTransferInNoZeroPadding = GetInt("MaterialTransferInNoZeroPadding", 4);

            settings.AutoMaterialTransferOutNo = GetBool("AutoMaterialTransferOutNo", false);
            settings.MaterialTransferOutNoPrefix = GetValue("MaterialTransferOutNoPrefix", "MTO");
            settings.MaterialTransferOutNoStartNumber = GetInt("MaterialTransferOutNoStartNumber", 1);
            settings.MaterialTransferOutNoZeroPadding = GetInt("MaterialTransferOutNoZeroPadding", 4);

            // Goods Receipt
            settings.AutoGoodsReceiptNo = GetBool("AutoGoodsReceiptNo", false);
            settings.GoodsReceiptNoPrefix = GetValue("GoodsReceiptNoPrefix", "GRN");
            settings.GoodsReceiptNoStartNumber = GetInt("GoodsReceiptNoStartNumber", 1);
            settings.GoodsReceiptNoZeroPadding = GetInt("GoodsReceiptNoZeroPadding", 4);

            // Workshop Settings
            settings.AutoJobOrderNo = GetBool("AutoJobOrderNo", false);
            settings.JobOrderNoPrefix = GetValue("JobOrderNoPrefix", "JO");
            settings.JobOrderNoStartNumber = GetInt("JobOrderNoStartNumber", 1);
            settings.JobOrderNoZeroPadding = GetInt("JobOrderNoZeroPadding", 4);

            settings.AutoJobCardNo = GetBool("AutoJobCardNo", false);
            settings.JobCardNoPrefix = GetValue("JobCardNoPrefix", "JC");
            settings.JobCardNoStartNumber = GetInt("JobCardNoStartNumber", 1);
            settings.JobCardNoZeroPadding = GetInt("JobCardNoZeroPadding", 4);

            settings.AutoInternalMaterialTransferNo = GetBool("AutoInternalMaterialTransferNo", false);
            settings.InternalMaterialTransferNoPrefix = GetValue("InternalMaterialTransferNoPrefix", "IMT");
            settings.InternalMaterialTransferNoStartNumber = GetInt("InternalMaterialTransferNoStartNumber", 1);
            settings.InternalMaterialTransferNoZeroPadding = GetInt("InternalMaterialTransferNoZeroPadding", 4);

            // Sales Invoice
            settings.AutoSalesInvoiceNo = GetBool("AutoSalesInvoiceNo", false);
            settings.SalesInvoiceNoPrefix = GetValue("SalesInvoiceNoPrefix", "INV");
            settings.SalesInvoiceNoStartNumber = GetInt("SalesInvoiceNoStartNumber", 1);
            settings.SalesInvoiceNoZeroPadding = GetInt("SalesInvoiceNoZeroPadding", 4);

            // Sales Invoice Return
            settings.AutoSalesInvoiceRtnNo = GetBool("AutoSalesInvoiceRtnNo", false);
            settings.SalesInvoiceRtnNoPrefix = GetValue("SalesInvoiceRtnNoPrefix", "SIR");
            settings.SalesInvoiceRtnNoStartNumber = GetInt("SalesInvoiceRtnNoStartNumber", 1);
            settings.SalesInvoiceRtnNoZeroPadding = GetInt("SalesInvoiceRtnNoZeroPadding", 4);

            // Purchase Return
            settings.AutoPurchaseReturnNo = GetBool("AutoPurchaseReturnNo", false);
            settings.PurchaseReturnNoPrefix = GetValue("PurchaseReturnNoPrefix", "PRN");
            settings.PurchaseReturnNoStartNumber = GetInt("PurchaseReturnNoStartNumber", 1);
            settings.PurchaseReturnNoZeroPadding = GetInt("PurchaseReturnNoZeroPadding", 4);

            // Delivery Note Return
            settings.AutoDeliveryNoteReturnNo = GetBool("AutoDeliveryNoteReturnNo", false);
            settings.DeliveryNoteReturnNoPrefix = GetValue("DeliveryNoteReturnNoPrefix", "DNR");
            settings.DeliveryNoteReturnNoStartNumber = GetInt("DeliveryNoteReturnNoStartNumber", 1);
            settings.DeliveryNoteReturnNoZeroPadding = GetInt("DeliveryNoteReturnNoZeroPadding", 4);

            // Damage Stock
            settings.AutoDamageStockNo = GetBool("AutoDamageStockNo", false);
            settings.DamageStockNoPrefix = GetValue("DamageStockNoPrefix", "DS");
            settings.DamageStockNoStartNumber = GetInt("DamageStockNoStartNumber", 1);
            settings.DamageStockNoZeroPadding = GetInt("DamageStockNoZeroPadding", 4);

            // Enquiry Registration
            settings.AutoEnquiryNo = GetBool("AutoEnquiryNo", false);
            settings.EnquiryNoPrefix = GetValue("EnquiryNoPrefix", "ENQ");
            settings.EnquiryNoStartNumber = GetInt("EnquiryNoStartNumber", 1);
            settings.EnquiryNoZeroPadding = GetInt("EnquiryNoZeroPadding", 4);

            // Customer Receipt
            settings.AutoCustomerReceiptNo = GetBool("AutoCustomerReceiptNo", false);
            settings.CustomerReceiptNoPrefix = GetValue("CustomerReceiptNoPrefix", "CR");
            settings.CustomerReceiptNoStartNumber = GetInt("CustomerReceiptNoStartNumber", 1);
            settings.CustomerReceiptNoZeroPadding = GetInt("CustomerReceiptNoZeroPadding", 6);

            // Supplier Payment
            settings.AutoSupplierPaymentNo = GetBool("AutoSupplierPaymentNo", true);
            settings.SupplierPaymentNoPrefix = GetValue("SupplierPaymentNoPrefix", "SP");
            settings.SupplierPaymentNoStartNumber = GetInt("SupplierPaymentNoStartNumber", 1);
            settings.SupplierPaymentNoZeroPadding = GetInt("SupplierPaymentNoZeroPadding", 4);

            // Expense Entry
            settings.AutoExpenseEntryNo = GetBool("AutoExpenseEntryNo", false);
            settings.ExpenseEntryNoPrefix = GetValue("ExpenseEntryNoPrefix", "EXP");
            settings.ExpenseEntryNoStartNumber = GetInt("ExpenseEntryNoStartNumber", 1);
            settings.ExpenseEntryNoZeroPadding = GetInt("ExpenseEntryNoZeroPadding", 4);

            // Account Opening Entry
            settings.AutoAccountOpeningEntryNo = GetBool("AutoAccountOpeningEntryNo", false);
            settings.AccountOpeningEntryNoPrefix = GetValue("AccountOpeningEntryNoPrefix", "AOE");
            settings.AccountOpeningEntryNoStartNumber = GetInt("AccountOpeningEntryNoStartNumber", 1);
            settings.AccountOpeningEntryNoZeroPadding = GetInt("AccountOpeningEntryNoZeroPadding", 4);

            // Default Behavior
            settings.DefaultFocusOn = GetValue("DefaultFocusOn", "ItemCode");
            settings.DefaultBillType = GetValue("DefaultBillType", "Cash");
            settings.AllowNegativeStock = GetBool("AllowNegativeStock", false);
            settings.ShowCostInSales = GetBool("ShowCostInSales", false);
            settings.EnableShiftClosing = GetBool("EnableShiftClosing", false);
            settings.RequireBarcode = GetBool("RequireBarcode", false);
            settings.BlockInvoiceOnCreditDaysExceeded = GetBool("BlockInvoiceOnCreditDaysExceeded", false);
            settings.BlockInvoiceOnCreditLimitExceeded = GetBool("BlockInvoiceOnCreditLimitExceeded", false);
            settings.ItemSearchMode = GetInt("ItemSearchMode", 1);
            settings.CurrencyDecimalPlaces = GetInt("CurrencyDecimalPlaces", 2);
            settings.QuantityDecimalPlaces = GetInt("QuantityDecimalPlaces", 2);

            // Default Accounts
            settings.DefaultCashAccountId = GetInt("DefaultCashAccount", 0);
            settings.DefaultBankAccountId = GetInt("DefaultBankAccount", 0);

            // Printer Settings
            settings.BarcodePrinterName = GetValue("BarcodePrinterName", string.Empty);
            settings.InvoicePrinterName = GetValue("InvoicePrinterName", string.Empty);

            // Database Backup Settings
            settings.BackupWhenClosing = GetBool("BackupWhenClosing", false);
            settings.BackupFolder = GetValue("BackupFolder", string.Empty);

            // Image Storage
            settings.ImageStorageMode = GetInt("ImageStorageMode", 0);
            settings.ImageClientFolder = GetValue("ImageClientFolder", "DefaultClient");
            settings.ImageLocalBasePath = GetValue("ImageLocalBasePath", @"D:\ERPImages");
            settings.ImageAwsRegion = GetValue("ImageAwsRegion", "us-east-1");
            settings.ImageAwsBucket = GetValue("ImageAwsBucket", "erp-images");
            settings.ImageAwsAccessKey = GetValue("ImageAwsAccessKey", string.Empty);
            settings.ImageAwsSecretKey = GetValue("ImageAwsSecretKey", string.Empty);
            settings.IsWebBased = GetBool("IsWebBased", false);

            // Tax Regime & Compliance Settings
            settings.TaxRegime = GetValue("TaxRegime", "Zatca");
            settings.DefaultGstStateCode = GetValue("DefaultGstStateCode", "32");
            settings.DefaultGstStateName = GetValue("DefaultGstStateName", "Kerala");
            settings.EnableGstComposition = GetBool("EnableGstComposition", false);
            settings.EnableUpiPaymentQr = GetBool("EnableUpiPaymentQr", true);
            settings.UpiVpaId = GetValue("UpiVpaId", string.Empty);
            settings.UpiPayeeName = GetValue("UpiPayeeName", string.Empty);
            settings.EWayBillThreshold = GetDecimal("EWayBillThreshold", 50000);

            // Zatca Settings
            var pVal = GetValue("ZatcaPhase", "1");
            settings.ZatcaPhase = pVal == "2" ? "2" : "1";
            settings.ZatcaConnection = GetValue("ZatcaConnection", "Simulation");
            settings.UploadSimplifiedXMLRealtime = GetBool("UploadSimplifiedXMLRealtime", true);

            // Transaction Grid Settings
            LoadGridSettings("SalesInvoice", settings.SalesInvoiceSettings);
            LoadGridSettings("SalesOrder", settings.SalesOrderSettings);
            LoadGridSettings("SalesQuotation", settings.SalesQuotationSettings);
            LoadGridSettings("ProformaInvoice", settings.ProformaInvoiceSettings);
            LoadGridSettings("SalesReturn", settings.SalesReturnSettings);
            LoadGridSettings("Purchase", settings.PurchaseSettings);
            LoadGridSettings("DeliveryNote", settings.DeliveryNoteSettings);
            LoadGridSettings("DeliveryNoteReturn", settings.DeliveryNoteReturnSettings);
            LoadGridSettings("PurchaseReturn", settings.PurchaseReturnSettings);
            LoadGridSettings("GoodsReceipt", settings.GoodsReceiptSettings);

            isDirty = false;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading settings: {ex.Message}", Severity.Error);
        }
    }

    private string GetValue(string key, string defaultVal)
    {
        return dbSettings.FirstOrDefault(x => x.SettingKey == key)?.SettingValue ?? defaultVal;
    }

    private bool GetBool(string key, bool defaultVal)
    {
        var value = GetValue(key, defaultVal ? "1" : "0");
        return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private int GetInt(string key, int defaultVal)
    {
        var value = GetValue(key, defaultVal.ToString());
        return int.TryParse(value, out var result) ? result : defaultVal;
    }

    private decimal GetDecimal(string key, decimal defaultVal)
    {
        var value = GetValue(key, defaultVal.ToString());
        return decimal.TryParse(value, out var result) ? result : defaultVal;
    }

    private void OnGstStateCodeChanged(string code)
    {
        settings.DefaultGstStateCode = code;
        var found = AshproSAASERP.Models.Settings.GstState.AllStates.FirstOrDefault(s => s.Code == code);
        if (found != null)
        {
            settings.DefaultGstStateName = found.Name;
        }
        MarkDirty();
    }

    private void SetValue(string key, string value)
    {
        var setting = dbSettings.FirstOrDefault(x => x.SettingKey == key);
        if (setting == null)
        {
            setting = new PermanentSettings { SettingKey = key };
            dbSettings.Add(setting);
        }
        setting.SettingValue = value;
    }

    private async Task Save()
    {
        try
        {
            if (!AshproSAASERP.Shared.Services.AppData.ShowTaxRegimeChange)
            {
                settings.TaxRegime = "Zatca";
            }
            // Item Code Settings
            SetValue("AutoItemCode", settings.AutoItemCode ? "1" : "0");
            SetValue("ItemCodePrefix", settings.ItemCodePrefix ?? "IT");
            SetValue("ItemCodeStartNumber", settings.ItemCodeStartNumber.ToString());
            SetValue("ItemCodeZeroPadding", settings.ItemCodeZeroPadding.ToString());
            SetValue("CategoryWiseItemCode", settings.CategoryWiseItemCode ? "1" : "0");
            SetValue("SubCategoryWiseItemCode", settings.SubCategoryWiseItemCode ? "1" : "0");
            SetValue("CodeSeparator", settings.CodeSeparator ?? "-");

            // Customer Code Settings
            SetValue("AutoCustomerCode", settings.AutoCustomerCode ? "1" : "0");
            SetValue("CustomerCodePrefix", settings.CustomerCodePrefix ?? "CUS");
            SetValue("CustomerCodeStartNumber", settings.CustomerCodeStartNumber.ToString());
            SetValue("CustomerCodeZeroPadding", settings.CustomerCodeZeroPadding.ToString());

            // Supplier Code Settings
            SetValue("AutoSupplierCode", settings.AutoSupplierCode ? "1" : "0");
            SetValue("SupplierCodePrefix", settings.SupplierCodePrefix ?? "SUP");
            SetValue("SupplierCodeStartNumber", settings.SupplierCodeStartNumber.ToString());
            SetValue("SupplierCodeZeroPadding", settings.SupplierCodeZeroPadding.ToString());

            // Category Code Settings
            SetValue("AutoCategoryCode", settings.AutoCategoryCode ? "1" : "0");
            SetValue("CategoryCodePrefix", settings.CategoryCodePrefix ?? "CAT");
            SetValue("CategoryCodeStartNumber", settings.CategoryCodeStartNumber.ToString());
            SetValue("CategoryCodeZeroPadding", settings.CategoryCodeZeroPadding.ToString());

            // SubCategory Code Settings
            SetValue("AutoSubCategoryCode", settings.AutoSubCategoryCode ? "1" : "0");
            SetValue("SubCategoryCodePrefix", settings.SubCategoryCodePrefix ?? "SUB");
            SetValue("SubCategoryCodeStartNumber", settings.SubCategoryCodeStartNumber.ToString());
            SetValue("SubCategoryCodeZeroPadding", settings.SubCategoryCodeZeroPadding.ToString());
            SetValue("CategoryWiseSubCategoryCode", settings.CategoryWiseSubCategoryCode ? "1" : "0");

            // Transaction Settings
            SetValue("AutoQuotationNo", settings.AutoQuotationNo ? "1" : "0");
            SetValue("QuotationNoPrefix", settings.QuotationNoPrefix ?? "QT");
            SetValue("QuotationNoStartNumber", settings.QuotationNoStartNumber.ToString());
            SetValue("QuotationNoZeroPadding", settings.QuotationNoZeroPadding.ToString());

            SetValue("AutoSalesOrderNo", settings.AutoSalesOrderNo ? "1" : "0");
            SetValue("SalesOrderNoPrefix", settings.SalesOrderNoPrefix ?? "SO");
            SetValue("SalesOrderNoStartNumber", settings.SalesOrderNoStartNumber.ToString());
            SetValue("SalesOrderNoZeroPadding", settings.SalesOrderNoZeroPadding.ToString());

            SetValue("AutoDeliveryNoteNo", settings.AutoDeliveryNoteNo ? "1" : "0");
            SetValue("DeliveryNoteNoPrefix", settings.DeliveryNoteNoPrefix ?? "DN");
            SetValue("DeliveryNoteNoStartNumber", settings.DeliveryNoteNoStartNumber.ToString());
            SetValue("DeliveryNoteNoZeroPadding", settings.DeliveryNoteNoZeroPadding.ToString());
            SetValue("DeliveryNoteStockAffect", settings.DeliveryNoteStockAffect ? "1" : "0");

            SetValue("AutoPurchaseOrderNo", settings.AutoPurchaseOrderNo ? "1" : "0");
            SetValue("PurchaseOrderNoPrefix", settings.PurchaseOrderNoPrefix ?? "PO");
            SetValue("PurchaseOrderNoStartNumber", settings.PurchaseOrderNoStartNumber.ToString());
            SetValue("PurchaseOrderNoZeroPadding", settings.PurchaseOrderNoZeroPadding.ToString());

            SetValue("AutoProformaInvoiceNo", settings.AutoProformaInvoiceNo ? "1" : "0");
            SetValue("ProformaInvoiceNoPrefix", settings.ProformaInvoiceNoPrefix ?? "PI");
            SetValue("ProformaInvoiceNoStartNumber", settings.ProformaInvoiceNoStartNumber.ToString());
            SetValue("ProformaInvoiceNoZeroPadding", settings.ProformaInvoiceNoZeroPadding.ToString());

            SetValue("AutoPurchaseNo", settings.AutoPurchaseNo ? "1" : "0");
            SetValue("PurchaseNoPrefix", settings.PurchaseNoPrefix ?? "PUR");
            SetValue("PurchaseNoStartNumber", settings.PurchaseNoStartNumber.ToString());
            SetValue("PurchaseNoZeroPadding", settings.PurchaseNoZeroPadding.ToString());

            SetValue("AutoJournalEntryNo", settings.AutoJournalEntryNo ? "1" : "0");
            SetValue("JournalEntryNoPrefix", settings.JournalEntryNoPrefix ?? "JV");
            SetValue("JournalEntryNoStartNumber", settings.JournalEntryNoStartNumber.ToString());
            SetValue("JournalEntryNoZeroPadding", settings.JournalEntryNoZeroPadding.ToString());

            SetValue("AutoStockAdjustmentNo", settings.AutoStockAdjustmentNo ? "1" : "0");
            SetValue("StockAdjustmentNoPrefix", settings.StockAdjustmentNoPrefix ?? "SA");
            SetValue("StockAdjustmentNoStartNumber", settings.StockAdjustmentNoStartNumber.ToString());
            SetValue("StockAdjustmentNoZeroPadding", settings.StockAdjustmentNoZeroPadding.ToString());

            SetValue("AutoMaterialTransferRequestNo", settings.AutoMaterialTransferRequestNo ? "1" : "0");
            SetValue("MaterialTransferRequestNoPrefix", settings.MaterialTransferRequestNoPrefix ?? "MTR");
            SetValue("MaterialTransferRequestNoStartNumber", settings.MaterialTransferRequestNoStartNumber.ToString());
            SetValue("MaterialTransferRequestNoZeroPadding", settings.MaterialTransferRequestNoZeroPadding.ToString());

            SetValue("AutoMaterialTransferInNo", settings.AutoMaterialTransferInNo ? "1" : "0");
            SetValue("MaterialTransferInNoPrefix", settings.MaterialTransferInNoPrefix ?? "MTI");
            SetValue("MaterialTransferInNoStartNumber", settings.MaterialTransferInNoStartNumber.ToString());
            SetValue("MaterialTransferInNoZeroPadding", settings.MaterialTransferInNoZeroPadding.ToString());

            SetValue("AutoMaterialTransferOutNo", settings.AutoMaterialTransferOutNo ? "1" : "0");
            SetValue("MaterialTransferOutNoPrefix", settings.MaterialTransferOutNoPrefix ?? "MTO");
            SetValue("MaterialTransferOutNoStartNumber", settings.MaterialTransferOutNoStartNumber.ToString());
            SetValue("MaterialTransferOutNoZeroPadding", settings.MaterialTransferOutNoZeroPadding.ToString());

            // Goods Receipt
            SetValue("AutoGoodsReceiptNo", settings.AutoGoodsReceiptNo ? "1" : "0");
            SetValue("GoodsReceiptNoPrefix", settings.GoodsReceiptNoPrefix ?? "GRN");
            SetValue("GoodsReceiptNoStartNumber", settings.GoodsReceiptNoStartNumber.ToString());
            SetValue("GoodsReceiptNoZeroPadding", settings.GoodsReceiptNoZeroPadding.ToString());

            // Workshop Settings
            SetValue("AutoJobOrderNo", settings.AutoJobOrderNo ? "1" : "0");
            SetValue("JobOrderNoPrefix", settings.JobOrderNoPrefix ?? "JO");
            SetValue("JobOrderNoStartNumber", settings.JobOrderNoStartNumber.ToString());
            SetValue("JobOrderNoZeroPadding", settings.JobOrderNoZeroPadding.ToString());

            SetValue("AutoJobCardNo", settings.AutoJobCardNo ? "1" : "0");
            SetValue("JobCardNoPrefix", settings.JobCardNoPrefix ?? "JC");
            SetValue("JobCardNoStartNumber", settings.JobCardNoStartNumber.ToString());
            SetValue("JobCardNoZeroPadding", settings.JobCardNoZeroPadding.ToString());

            SetValue("AutoInternalMaterialTransferNo", settings.AutoInternalMaterialTransferNo ? "1" : "0");
            SetValue("InternalMaterialTransferNoPrefix", settings.InternalMaterialTransferNoPrefix ?? "IMT");
            SetValue("InternalMaterialTransferNoStartNumber", settings.InternalMaterialTransferNoStartNumber.ToString());
            SetValue("InternalMaterialTransferNoZeroPadding", settings.InternalMaterialTransferNoZeroPadding.ToString());

            // Sales Invoice
            SetValue("AutoSalesInvoiceNo", settings.AutoSalesInvoiceNo ? "1" : "0");
            SetValue("SalesInvoiceNoPrefix", settings.SalesInvoiceNoPrefix ?? "INV");
            SetValue("SalesInvoiceNoStartNumber", settings.SalesInvoiceNoStartNumber.ToString());
            SetValue("SalesInvoiceNoZeroPadding", settings.SalesInvoiceNoZeroPadding.ToString());

            // Sales Invoice Return
            SetValue("AutoSalesInvoiceRtnNo", settings.AutoSalesInvoiceRtnNo ? "1" : "0");
            SetValue("SalesInvoiceRtnNoPrefix", settings.SalesInvoiceRtnNoPrefix ?? "SIR");
            SetValue("SalesInvoiceRtnNoStartNumber", settings.SalesInvoiceRtnNoStartNumber.ToString());
            SetValue("SalesInvoiceRtnNoZeroPadding", settings.SalesInvoiceRtnNoZeroPadding.ToString());

            // Purchase Return
            SetValue("AutoPurchaseReturnNo", settings.AutoPurchaseReturnNo ? "1" : "0");
            SetValue("PurchaseReturnNoPrefix", settings.PurchaseReturnNoPrefix ?? "PRN");
            SetValue("PurchaseReturnNoStartNumber", settings.PurchaseReturnNoStartNumber.ToString());
            SetValue("PurchaseReturnNoZeroPadding", settings.PurchaseReturnNoZeroPadding.ToString());

            // Delivery Note Return
            SetValue("AutoDeliveryNoteReturnNo", settings.AutoDeliveryNoteReturnNo ? "1" : "0");
            SetValue("DeliveryNoteReturnNoPrefix", settings.DeliveryNoteReturnNoPrefix ?? "DNR");
            SetValue("DeliveryNoteReturnNoStartNumber", settings.DeliveryNoteReturnNoStartNumber.ToString());
            SetValue("DeliveryNoteReturnNoZeroPadding", settings.DeliveryNoteReturnNoZeroPadding.ToString());

            // Damage Stock
            SetValue("AutoDamageStockNo", settings.AutoDamageStockNo ? "1" : "0");
            SetValue("DamageStockNoPrefix", settings.DamageStockNoPrefix ?? "DS");
            SetValue("DamageStockNoStartNumber", settings.DamageStockNoStartNumber.ToString());
            SetValue("DamageStockNoZeroPadding", settings.DamageStockNoZeroPadding.ToString());

            // Enquiry Registration
            SetValue("AutoEnquiryNo", settings.AutoEnquiryNo ? "1" : "0");
            SetValue("EnquiryNoPrefix", settings.EnquiryNoPrefix ?? "ENQ");
            SetValue("EnquiryNoStartNumber", settings.EnquiryNoStartNumber.ToString());
            SetValue("EnquiryNoZeroPadding", settings.EnquiryNoZeroPadding.ToString());

            // Customer Receipt
            SetValue("AutoCustomerReceiptNo", settings.AutoCustomerReceiptNo ? "1" : "0");
            SetValue("CustomerReceiptNoPrefix", settings.CustomerReceiptNoPrefix ?? "CR");
            SetValue("CustomerReceiptNoStartNumber", settings.CustomerReceiptNoStartNumber.ToString());
            SetValue("CustomerReceiptNoZeroPadding", settings.CustomerReceiptNoZeroPadding.ToString());

            // Supplier Payment
            SetValue("AutoSupplierPaymentNo", settings.AutoSupplierPaymentNo ? "1" : "0");
            SetValue("SupplierPaymentNoPrefix", settings.SupplierPaymentNoPrefix ?? "SP");
            SetValue("SupplierPaymentNoStartNumber", settings.SupplierPaymentNoStartNumber.ToString());
            SetValue("SupplierPaymentNoZeroPadding", settings.SupplierPaymentNoZeroPadding.ToString());

            // Expense Entry
            SetValue("AutoExpenseEntryNo", settings.AutoExpenseEntryNo ? "1" : "0");
            SetValue("ExpenseEntryNoPrefix", settings.ExpenseEntryNoPrefix ?? "EXP");
            SetValue("ExpenseEntryNoStartNumber", settings.ExpenseEntryNoStartNumber.ToString());
            SetValue("ExpenseEntryNoZeroPadding", settings.ExpenseEntryNoZeroPadding.ToString());

            // Account Opening Entry
            SetValue("AutoAccountOpeningEntryNo", settings.AutoAccountOpeningEntryNo ? "1" : "0");
            SetValue("AccountOpeningEntryNoPrefix", settings.AccountOpeningEntryNoPrefix ?? "AOE");
            SetValue("AccountOpeningEntryNoStartNumber", settings.AccountOpeningEntryNoStartNumber.ToString());
            SetValue("AccountOpeningEntryNoZeroPadding", settings.AccountOpeningEntryNoZeroPadding.ToString());

            // Default Behavior
            SetValue("DefaultFocusOn", settings.DefaultFocusOn ?? "ItemCode");
            SetValue("DefaultBillType", settings.DefaultBillType ?? "Cash");
            SetValue("AllowNegativeStock", settings.AllowNegativeStock ? "1" : "0");
            SetValue("ShowCostInSales", settings.ShowCostInSales ? "1" : "0");
            SetValue("EnableShiftClosing", settings.EnableShiftClosing ? "1" : "0");
            SetValue("RequireBarcode", settings.RequireBarcode ? "1" : "0");
            SetValue("BlockInvoiceOnCreditDaysExceeded", settings.BlockInvoiceOnCreditDaysExceeded ? "1" : "0");
            SetValue("BlockInvoiceOnCreditLimitExceeded", settings.BlockInvoiceOnCreditLimitExceeded ? "1" : "0");
            SetValue("ItemSearchMode", settings.ItemSearchMode.ToString());
            SetValue("CurrencyDecimalPlaces", settings.CurrencyDecimalPlaces.ToString());
            SetValue("QuantityDecimalPlaces", settings.QuantityDecimalPlaces.ToString());

            // Default Accounts
            SetValue("DefaultCashAccount", settings.DefaultCashAccountId > 0 ? settings.DefaultCashAccountId.ToString() : string.Empty);
            SetValue("DefaultBankAccount", settings.DefaultBankAccountId > 0 ? settings.DefaultBankAccountId.ToString() : string.Empty);

            // Printer Settings
            SetValue("BarcodePrinterName", settings.BarcodePrinterName ?? string.Empty);
            SetValue("InvoicePrinterName", settings.InvoicePrinterName ?? string.Empty);

            // Database Backup Settings
            SetValue("BackupWhenClosing", settings.BackupWhenClosing ? "1" : "0");
            SetValue("BackupFolder", settings.BackupFolder ?? string.Empty);

            // Image Storage
            SetValue("ImageStorageMode", settings.ImageStorageMode.ToString());
            SetValue("ImageClientFolder", settings.ImageClientFolder ?? "DefaultClient");
            SetValue("ImageLocalBasePath", settings.ImageLocalBasePath ?? @"D:\ERPImages");
            SetValue("ImageAwsRegion", settings.ImageAwsRegion ?? "us-east-1");
            SetValue("ImageAwsBucket", settings.ImageAwsBucket ?? "erp-images");
            SetValue("ImageAwsAccessKey", settings.ImageAwsAccessKey ?? string.Empty);
            SetValue("ImageAwsSecretKey", settings.ImageAwsSecretKey ?? string.Empty);
            SetValue("IsWebBased", settings.IsWebBased ? "1" : "0");

            // Tax Regime & Compliance Settings
            SetValue("TaxRegime", settings.TaxRegime ?? "Zatca");
            SetValue("DefaultGstStateCode", settings.DefaultGstStateCode ?? "32");
            SetValue("DefaultGstStateName", settings.DefaultGstStateName ?? "Kerala");
            SetValue("EnableGstComposition", settings.EnableGstComposition ? "1" : "0");
            SetValue("EnableUpiPaymentQr", settings.EnableUpiPaymentQr ? "1" : "0");
            SetValue("UpiVpaId", settings.UpiVpaId ?? string.Empty);
            SetValue("UpiPayeeName", settings.UpiPayeeName ?? string.Empty);
            SetValue("EWayBillThreshold", settings.EWayBillThreshold.ToString());

            // Zatca Settings
            SetValue("ZatcaPhase", settings.ZatcaPhase ?? "1");
            SetValue("ZatcaConnection", settings.ZatcaConnection ?? "Simulation");
            SetValue("UploadSimplifiedXMLRealtime", settings.UploadSimplifiedXMLRealtime ? "1" : "0");

            // Transaction Grid Settings
            SaveGridSettings("SalesInvoice", settings.SalesInvoiceSettings);
            SaveGridSettings("SalesOrder", settings.SalesOrderSettings);
            SaveGridSettings("SalesQuotation", settings.SalesQuotationSettings);
            SaveGridSettings("ProformaInvoice", settings.ProformaInvoiceSettings);
            SaveGridSettings("SalesReturn", settings.SalesReturnSettings);
            SaveGridSettings("Purchase", settings.PurchaseSettings);
            SaveGridSettings("DeliveryNote", settings.DeliveryNoteSettings);
            SaveGridSettings("DeliveryNoteReturn", settings.DeliveryNoteReturnSettings);
            SaveGridSettings("PurchaseReturn", settings.PurchaseReturnSettings);
            SaveGridSettings("GoodsReceipt", settings.GoodsReceiptSettings);

            // Save to database
            var res = await api.UpdatePermanentSettings(dbSettings);

            if (res.Id > 0)
            {
                // Also save menu configuration if Custom type is selected
                if (selectedMenuConfig is not null && selectedMenuConfig.IsCustomType)
                {
                    var menuResult = await api.SaveCustomMenuVisibility(selectedMenuConfig.MenuItems);
                    if (menuResult.Id != 0)
                    {
                        Snackbar.Add(menuResult.Message ?? "Warning: Menu configuration may not be saved", Severity.Warning);
                    }
                }

                // Reload static settings
                PermanentSettingsData.Load(dbSettings);
                isDirty = false;
                showCloseConfirmation = false;

                if (menuConfigChanged)
                {
                    Snackbar.Add("Settings saved successfully! Please refresh the page to see menu changes.", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Settings saved successfully!", Severity.Success);
                }

                menuConfigChanged = false;

                await OnClose.InvokeAsync();
            }
            else
            {
                Snackbar.Add(res.Message ?? "Failed to save settings", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error saving settings: {ex.Message}", Severity.Error);
        }
    }

    private void HandleClose()
    {
        if (isDirty)
        {
            // Show inline confirmation
            showCloseConfirmation = true;
        }
        else
        {
            OnClose.InvokeAsync();
        }
    }

    private void CancelClose()
    {
        showCloseConfirmation = false;
    }

    private async Task DiscardAndClose()
    {
        showCloseConfirmation = false;
        await OnClose.InvokeAsync();
    }

    private async Task SaveAndClose()
    {
        await Save();
    }

    // Removed HandleKeyDown - Enter key should not close the dialog
    // Users can use Tab to navigate between fields
    // private async Task HandleKeyDown(KeyboardEventArgs e)
    // {
    //     if (e.Key == "Enter" && isDirty)
    //     {
    //         await Save();
    //     }
    // }

    /// <summary>
    /// When CategoryWise is toggled off, also turn off SubCategoryWise
    /// </summary>
    private void OnCategoryWiseChanged()
    {
        if (!settings.CategoryWiseItemCode)
        {
            settings.SubCategoryWiseItemCode = false;
        }
    }
    /// <summary>
    /// When CategoryWise for SubCategory is toggled, mark dirty
    /// </summary>
    private void OnCategoryWiseSubCategoryChanged()
    {
        MarkDirty();
    }

    /// <summary>
    /// Generates preview text showing expected subcategory code format
    /// </summary>
    private string GetSubCategoryCodePreview()
    {
        var prefix = settings.SubCategoryCodePrefix ?? "SUB";
        var padding = settings.SubCategoryCodeZeroPadding;
        var sampleNum = "1".PadLeft(padding, '0');
        var maxNum = new string('9', Math.Max(padding, 3));

        if (settings.CategoryWiseSubCategoryCode)
        {
            // SUB01, SUB02 (separate counter per category, same code can exist across categories)
            return $"{prefix}{sampleNum} → {prefix}{maxNum} (per category)";
        }
        else
        {
            // SUB01, SUB02 (using global prefix)  
            return $"{prefix}{sampleNum} → {prefix}{maxNum} (global)";
        }
    }

    /// <summary>
    /// Generates preview text showing expected item code format
    /// </summary>
    private string GetItemCodePreview()
    {
        var sep = settings.CodeSeparator ?? "-";
        var prefix = settings.ItemCodePrefix ?? "IT";
        var padding = settings.ItemCodeZeroPadding;
        var sampleNum = "1".PadLeft(padding, '0');
        var maxNum = new string('9', Math.Max(padding, 4));

        if (settings.CategoryWiseItemCode && settings.SubCategoryWiseItemCode)
        {
            // CAT01-SUB01-IT001
            return $"CAT01{sep}SUB01{sep}{prefix}{sampleNum} → CAT01{sep}SUB01{sep}{prefix}{maxNum}";
        }
        else if (settings.CategoryWiseItemCode)
        {
            // CAT01-IT001
            return $"CAT01{sep}{prefix}{sampleNum} → CAT01{sep}{prefix}{maxNum}";
        }
        else
        {
            // IT001
            return $"{prefix}{sampleNum} → {prefix}{maxNum}";
        }
    }

    /// <summary>
    /// Generates simple preview text (prefix + number range)
    /// </summary>
    private string GetSimpleCodePreview(string? prefix, int padding)
    {
        prefix ??= "CODE";
        var sampleNum = "1".PadLeft(padding, '0');
        var maxNum = new string('9', Math.Max(padding, 4));
        return $"{prefix}{sampleNum} → {prefix}{maxNum}";
    }


    private string GetMenuIcon(string menuName)
    {
        return menuName switch
        {
            "Masters" => Icons.Material.Filled.Folder,
            "Transactions" => Icons.Material.Filled.ReceiptLong,
            "Settings" => Icons.Material.Filled.Settings,
            "Dashboard" => Icons.Material.Filled.Dashboard,
            _ => Icons.Material.Filled.Menu
        };
    }

    #region Transaction Grid Settings Helpers
    private void LoadGridSettings(string prefix, TransactionGridSettings s)
    {
        s.ShowItemCode = GetBool($"{prefix}_ShowItemCode", false);
        s.ShowBarcode = GetBool($"{prefix}_ShowBarcode", true);
        s.ShowItemName = GetBool($"{prefix}_ShowItemName", true);
        s.ShowArabicName = GetBool($"{prefix}_ShowArabicName", false);
        s.ShowUnit = GetBool($"{prefix}_ShowUnit", true);
        s.ShowExRate = GetBool($"{prefix}_ShowExRate", true);
        s.ShowIncRate = GetBool($"{prefix}_ShowIncRate", false);
        s.ShowGross = GetBool($"{prefix}_ShowGross", true);
        s.ShowDiscPercentage = GetBool($"{prefix}_ShowDiscPercentage", true);
        s.ShowDiscAmount = GetBool($"{prefix}_ShowDiscAmount", true);
        s.ShowTaxableAmount = GetBool($"{prefix}_ShowTaxableAmount", false);
        s.ShowTaxPercentage = GetBool($"{prefix}_ShowTaxPercentage", true);
        s.ShowTaxAmount = GetBool($"{prefix}_ShowTaxAmount", true);
        s.ShowAmount = GetBool($"{prefix}_ShowAmount", true);

        s.AllowEditExcRate = GetBool($"{prefix}_AllowEditExcRate", true);
        s.AllowEditIncRate = GetBool($"{prefix}_AllowEditIncRate", false);
        s.AllowEditDiscPer = GetBool($"{prefix}_AllowEditDiscPer", true);
        s.AllowEditDiscAmount = GetBool($"{prefix}_AllowEditDiscAmount", false);

        s.EnableUpdate = GetBool($"{prefix}_EnableUpdate", false) || GetBool($"Enable{prefix}Update", false);
        s.EnableCancel = GetBool($"{prefix}_EnableCancel", false) || GetBool($"Enable{prefix}Cancel", false);
    }

    private void SaveGridSettings(string prefix, TransactionGridSettings s)
    {
        SetValue($"{prefix}_ShowItemCode", s.ShowItemCode ? "1" : "0");
        SetValue($"{prefix}_ShowBarcode", s.ShowBarcode ? "1" : "0");
        SetValue($"{prefix}_ShowItemName", s.ShowItemName ? "1" : "0");
        SetValue($"{prefix}_ShowArabicName", s.ShowArabicName ? "1" : "0");
        SetValue($"{prefix}_ShowUnit", s.ShowUnit ? "1" : "0");
        SetValue($"{prefix}_ShowExRate", s.ShowExRate ? "1" : "0");
        SetValue($"{prefix}_ShowIncRate", s.ShowIncRate ? "1" : "0");
        SetValue($"{prefix}_ShowGross", s.ShowGross ? "1" : "0");
        SetValue($"{prefix}_ShowDiscPercentage", s.ShowDiscPercentage ? "1" : "0");
        SetValue($"{prefix}_ShowDiscAmount", s.ShowDiscAmount ? "1" : "0");
        SetValue($"{prefix}_ShowTaxableAmount", s.ShowTaxableAmount ? "1" : "0");
        SetValue($"{prefix}_ShowTaxPercentage", s.ShowTaxPercentage ? "1" : "0");
        SetValue($"{prefix}_ShowTaxAmount", s.ShowTaxAmount ? "1" : "0");
        SetValue($"{prefix}_ShowAmount", s.ShowAmount ? "1" : "0");

        SetValue($"{prefix}_AllowEditExcRate", s.AllowEditExcRate ? "1" : "0");
        SetValue($"{prefix}_AllowEditIncRate", s.AllowEditIncRate ? "1" : "0");
        SetValue($"{prefix}_AllowEditDiscPer", s.AllowEditDiscPer ? "1" : "0");
        SetValue($"{prefix}_AllowEditDiscAmount", s.AllowEditDiscAmount ? "1" : "0");

        SetValue($"{prefix}_EnableUpdate", s.EnableUpdate ? "1" : "0");
        SetValue($"{prefix}_EnableCancel", s.EnableCancel ? "1" : "0");
    }

    private void ApplyToAllTransactions(TransactionGridSettings source)
    {
        settings.SalesInvoiceSettings.CopyFrom(source);
        settings.SalesOrderSettings.CopyFrom(source);
        settings.SalesQuotationSettings.CopyFrom(source);
        settings.ProformaInvoiceSettings.CopyFrom(source);
        settings.SalesReturnSettings.CopyFrom(source);
        settings.PurchaseSettings.CopyFrom(source);
        settings.DeliveryNoteSettings.CopyFrom(source);
        settings.DeliveryNoteReturnSettings.CopyFrom(source);
        settings.PurchaseReturnSettings.CopyFrom(source);
        settings.GoodsReceiptSettings.CopyFrom(source);
        MarkDirty();
        Snackbar.Add("Grid settings copied to all transaction types. Click Save to persist.", Severity.Info);
        StateHasChanged();
    }
    #endregion
}


