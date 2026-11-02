namespace AshproSAASERP.Interface;

public interface ICommonTranInterface<T> where T : class
{
    Task<OutputResponse> CreateAsync(T entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<T> GetAsync(int Id);
    Task<OutputResponse> UpdateAsync(T entity);
    Task<int> GetNextVchrNo(int BranchId);
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
}
