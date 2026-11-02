using ZakiSAASERP.Models.Transactions.Purchase;

namespace ZakiSAASERP.Interface.Transaction.Purchase;

public interface ISupplierPaymentRepo
{
    Task<OutputResponse> CreateAsync(SupplierPaymentDTO entity);
    Task<OutputResponse> UpdateAsync(SupplierPaymentDTO entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<SupplierPaymentDTO> GetAsync(int id);
    Task<(List<SupplierPaymentHdr> PTransactions, int TotalCount)> GetAllWithPagination(PaginationModel pagination);
    Task<string> GetNextVoucherNoAsync(int branchId);
    Task<List<SupplierPaymentDtls>> GetPendingPurchaseInvoices(int supplierId);
    Task<decimal> GetSupplierBalanceAsync(int supplierId, int branchId);
}
