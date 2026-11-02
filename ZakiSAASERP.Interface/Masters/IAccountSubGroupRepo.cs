namespace ZakiSAASERP.Interface.Masters;

public interface IAccountSubGroupRepo : ICommonInterface<AccountSubGroup>
{
    Task<OutputResponse> GetNextCode(int accountGroupId);
}
