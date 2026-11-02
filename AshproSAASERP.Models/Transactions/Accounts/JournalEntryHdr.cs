using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.Transactions.Accounts;

[Table("JournalEntryHdr")]
public class JournalEntryHdr
{
    [Key]
    public int JournalId { get; set; }

    [Required(ErrorMessage = "Journal No is required. *")]
    [StringLength(50)]
    public string JournalNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Journal Date is required. *")]
    public DateTime JournalDate { get; set; } = DateTime.Now;

    [Range(1, int.MaxValue, ErrorMessage = "Branch is required. *")]
    public int FK_BranchId { get; set; }

    public int? FK_UserId { get; set; }

    [StringLength(2000)]
    public string? Narration { get; set; }

    [StringLength(50)]
    public string? ReferenceNo { get; set; }

    public int? FK_ProjectId { get; set; }

    public int? FK_SalesmanId { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal TotalDebit { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal TotalCredit { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Draft";

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    public DateTime? ModifiedDate { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FK_BranchId))]
    public BranchMaster? Branch { get; set; }

    [ForeignKey(nameof(FK_UserId))]
    public UserDetails? User { get; set; }

    public List<JournalEntryDtls> Details { get; set; } = new();
}
