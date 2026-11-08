namespace ZakiSAASERP.Models.Settings;

public class SettingsModel
{
    // Item Code
    public bool AutoItemCode { get; set; }
    public string ItemCodePrefix { get; set; } = "IT";
    public int ItemCodeStartNumber { get; set; } = 1;
    public int ItemCodeZeroPadding { get; set; } = 3;
    public bool CategoryWiseItemCode { get; set; }
    public bool SubCategoryWiseItemCode { get; set; }
    public string CodeSeparator { get; set; } = "-";

    // Customer Code
    public bool AutoCustomerCode { get; set; }
    public string CustomerCodePrefix { get; set; } = "CUS";
    public int CustomerCodeStartNumber { get; set; } = 1;
    public int CustomerCodeZeroPadding { get; set; } = 3;

    // Supplier Code
    public bool AutoSupplierCode { get; set; }
    public string SupplierCodePrefix { get; set; } = "SUP";
    public int SupplierCodeStartNumber { get; set; } = 1;
    public int SupplierCodeZeroPadding { get; set; } = 3;

    // Category Code
    public bool AutoCategoryCode { get; set; }
    public string CategoryCodePrefix { get; set; } = "CAT";
    public int CategoryCodeStartNumber { get; set; } = 1;
    public int CategoryCodeZeroPadding { get; set; } = 2;

    // SubCategory Code
    public bool AutoSubCategoryCode { get; set; }
    public string SubCategoryCodePrefix { get; set; } = "SUB";
    public int SubCategoryCodeStartNumber { get; set; } = 1;
    public int SubCategoryCodeZeroPadding { get; set; } = 2;
    public bool CategoryWiseSubCategoryCode { get; set; }

    // Transaction Code Settings
    // Quotation
    public bool AutoQuotationNo { get; set; }
    public string QuotationNoPrefix { get; set; } = "QT";
    public int QuotationNoStartNumber { get; set; } = 1;
    public int QuotationNoZeroPadding { get; set; } = 4;

    // Sales Order
    public bool AutoSalesOrderNo { get; set; }
    public string SalesOrderNoPrefix { get; set; } = "SO";
    public int SalesOrderNoStartNumber { get; set; } = 1;
    public int SalesOrderNoZeroPadding { get; set; } = 4;

    // Delivery Note
    public bool AutoDeliveryNoteNo { get; set; }
    public string DeliveryNoteNoPrefix { get; set; } = "DN";
    public int DeliveryNoteNoStartNumber { get; set; } = 1;
    public int DeliveryNoteNoZeroPadding { get; set; } = 4;
    public bool DeliveryNoteStockAffect { get; set; }

    // Purchase Order
    public bool AutoPurchaseOrderNo { get; set; }
    public string PurchaseOrderNoPrefix { get; set; } = "PO";
    public int PurchaseOrderNoStartNumber { get; set; } = 1;
    public int PurchaseOrderNoZeroPadding { get; set; } = 4;

    // Proforma Invoice
    public bool AutoProformaInvoiceNo { get; set; }
    public string ProformaInvoiceNoPrefix { get; set; } = "PI";
    public int ProformaInvoiceNoStartNumber { get; set; } = 1;
    public int ProformaInvoiceNoZeroPadding { get; set; } = 4;

    // Purchase
    public bool AutoPurchaseNo { get; set; }
    public string PurchaseNoPrefix { get; set; } = "PUR";
    public int PurchaseNoStartNumber { get; set; } = 1;
    public int PurchaseNoZeroPadding { get; set; } = 4;

    // Journal Entry
    public bool AutoJournalEntryNo { get; set; }
    public string JournalEntryNoPrefix { get; set; } = "JV";
    public int JournalEntryNoStartNumber { get; set; } = 1;
    public int JournalEntryNoZeroPadding { get; set; } = 4;

    // Stock Adjustment
    public bool AutoStockAdjustmentNo { get; set; }
    public string StockAdjustmentNoPrefix { get; set; } = "SA";
    public int StockAdjustmentNoStartNumber { get; set; } = 1;
    public int StockAdjustmentNoZeroPadding { get; set; } = 4;

    // Material Transfer Request
    public bool AutoMaterialTransferRequestNo { get; set; }
    public string MaterialTransferRequestNoPrefix { get; set; } = "MTR";
    public int MaterialTransferRequestNoStartNumber { get; set; } = 1;
    public int MaterialTransferRequestNoZeroPadding { get; set; } = 4;

    // Material Transfer In
    public bool AutoMaterialTransferInNo { get; set; }
    public string MaterialTransferInNoPrefix { get; set; } = "MTI";
    public int MaterialTransferInNoStartNumber { get; set; } = 1;
    public int MaterialTransferInNoZeroPadding { get; set; } = 4;

    // Material Transfer Out
    public bool AutoMaterialTransferOutNo { get; set; }
    public string MaterialTransferOutNoPrefix { get; set; } = "MTO";
    public int MaterialTransferOutNoStartNumber { get; set; } = 1;
    public int MaterialTransferOutNoZeroPadding { get; set; } = 4;

    // Job Order
    public bool AutoJobOrderNo { get; set; }
    public string JobOrderNoPrefix { get; set; } = "JO";
    public int JobOrderNoStartNumber { get; set; } = 1;
    public int JobOrderNoZeroPadding { get; set; } = 4;

    // Job Card
    public bool AutoJobCardNo { get; set; }
    public string JobCardNoPrefix { get; set; } = "JC";
    public int JobCardNoStartNumber { get; set; } = 1;
    public int JobCardNoZeroPadding { get; set; } = 4;

    // Internal Material Transfer
    public bool AutoInternalMaterialTransferNo { get; set; }
    public string InternalMaterialTransferNoPrefix { get; set; } = "IMT";
    public int InternalMaterialTransferNoStartNumber { get; set; } = 1;
    public int InternalMaterialTransferNoZeroPadding { get; set; } = 4;

    // Default Behavior
    public string DefaultFocusOn { get; set; } = "ItemCode";
    public string DefaultBillType { get; set; } = "Cash";
    public bool AllowNegativeStock { get; set; }
    public bool ShowCostInSales { get; set; }
    public bool RequireBarcode { get; set; }
    public bool BlockInvoiceOnCreditDaysExceeded { get; set; }
    public bool BlockInvoiceOnCreditLimitExceeded { get; set; }
    public bool EnableShiftClosing { get; set; } = false;

    // Item Search Mode: 1=ItemCode, 2=MultiUnit
    public int ItemSearchMode { get; set; } = 1;

    // Decimal Places
    public int CurrencyDecimalPlaces { get; set; } = 2;
    public int QuantityDecimalPlaces { get; set; } = 2;

    // Image Storage Settings
    public int ImageStorageMode { get; set; } = 0; // 0=Local, 1=AWS
    public string ImageClientFolder { get; set; } = "DefaultClient";
    public string ImageLocalBasePath { get; set; } = @"D:\ERPImages";
    public string ImageAwsRegion { get; set; } = "us-east-1";
    public string ImageAwsBucket { get; set; } = "erp-images";
    public string ImageAwsAccessKey { get; set; } = string.Empty;
    public string ImageAwsSecretKey { get; set; } = string.Empty;
    public bool IsWebBased { get; set; }

    // Goods Receipt
    public bool AutoGoodsReceiptNo { get; set; }
    public string GoodsReceiptNoPrefix { get; set; } = "GRN";
    public int GoodsReceiptNoStartNumber { get; set; } = 1;
    public int GoodsReceiptNoZeroPadding { get; set; } = 4;

    // Tax Regime Settings
    public string TaxRegime { get; set; } = "Zatca"; // "Zatca" (Saudi Arabia), "IndiaGst" (India), "Standard" (Generic VAT)

    // GST Settings (India)
    public string DefaultGstStateCode { get; set; } = "32";
    public string DefaultGstStateName { get; set; } = "Kerala";
    public bool EnableGstComposition { get; set; } = false;
    public bool EnableUpiPaymentQr { get; set; } = true;
    public string? UpiVpaId { get; set; } = string.Empty;
    public string? UpiPayeeName { get; set; } = string.Empty;
    public decimal EWayBillThreshold { get; set; } = 50000;

    // Zatca Settings
    public string ZatcaPhase { get; set; } = "1";
    public string ZatcaConnection { get; set; } = "Simulation";
    public bool UploadSimplifiedXMLRealtime { get; set; } = true;
    public int DefaultCashAccountId { get; set; }
    public int DefaultBankAccountId { get; set; }

    // Sales Invoice
    public bool AutoSalesInvoiceNo { get; set; }
    public string SalesInvoiceNoPrefix { get; set; } = "INV";
    public int SalesInvoiceNoStartNumber { get; set; } = 1;
    public int SalesInvoiceNoZeroPadding { get; set; } = 4;

    // Sales Invoice Return
    public bool AutoSalesInvoiceRtnNo { get; set; }
    public string SalesInvoiceRtnNoPrefix { get; set; } = "SIR";
    public int SalesInvoiceRtnNoStartNumber { get; set; } = 1;
    public int SalesInvoiceRtnNoZeroPadding { get; set; } = 4;

    // Purchase Return
    public bool AutoPurchaseReturnNo { get; set; }
    public string PurchaseReturnNoPrefix { get; set; } = "PRN";
    public int PurchaseReturnNoStartNumber { get; set; } = 1;
    public int PurchaseReturnNoZeroPadding { get; set; } = 4;

    // Delivery Note Return
    public bool AutoDeliveryNoteReturnNo { get; set; }
    public string DeliveryNoteReturnNoPrefix { get; set; } = "DNR";
    public int DeliveryNoteReturnNoStartNumber { get; set; } = 1;
    public int DeliveryNoteReturnNoZeroPadding { get; set; } = 4;

    // Damage Stock
    public bool AutoDamageStockNo { get; set; }
    public string DamageStockNoPrefix { get; set; } = "DS";
    public int DamageStockNoStartNumber { get; set; } = 1;
    public int DamageStockNoZeroPadding { get; set; } = 4;

    // Enquiry Registration
    public bool AutoEnquiryNo { get; set; }
    public string EnquiryNoPrefix { get; set; } = "ENQ";
    public int EnquiryNoStartNumber { get; set; } = 1;
    public int EnquiryNoZeroPadding { get; set; } = 4;

    // Customer Receipt
    public bool AutoCustomerReceiptNo { get; set; }
    public string CustomerReceiptNoPrefix { get; set; } = "CR";
    public int CustomerReceiptNoStartNumber { get; set; } = 1;
    public int CustomerReceiptNoZeroPadding { get; set; } = 6;

    // Supplier Payment
    public bool AutoSupplierPaymentNo { get; set; } = true;
    public string SupplierPaymentNoPrefix { get; set; } = "SP";
    public int SupplierPaymentNoStartNumber { get; set; } = 1;
    public int SupplierPaymentNoZeroPadding { get; set; } = 4;

    // Expense Entry
    public bool AutoExpenseEntryNo { get; set; }
    public string ExpenseEntryNoPrefix { get; set; } = "EXP";
    public int ExpenseEntryNoStartNumber { get; set; } = 1;
    public int ExpenseEntryNoZeroPadding { get; set; } = 4;

    // Account Opening Entry
    public bool AutoAccountOpeningEntryNo { get; set; }
    public string AccountOpeningEntryNoPrefix { get; set; } = "AOE";
    public int AccountOpeningEntryNoStartNumber { get; set; } = 1;
    public int AccountOpeningEntryNoZeroPadding { get; set; } = 4;

    // Printer Settings (Windows Desktop)
    public string BarcodePrinterName { get; set; } = string.Empty;
    public string InvoicePrinterName { get; set; } = string.Empty;
    public string SalesReturnPrintFormat { get; set; } = "A4";

    // Database Backup Settings
    public bool BackupWhenClosing { get; set; } = false;
    public string BackupFolder { get; set; } = string.Empty;

    // Transaction Grid Settings (Column Visibility & Inline Editing)
    public TransactionGridSettings SalesInvoiceSettings { get; set; } = new();
    public TransactionGridSettings SalesOrderSettings { get; set; } = new();
    public TransactionGridSettings SalesQuotationSettings { get; set; } = new();
    public TransactionGridSettings ProformaInvoiceSettings { get; set; } = new();
    public TransactionGridSettings SalesReturnSettings { get; set; } = new();
    public TransactionGridSettings PurchaseSettings { get; set; } = new();
    public TransactionGridSettings DeliveryNoteSettings { get; set; } = new();
    public TransactionGridSettings DeliveryNoteReturnSettings { get; set; } = new();
    public TransactionGridSettings PurchaseReturnSettings { get; set; } = new();
    public TransactionGridSettings GoodsReceiptSettings { get; set; } = new();

    // Sales Invoice & Sales Return Operations
    public bool EnableSalesInvoiceUpdate { get => SalesInvoiceSettings.EnableUpdate; set => SalesInvoiceSettings.EnableUpdate = value; }
    public bool EnableSalesInvoiceCancel { get => SalesInvoiceSettings.EnableCancel; set => SalesInvoiceSettings.EnableCancel = value; }
    public bool EnableSalesReturnUpdate { get => SalesReturnSettings.EnableUpdate; set => SalesReturnSettings.EnableUpdate = value; }
    public bool EnableSalesReturnCancel { get => SalesReturnSettings.EnableCancel; set => SalesReturnSettings.EnableCancel = value; }
}
