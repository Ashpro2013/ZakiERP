namespace AshproSAASERP.Interface.Reports;

public interface IPurchaseReportRepo
{
    Task<PurchaseReportResponse> GetPurchaseReportAsync(PurchaseReportRequest req);
    Task<PurchaseDetailsReportResponse> GetPurchaseDetailsAsync(PurchaseDetailsReportRequest req);
    Task<PurchaseOrderReportResponse> GetPurchaseOrderReport(PurchaseOrderReportRequest request);
    Task<PurchaseMonitorResponse> GetPurchaseMonitorReportAsync(PurchaseMonitorRequest req);
    Task<List<PurchaseMonitorPaymentItem>> GetPaymentsByPurchaseAsync(int purchaseId);
    Task<PurchaseReturnReportResponse> GetPurchaseReturnReportAsync(PurchaseReturnReportRequest req);
    Task<PurchaseReturnDetailsReportResponse> GetPurchaseReturnDetailsAsync(PurchaseReturnDetailsReportRequest req);
}
