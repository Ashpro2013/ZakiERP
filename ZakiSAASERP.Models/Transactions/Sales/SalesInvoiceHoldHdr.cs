using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Sales;

[Table("SalesInvoiceHoldHdr")]
public class SalesInvoiceHoldHdr
{
    [Key]
    public int SalesInvoiceHoldHdrId { get; set; }
    [Required(ErrorMessage = "Sales Invoice No is required")]
    public string SalesInvoiceNo { get; set; } = default!;
    public string? CustomerName { get; set; }
    public string? RefNo { get; set; }
    [NotMapped]
    public string? Terminal
    {
        get => RefNo;
        set => RefNo = value;
    }
    public decimal GrossAmount { get; set; }
    public decimal LineDsicount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? AdditionalAmount { get; set; }
    public decimal? Freight { get; set; }
    public decimal? AdditionalDiscPertge { get; set; }
    public decimal? AdditionalDiscAmnt { get; set; }
    public decimal GrandTotal { get; set; }
    public int AdditionalDiscountType { get; set; } = 1;
    public string? Remarks { get; set; }
    public int FK_BranchId { get; set; }
    public int FK_CustomerId { get; set; }
    public int FK_SalesManId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }
    public int? FK_ProjectId { get; set; }
    public DateTime? SalesInvoiceDate { get; set; }
    public DateTime? EntryDate { get; set; } = DateTime.Now;
    public int? DeliveryNoteId { get; set; }
    public string? DeliveryNoteIds { get; set; } // Comma-separated list of DN IDs when multiple DNs are used
    public int FK_StoreId { get; set; }
    public string? HijriDate { get; set; }
    public string? InvoiceQRCode { get; set; }
    public int? CurrencyId { get; set; }
    public decimal? ExchangeRate { get; set; } = 1;
    public string? BillType { get; set; } = "Cash";
    public int? QuotationId { get; set; }
    [NotMapped]
    public string? QuotationNo { get; set; }
    [NotMapped]
    public decimal? Tendered { get; set; }
    [NotMapped]
    public decimal? Changes { get; set; }
    public int? SalesOrderId { get; set; }
    public int? ProformaInvoiceId { get; set; }
    [ForeignKey(nameof(FK_BranchId))]
    public virtual BranchMaster? Branch { get; set; }
    [ForeignKey(nameof(FK_CustomerId))]
    public virtual CustomerMaster? Customer { get; set; }
    [ForeignKey(nameof(FK_SalesManId))]
    public virtual SalesManMaster? SalesMan { get; set; }
    [ForeignKey(nameof(FK_UserId))]
    public virtual UserDetails? User { get; set; }
    [ForeignKey(nameof(FK_FinancialYearId))]
    public virtual FinancialYear? FinancialYear { get; set; }
    [ForeignKey(nameof(FK_ProjectId))]
    public virtual ProjectMaster? Project { get; set; }
    [InverseProperty("SalesInvoiceHoldHdr")]
    public List<SalesInvoiceHoldDtls> SalesInvoiceHoldDtls { get; set; } = new();
}
