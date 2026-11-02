namespace ZakiSAASERP.Models.User;

[Table("UserPrivilegeHdr")]
public class UserPrivilegeHdr
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int FK_BranchId { get; set; }
}
