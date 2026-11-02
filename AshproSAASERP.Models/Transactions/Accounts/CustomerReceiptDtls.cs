namespace AshproSAASERP.Models.Transactions.Accounts;

[Table("CustomerReceiptDtls")]
public class CustomerReceiptDtls
{
    [Key]
    public int CustomerReceiptDtlsId { get; set; }

    [Required]
    public int FK_CustomerReceiptId { get; set; }

    public int? FK_InvoiceId { get; set; }

    [Required]
    [StringLength(50)]
    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime? InvoiceDate { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal InvoiceAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal ReturnAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal OutstandingAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal PaymentAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal RemainingBalance { get; set; }

    [StringLength(500)]
    public string? Narration { get; set; }

    public int RecordNo { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FK_CustomerReceiptId))]
    public CustomerReceiptHdr? Receipt { get; set; }

    [ForeignKey(nameof(FK_InvoiceId))]
    public SalesInvoiceHdr? Invoice { get; set; }

    // NotMapped properties for UI
    [NotMapped]
    public string? CustomerName { get; set; }

    [NotMapped]
    public bool IsSelected { get; set; }

    [NotMapped]
    public decimal RemainingAmount => OutstandingAmount - PaymentAmount - Discount;

    [NotMapped]
    public bool IsOpeningEntry { get; set; }
}
