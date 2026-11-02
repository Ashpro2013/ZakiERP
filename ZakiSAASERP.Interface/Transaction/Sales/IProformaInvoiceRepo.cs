namespace ZakiSAASERP.Interface.Transaction.Sales;

public interface IProformaInvoiceRepo : ICommonTranInterface<ProformaInvoiceDTO>
{
    Task<string> GetNextProformaInvoiceNoAsync();
    Task<ProformaInvoiceDTO> CreateFromQuotationAsync(int quotationId);
}
