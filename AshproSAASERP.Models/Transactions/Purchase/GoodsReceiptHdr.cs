using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.Transactions.Purchase;

[Table("GoodsReceiptHdr")]
public class GoodsReceiptHdr
{
    [Key]
    public int GoodsReceiptId { get; set; }
    [Required(ErrorMessage = "Goods Receipt No is required")]
    public string GoodsReceiptNo { get; set; } = default!;
    public string? RefNo { get; set; }
    public string? OurPORefNo { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal LineDsicount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Remarks { get; set; }
    public decimal? AdditionalAmount { get; set; }
    public decimal? Freight { get; set; }
    public decimal? AdditionalDiscAmnt { get; set; }
    public decimal? AdditionalDiscPertge { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int FK_BranchId { get; set; }
    public int FK_SupplierId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }
    public int? FK_PurchaserId { get; set; }
    public int? FK_CurrencyId { get; set; }
    public DateTime? GoodsReceiptDate { get; set; }
    public DateTime? EntryDate { get; set; } = DateTime.Now;

    [ForeignKey("FK_BranchId")]
    public virtual BranchMaster? Branch { get; set; }
    [ForeignKey("FK_SupplierId")]
    public virtual SupplierMaster? Supplier { get; set; }
    [ForeignKey("FK_UserId")]
    public virtual UserDetails? User { get; set; }
    [ForeignKey("FK_FinancialYearId")]
    public virtual FinancialYear? FinancialYear { get; set; }
    [ForeignKey("FK_PurchaserId")]
    public virtual PurchaserMaster? Purchaser { get; set; }

    public List<GoodsReceiptDtls> GoodsReceiptDtls { get; set; } = new();
}
