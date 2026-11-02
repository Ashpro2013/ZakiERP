using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.Transactions.Sales;

[Table("ProformaInvoiceDtls")]
public class ProformaInvoiceDtls
{
    [Key]
    public int ProformaInvoiceDtlsId { get; set; }
    public int FK_ProformaInvoiceHdrId { get; set; }
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

    // Source Link
    public int? FK_SourceDtlsId { get; set; }

    [ForeignKey("FK_ProformaInvoiceHdrId")]
    public virtual ProformaInvoiceHdr? ProformaInvoiceHdr { get; set; }
    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }
    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }
    public int? FK_StoreId { get; set; }
    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }
}
