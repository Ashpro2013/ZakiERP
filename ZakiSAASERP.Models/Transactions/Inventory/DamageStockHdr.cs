using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Inventory;

[Table("DamageStockHdr")]
public class DamageStockHdr
{
    [Key]
    public int DamageStockId { get; set; }
    [Required(ErrorMessage = "Damage Stock No is required")]
    public string DamageStockNo { get; set; } = default!;
    public DateTime? EntryDate { get; set; } = DateTime.Now;
    public string? Remarks { get; set; }

    // Simplified Account/Project as requested
    public int? FK_ProjectId { get; set; }
    public int? FK_AccountId { get; set; }

    public decimal TotalValue { get; set; }

    public bool IsDeleted { get; set; } = false;
    public int FK_BranchId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }

    [ForeignKey("FK_BranchId")]
    public virtual BranchMaster? Branch { get; set; }
    [ForeignKey("FK_UserId")]
    public virtual UserDetails? User { get; set; }
    [ForeignKey("FK_FinancialYearId")]
    public virtual FinancialYear? FinancialYear { get; set; }
    [ForeignKey("FK_ProjectId")]
    public virtual ProjectMaster? Project { get; set; }
    [ForeignKey("FK_AccountId")]
    public virtual AccountMaster? Account { get; set; }

    public List<DamageStockDtls> DamageStockDtls { get; set; } = new();
}
