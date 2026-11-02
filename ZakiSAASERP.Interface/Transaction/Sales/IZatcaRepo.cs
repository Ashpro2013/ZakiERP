namespace ZakiSAASERP.Interface.Transaction.Sales;

public interface IZatcaRepo
{
    Task<PendingIcvData> GetPendingIcvDataAsync(ICVReqDTO request);
    Task<IcvPendingPDTO> GetPendingIcvListAsync(IcvPendReqDTO pendReqDTO);
    Task<OutputResponse> ReUploadSelected(ExInvoiceICV invoiceICV,  CompanyDetails company = null);
    Task ReUploadAll(List<ExInvoiceICV> entInvoices,  CompanyDetails company = null);
    Task<CSRResponse> GenerateCSR(CSRRequest request);
    Task<CSRResponse> GetCSR(int BranchId);
    Task<OutputResponse> AddNewCSR(CSRResponse dto);
    Task<int> GetFailedCount();

}
