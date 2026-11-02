namespace ZakiSAASERP.Interface.Reports
{
    public interface IAccountReportRepo
    {
        Task<JournalEntryReportResponse> GetJournalEntryReport(JournalEntryReportRequest entries);
        Task<TrialBalanceReportResponse> GetTrialBalanceReport(TrialBalanceReportRequest request);
        Task<ExpenseReportResponse> GetExpenseReport(ExpenseReportRequest request);
        Task<ProfitAndLossReportResponse> GetProfitAndLossReport(ProfitAndLossReportRequest request);
        Task<BalanceSheetReportResponse> GetBalanceSheetReport(BalanceSheetReportRequest request);
        Task<ChartOfAccountReportResponse> GetChartOfAccountReport(ChartOfAccountReportRequest request);
        Task<LedgerReportResponse> GetLedgerReport(LedgerReportRequest request);
    }
}
