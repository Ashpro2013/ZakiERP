using AshproSAASERP.Models.Transactions.Inventory;

namespace AshproSAASERP.Interface.Transaction.Inventory;

public interface IMaterialTransferOutRepo
{
    Task<string> GetVoucherNo(int branchId);
    Task<int> GetNextVchrNo(int branchId);
    Task<OutputResponse> CreateAsync(MaterialTransferOutHdr hdr);
    Task<OutputResponse> UpdateAsync(MaterialTransferOutHdr hdr);
    Task<MaterialTransferOutHdr?> GetByIdAsync(int id);
    Task<OutputResponse> DeleteMaterialTransferOut(int id);
    Task<TransactionPDTO> GetAllAsync(PaginationModel pagination);
}
