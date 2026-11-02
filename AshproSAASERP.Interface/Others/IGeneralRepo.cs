namespace AshproSAASERP.Interface.Others;

public interface IGeneralRepo
{
    Task<ChartEntity> GetChartValues(int branchId, string format);
    Task<List<CommonModel>> GetComboFill(string type, int? branchId);
    Task<List<ExMasters>> GetMasters(string head, int? branchId);
    Task<int> GetNewCode(string head, int? branchId);
    Task<decimal> GetValueMethod(string head, int? val);
    string GetZatcaQRCodeValFirstPhase(TransactionHdr Invoice, CompanyDetails CompanyData, DateTime? invoiceDate);
}
