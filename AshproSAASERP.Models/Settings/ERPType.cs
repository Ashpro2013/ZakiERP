namespace AshproSAASERP.Models.Settings;

[Table("ERPType")]
public class ERPType
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string TypeName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsCustom { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
