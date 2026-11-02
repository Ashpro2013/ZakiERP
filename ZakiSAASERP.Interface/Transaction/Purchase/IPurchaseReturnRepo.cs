namespace ZakiSAASERP.Interface.Transaction.Purchase;

public interface IPurchaseReturnRepo : ICommonTranInterface<PurchaseReturnDTO>
{
    Task<string> GetNextPurchaseReturnNoAsync();
    Task<List<TransactionDtls>> GetPurchaseReturnBalance(int purchaseId);
}
