using ZakiSAASERP.Models.Transactions.Sales;

namespace ZakiSAASERP.Interface.Transaction.Sales;

public interface IEnquiryRepo
{
    Task<string> SaveAsync(RFQHdr rfqHdr);
    Task<string> UpdateAsync(RFQHdr rfqHdr);
    Task<bool> DeleteAsync(int id);
    Task<RFQHdr> GetByIdAsync(int id);
    Task<List<RFQHdr>> GetAllAsync(string conn);
    Task<string> GetNextEnquiryNoAsync(string conn, int branchId);
    Task<CustomerMaster> GetCustomerByIdAsync(int id);
    Task<List<StoreMaster>> GetStoresAsync(string conn);
}
