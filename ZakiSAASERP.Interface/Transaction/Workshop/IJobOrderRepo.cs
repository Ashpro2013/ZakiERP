using ZakiSAASERP.Models.Transactions.Workshop;

namespace ZakiSAASERP.Interface.Transaction.Workshop;

public interface IJobOrderRepo
{
    Task<string> GetVoucherNo(int branchId);
    Task<int> GetNextVchrNo(int branchId);
    Task<OutputResponse> CreateAsync(JobOrderHdr hdr);
    Task<OutputResponse> UpdateAsync(JobOrderHdr hdr);
    Task<JobOrderHdr?> GetByIdAsync(int id);
    Task<OutputResponse> DeleteAsync(int id);
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
}
