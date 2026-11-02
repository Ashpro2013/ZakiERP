namespace ZakiSAASERP.Models.User;

[Table("UserBranch")]
public class UserBranch
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int FK_BranchId { get; set; }

    [Required]
    public int FK_UserId { get; set; }
}

