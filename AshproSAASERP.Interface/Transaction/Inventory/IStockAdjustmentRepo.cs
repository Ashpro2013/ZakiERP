using AshproSAASERP.Models.Transactions.Inventory;

namespace AshproSAASERP.Interface.Transaction.Inventory;

public interface IStockAdjustmentRepo
{
    Task<string> GetVoucherNo(int branchId);
    Task<int> GetNextVchrNo(int branchId);
    Task<OutputResponse> CreateAsync(StockAdjustmentHdr hdr);
    Task<OutputResponse> UpdateAsync(StockAdjustmentHdr hdr);
    Task<StockAdjustmentHdr?> GetByIdAsync(int id);
    Task<OutputResponse> DeleteStockAdjustment(int id);
    Task<decimal> GetItemCurrentStock(int itemId, int? storeId, int? excludeVoucherId = 0);
    Task<TransactionPDTO> GetAllAsync(PaginationModel pagination);
}
