using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.Transactions.Purchase;

[Table("GoodsReceiptDtls")]
public class GoodsReceiptDtls
{
    [Key]
    public int GoodsReceiptDtlsId { get; set; }
    public int FK_GoodsReceiptHdrId { get; set; }
    public int FK_ItemId { get; set; }
    [Required(ErrorMessage = "Item Name is required")]
    public string ItemName { get; set; }
    public string? ItemArabicName { get; set; }
    public string? Barcode { get; set; }
    public int FK_UnitId { get; set; }

    [NotMapped]
    public string? UnitName { get; set; }
    [NotMapped]
    public int SL { get; set; }
    [NotMapped]
    private string? _itemCode;
    [NotMapped]
    public string ItemCode
    {
        get => _itemCode ?? (FK_ItemId > 0 ? FK_ItemId.ToString("D5") : "");
        set => _itemCode = value;
    }

    public decimal Quantity { get; set; }
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

    [ForeignKey("FK_GoodsReceiptHdrId")]
    public virtual GoodsReceiptHdr? GoodsReceiptHdr { get; set; }
    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    public int? FK_StoreId { get; set; }
    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }
    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }
}
