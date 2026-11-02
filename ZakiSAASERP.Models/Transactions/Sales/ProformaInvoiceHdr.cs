using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Sales;

[Table("ProformaInvoiceHdr")]
public class ProformaInvoiceHdr
{
    [Key]
    public int ProformaInvoiceId { get; set; }
    [Required(ErrorMessage = "Proforma Invoice No is required")]
    public string ProformaInvoiceNo { get; set; } = default!;
    public string? CustomerName { get; set; }
    public string? RefNo { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal LineDsicount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? AdditionalAmount { get; set; }
    public decimal? AdditionalDiscPertge { get; set; }
    public decimal? AdditionalDiscAmnt { get; set; }
    public decimal? InclusiveDiscAmnt { get; set; }  // Inc Dis Amount = BaseDiscount + VAT on Discount
    public decimal GrandTotal { get; set; }
    public int AdditionalDiscountType { get; set; } = 1;
    public string? Remarks { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int FK_BranchId { get; set; }
    public int FK_CustomerId { get; set; }
    public int FK_SalesManId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }
    public int? FK_ProjectId { get; set; }
    public DateTime? ProformaInvoiceDate { get; set; }
    public DateTime? EntryDate { get; set; } = DateTime.Now;

    // Additional Business Fields
    public string? BillType { get; set; } // Credit, Cash, Card, Cheque, Transfer

    public string? CustomerVatNo { get; set; }
    public DateTime? DueDate { get; set; }
    public int? CreditDays { get; set; }
    public int? FK_StoreId { get; set; }


    // UI Optimization Fields
    public string? HijriDate { get; set; }

    public int? FK_AccountId { get; set; }
    [NotMapped]
    public string? AccountNo { get; set; }
    [NotMapped]
    public string? AccountName { get; set; }
    public string? Attention { get; set; }
    public string? DeliveryTo { get; set; }
    public string? SectionNo { get; set; }

    // Source Document Tracking
    public string? SourceDocType { get; set; }
    public string? SourceDocNo { get; set; }
    public int? QuotationId { get; set; }


    [ForeignKey("FK_BranchId")]
    public virtual BranchMaster? Branch { get; set; }
    [ForeignKey("FK_CustomerId")]
    public virtual CustomerMaster? Customer { get; set; }
    [ForeignKey("FK_SalesManId")]
    public virtual SalesManMaster? SalesMan { get; set; }
    [ForeignKey("FK_UserId")]
    public virtual UserDetails? User { get; set; }
    [ForeignKey("FK_FinancialYearId")]
    public virtual FinancialYear? FinancialYear { get; set; }
    [ForeignKey("FK_ProjectId")]
    public virtual ProjectMaster? Project { get; set; }


    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }

    public List<ProformaInvoiceDtls> ProformaInvoiceDtls { get; set; } = new();
}
