namespace ZakiSAASERP.Interface.Transaction.Purchase;

public interface IPurchaseRepo
{
    Task<OutputResponse> CreateAsync(PurchaseDTO entity);
    Task<OutputResponse> UpdateAsync(PurchaseDTO entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<PurchaseDTO> GetAsync(int id);
    Task<(List<ExMasters> PTransactions, int TotalCount)> GetAllWithPagination(PaginationModel pagination);
    Task<int> GetNextVchrNo(int branchId);
    Task<string> GetNextPurchaseNoAsync();
}
