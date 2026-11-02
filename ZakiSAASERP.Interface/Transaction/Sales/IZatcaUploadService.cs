namespace ZakiSAASERP.Interface.Transaction.Sales;

public interface IZatcaUploadService
{
    Task<ZatcaUploadResponse> UploadZatcaInvoice(TransactionHdr master, List<TransactionDtls> transactions, CompanyDetails company, CustomerMaster customer, string InvType, bool blnUploadXML, bool IsCash);
    Task<CSRResponse> GenerateCSR(CSRRequest request);
    Task UploadPendingSimplifiedInvoices(int BranchId);
}
