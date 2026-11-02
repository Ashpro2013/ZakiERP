using AshproSAASERP.Models.Transactions.Accounts;

namespace AshproSAASERP.Interface.Transaction.Accounts;

public interface IJournalEntryRepo
{
    Task<string> GetNextJournalNoAsync();
    Task<OutputResponse> CreateAsync(JournalEntryDTO entity);
    Task<OutputResponse> UpdateAsync(JournalEntryDTO entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<List<JournalEntryHdr>> GetAllAsync();
    Task<JournalEntryDTO?> GetByIdAsync(int id);
}
