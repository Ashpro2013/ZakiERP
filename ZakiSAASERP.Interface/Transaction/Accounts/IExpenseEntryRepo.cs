using ZakiSAASERP.Models.Transactions.Accounts;

namespace ZakiSAASERP.Interface.Transaction.Accounts;

public interface IExpenseEntryRepo
{
    Task<string> GetNextExpenseNoAsync();
    Task<OutputResponse> CreateAsync(ExpenseEntryDTO entity);
    Task<OutputResponse> UpdateAsync(ExpenseEntryDTO entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<List<ExpenseEntryHdr>> GetAllAsync();
    Task<ExpenseEntryDTO?> GetByIdAsync(int id);
    Task<(List<ExpenseEntryHdr>, int)> GetAllWithPagination(PaginationModel pagination);
}
