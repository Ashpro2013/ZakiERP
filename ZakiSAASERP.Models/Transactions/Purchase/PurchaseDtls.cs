using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.Transactions.Purchase;

[Table("PurchaseDtls")]
public class PurchaseDtls
{
    [Key]
    public int PurchaseDtlsId { get; set; }

    public int FK_PurchaseHdrId { get; set; }
    public int FK_ItemId { get; set; }

    public string? ItemName { get; set; }
    public string? Barcode { get; set; }

    public int FK_UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Damage { get; set; } = 0;
    public decimal QtyPerUnit { get; set; }

    public decimal ExRate { get; set; }  // Purchase Rate
    public decimal InRate { get; set; }  // Inclusive Rate

    public decimal GrossAmount { get; set; }
    public decimal DiscountPerc { get; set; }
    public decimal DiscountAmnt { get; set; }
    public decimal TaxableAmnt { get; set; }
    public decimal TaxPerc { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    // Purchase-Specific Costing
    public decimal? LandingCost { get; set; }

    // Additional Fields
    public int? FK_StoreId { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Source Link
    public int? FK_SourceDtlsId { get; set; }

    // Navigation Properties
    [ForeignKey("FK_PurchaseHdrId")]
    public virtual PurchaseHdr? PurchaseHdr { get; set; }

    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }
}
