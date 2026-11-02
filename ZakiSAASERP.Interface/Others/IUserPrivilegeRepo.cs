namespace ZakiSAASERP.Interface.Others;

public interface IUserPrivilegeRepo : ICommonTranInterface<UserPrivilegeDTO>
{
    Task<List<WebUserPrivilege>> GetWebUserPrivileges(int id);
    Task<List<UserPrivilegeHdr>> GetAllAsync();
}
