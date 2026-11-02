namespace AshproSAASERP.Models.ViewModel;

public record LoginModel
{
    [Required(ErrorMessage = "User Name is required. *")]
    public string UserName { get; set; }
    [Required(ErrorMessage = "Password is required. *")]
    public string Password { get; set; }
    public string BranchName { get; set; }
    public string FYear { get; set; }
}
