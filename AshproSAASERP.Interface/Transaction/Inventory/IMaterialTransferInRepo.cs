using AshproSAASERP.Models.Transactions.Inventory;

namespace AshproSAASERP.Interface.Transaction.Inventory;

public interface IMaterialTransferInRepo
{
    Task<string> GetVoucherNo(int branchId);
    Task<int> GetNextVchrNo(int branchId);
    Task<OutputResponse> CreateAsync(MaterialTransferInHdr entity);
    Task<OutputResponse> UpdateAsync(MaterialTransferInHdr entity);
    Task<MaterialTransferInHdr?> GetByIdAsync(int id);
    Task<OutputResponse> DeleteMaterialTransferIn(int id);
    Task<TransactionPDTO> GetAllAsync(PaginationModel pagination);
}
