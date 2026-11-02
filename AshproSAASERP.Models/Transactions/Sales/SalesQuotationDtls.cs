using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.Transactions.Sales;

[Table("SalesQuotationDtls")]
public class SalesQuotationDtls
{
    [Key]
    public int SalesQuotationDtlsId { get; set; }
    public int FK_SalesQuotationHdrId { get; set; }
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
    public bool IsDeleted { get; set; } = false;
    [ForeignKey("FK_SalesQuotationHdrId")]
    public virtual SalesQuotationHdr? SalesQuotationHdr { get; set; }
    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    public int? FK_StoreId { get; set; }
    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }
}

