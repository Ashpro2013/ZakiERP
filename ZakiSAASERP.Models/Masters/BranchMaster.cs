namespace ZakiSAASERP.Models.Masters;

[Table("BranchMaster")]
public class BranchMaster
{
    [Key]
    public int BranchId { get; set; }
    [Required(ErrorMessage = "Branch Code is required. *")]
    public string Code { get; set; }
    [Required(ErrorMessage = "Branch Name is required. *")]
    public string Name { get; set; }
    public string? Remarks { get; set; }
    public int? AccountId { get; set; }
    public bool ShowInLogin { get; set; } = true;
}
