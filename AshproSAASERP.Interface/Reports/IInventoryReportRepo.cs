namespace AshproSAASERP.Interface.Reports;

public interface IInventoryReportRepo
{
    Task<StockReportResponse> GetStockReportAsync(StockReportRequest req);
    Task<StockTransferReportResponse> GetStockTransferReportAsync(StockTransferReportRequest req);
    Task<ItemTransactionHistoryReportResponse> GetItemTransactionHistoryReportAsync(ItemTransactionHistoryReportRequest req);
    Task<StockAdjustmentReportResponse> GetStockAdjustmentReportAsync(StockAdjustmentReportRequest req);
}
