namespace AshproSAASERP.Models.Company;

public class ExLCompany : LoginCompany
{
    [Required(ErrorMessage = "User Name is required")]
    public string UserName { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string PassWord { get; set; }
}
