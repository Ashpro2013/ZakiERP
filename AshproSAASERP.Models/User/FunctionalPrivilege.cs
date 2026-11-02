namespace AshproSAASERP.Models.User;

[Table("FunctionalPrivilege")]
public class FunctionalPrivilege
{
    [Key]
    public int id { get; set; }
    public int FK_UserPrivilegeHdrId { get; set; }
    public int FK_FunctionDetailsId { get; set; }
    public bool Enable { get; set; }

    [ForeignKey(nameof(FK_UserPrivilegeHdrId))]
    public virtual UserPrivilegeHdr? UserPrivilegeHdr { get; set; }
    [ForeignKey(nameof(FK_FunctionDetailsId))]
    public virtual FunctionDetails? FunctionDetails { get; set; }
}
