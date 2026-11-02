namespace ZakiSAASERP.Models.Settings;

[Table("ClientERPSettings")]
public class ClientERPSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ERPTypeId { get; set; } = 1; // Default to Trading

    public DateTime LastModified { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string? ModifiedBy { get; set; }
}
