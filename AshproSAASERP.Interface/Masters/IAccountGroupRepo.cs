namespace AshproSAASERP.Interface.Masters;

public interface IAccountGroupRepo : ICommonInterface<AccountGroup>
{
    Task<OutputResponse> GetNextCode(int accountHeadId);
}
