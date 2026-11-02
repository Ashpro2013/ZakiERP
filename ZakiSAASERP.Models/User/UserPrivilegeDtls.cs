namespace ZakiSAASERP.Models.User;

[Table("UserPrivilegeDtls")]
public class UserPrivilegeDtls
{
    [Key]
    public int Id { get; set; }
    public int FK_UserPrivilegeHdrId { get; set; }
    public int FK_MenuDetailsId { get; set; }
    public bool Visible { get; set; }
    public bool SaveData { get; set; }
    public bool EditData { get; set; }
    public bool DeleteData { get; set; }

    [ForeignKey(nameof(FK_UserPrivilegeHdrId))]
    public virtual UserPrivilegeHdr? UserPrivilegeHdr { get; set; }
    [ForeignKey(nameof(FK_MenuDetailsId))]
    public virtual MenuDetails? MenuDetails { get; set; }
}
