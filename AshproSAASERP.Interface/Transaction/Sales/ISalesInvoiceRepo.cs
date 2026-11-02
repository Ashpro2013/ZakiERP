using AshproSAASERP.Models.Transactions.Sales;

namespace AshproSAASERP.Interface.Transaction.Sales;

public interface ISalesInvoiceRepo : ICommonTranInterface<SalesInvoiceDTO>
{
    Task<string> GetNextInvoiceNoAsync();
    Task<List<SalesInvoiceHdr>> GetByBranchAsync();
    Task<DateTime?> GetOldestUnpaidInvoiceDateAsync(int customerId);
}
