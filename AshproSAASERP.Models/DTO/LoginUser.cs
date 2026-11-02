namespace AshproSAASERP.Models.DTO;

public class LoginRequest
{
    public string EmailOrUserName { get; set; } = string.Empty; // e.g. ashraf@ASH
    public string Password { get; set; } = string.Empty;
    public bool IsMaui { get; set; } = false;
}
