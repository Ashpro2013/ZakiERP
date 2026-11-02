namespace AshproSAASERP.Interface;

public interface ICommonInterface<T> where T : class
{
    Task<OutputResponse> CreateAsync(T entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<T> GetAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<OutputResponse> UpdateAsync(T entity);
    Task<(List<T>, int)> GetAllWithPagination(PaginationModel pagination);
}
