using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.Models.ViewModel;

namespace AshproSAASERP.POS.Services;

public static class TransactionHelper
{
    public static TransactionDtls GetTransactionAfterEdit(TransactionDtls transaction, bool blTakeIncRate = false, bool isFromDiscAmnt = false)
    {
        transaction.GrossAmount = transaction.Quantity * transaction.ExRate;
        if (!isFromDiscAmnt)
        {
            transaction.DiscountAmnt = (transaction.GrossAmount * transaction.DiscountPerc) / 100m;
        }
        transaction.TaxableAmnt = transaction.GrossAmount - transaction.DiscountAmnt;
        transaction.TaxAmount = (transaction.TaxableAmnt * transaction.TaxPerc) / 100m;
        if (!blTakeIncRate || transaction.DiscountAmnt > 0)
        {
            transaction.LineTotal = transaction.TaxableAmnt + transaction.TaxAmount;
        }
        else
        {
            transaction.LineTotal = transaction.InRate * transaction.Quantity;
        }
        return transaction;
    }
}
