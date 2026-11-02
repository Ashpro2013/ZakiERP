namespace ZakiSAASERP.Models.Masters;

[Table("UnitMaster")]
public class UnitMaster
{
    [Key]
    public int UnitId { get; set; }
    [Required(ErrorMessage = "Unit Code is required. *")]
    public string Code { get; set; }
    [Required(ErrorMessage = "Unit Name is required. *")]
    public string Name { get; set; }
    public string? Remarks { get; set; }
}
