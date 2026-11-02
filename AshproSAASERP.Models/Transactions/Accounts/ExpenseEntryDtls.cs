using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.Transactions.Accounts;

[Table("ExpenseEntryDtls")]
public class ExpenseEntryDtls
{
    [Key]
    public int ExpenseDtlId { get; set; }

    [Required]
    [Column("FK_ExpenseEntryId")]
    public int FK_ExpenseEntryId { get; set; }

    [Required(ErrorMessage = "Expense Account is required. *")]
    [Range(1, int.MaxValue, ErrorMessage = "Expense Account is required. *")]
    [Column("FK_AccountId")]
    public int FK_AccountId { get; set; }

    [Column("FK_SupplierId")]
    public int? FK_SupplierId { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal Amount { get; set; }

    [Column("VatPer", TypeName = "decimal(18, 3)")]
    public decimal VatPer { get; set; }

    [Column("VatAmt", TypeName = "decimal(18, 3)")]
    public decimal VatAmt { get; set; }

    [Column("NetAmt", TypeName = "decimal(18, 3)")]
    public decimal IncAmt { get; set; }

    [StringLength(500)]
    public string? InvoiceNo { get; set; }

    [StringLength(2000)]
    public string? Narration { get; set; }

    [StringLength(500)]
    [Column("VatNo")]
    public string? VatNo { get; set; }


    [Column("LineNo")]
    public int LineNo { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(FK_ExpenseEntryId))]
    public ExpenseEntryHdr? ExpenseHdr { get; set; }

    [ForeignKey(nameof(FK_AccountId))]
    public AccountMaster? Account { get; set; }

    [ForeignKey(nameof(FK_SupplierId))]
    public SupplierMaster? Supplier { get; set; }

    // NotMapped properties for UI
    [NotMapped]
    public string? AccountName { get; set; }

    [NotMapped]
    public string? AccountCode { get; set; }

    [NotMapped]
    public string? SupplierName { get; set; }

    [NotMapped]
    public int FilterAccountTypeId { get; set; }

    [NotMapped]
    public int? VatId { get; set; }
}
