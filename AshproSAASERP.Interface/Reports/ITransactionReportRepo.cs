namespace AshproSAASERP.Interface.Reports;

public interface ITransactionReportRepo
{
    Task<TransactionMonitorResponse> GetTransactionMonitorReportAsync(TransactionMonitorRequest req);
}
