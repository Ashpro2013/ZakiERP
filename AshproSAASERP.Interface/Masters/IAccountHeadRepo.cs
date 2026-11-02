namespace AshproSAASERP.Interface.Masters;

public interface IAccountHeadRepo : ICommonInterface<AccountHeadMaster>
{
    Task<OutputResponse> GetNextCode(int typeId);
}
