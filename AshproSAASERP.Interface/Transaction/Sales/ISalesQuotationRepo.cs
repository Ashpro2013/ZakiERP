namespace AshproSAASERP.Interface.Transaction.Sales;

public interface ISalesQuotationRepo : ICommonTranInterface<SalesQuotationDTO>
{
    Task<string> GetNextQuotationNoAsync();
}
