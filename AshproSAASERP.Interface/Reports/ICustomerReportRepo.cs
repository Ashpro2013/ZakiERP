namespace AshproSAASERP.Interface.Reports;

public interface ICustomerReportRepo
{
    Task<CustomerStatementResponse> GetCustomerStatementAsync(CustomerStatementRequest req);
    Task<List<CustomerSummaryDTO>> GetCustomerSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null);
    Task<List<CustomerPendingInvoiceDTO>> GetCustomerPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? customerId = null);
    Task<List<CustomerListDTO>> GetCustomerListAsync();
}
