using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.ViewModel;

public class ExUserPrivilege : UserPrivilegeDtls
{
    public int MenuId { get; set; }
    public string MenuCaption { get; set; } = string.Empty;
    public string MenuTitle { get; set; } = string.Empty;
    public string? ArabicCaption { get; set; }
    public string? ArabicTitle { get; set; }
}

