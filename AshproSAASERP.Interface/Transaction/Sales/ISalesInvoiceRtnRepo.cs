namespace AshproSAASERP.Interface.Transaction.Sales;

public interface ISalesInvoiceRtnRepo : ICommonTranInterface<SalesInvoiceRtnDTO>
{
    Task<string> GetNextSalesInvoiceRtnNoAsync();
}
