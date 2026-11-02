namespace ZakiSAASERP.Models.Services;

using ZakiSAASERP.Models.Settings;

/// <summary>
/// Static class for permanent settings - similar to AppData pattern.
/// Loaded once after login, updated only when settings are changed.
/// Access directly via PermanentSettingsData.AutoItemCode, etc.
/// </summary>
public static class PermanentSettingsData
{
    // ============ Item Code Settings ============
    public static bool AutoItemCode { get; set; } = false;
    public static string ItemCodePrefix { get; set; } = "IT";
    public static int ItemCodeStartNumber { get; set; } = 1;
    public static int ItemCodeZeroPadding { get; set; } = 3;
    public static bool CategoryWiseItemCode { get; set; } = false;
    public static bool SubCategoryWiseItemCode { get; set; } = false;
    public static string CodeSeparator { get; set; } = "-";

    // ============ Customer Code Settings ============
    public static bool AutoCustomerCode { get; set; } = false;
    public static string CustomerCodePrefix { get; set; } = "CUS";
    public static int CustomerCodeStartNumber { get; set; } = 1;
    public static int CustomerCodeZeroPadding { get; set; } = 3;

    // ============ Supplier Code Settings ============
    public static bool AutoSupplierCode { get; set; } = false;
    public static string SupplierCodePrefix { get; set; } = "SUP";
    public static int SupplierCodeStartNumber { get; set; } = 1;
    public static int SupplierCodeZeroPadding { get; set; } = 3;

    // ============ Category Code Settings ============
    public static bool AutoCategoryCode { get; set; } = false;
    public static string CategoryCodePrefix { get; set; } = "CAT";
    public static int CategoryCodeStartNumber { get; set; } = 1;
    public static int CategoryCodeZeroPadding { get; set; } = 2;

    // ============ SubCategory Code Settings ============
    public static bool AutoSubCategoryCode { get; set; } = false;
    public static string SubCategoryCodePrefix { get; set; } = "SUB";
    public static int SubCategoryCodeStartNumber { get; set; } = 1;
    public static int SubCategoryCodeZeroPadding { get; set; } = 2;
    public static bool CategoryWiseSubCategoryCode { get; set; } = false;

    // ============ Transaction Settings ============
    // Quotation
    public static bool AutoQuotationNo { get; set; } = false;
    public static string QuotationNoPrefix { get; set; } = "QT";
    public static int QuotationNoStartNumber { get; set; } = 1;
    public static int QuotationNoZeroPadding { get; set; } = 4;

    // Sales Order
    public static bool AutoSalesOrderNo { get; set; } = false;
    public static string SalesOrderNoPrefix { get; set; } = "SO";
    public static int SalesOrderNoStartNumber { get; set; } = 1;
    public static int SalesOrderNoZeroPadding { get; set; } = 4;

    // Delivery Note
    public static bool AutoDeliveryNoteNo { get; set; } = false;
    public static string DeliveryNoteNoPrefix { get; set; } = "DN";
    public static int DeliveryNoteNoStartNumber { get; set; } = 1;

    public static int DeliveryNoteNoZeroPadding { get; set; } = 4;
    public static bool DeliveryNoteStockAffect { get; set; } = false;

    // Sales Invoice
    public static bool AutoSalesInvoiceNo { get; set; } = false;
    public static string SalesInvoiceNoPrefix { get; set; } = "SIN";
    public static int SalesInvoiceNoStartNumber { get; set; } = 1;
    public static int SalesInvoiceNoZeroPadding { get; set; } = 4;

    // Sales Invoice Return
    public static bool AutoSalesInvoiceRtnNo { get; set; } = false;
    public static string SalesInvoiceRtnNoPrefix { get; set; } = "SR";
    public static int SalesInvoiceRtnNoStartNumber { get; set; } = 1;
    public static int SalesInvoiceRtnNoZeroPadding { get; set; } = 4;

    // Purchase Order
    public static bool AutoPurchaseOrderNo { get; set; } = false;
    public static string PurchaseOrderNoPrefix { get; set; } = "PO";
    public static int PurchaseOrderNoStartNumber { get; set; } = 1;
    public static int PurchaseOrderNoZeroPadding { get; set; } = 4;

    // Transaction: Purchase Invoice
    public static bool AutoPurchaseNo { get; set; } = false;
    public static string PurchaseNoPrefix { get; set; } = "PUR";
    public static int PurchaseNoStartNumber { get; set; } = 1;
    public static int PurchaseNoZeroPadding { get; set; } = 4;

    // Transaction: Proforma Invoice
    public static bool AutoProformaInvoiceNo { get; set; } = false;
    public static string ProformaInvoiceNoPrefix { get; set; } = "PI";
    public static int ProformaInvoiceNoStartNumber { get; set; } = 1;
    public static int ProformaInvoiceNoZeroPadding { get; set; } = 4;

    // Transaction: Purchase Return
    public static bool AutoPurchaseReturnNo { get; set; } = false;
    public static string PurchaseReturnNoPrefix { get; set; } = "PR";
    public static int PurchaseReturnNoStartNumber { get; set; } = 1;
    public static int PurchaseReturnNoZeroPadding { get; set; } = 4;

    // Transaction: Goods Receipt
    public static bool AutoGoodsReceiptNo { get; set; } = false;
    public static string GoodsReceiptNoPrefix { get; set; } = "GRN";
    public static int GoodsReceiptNoStartNumber { get; set; } = 1;
    public static int GoodsReceiptNoZeroPadding { get; set; } = 4;

    // Transaction: Expense Entry
    public static bool AutoExpenseEntryNo { get; set; } = false;
    public static string ExpenseEntryNoPrefix { get; set; } = "EXP";
    public static int ExpenseEntryNoStartNumber { get; set; } = 1;
    public static int ExpenseEntryNoZeroPadding { get; set; } = 4;

    // Transaction: Stock Adjustment
    public static bool AutoStockAdjustmentNo { get; set; } = false;
    public static string StockAdjustmentNoPrefix { get; set; } = "SA";
    public static int StockAdjustmentNoStartNumber { get; set; } = 1;
    public static int StockAdjustmentNoZeroPadding { get; set; } = 4;

    // Transaction: Material Transfer Request
    public static bool AutoMaterialTransferRequestNo { get; set; } = false;
    public static string MaterialTransferRequestNoPrefix { get; set; } = "MTR";
    public static int MaterialTransferRequestNoStartNumber { get; set; } = 1;
    public static int MaterialTransferRequestNoZeroPadding { get; set; } = 4;

    // Transaction: Material Transfer IN
    public static bool AutoMaterialTransferInNo { get; set; } = false;
    public static string MaterialTransferInNoPrefix { get; set; } = "MTI";
    public static int MaterialTransferInNoStartNumber { get; set; } = 1;
    public static int MaterialTransferInNoZeroPadding { get; set; } = 4;

    // Transaction: Material Transfer OUT
    public static bool AutoMaterialTransferOutNo { get; set; } = false;
    public static string MaterialTransferOutNoPrefix { get; set; } = "MTO";
    public static int MaterialTransferOutNoStartNumber { get; set; } = 1;
    public static int MaterialTransferOutNoZeroPadding { get; set; } = 4;

    // Transaction: Job Order
    public static bool AutoJobOrderNo { get; set; } = false;
    public static string JobOrderNoPrefix { get; set; } = "JO";
    public static int JobOrderNoStartNumber { get; set; } = 1;
    public static int JobOrderNoZeroPadding { get; set; } = 4;

    // Transaction: Job Card
    public static bool AutoJobCardNo { get; set; } = false;
    public static string JobCardNoPrefix { get; set; } = "JC";
    public static int JobCardNoStartNumber { get; set; } = 1;
    public static int JobCardNoZeroPadding { get; set; } = 4;

    // Transaction: Internal Material Transfer
    public static bool AutoInternalMaterialTransferNo { get; set; } = false;
    public static string InternalMaterialTransferNoPrefix { get; set; } = "IMT";
    public static int InternalMaterialTransferNoStartNumber { get; set; } = 1;
    public static int InternalMaterialTransferNoZeroPadding { get; set; } = 4;

    public static int ImageStorageMode { get; set; } = 0; // 0=Local, 1=AWS S3
    public static string ImageClientFolder { get; set; } = "DefaultClient";
    public static string ImageLocalBasePath { get; set; } = @"D:\ERPImages";

    // AWS S3 Storage
    public static string ImageAwsRegion { get; set; } = "us-east-1";
    public static string ImageAwsBucket { get; set; } = "erp-images";
    public static string ImageAwsAccessKey { get; set; } = string.Empty;
    public static string ImageAwsSecretKey { get; set; } = string.Empty;

    // ============ Web Settings ============
    public static bool IsWebBased { get; set; } = false;

    // ============ Printer Settings ============
    public static string BarcodePrinterName { get; set; } = string.Empty;
    public static string InvoicePrinterName { get; set; } = string.Empty;

    // ============ Default Behavior ============
    public static string DefaultFocusOn { get; set; } = "ItemCode";
    public static string DefaultBillType { get; set; } = "Cash";
    public static bool AllowNegativeStock { get; set; } = false;
    public static bool ShowCostInSales { get; set; } = false;
    public static bool RequireBarcode { get; set; } = false;
    public static bool BlockInvoiceOnCreditDaysExceeded { get; set; } = false;
    public static bool EnableShiftClosing { get; set; } = false;
    public static int ItemSearchMode { get; set; } = 1;

    // ============ Decimal Places Settings ============
    public static int CurrencyDecimalPlaces { get; set; } = 2;
    public static int QuantityDecimalPlaces { get; set; } = 2;
    public static string CurrencyFormat => $"N{Math.Clamp(CurrencyDecimalPlaces, 0, 4)}";
    public static string QuantityFormat => $"N{Math.Clamp(QuantityDecimalPlaces, 0, 4)}";

    public static string FormatCurrency(decimal? value) => (value ?? 0).ToString(CurrencyFormat);
    public static string FormatCurrency(decimal value) => value.ToString(CurrencyFormat);
    public static string FormatQuantity(decimal? value) => (value ?? 0).ToString(QuantityFormat);
    public static string FormatQuantity(decimal value) => value.ToString(QuantityFormat);

    // ============ Tax Regime Settings ============
    public static string TaxRegime { get; set; } = "Zatca";
    public static bool IsZatca => true;
    public static bool IsIndiaGst => false;
    public static bool IsStandardVat => false;

    // ============ GST Settings (India) ============
    public static string DefaultGstStateCode { get; set; } = "32";
    public static string DefaultGstStateName { get; set; } = "Kerala";
    public static bool EnableGstComposition { get; set; } = false;
    public static bool EnableUpiPaymentQr { get; set; } = true;
    public static string UpiVpaId { get; set; } = string.Empty;
    public static string UpiPayeeName { get; set; } = string.Empty;
    public static decimal EWayBillThreshold { get; set; } = 50000;

    // ============ Zatca Settings ============
    public static int ZatcaPhase { get; set; } = 1;
    public static string ZatcaConnection { get; set; } = "Simulation";
    public static bool UploadSimplifiedXMLRealtime { get; set; } = true;
    public static bool BlockInvoiceOnCreditLimitExceeded { get; set; } = false;

    // ============ Transaction Grid Settings ============
    public static TransactionGridSettings SalesInvoiceSettings { get; set; } = new();
    public static TransactionGridSettings SalesOrderSettings { get; set; } = new();
    public static TransactionGridSettings SalesQuotationSettings { get; set; } = new();
    public static TransactionGridSettings ProformaInvoiceSettings { get; set; } = new();
    public static TransactionGridSettings SalesReturnSettings { get; set; } = new();
    public static TransactionGridSettings PurchaseSettings { get; set; } = new();
    public static TransactionGridSettings DeliveryNoteSettings { get; set; } = new();
    public static TransactionGridSettings DeliveryNoteReturnSettings { get; set; } = new();
    public static TransactionGridSettings PurchaseReturnSettings { get; set; } = new();
    public static TransactionGridSettings GoodsReceiptSettings { get; set; } = new();

    // ============ Sales Invoice & Sales Return Operations ============
    public static bool EnableSalesInvoiceUpdate { get => SalesInvoiceSettings.EnableUpdate; set => SalesInvoiceSettings.EnableUpdate = value; }
    public static bool EnableSalesInvoiceCancel { get => SalesInvoiceSettings.EnableCancel; set => SalesInvoiceSettings.EnableCancel = value; }
    public static bool EnableSalesReturnUpdate { get => SalesReturnSettings.EnableUpdate; set => SalesReturnSettings.EnableUpdate = value; }
    public static bool EnableSalesReturnCancel { get => SalesReturnSettings.EnableCancel; set => SalesReturnSettings.EnableCancel = value; }

    public static TransactionGridSettings GetTransactionSettings(string transactionType)
    {
        return transactionType?.ToLowerInvariant() switch
        {
            "salesinvoice" or "sales_invoice" or "invoice" => SalesInvoiceSettings,
            "salesorder" or "sales_order" or "order" => SalesOrderSettings,
            "salesquotation" or "sales_quotation" or "quotation" or "quote" => SalesQuotationSettings,
            "proformainvoice" or "proforma_invoice" or "proforma" => ProformaInvoiceSettings,
            "salesinvoicertn" or "salesreturn" or "sales_return" or "return" => SalesReturnSettings,
            "purchase" or "purchaseinvoice" => PurchaseSettings,
            "deliverynote" or "delivery_note" or "dn" => DeliveryNoteSettings,
            "deliverynotereturn" or "delivery_note_return" or "deliverynotertn" or "dnr" => DeliveryNoteReturnSettings,
            "purchasereturn" or "purchase_return" or "purchasertn" or "prn" => PurchaseReturnSettings,
            "goodsreceipt" or "goods_receipt" or "grn" => GoodsReceiptSettings,
            _ => SalesInvoiceSettings
        };
    }

    // ============ Default Accounts Settings ============
    public static int DefaultBankAccountId { get; set; } = 0;
    public static int DefaultCashAccountId { get; set; } = 0;

    // ============ Database Backup Settings ============
    public static bool BackupWhenClosing { get; set; } = false;
    public static string BackupFolder { get; set; } = string.Empty;

    // ============ Is Loaded Flag ============
    public static bool IsLoaded { get; private set; } = false;

    /// <summary>
    /// Load settings from database list - call this after login
    /// </summary>
    public static void Load(List<ZakiSAASERP.Models.Masters.PermanentSettings> settings)
    {
        foreach (var s in settings)
        {
            switch (s.SettingKey)
            {
                // Item Code
                case "AutoItemCode": AutoItemCode = IsTrue(s.SettingValue); break;
                case "ItemCodePrefix": ItemCodePrefix = s.SettingValue ?? "IT"; break;
                case "ItemCodeStartNumber": ItemCodeStartNumber = ToInt(s.SettingValue, 1); break;
                case "ItemCodeZeroPadding": ItemCodeZeroPadding = ToInt(s.SettingValue, 3); break;
                case "CategoryWiseItemCode": CategoryWiseItemCode = IsTrue(s.SettingValue); break;
                case "SubCategoryWiseItemCode": SubCategoryWiseItemCode = IsTrue(s.SettingValue); break;
                case "CodeSeparator": CodeSeparator = s.SettingValue ?? "-"; break;

                // Customer Code
                case "AutoCustomerCode": AutoCustomerCode = IsTrue(s.SettingValue); break;
                case "CustomerCodePrefix": CustomerCodePrefix = s.SettingValue ?? "CUS"; break;
                case "CustomerCodeStartNumber": CustomerCodeStartNumber = ToInt(s.SettingValue, 1); break;
                case "CustomerCodeZeroPadding": CustomerCodeZeroPadding = ToInt(s.SettingValue, 3); break;

                // Supplier Code
                case "AutoSupplierCode": AutoSupplierCode = IsTrue(s.SettingValue); break;
                case "SupplierCodePrefix": SupplierCodePrefix = s.SettingValue ?? "SUP"; break;
                case "SupplierCodeStartNumber": SupplierCodeStartNumber = ToInt(s.SettingValue, 1); break;
                case "SupplierCodeZeroPadding": SupplierCodeZeroPadding = ToInt(s.SettingValue, 3); break;

                // Category Code
                case "AutoCategoryCode": AutoCategoryCode = IsTrue(s.SettingValue); break;
                case "CategoryCodePrefix": CategoryCodePrefix = s.SettingValue ?? "CAT"; break;
                case "CategoryCodeStartNumber": CategoryCodeStartNumber = ToInt(s.SettingValue, 1); break;
                case "CategoryCodeZeroPadding": CategoryCodeZeroPadding = ToInt(s.SettingValue, 2); break;

                // SubCategory Code
                case "AutoSubCategoryCode": AutoSubCategoryCode = IsTrue(s.SettingValue); break;
                case "SubCategoryCodePrefix": SubCategoryCodePrefix = s.SettingValue ?? "SUB"; break;
                case "SubCategoryCodeStartNumber": SubCategoryCodeStartNumber = ToInt(s.SettingValue, 1); break;
                case "SubCategoryCodeZeroPadding": SubCategoryCodeZeroPadding = ToInt(s.SettingValue, 2); break;
                case "CategoryWiseSubCategoryCode": CategoryWiseSubCategoryCode = IsTrue(s.SettingValue); break;

                // Transaction: Quotation
                case "AutoQuotationNo": AutoQuotationNo = IsTrue(s.SettingValue); break;
                case "QuotationNoPrefix": QuotationNoPrefix = s.SettingValue ?? "QT"; break;
                case "QuotationNoStartNumber": QuotationNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "QuotationNoZeroPadding": QuotationNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Sales Order
                case "AutoSalesOrderNo": AutoSalesOrderNo = IsTrue(s.SettingValue); break;
                case "SalesOrderNoPrefix": SalesOrderNoPrefix = s.SettingValue ?? "SO"; break;
                case "SalesOrderNoStartNumber": SalesOrderNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "SalesOrderNoZeroPadding": SalesOrderNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Delivery Note
                case "AutoDeliveryNoteNo": AutoDeliveryNoteNo = IsTrue(s.SettingValue); break;
                case "DeliveryNoteNoPrefix": DeliveryNoteNoPrefix = s.SettingValue ?? "DN"; break;
                case "DeliveryNoteNoStartNumber": DeliveryNoteNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "DeliveryNoteNoZeroPadding": DeliveryNoteNoZeroPadding = ToInt(s.SettingValue, 4); break;
                case "DeliveryNoteStockAffect": DeliveryNoteStockAffect = IsTrue(s.SettingValue); break;

                // Transaction: Sales Invoice
                case "AutoSalesInvoiceNo": AutoSalesInvoiceNo = IsTrue(s.SettingValue); break;
                case "SalesInvoiceNoPrefix": SalesInvoiceNoPrefix = s.SettingValue ?? "SIN"; break;
                case "SalesInvoiceNoStartNumber": SalesInvoiceNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "SalesInvoiceNoZeroPadding": SalesInvoiceNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Sales Invoice Return
                case "AutoSalesInvoiceRtnNo": AutoSalesInvoiceRtnNo = IsTrue(s.SettingValue); break;
                case "SalesInvoiceRtnNoPrefix": SalesInvoiceRtnNoPrefix = s.SettingValue ?? "SR"; break;
                case "SalesInvoiceRtnNoStartNumber": SalesInvoiceRtnNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "SalesInvoiceRtnNoZeroPadding": SalesInvoiceRtnNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Purchase Order
                case "AutoPurchaseOrderNo": AutoPurchaseOrderNo = IsTrue(s.SettingValue); break;
                case "PurchaseOrderNoPrefix": PurchaseOrderNoPrefix = s.SettingValue ?? "PO"; break;
                case "PurchaseOrderNoStartNumber": PurchaseOrderNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "PurchaseOrderNoZeroPadding": PurchaseOrderNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Purchase Invoice
                case "AutoPurchaseNo": AutoPurchaseNo = IsTrue(s.SettingValue); break;
                case "PurchaseNoPrefix": PurchaseNoPrefix = s.SettingValue ?? "PUR"; break;
                case "PurchaseNoStartNumber": PurchaseNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "PurchaseNoZeroPadding": PurchaseNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Proforma Invoice
                case "AutoProformaInvoiceNo": AutoProformaInvoiceNo = IsTrue(s.SettingValue); break;
                case "ProformaInvoiceNoPrefix": ProformaInvoiceNoPrefix = s.SettingValue ?? "PI"; break;
                case "ProformaInvoiceNoStartNumber": ProformaInvoiceNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "ProformaInvoiceNoZeroPadding": ProformaInvoiceNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Purchase Return
                case "AutoPurchaseReturnNo": AutoPurchaseReturnNo = IsTrue(s.SettingValue); break;
                case "PurchaseReturnNoPrefix": PurchaseReturnNoPrefix = s.SettingValue ?? "PR"; break;
                case "PurchaseReturnNoStartNumber": PurchaseReturnNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "PurchaseReturnNoZeroPadding": PurchaseReturnNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Goods Receipt
                case "AutoGoodsReceiptNo": AutoGoodsReceiptNo = IsTrue(s.SettingValue); break;
                case "GoodsReceiptNoPrefix": GoodsReceiptNoPrefix = s.SettingValue ?? "GRN"; break;
                case "GoodsReceiptNoStartNumber": GoodsReceiptNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "GoodsReceiptNoZeroPadding": GoodsReceiptNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Expense Entry
                case "AutoExpenseEntryNo": AutoExpenseEntryNo = IsTrue(s.SettingValue); break;
                case "ExpenseEntryNoPrefix": ExpenseEntryNoPrefix = s.SettingValue ?? "EXP"; break;
                case "ExpenseEntryNoStartNumber": ExpenseEntryNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "ExpenseEntryNoZeroPadding": ExpenseEntryNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Stock Adjustment
                case "AutoStockAdjustmentNo": AutoStockAdjustmentNo = IsTrue(s.SettingValue); break;
                case "StockAdjustmentNoPrefix": StockAdjustmentNoPrefix = s.SettingValue ?? "SA"; break;
                case "StockAdjustmentNoStartNumber": StockAdjustmentNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "StockAdjustmentNoZeroPadding": StockAdjustmentNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Material Transfer Request
                case "AutoMaterialTransferRequestNo": AutoMaterialTransferRequestNo = IsTrue(s.SettingValue); break;
                case "MaterialTransferRequestNoPrefix": MaterialTransferRequestNoPrefix = s.SettingValue ?? "MTR"; break;
                case "MaterialTransferRequestNoStartNumber": MaterialTransferRequestNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "MaterialTransferRequestNoZeroPadding": MaterialTransferRequestNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Material Transfer IN
                case "AutoMaterialTransferInNo": AutoMaterialTransferInNo = IsTrue(s.SettingValue); break;
                case "MaterialTransferInNoPrefix": MaterialTransferInNoPrefix = s.SettingValue ?? "MTI"; break;
                case "MaterialTransferInNoStartNumber": MaterialTransferInNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "MaterialTransferInNoZeroPadding": MaterialTransferInNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Material Transfer OUT
                case "AutoMaterialTransferOutNo": AutoMaterialTransferOutNo = IsTrue(s.SettingValue); break;
                case "MaterialTransferOutNoPrefix": MaterialTransferOutNoPrefix = s.SettingValue ?? "MTO"; break;
                case "MaterialTransferOutNoStartNumber": MaterialTransferOutNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "MaterialTransferOutNoZeroPadding": MaterialTransferOutNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Job Order
                case "AutoJobOrderNo": AutoJobOrderNo = IsTrue(s.SettingValue); break;
                case "JobOrderNoPrefix": JobOrderNoPrefix = s.SettingValue ?? "JO"; break;
                case "JobOrderNoStartNumber": JobOrderNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "JobOrderNoZeroPadding": JobOrderNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Job Card
                case "AutoJobCardNo": AutoJobCardNo = IsTrue(s.SettingValue); break;
                case "JobCardNoPrefix": JobCardNoPrefix = s.SettingValue ?? "JC"; break;
                case "JobCardNoStartNumber": JobCardNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "JobCardNoZeroPadding": JobCardNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Transaction: Internal Material Transfer
                case "AutoInternalMaterialTransferNo": AutoInternalMaterialTransferNo = IsTrue(s.SettingValue); break;
                case "InternalMaterialTransferNoPrefix": InternalMaterialTransferNoPrefix = s.SettingValue ?? "IMT"; break;
                case "InternalMaterialTransferNoStartNumber": InternalMaterialTransferNoStartNumber = ToInt(s.SettingValue, 1); break;
                case "InternalMaterialTransferNoZeroPadding": InternalMaterialTransferNoZeroPadding = ToInt(s.SettingValue, 4); break;

                // Default Behavior
                case "DefaultFocusOn": DefaultFocusOn = s.SettingValue ?? "ItemCode"; break;
                case "DefaultBillType": DefaultBillType = s.SettingValue ?? "Cash"; break;
                case "AllowNegativeStock": AllowNegativeStock = IsTrue(s.SettingValue); break;
                case "ShowCostInSales": ShowCostInSales = IsTrue(s.SettingValue); break;
                case "RequireBarcode": RequireBarcode = IsTrue(s.SettingValue); break;
                case "BlockInvoiceOnCreditDaysExceeded": BlockInvoiceOnCreditDaysExceeded = IsTrue(s.SettingValue); break;
                case "BlockInvoiceOnCreditLimitExceeded": BlockInvoiceOnCreditLimitExceeded = IsTrue(s.SettingValue); break;
                case "EnableShiftClosing": EnableShiftClosing = IsTrue(s.SettingValue); break;

                // Item Search Mode
                case "ItemSearchMode": ItemSearchMode = ToInt(s.SettingValue, 1); break;

                // Decimal Places
                case "CurrencyDecimalPlaces": CurrencyDecimalPlaces = ToInt(s.SettingValue, 2); break;
                case "QuantityDecimalPlaces": QuantityDecimalPlaces = ToInt(s.SettingValue, 2); break;

                // Image Storage
                case "ImageStorageMode": ImageStorageMode = ToInt(s.SettingValue, 0); break;
                case "ImageClientFolder": ImageClientFolder = s.SettingValue ?? "DefaultClient"; break;
                case "ImageLocalBasePath": ImageLocalBasePath = s.SettingValue ?? @"D:\ERPImages"; break;

                // AWS S3 Storage
                case "ImageAwsRegion": ImageAwsRegion = s.SettingValue ?? "us-east-1"; break;
                case "ImageAwsBucket": ImageAwsBucket = s.SettingValue ?? "erp-images"; break;
                case "ImageAwsAccessKey": ImageAwsAccessKey = s.SettingValue ?? string.Empty; break;
                case "ImageAwsSecretKey": ImageAwsSecretKey = s.SettingValue ?? string.Empty; break;

                // Web Settings
                case "IsWebBased": IsWebBased = IsTrue(s.SettingValue); break;

                // Tax Regime Settings
                case "DefaultGstStateCode": DefaultGstStateCode = s.SettingValue ?? "32"; break;
                case "DefaultGstStateName": DefaultGstStateName = s.SettingValue ?? "Kerala"; break;
                case "EnableGstComposition": EnableGstComposition = IsTrue(s.SettingValue); break;
                case "EnableUpiPaymentQr": EnableUpiPaymentQr = IsTrue(s.SettingValue); break;
                case "UpiVpaId": UpiVpaId = s.SettingValue ?? string.Empty; break;
                case "UpiPayeeName": UpiPayeeName = s.SettingValue ?? string.Empty; break;
                case "EWayBillThreshold": EWayBillThreshold = ToDecimal(s.SettingValue, 50000); break;

                // Zatca Settings
                case "ZatcaPhase": ZatcaPhase = ToInt(s.SettingValue, 1); break;
                case "ZatcaConnection": ZatcaConnection = s.SettingValue ?? "Simulation"; break;

                // Printer Settings
                case "BarcodePrinterName": BarcodePrinterName = s.SettingValue ?? string.Empty; break;
                case "InvoicePrinterName": InvoicePrinterName = s.SettingValue ?? string.Empty; break;

                // Default Accounts Settings
                case "DefaultBankAccount": DefaultBankAccountId = ToInt(s.SettingValue, 0); break;
                case "DefaultCashAccount": DefaultCashAccountId = ToInt(s.SettingValue, 0); break;

                // Database Backup Settings
                case "BackupWhenClosing": BackupWhenClosing = IsTrue(s.SettingValue); break;
                case "BackupFolder": BackupFolder = s.SettingValue ?? string.Empty; break;
            }
        }

        // Load Transaction Grid Settings
        LoadTransactionGridSettings("SalesInvoice", SalesInvoiceSettings, settings);
        LoadTransactionGridSettings("SalesOrder", SalesOrderSettings, settings);
        LoadTransactionGridSettings("SalesQuotation", SalesQuotationSettings, settings);
        LoadTransactionGridSettings("ProformaInvoice", ProformaInvoiceSettings, settings);
        LoadTransactionGridSettings("SalesReturn", SalesReturnSettings, settings);
        LoadTransactionGridSettings("Purchase", PurchaseSettings, settings);
        LoadTransactionGridSettings("DeliveryNote", DeliveryNoteSettings, settings);
        LoadTransactionGridSettings("DeliveryNoteReturn", DeliveryNoteReturnSettings, settings);
        LoadTransactionGridSettings("PurchaseReturn", PurchaseReturnSettings, settings);
        LoadTransactionGridSettings("GoodsReceipt", GoodsReceiptSettings, settings);

        IsLoaded = true;
    }

    /// <summary>
    /// Reset all settings to defaults - call on logout
    /// </summary>
    public static void Clear()
    {
        AutoItemCode = false;
        ItemCodePrefix = "IT";
        ItemCodeStartNumber = 1;
        ItemCodeZeroPadding = 3;
        CategoryWiseItemCode = false;
        SubCategoryWiseItemCode = false;
        CodeSeparator = "-";

        AutoCustomerCode = false;
        CustomerCodePrefix = "CUS";
        CustomerCodeStartNumber = 1;
        CustomerCodeZeroPadding = 3;

        AutoSupplierCode = false;
        SupplierCodePrefix = "SUP";
        SupplierCodeStartNumber = 1;
        SupplierCodeZeroPadding = 3;

        AutoCategoryCode = false;
        CategoryCodePrefix = "CAT";
        CategoryCodeStartNumber = 1;
        CategoryCodeZeroPadding = 2;

        AutoSubCategoryCode = false;
        SubCategoryCodePrefix = "SUB";
        SubCategoryCodeStartNumber = 1;
        SubCategoryCodeZeroPadding = 2;
        CategoryWiseSubCategoryCode = false;

        // Transactions
        AutoQuotationNo = false;
        QuotationNoPrefix = "QT";
        QuotationNoStartNumber = 1;
        QuotationNoZeroPadding = 4;

        AutoSalesOrderNo = false;
        SalesOrderNoPrefix = "SO";
        SalesOrderNoStartNumber = 1;
        SalesOrderNoZeroPadding = 4;

        AutoDeliveryNoteNo = false;
        DeliveryNoteNoPrefix = "DN";
        DeliveryNoteNoStartNumber = 1;
        DeliveryNoteNoStartNumber = 1;
        DeliveryNoteNoZeroPadding = 4;
        DeliveryNoteStockAffect = false;

        AutoSalesInvoiceNo = false;
        SalesInvoiceNoPrefix = "SIN";
        SalesInvoiceNoStartNumber = 1;
        SalesInvoiceNoZeroPadding = 4;

        AutoSalesInvoiceRtnNo = false;
        SalesInvoiceRtnNoPrefix = "SR";
        SalesInvoiceRtnNoStartNumber = 1;
        SalesInvoiceRtnNoZeroPadding = 4;

        AutoPurchaseOrderNo = false;
        PurchaseOrderNoPrefix = "PO";
        PurchaseOrderNoStartNumber = 1;
        PurchaseOrderNoZeroPadding = 4;

        AutoPurchaseNo = false;
        PurchaseNoPrefix = "PUR";
        PurchaseNoStartNumber = 1;
        PurchaseNoZeroPadding = 4;

        AutoProformaInvoiceNo = false;
        ProformaInvoiceNoPrefix = "PI";
        ProformaInvoiceNoStartNumber = 1;
        ProformaInvoiceNoZeroPadding = 4;

        AutoPurchaseReturnNo = false;
        PurchaseReturnNoPrefix = "PR";
        PurchaseReturnNoStartNumber = 1;
        PurchaseReturnNoZeroPadding = 4;

        AutoGoodsReceiptNo = false;
        GoodsReceiptNoPrefix = "GRN";
        GoodsReceiptNoStartNumber = 1;
        GoodsReceiptNoZeroPadding = 4;

        AutoExpenseEntryNo = false;
        ExpenseEntryNoPrefix = "EXP";
        ExpenseEntryNoStartNumber = 1;
        ExpenseEntryNoZeroPadding = 4;

        AutoStockAdjustmentNo = false;
        StockAdjustmentNoPrefix = "SA";
        StockAdjustmentNoStartNumber = 1;
        StockAdjustmentNoZeroPadding = 4;

        AutoMaterialTransferRequestNo = false;
        MaterialTransferRequestNoPrefix = "MTR";
        MaterialTransferRequestNoStartNumber = 1;
        MaterialTransferRequestNoZeroPadding = 4;

        AutoMaterialTransferInNo = false;
        MaterialTransferInNoPrefix = "MTI";
        MaterialTransferInNoStartNumber = 1;
        MaterialTransferInNoZeroPadding = 4;

        AutoMaterialTransferOutNo = false;
        MaterialTransferOutNoPrefix = "MTO";
        MaterialTransferOutNoStartNumber = 1;
        MaterialTransferOutNoZeroPadding = 4;

        AutoJobOrderNo = false;
        JobOrderNoPrefix = "JO";
        JobOrderNoStartNumber = 1;
        JobOrderNoZeroPadding = 4;

        AutoJobCardNo = false;
        JobCardNoPrefix = "JC";
        JobCardNoStartNumber = 1;
        JobCardNoZeroPadding = 4;

        AutoInternalMaterialTransferNo = false;
        InternalMaterialTransferNoPrefix = "IMT";
        InternalMaterialTransferNoStartNumber = 1;
        InternalMaterialTransferNoZeroPadding = 4;

        DefaultFocusOn = "ItemCode";
        DefaultBillType = "Cash";
        AllowNegativeStock = false;
        ShowCostInSales = false;
        RequireBarcode = false;
        BlockInvoiceOnCreditDaysExceeded = false;
        BlockInvoiceOnCreditLimitExceeded = false;
        EnableShiftClosing = false;

        ItemSearchMode = 1;

        CurrencyDecimalPlaces = 2;
        QuantityDecimalPlaces = 2;

        // Image Storage
        ImageStorageMode = 0;
        ImageClientFolder = "DefaultClient";
        ImageLocalBasePath = @"D:\ERPImages";

        // AWS S3 Storage
        ImageAwsRegion = "us-east-1";
        ImageAwsBucket = "erp-images";
        ImageAwsAccessKey = string.Empty;
        ImageAwsSecretKey = string.Empty;

        IsWebBased = false;

        TaxRegime = "Zatca";
        DefaultGstStateCode = "32";
        DefaultGstStateName = "Kerala";
        EnableGstComposition = false;
        EnableUpiPaymentQr = true;
        UpiVpaId = string.Empty;
        UpiPayeeName = string.Empty;
        EWayBillThreshold = 50000;

        ZatcaPhase = 1;
        ZatcaConnection = "Simulation";

        BarcodePrinterName = string.Empty;
        InvoicePrinterName = string.Empty;

        DefaultBankAccountId = 0;
        DefaultCashAccountId = 0;

        BackupWhenClosing = false;
        BackupFolder = string.Empty;

        IsLoaded = false;

        // Reset Transaction Grid Settings
        SalesInvoiceSettings.ResetToDefaults();
        SalesOrderSettings.ResetToDefaults();
        SalesQuotationSettings.ResetToDefaults();
        ProformaInvoiceSettings.ResetToDefaults();
        SalesReturnSettings.ResetToDefaults();
        PurchaseSettings.ResetToDefaults();
        DeliveryNoteSettings.ResetToDefaults();
        DeliveryNoteReturnSettings.ResetToDefaults();
        PurchaseReturnSettings.ResetToDefaults();
        GoodsReceiptSettings.ResetToDefaults();
    }

    private static void LoadTransactionGridSettings(string prefix, TransactionGridSettings s, List<ZakiSAASERP.Models.Masters.PermanentSettings> settings)
    {
        var dict = settings.ToDictionary(x => x.SettingKey, x => x.SettingValue, StringComparer.OrdinalIgnoreCase);

        bool GetBool(string key, bool def) =>
            dict.TryGetValue($"{prefix}_{key}", out var v) ? IsTrue(v) : def;

        s.ShowItemCode = GetBool("ShowItemCode", false);
        s.ShowBarcode = GetBool("ShowBarcode", true);
        s.ShowItemName = GetBool("ShowItemName", true);
        s.ShowArabicName = GetBool("ShowArabicName", false);
        s.ShowUnit = GetBool("ShowUnit", true);
        s.ShowExRate = GetBool("ShowExRate", true);
        s.ShowIncRate = GetBool("ShowIncRate", false);
        s.ShowGross = GetBool("ShowGross", true);
        s.ShowDiscPercentage = GetBool("ShowDiscPercentage", true);
        s.ShowDiscAmount = GetBool("ShowDiscAmount", true);
        s.ShowTaxableAmount = GetBool("ShowTaxableAmount", false);
        s.ShowTaxPercentage = GetBool("ShowTaxPercentage", true);
        s.ShowTaxAmount = GetBool("ShowTaxAmount", true);
        s.ShowAmount = GetBool("ShowAmount", true);

        s.AllowEditExcRate = GetBool("AllowEditExcRate", true);
        s.AllowEditIncRate = GetBool("AllowEditIncRate", true);
        s.AllowEditDiscPer = GetBool("AllowEditDiscPer", true);
        s.AllowEditDiscAmount = GetBool("AllowEditDiscAmount", true);

        s.EnableUpdate = GetBool("EnableUpdate", false) || (dict.TryGetValue($"Enable{prefix}Update", out var eu) && IsTrue(eu));
        s.EnableCancel = GetBool("EnableCancel", false) || (dict.TryGetValue($"Enable{prefix}Cancel", out var ec) && IsTrue(ec));
    }

    private static bool IsTrue(string? value)
        => value == "1" || (value?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false);

    private static int ToInt(string? value, int defaultValue)
        => int.TryParse(value, out var result) ? result : defaultValue;

    private static decimal ToDecimal(string? value, decimal defaultValue)
        => decimal.TryParse(value, out var result) ? result : defaultValue;
}
