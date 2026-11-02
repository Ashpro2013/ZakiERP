using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Transactions.Sales;

namespace ZakiSAASERP.Interface.Transaction.Sales;

public interface ISalesInvoiceHoldRepo
{
    Task<OutputResponse> CreateAsync(SalesInvoiceHoldDTO entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<OutputResponse> DeleteAllAsync(int branchId, string? terminal = null);
    Task<SalesInvoiceHoldDTO> GetAsync(int id);
    Task<List<SalesInvoiceHoldHdr>> GetAllHeldAsync(int branchId, string? terminal = null);
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
}

