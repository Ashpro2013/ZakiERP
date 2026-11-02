using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.Transactions.Sales;

[Table("SalesInvoiceHoldDtls")]
public class SalesInvoiceHoldDtls
{
    [Key]
    public int SalesInvoiceHoldDtlsId { get; set; }
    public int FK_SalesInvoiceHoldHdrId { get; set; }
    public int FK_ItemId { get; set; }
    [Required(ErrorMessage = "Item Name is required")]
    public string ItemName { get; set; }
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
    public bool IsDeleted { get; set; } = false;

    [ForeignKey(nameof(FK_SalesInvoiceHoldHdrId))]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual SalesInvoiceHoldHdr? SalesInvoiceHoldHdr { get; set; }
    [ForeignKey(nameof(FK_UnitId))]
    public virtual UnitMaster? Unit { get; set; }
    [ForeignKey(nameof(FK_ItemId))]
    public virtual ItemMaster? Item { get; set; }

    public int? FK_StoreId { get; set; }
    [ForeignKey(nameof(FK_StoreId))]
    public virtual StoreMaster? Store { get; set; }

    public int? FK_DeliveryNoteDtlsId { get; set; }
    public decimal DeliveredQty { get; set; }
}
