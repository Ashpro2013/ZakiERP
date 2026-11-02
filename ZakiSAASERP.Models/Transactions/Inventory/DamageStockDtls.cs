using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.Transactions.Inventory;

[Table("DamageStockDtls")]
public class DamageStockDtls
{
    [Key]
    public int DamageStockDtlsId { get; set; }
    public int FK_DamageStockHdrId { get; set; }
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

    public decimal Quantity { get; set; }
    public decimal QtyPerUnit { get; set; } = 1;
    public decimal Cost { get; set; }
    public decimal Rate { get; set; } // Added Rate as per screenshot (even if cost is relevant for damage, screenshot showed rate/value)
    public decimal Value { get; set; } // Total for the line

    public bool IsDeleted { get; set; } = false;

    [ForeignKey("FK_DamageStockHdrId")]
    public virtual DamageStockHdr? DamageStockHdr { get; set; }
    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    public int? FK_StoreId { get; set; }
    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }

    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }
}
