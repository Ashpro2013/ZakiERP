namespace ZakiSAASERP.Models.DTO;

/// <summary>
/// Result data extracted from an uploaded vendor purchase invoice / receipt via AI OCR.
/// </summary>
public class PurchaseInvoiceOcrResult
{
    public string? SupplierName { get; set; }
    public string? SupplierVatNo { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? BillType { get; set; } = "Credit";
    public string? Currency { get; set; } = "SAR";
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Remarks { get; set; }
    public List<PurchaseInvoiceOcrLineItem> LineItems { get; set; } = new();

    // ERP Matching Information
    public int? MatchedSupplierId { get; set; }
    public string? MatchedSupplierName { get; set; }
    public bool IsSupplierMatched => MatchedSupplierId.HasValue && MatchedSupplierId.Value > 0;
}

/// <summary>
/// Extracted line item from an invoice with system matching information.
/// </summary>
public class PurchaseInvoiceOcrLineItem
{
    public int SL { get; set; }
    public string? ItemName { get; set; }
    public string? ItemArabicName { get; set; }
    public string? Barcode { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? UnitName { get; set; } = "PCS";
    public decimal UnitPrice { get; set; } // Rate ex VAT
    public decimal DiscountPerc { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPerc { get; set; } = 15;
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    // System matching properties
    public int? MatchedItemId { get; set; }
    public string? MatchedItemName { get; set; }
    public string? MatchedBarcode { get; set; }
    public int? MatchedUnitId { get; set; }
    public string? MatchedUnitName { get; set; }
    public int? MatchedStoreId { get; set; }
    public bool IsSelected { get; set; } = true;
    public bool IsMatched => MatchedItemId.HasValue && MatchedItemId.Value > 0;
}

