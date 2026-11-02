using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Purchase;

[Table("PurchaseOrderHdr")]
public class PurchaseOrderHdr
{
    [Key]
    public int PurchaseOrderId { get; set; }
    [Required(ErrorMessage = "Purchase Order No is required")]
    public string PurchaseOrderNo { get; set; } = default!;
    public string? RefNo { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal LineDsicount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Remarks { get; set; }
    public string? DeliveryTerms { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTo { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int FK_BranchId { get; set; }
    public int FK_SupplierId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }
    public int? FK_PurchaserId { get; set; }
    public DateTime? PurchaseOrderDate { get; set; }
    public DateTime? EntryDate { get; set; } = DateTime.Now;

    [ForeignKey(nameof(FK_BranchId))]
    public virtual BranchMaster? Branch { get; set; }

    [ForeignKey(nameof(FK_SupplierId))]
    public virtual SupplierMaster? Supplier { get; set; }

    [ForeignKey(nameof(FK_UserId))]
    public virtual UserDetails? User { get; set; }

    [ForeignKey(nameof(FK_FinancialYearId))]
    public virtual FinancialYear? FinancialYear { get; set; }

    [ForeignKey(nameof(FK_PurchaserId))]
    public virtual PurchaserMaster? Purchaser { get; set; }

    [InverseProperty("PurchaseOrderHdr")]
    public List<PurchaseOrderDtls> PurchaseOrderDtls { get; set; } = new();
}
