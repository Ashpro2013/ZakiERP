public interface ISupplierReportRepo
{
    Task<List<SupplierStatementDTO>> GetSupplierStatementAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null);
    Task<List<SupplierListDTO>> GetSupplierListAsync();
    Task<List<SupplierSummaryDTO>> GetSupplierSummaryAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null);
    Task<List<SupplierPendingInvoiceDTO>> GetSupplierPendingInvoiceAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId = null);
    Task<List<SupplierPaymentReportDTO>> GetSupplierPaymentReportAsync(DateTime fromDate, DateTime toDate, int branchId, int? supplierId, string? billType);
}
