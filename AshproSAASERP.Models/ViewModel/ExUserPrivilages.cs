using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.ViewModel;

public class ExUserPrivilege : UserPrivilegeDtls
{
    public int MenuId { get; set; }
    public string MenuCaption { get; set; }
    public string MenuTitle { get; set; }
}
