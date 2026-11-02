namespace ZakiSAASERP.Models.Transactions.Purchase;

[Table("SupplierPaymentDtls")]
public class SupplierPaymentDtls
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int FK_PaymentId { get; set; }

    public int? FK_PurchaseId { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal InvoiceAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal OutstandingAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal PayingAmount { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal RemainingBalance { get; set; }

    [StringLength(500)]
    public string? Narration { get; set; }

    public bool IsDeleted { get; set; }

    // NotMapped properties for UI and backward compatibility
    [NotMapped]
    public string? PurchaseNo { get; set; }

    [NotMapped]
    public DateTime? PurchaseDate { get; set; }

    [NotMapped]
    public string? SupplierInvoice { get; set; }

    [NotMapped]
    public bool IsSelected { get; set; }

    // Backward compatibility properties
    [NotMapped]
    public decimal PurchaseAmount
    {
        get => InvoiceAmount;
        set => InvoiceAmount = value;
    }

    [NotMapped]
    public decimal PaidAmt
    {
        get => PaidAmount;
        set => PaidAmount = value;
    }

    [NotMapped]
    public decimal DiscountReceived { get; set; }

    [NotMapped]
    public decimal PendingAmt { get; set; }

    [NotMapped]
    public decimal ReturnedAmount { get; set; } // New Field

    [NotMapped]
    public bool IsOpeningEntry { get; set; }

    [NotMapped]
    public string? OpeningInvoiceNo { get; set; } // To identify which opening entry

    [NotMapped]
    public decimal Balance
    {
        get => RemainingBalance;
        set => RemainingBalance = value;
    }
}
