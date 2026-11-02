namespace AshproSAASERP.Models.ViewModel;

public class TransactionDtls
{
    public int SL { get; set; }
    public int DtlsId { get; set; }
    public int HdrId { get; set; }
    public int FK_ItemId { get; set; }
    private string? _itemCode;
    public string? ItemCode
    {
        get => !string.IsNullOrEmpty(_itemCode) ? _itemCode : (FK_ItemId > 0 ? FK_ItemId.ToString("D5") : "");
        set => _itemCode = value;
    }
    public string? ItemName { get; set; }
    public string? ItemArabicName { get; set; }
    public string? Barcode { get; set; }
    public int FK_UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal Quantity { get; set; }
    public decimal RequestedQty { get; set; }  // For approval/rejection reports
    public decimal QtyPerUnit { get; set; }
    public decimal ExRate { get; set; }
    public decimal InRate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountPerc { get; set; }
    public decimal DiscountAmnt { get; set; }
    public decimal TaxableAmnt { get; set; }
    public decimal TaxPerc { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int FK_ItemBaseUnit { get; set; }
    public int FK_StoreId { get; set; }
    public string StoreName { get; set; }
    public string FromStoreName { get; set; }
    public string ToStoreName { get; set; }
    public List<CommonModel> AvailableUnits { get; set; } = new();
    public string? Description { get; set; }
    public int? RefDtlsId { get; set; }
    public string? RefType { get; set; }
    public int? FK_DeliveryNoteDtlsId { get; set; }
    public int? FK_SalesInvoiceDtlsId { get; set; }
    public decimal DeliveredQty { get; set; } // Quantity that has been delivered via Delivery Note
}
