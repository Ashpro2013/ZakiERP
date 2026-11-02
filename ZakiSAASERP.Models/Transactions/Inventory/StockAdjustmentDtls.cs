using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.Transactions.Inventory;

[Table("StockAdjustmentDtls")]
public class StockAdjustmentDtls
{
    [Key]
    public int Id { get; set; }

    public int FK_StockAdjId { get; set; }

    public int FK_ItemId { get; set; }

    public int FK_UnitId { get; set; }

    public int FK_StoreId { get; set; }

    /// <summary>
    /// Quantity adjusted. Positive adds to stock, Negative removes from stock.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Valuation rate of the item at the time of adjustment
    /// </summary>
    public decimal Rate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    // Navigation Properties
    [ForeignKey("FK_StockAdjId")]
    public virtual StockAdjustmentHdr? Header { get; set; }

    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    [NotMapped] public int SL { get; set; } = 1;
    [NotMapped] public string? Barcode { get; set; }
    [NotMapped] public string? ItemName { get; set; }
    [NotMapped] public string? UnitName { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal QtyPerUnit { get; set; }
    public decimal CurrentQty { get; set; }
    public decimal NewQty { get; set; }
}
