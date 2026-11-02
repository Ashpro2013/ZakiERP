using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.ViewModel;

public class ExFuncDetails : FunctionDetails
{
    public int SL { get; set; }
    public string MenuCaption { get; set; }
    public bool Enable { get; set; }
}