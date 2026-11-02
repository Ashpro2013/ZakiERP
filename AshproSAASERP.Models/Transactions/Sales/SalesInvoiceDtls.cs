using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.Transactions.Sales;

[Table("SalesInvoiceDtls")]
public class SalesInvoiceDtls
{
    [Key]
    public int SalesInvoiceDtlsId { get; set; }
    public int FK_SalesInvoiceHdrId { get; set; }
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

    [ForeignKey("FK_SalesInvoiceHdrId")]
    public virtual SalesInvoiceHdr? SalesInvoiceHdr { get; set; }
    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }
    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }

    public int? FK_StoreId { get; set; }
    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }

    public int? FK_DeliveryNoteDtlsId { get; set; }
    public decimal DeliveredQty { get; set; }
}
