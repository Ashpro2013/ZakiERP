namespace ZakiSAASERP.Models.Transactions.Purchase;

[Table("SupplierPaymentHdr")]
public class SupplierPaymentHdr
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    public string VoucherNo { get; set; }

    public DateTime? PaidDate { get; set; } = DateTime.Now;

    public int FK_SupplierId { get; set; }

    [NotMapped]
    public string SupplierName { get; set; }

    [NotMapped]
    public string? SupplierCode { get; set; }

    public string? RVNo { get; set; }

    public int FK_CashBankId { get; set; }

    [NotMapped]
    public string? CashBankName { get; set; }

    public string? PayMode { get; set; } = "CASH";

    public string? ChequeNo { get; set; }

    public DateTime? ChequeDate { get; set; }

    public string? Bank { get; set; }

    public int FK_SalesmanId { get; set; }

    [NotMapped]
    public string? SalesmanName { get; set; }

    [NotMapped]
    public string? SalesmanCode { get; set; }

    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal AdvanceAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal AllocatedAmount { get; set; }

    public int FK_ProjectId { get; set; }

    [NotMapped]
    public string? ProjectName { get; set; }

    [NotMapped]
    public bool IsPurchaseWise { get; set; } = true;

    [NotMapped]
    public bool IsAdvanceOnly { get; set; }

    public string? Narration { get; set; }

    public int FK_BranchId { get; set; }

    public int FK_UserId { get; set; }

    public int FK_FinancialYearId { get; set; }

    public bool IsDeleted { get; set; }

    [NotMapped]
    public decimal Balance { get; set; }

    public DateTime? EntryDate { get; set; }

    [NotMapped]
    public string? DiscountDetails { get; set; }

    [NotMapped]
    public decimal ExpenseWithVAT { get; set; }
}
