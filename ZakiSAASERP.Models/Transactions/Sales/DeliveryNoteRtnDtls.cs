using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.Transactions.Sales;

[Table("DeliveryNoteRtnDtls")]
public class DeliveryNoteRtnDtls
{
    [Key]
    public int DeliveryNoteRtnDtlsId { get; set; }

    public int FK_DeliveryNoteRtnHdrId { get; set; }

    public int FK_ItemId { get; set; }

    [Required(ErrorMessage = "Item Name is required")]
    public string ItemName { get; set; } = default!;

    public string? Barcode { get; set; }

    public int FK_UnitId { get; set; }

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

    public bool IsDeleted { get; set; }

    [ForeignKey("FK_DeliveryNoteRtnHdrId")]
    public virtual DeliveryNoteRtnHdr? DeliveryNoteRtnHdr { get; set; }

    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    public int? FK_StoreId { get; set; }
    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }
}
