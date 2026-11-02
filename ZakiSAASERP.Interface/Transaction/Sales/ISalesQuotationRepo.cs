namespace ZakiSAASERP.Interface.Transaction.Sales;

public interface ISalesQuotationRepo : ICommonTranInterface<SalesQuotationDTO>
{
    Task<string> GetNextQuotationNoAsync();
}
