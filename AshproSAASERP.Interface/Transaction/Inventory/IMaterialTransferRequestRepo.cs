using AshproSAASERP.Models.Transactions.Inventory;

namespace AshproSAASERP.Interface.Transaction.Inventory;

public interface IMaterialTransferRequestRepo
{
    Task<string> GetVoucherNo(int branchId);
    Task<int> GetNextVchrNo(int branchId);
    Task<OutputResponse> CreateAsync(MaterialTransferRequestHdr hdr);
    Task<OutputResponse> UpdateAsync(MaterialTransferRequestHdr hdr);
    Task<MaterialTransferRequestHdr?> GetByIdAsync(int id);
    Task<OutputResponse> DeleteMaterialTransferRequest(int id);
    Task<TransactionPDTO> GetAllAsync(PaginationModel pagination);

    // Approval Methods
    Task<OutputResponse> ApproveMaterialTransferRequest(int id, int userId, List<MaterialTransferRequestDtls> updatedDetails);
    Task<OutputResponse> RejectMaterialTransferRequest(int id, int userId, string? rejectionReason);
    Task<List<MaterialTransferRequestHdr>> GetApprovalList(MTRApprovalFilter filter);
}
