using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.Transactions.Purchase;

[Table("PurchaseHdr")]
public class PurchaseHdr
{
    [Key]
    public int PurchaseId { get; set; }

    public string? PurchaseNo { get; set; }

    public string? RefNo { get; set; }
    public string? InvoiceNo { get; set; }
    public string? PONo { get; set; }
    public DateTime? PODate { get; set; }
    public string? GoodsRecNo { get; set; }
    public string? RFQNo { get; set; }

    // Financial Totals (following Quotation pattern)
    public decimal GrossAmount { get; set; }
    public decimal LineDsicount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? AdditionalAmount { get; set; }
    public decimal? AdditionalDiscPertge { get; set; }
    public decimal? AdditionalDiscAmnt { get; set; }
    public int AdditionalDiscountType { get; set; } = 1;
    public decimal GrandTotal { get; set; }

    // Payment Details
    public string? BillType { get; set; }
    public decimal? ReceivedAmount { get; set; }
    public decimal? PaymentAmount { get; set; }

    // Supplier Information
    public int FK_SupplierId { get; set; }
    public string? SupplierName { get; set; }
    [RegularExpression(@"^\d{15}$", ErrorMessage = "VAT No must be exactly 15 digits")]
    public string? SupplierVatNo { get; set; }

    // General Fields
    public string? Remarks { get; set; }
    public string? Narration { get; set; }
    public string? Purchaser { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Foreign Keys
    public int FK_BranchId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }
    public int? FK_PurchaserId { get; set; }
    public int? FK_ProjectId { get; set; }
    public int? FK_StoreId { get; set; }
    public int? FK_CurrencyId { get; set; }
    public int? FK_AccountId { get; set; }

    // Dates
    public DateTime? PurchaseDate { get; set; }
    public DateTime? EntryDate { get; set; } = DateTime.Now;

    // Navigation Properties
    [ForeignKey("FK_BranchId")]
    public virtual BranchMaster? Branch { get; set; }

    [ForeignKey("FK_SupplierId")]
    public virtual SupplierMaster? Supplier { get; set; }

    [ForeignKey("FK_UserId")]
    public virtual UserDetails? User { get; set; }

    [ForeignKey("FK_FinancialYearId")]
    public virtual FinancialYear? FinancialYear { get; set; }

    [ForeignKey("FK_PurchaserId")]
    public virtual PurchaserMaster? PurchaserMaster { get; set; }

    [ForeignKey("FK_ProjectId")]
    public virtual ProjectMaster? Project { get; set; }

    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }

    public List<PurchaseDtls> PurchaseDtls { get; set; } = new();
}
