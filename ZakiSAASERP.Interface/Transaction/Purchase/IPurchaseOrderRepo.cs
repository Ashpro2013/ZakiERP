namespace ZakiSAASERP.Interface.Transaction.Purchase;

public interface IPurchaseOrderRepo
{
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
    Task<OutputResponse> CreateAsync(PurchaseOrderDTO dto);
    Task<OutputResponse> DeleteAsync(int id);
    Task<PurchaseOrderDTO> GetAsync(int id);
    Task<OutputResponse> UpdateAsync(PurchaseOrderDTO dto);
    Task<int> GetNextVchrNo(int branchId);
    Task<string> GetNextPurchaseOrderNoAsync();
}
