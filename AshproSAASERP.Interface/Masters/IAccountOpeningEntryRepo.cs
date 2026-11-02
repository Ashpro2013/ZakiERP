namespace AshproSAASERP.Interface.Masters
{
    public interface IAccountOpeningEntryRepo
    {
        Task<List<AccountOpeningEntryHdr>> GetAllAsync();
        Task<AccountOpeningEntryHdr> GetAsync(int id);
        Task<OutputResponse> CreateAsync(AccountOpeningEntryHdr value, List<AccountOpeningEntryDtls> details);
        Task<OutputResponse> UpdateAsync(AccountOpeningEntryHdr value, List<AccountOpeningEntryDtls> details);
        Task<OutputResponse> DeleteAsync(int id);
        Task<(List<AccountOpeningEntryHdr>, int)> GetAllWithPagination(PaginationModel pagination);
        Task<string> GetNewDocNo(int branchId);
        Task<List<AccountOpeningEntryDtls>> GetExistingOpeningBalances(int branchId);
        Task<AccountOpeningEntryDTO> GetFullAsync(int id);
    }
}
