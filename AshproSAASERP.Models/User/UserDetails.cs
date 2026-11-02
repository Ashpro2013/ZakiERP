namespace AshproSAASERP.Models.User;

[Table("UserDetails")]
public class UserDetails
{
    [Key]
    public int UserId { get; set; }

    public int PrevilageId { get; set; }

    [Required(ErrorMessage = "UserName is required")]
    [StringLength(50, ErrorMessage = "UserName cannot exceed 50 characters")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string PassWord { get; set; } = string.Empty;

    public int FK_BranchId { get; set; }
}
