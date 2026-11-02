namespace ZakiSAASERP.Shared.Services;

public class TransactionHelper
{
    public static TransactionDtls GetTransactionAfterEdit(TransactionDtls transaction,bool blTakeIncRate = false,bool IsFromDiscAmnt = false)
    {
        transaction.GrossAmount = transaction.Quantity * transaction.ExRate;
        if (!IsFromDiscAmnt)
        {
            transaction.DiscountAmnt = (transaction.GrossAmount * transaction.DiscountPerc) / 100;
        }
        transaction.TaxableAmnt = transaction.GrossAmount - transaction.DiscountAmnt;
        transaction.TaxAmount = (transaction.TaxableAmnt * transaction.TaxPerc) / 100;
        if(!blTakeIncRate || transaction.DiscountAmnt > 0)
        {
            transaction.LineTotal = transaction.TaxableAmnt + transaction.TaxAmount;
        }
        else
        {
            transaction.LineTotal = transaction.InRate * transaction.Quantity;
        }
        return transaction;
    }
    public static ZakiSAASERP.Models.Transactions.Purchase.GoodsReceiptDtls GetTransactionAfterEdit(ZakiSAASERP.Models.Transactions.Purchase.GoodsReceiptDtls transaction)
    {
        transaction.GrossAmount = transaction.Quantity * transaction.ExRate;
        transaction.DiscountAmnt = (transaction.GrossAmount * transaction.DiscountPerc) / 100;
        transaction.TaxableAmnt = transaction.GrossAmount - transaction.DiscountAmnt;
        transaction.TaxAmount = (transaction.TaxableAmnt * transaction.TaxPerc) / 100;
        transaction.LineTotal = transaction.TaxableAmnt + transaction.TaxAmount;
        return transaction;
    }
    public static async Task<(decimal?, decimal?)> AdditionalDiscountMethod(List<TransactionDtls> entTransactions, int iType, decimal? AdditionalDiscPertge, decimal? AdditionalDiscAmnt, decimal GrandTotal,string strCurrencyFormat = "F2")
    {
        if (!entTransactions.Any()) { return (null, null); }
        if (iType == 2)
        {
            AdditionalDiscPertge = (GrandTotal == 0) ? 0 : (100 * (AdditionalDiscAmnt ?? 0)) / GrandTotal;
        }

        decimal basePer = (100 - (AdditionalDiscPertge ?? 0)) / 100;
        foreach (var item in entTransactions)
        {
            if (AdditionalDiscPertge > 0)
            {
                item.LineTotal = item.LineTotal * basePer;
                // Inclusive recalculation - also reduces tax
                item.TaxAmount = CrudHelper.GetInclusiveAmount(item.LineTotal, item.TaxPerc);
                item.TaxableAmnt = CrudHelper.GetInclusiveBaseAmount(item.LineTotal, item.TaxPerc);
                decimal discAmnt = item.GrossAmount - item.TaxableAmnt;
                string discAmntStr = discAmnt.ToString(strCurrencyFormat);
                item.DiscountAmnt = discAmntStr.toDecimal();
                item.DiscountPerc = (item.GrossAmount == 0) ? 0 : (item.DiscountAmnt / item.GrossAmount * 100);
            }
            else
            {
                item.DiscountPerc = 0;
                TransactionHelper.GetTransactionAfterEdit(item);
            }
        }

        if (iType == 1) AdditionalDiscAmnt = (GrandTotal * (AdditionalDiscPertge ?? 0)) / 100;
        return (AdditionalDiscPertge, AdditionalDiscAmnt);
    }
    public static async Task<(decimal?, decimal?)> AddDiscType2Method(List<TransactionDtls> entTransactions, int iType, decimal? AdditionalDiscPertge, decimal? AdditionalDiscAmnt, decimal GrandTotal)
    {
        if (!entTransactions.Any()) { return (null, null); }
        if (iType == 2)
        {
            // For "Ex Amount", the amount applies to the TAXABLE base
            decimal totalTaxable = entTransactions.Sum(x => x.GrossAmount);
            AdditionalDiscPertge = (totalTaxable == 0) ? 0 : (100 * (AdditionalDiscAmnt ?? 0)) / totalTaxable;
        }

        foreach (var item in entTransactions)
        {
            item.DiscountPerc = AdditionalDiscPertge ?? 0;
            TransactionHelper.GetTransactionAfterEdit(item);
        }

        if (iType == 1)
        {
            decimal totalTaxable = entTransactions.Sum(x => x.GrossAmount);
            AdditionalDiscAmnt = (totalTaxable * (AdditionalDiscPertge ?? 0)) / 100;
        }
        return (AdditionalDiscPertge, AdditionalDiscAmnt);
    }
    public static (decimal?, decimal?, decimal?) AddDiscType3Method(int iType, decimal? AdditionalDiscPertge, decimal? AdditionalDiscAmnt, decimal NetAmount)
    {
        if (NetAmount == 0) { return (null, null, null); }
        decimal GrandTotal = NetAmount;
        if (iType == 1)
        {
            if (AdditionalDiscPertge > 0)
            {
                AdditionalDiscAmnt = GrandTotal * AdditionalDiscPertge / 100;
                GrandTotal = NetAmount - (AdditionalDiscAmnt ?? 0);
            }
            else
            {
                AdditionalDiscAmnt = 0;
                GrandTotal = NetAmount;
            }
        }
        else
        {

            if (AdditionalDiscAmnt > 0)
            {
                AdditionalDiscPertge = (100 * AdditionalDiscAmnt ?? 0) / GrandTotal;
                GrandTotal = NetAmount - (AdditionalDiscAmnt ?? 0);
            }
            else
            {
                AdditionalDiscPertge = 0;
                GrandTotal = NetAmount;
            }
        }
        return (AdditionalDiscPertge, AdditionalDiscAmnt, GrandTotal);
    }
}
