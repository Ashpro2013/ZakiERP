using ZakiSAASERP.Models.Transactions.Inventory;

namespace ZakiSAASERP.Interface.Transaction.Inventory;

public interface IInternalMaterialTransferRepo
{
    Task<string> GetVoucherNo(int branchId);
    Task<int> GetNextVchrNo(int branchId);
    Task<OutputResponse> CreateAsync(InternalMaterialTransferHdr entity);
    Task<OutputResponse> UpdateAsync(InternalMaterialTransferHdr entity);
    Task<InternalMaterialTransferHdr?> GetByIdAsync(int id);
    Task<OutputResponse> DeleteAsync(int id);
    Task<TransactionPDTO> GetAllAsync(PaginationModel pagination);
}
