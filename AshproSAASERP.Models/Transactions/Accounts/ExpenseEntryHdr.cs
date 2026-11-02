using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.Transactions.Accounts;

[Table("ExpenseEntryHdr")]
public class ExpenseEntryHdr
{
    [Key]
    public int ExpenseEntryId { get; set; }

    [Required(ErrorMessage = "Expense No is required. *")]
    [StringLength(500)]
    [Column("ExpenseNo")]
    public string ExpenseNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Entry Date is required. *")]
    public DateTime EntryDate { get; set; } = DateTime.Now;

    [Range(1, int.MaxValue, ErrorMessage = "Payment Account is required. *")]
    [Column("FK_AccountId")]
    public int FK_AccountId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Branch is required. *")]
    [Column("FK_BranchId")]
    public int FK_BranchId { get; set; }

    public int? FK_UserId { get; set; }

    [Column("FK_ProjectId")]
    public int? FK_ProjectId { get; set; }

    [Column("FK_SalesmanId")]
    public int? FK_SalesmanId { get; set; }

    [Column("FK_SupplierId")]
    public int? FK_SupplierId { get; set; }

    [Column("Gross", TypeName = "decimal(18, 3)")]
    public decimal Gross { get; set; }

    [Column("VatTotal", TypeName = "decimal(18, 3)")]
    public decimal VatTotal { get; set; }

    [Column("NetTotal", TypeName = "decimal(18, 3)")]
    public decimal NetTotal { get; set; }

    [StringLength(2000)]
    [Column("Narration")]
    public string? Narration { get; set; }

    [StringLength(500)]
    [Column("ReferenceNo")]
    public string? ReferenceNo { get; set; }

    [StringLength(50)]
    public string? BillType { get; set; }

    [StringLength(500)]
    [Column("VatNo")]
    public string? VatNo { get; set; }

    [NotMapped]
    public string TransactionType { get; set; } = "Expense";

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? ModifiedDate { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FK_AccountId))]
    public AccountMaster? PaymentAccount { get; set; }

    [ForeignKey(nameof(FK_BranchId))]
    public BranchMaster? Branch { get; set; }

    [ForeignKey(nameof(FK_UserId))]
    public UserDetails? User { get; set; }

    [ForeignKey(nameof(FK_ProjectId))]
    public ProjectMaster? Project { get; set; }

    [ForeignKey(nameof(FK_SalesmanId))]
    public SalesManMaster? Salesman { get; set; }

    [ForeignKey(nameof(FK_SupplierId))]
    public SupplierMaster? Supplier { get; set; }

    public List<ExpenseEntryDtls> ExpenseEntryDtls { get; set; } = new();

    // NotMapped properties for UI
    [NotMapped]
    public string? PaymentAccountName { get; set; }

    [NotMapped]
    public string? PaymentAccountCode { get; set; }

    [NotMapped]
    public string? SupplierName { get; set; }
}
