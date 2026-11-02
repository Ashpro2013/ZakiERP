namespace ZakiSAASERP.Models.Company;

[Table("AdminUsers")]
public class AdminUser
{
    [Key]
    public int UserId { get; set; }
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
}
