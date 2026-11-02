namespace AshproSAASERP.Models.Settings;

[Table("ERPTypePreset")]
public class ERPTypePreset
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ERPTypeId { get; set; }

    [Required]
    public int MenuDetailId { get; set; }

    public bool IsVisible { get; set; } = true;
}
