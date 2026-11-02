using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.Transactions.Accounts;

[Table("JournalEntryDtls")]
public class JournalEntryDtls
{
    [Key]
    public int JournalDtlId { get; set; }

    [Required]
    public int FK_JournalId { get; set; }

    [Required(ErrorMessage = "Account is required. *")]
    [Range(1, int.MaxValue, ErrorMessage = "Account is required. *")]
    public int FK_AccountId { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal DebitAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal CreditAmount { get; set; }

    [StringLength(500)]
    public string? Narration { get; set; }

    public int LineNo { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FK_JournalId))]
    public JournalEntryHdr? JournalHdr { get; set; }

    [ForeignKey(nameof(FK_AccountId))]
    public AccountMaster? Account { get; set; }

    // NotMapped properties for UI
    [NotMapped]
    public string? AccountName { get; set; }

    [NotMapped]
    public string? AccountCode { get; set; }

    [StringLength(50)]
    public string? InvoiceNo { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal InvoiceAmt { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal DebitVatper { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal CreditVatper { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal DebitVat { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal CreditVat { get; set; }

    [StringLength(50)]
    public string? VatNo { get; set; }

    [StringLength(200)]
    public string? Name { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal VatPer { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal TotalDebit { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal TotalCredit { get; set; }

    public int? VatId { get; set; }

    [StringLength(50)]
    public string? AccType { get; set; }

    [NotMapped]
    public int FilterAccountTypeId { get; set; }
}
