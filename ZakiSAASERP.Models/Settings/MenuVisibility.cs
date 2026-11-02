namespace ZakiSAASERP.Models.Settings;

[Table("MenuVisibility")]
public class MenuVisibility
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int MenuDetailId { get; set; }

    public bool IsVisible { get; set; } = true;

    public DateTime LastModified { get; set; } = DateTime.Now;
}
