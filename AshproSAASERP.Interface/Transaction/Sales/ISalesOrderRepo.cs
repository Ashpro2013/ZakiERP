namespace AshproSAASERP.Interface.Transaction.Sales;

public interface ISalesOrderRepo : ICommonTranInterface<SalesOrderDTO>
{
    Task<string> GetNextSalesOrderNoAsync();
}
