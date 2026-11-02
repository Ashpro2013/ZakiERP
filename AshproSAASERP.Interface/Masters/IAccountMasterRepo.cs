namespace AshproSAASERP.Interface.Masters;

public interface IAccountMasterRepo : ICommonInterface<AccountMaster>
{
    Task<OutputResponse> GetNextCode(int subGroupId);
    Task<(List<AccountMaster>, int)> GetAllWithPagination(PaginationModel pagination);
    Task<List<AccountMaster>> GetCashAndBankAccountsAsync();
}
