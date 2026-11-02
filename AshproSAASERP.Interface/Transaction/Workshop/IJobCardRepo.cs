using AshproSAASERP.Models.Transactions.Workshop;

namespace AshproSAASERP.Interface.Transaction.Workshop;

public interface IJobCardRepo
{
    Task<string> GetVoucherNo(int branchId);
    Task<int> GetNextVchrNo(int branchId);
    Task<OutputResponse> CreateAsync(JobCardHdr hdr);
    Task<OutputResponse> UpdateAsync(JobCardHdr hdr);
    Task<JobCardHdr?> GetByIdAsync(int id);
    Task<OutputResponse> DeleteAsync(int id);
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
    Task<List<JobCardHistoryDTO>> GetJobCardHistory(string plateNo);
}
