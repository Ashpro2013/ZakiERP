using ZakiSAASERP.Models.Transactions.Accounts;

namespace ZakiSAASERP.Interface.Transaction.Accounts;

public interface ICustomerReceiptRepo
{
    Task<string> GetNextReceiptNoAsync();
    Task<OutputResponse> CreateAsync(CustomerReceiptDTO receipt);
    Task<OutputResponse> UpdateAsync(CustomerReceiptDTO receipt);
    Task<OutputResponse> DeleteAsync(int id);
    Task<List<CustomerReceiptHdr>> GetAllAsync();
    Task<CustomerReceiptDTO?> GetByIdAsync(int id);
    Task<List<OutstandingInvoiceDTO>> GetCustomerOutstandingInvoicesAsync(int customerId, int branchId);
    Task<decimal> GetCustomerBalanceAsync(int customerId, int branchId);
}
