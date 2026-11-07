using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;

namespace ZakiSAASERP.POS.Services;

public static class TransactionHelper
{
    public static TransactionDtls GetTransactionAfterEdit(TransactionDtls transaction, bool blTakeIncRate = false, bool isFromDiscAmnt = false)
    {
        transaction.GrossAmount = Math.Round(transaction.Quantity * transaction.ExRate, 2, MidpointRounding.AwayFromZero);
        if (!isFromDiscAmnt)
        {
            transaction.DiscountAmnt = Math.Round((transaction.GrossAmount * transaction.DiscountPerc) / 100m, 2, MidpointRounding.AwayFromZero);
        }
        transaction.TaxableAmnt = transaction.GrossAmount - transaction.DiscountAmnt;
        transaction.TaxAmount = Math.Round((transaction.TaxableAmnt * transaction.TaxPerc) / 100m, 2, MidpointRounding.AwayFromZero);
        if (!blTakeIncRate || transaction.DiscountAmnt > 0)
        {
            transaction.LineTotal = transaction.TaxableAmnt + transaction.TaxAmount;
        }
        else
        {
            transaction.LineTotal = Math.Round(transaction.InRate * transaction.Quantity, 2, MidpointRounding.AwayFromZero);
        }
        return transaction;
    }
}
