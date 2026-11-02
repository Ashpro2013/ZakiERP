using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Accounts;

[Table("CustomerReceiptHdr")]
public class CustomerReceiptHdr
{
    [Key]
    public int CustomerReceiptId { get; set; }

    [Required(ErrorMessage = "Receipt No is required. *")]
    [StringLength(50)]
    public string ReceiptNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Receipt Date is required. *")]
    public DateTime ReceiptDate { get; set; } = DateTime.Now;

    [Range(1, int.MaxValue, ErrorMessage = "Customer is required. *")]
    public int FK_CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Branch is required. *")]
    public int FK_BranchId { get; set; }

    public int? FK_UserId { get; set; }

    public int? FK_ProjectId { get; set; }

    public int? FK_SalesManId { get; set; }

    [Required]
    [StringLength(50)]
    public string PaymentMode { get; set; } = "Cash";

    [StringLength(200)]
    public string? BankName { get; set; }

    [StringLength(50)]
    public string? ChequeNo { get; set; }

    public DateTime? ChequeDate { get; set; }

    [StringLength(50)]
    public string? BankAccountNo { get; set; }

    [StringLength(100)]
    public string? RvNumber { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal AdvanceAmount { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal AllocatedAmount { get; set; }

    [StringLength(2000)]
    public string? Remarks { get; set; }

    public int? FK_AccountId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? ModifiedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(FK_CustomerId))]
    public CustomerMaster? Customer { get; set; }

    [ForeignKey(nameof(FK_BranchId))]
    public BranchMaster? Branch { get; set; }

    [ForeignKey(nameof(FK_UserId))]
    public UserDetails? User { get; set; }

    [ForeignKey(nameof(FK_ProjectId))]
    public ProjectMaster? Project { get; set; }

    [ForeignKey(nameof(FK_AccountId))]
    public AccountMaster? Account { get; set; }

    [ForeignKey(nameof(FK_SalesManId))]
    public SalesManMaster? SalesMan { get; set; }

    public List<CustomerReceiptDtls> Details { get; set; } = new();
}
