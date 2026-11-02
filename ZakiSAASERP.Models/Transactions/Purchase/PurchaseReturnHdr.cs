using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Purchase;

[Table("PurchaseReturnHdr")]
public class PurchaseReturnHdr
{
    [Key]
    public int PurchaseReturnId { get; set; }
    [Required]
    public string PurchaseReturnNo { get; set; } = default!;
    public DateTime? EntryDate { get; set; } = DateTime.Now;
    public string? RefNo { get; set; }
    public int? FK_PurchaseId { get; set; }
    [NotMapped]
    public string? PurchaseVoucherNo { get; set; } // For UI display
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public int? FK_SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? BillType { get; set; } // Cash, Bank, Credit
    public int? FK_AccountId { get; set; }
    public string? AccountNo { get; set; }
    public int? CurrencyId { get; set; }
    public decimal? CurrencyRate { get; set; }
    public int? FK_PurchaserId { get; set; }
    public int? FK_ProjectId { get; set; }
    public string? Remarks { get; set; }

    public decimal GrossAmount { get; set; }
    public decimal LineDsicount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? AdditionalAmount { get; set; }
    public decimal? Freight { get; set; }
    public decimal? AdditionalDiscPertge { get; set; }
    public decimal? AdditionalDiscAmnt { get; set; }
    public int AdditionalDiscountType { get; set; } = 1;
    public decimal GrandTotal { get; set; }

    public bool IsDeleted { get; set; } = false;
    public int FK_BranchId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }
    public DateTime? PurchaseReturnDate { get; set; }

    [ForeignKey("FK_BranchId")]
    public virtual BranchMaster? Branch { get; set; }
    [ForeignKey("FK_SupplierId")]
    public virtual SupplierMaster? Supplier { get; set; }
    [ForeignKey("FK_PurchaserId")]
    public virtual PurchaserMaster? Purchaser { get; set; }
    [ForeignKey("FK_UserId")]
    public virtual UserDetails? User { get; set; }
    [ForeignKey("FK_FinancialYearId")]
    public virtual FinancialYear? FinancialYear { get; set; }
    [ForeignKey("FK_ProjectId")]
    public virtual ProjectMaster? Project { get; set; }
    [ForeignKey("CurrencyId")]
    public virtual CurrencyMaster? Currency { get; set; }
    [ForeignKey("FK_PurchaseId")]
    public virtual PurchaseHdr? Purchase { get; set; }

    public List<PurchaseReturnDtls> PurchaseReturnDtls { get; set; } = new();
}
