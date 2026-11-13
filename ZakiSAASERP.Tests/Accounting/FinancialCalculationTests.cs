using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Tests.Accounting;

public class FinancialCalculationTests
{
    [Fact]
    public void GetTransactionAfterEdit_CalculatesGrossTaxableAndLineTotal_Correctly()
    {
        // Arrange: 10 items @ 100 SAR, 10% discount, 15% VAT
        var item = new TransactionDtls
        {
            Quantity = 10,
            ExRate = 100m,
            DiscountPerc = 10m,
            TaxPerc = 15m
        };

        // Act
        var result = TransactionHelper.GetTransactionAfterEdit(item);

        // Assert
        Assert.Equal(1000m, result.GrossAmount);      // 10 * 100
        Assert.Equal(100m, result.DiscountAmnt);      // 10% of 1000
        Assert.Equal(900m, result.TaxableAmnt);       // 1000 - 100
        Assert.Equal(135m, result.TaxAmount);         // 15% of 900
        Assert.Equal(1035m, result.LineTotal);        // 900 + 135
    }

    [Fact]
    public void GetTransactionAfterEdit_WhenDiscountAmountProvided_PreservesDiscountAmount()
    {
        // Arrange: 5 items @ 200 SAR = 1000 Gross, fixed discount of 150 SAR, 15% VAT
        var item = new TransactionDtls
        {
            Quantity = 5,
            ExRate = 200m,
            DiscountAmnt = 150m,
            TaxPerc = 15m
        };

        // Act
        var result = TransactionHelper.GetTransactionAfterEdit(item, IsFromDiscAmnt: true);

        // Assert
        Assert.Equal(1000m, result.GrossAmount);
        Assert.Equal(150m, result.DiscountAmnt);
        Assert.Equal(850m, result.TaxableAmnt);       // 1000 - 150
        Assert.Equal(127.50m, result.TaxAmount);      // 15% of 850
        Assert.Equal(977.50m, result.LineTotal);      // 850 + 127.50
    }

    [Fact]
    public void GetTransactionAfterEdit_WithZeroTaxAndZeroDiscount_MatchesGrossAmount()
    {
        // Arrange
        var item = new TransactionDtls
        {
            Quantity = 4,
            ExRate = 50m,
            DiscountPerc = 0m,
            TaxPerc = 0m
        };

        // Act
        var result = TransactionHelper.GetTransactionAfterEdit(item);

        // Assert
        Assert.Equal(200m, result.GrossAmount);
        Assert.Equal(0m, result.DiscountAmnt);
        Assert.Equal(200m, result.TaxableAmnt);
        Assert.Equal(0m, result.TaxAmount);
        Assert.Equal(200m, result.LineTotal);
    }

    [Fact]
    public void GetTransactionAfterEdit_WhenTakeIncRateIsTrue_UsesInRateForLineTotal()
    {
        // Arrange: InRate is 115 SAR (inclusive of 15% tax on 100 SAR base)
        var item = new TransactionDtls
        {
            Quantity = 2,
            ExRate = 100m,
            InRate = 115m,
            TaxPerc = 15m,
            DiscountPerc = 0m
        };

        // Act
        var result = TransactionHelper.GetTransactionAfterEdit(item, blTakeIncRate: true);

        // Assert
        Assert.Equal(230m, result.LineTotal); // InRate * Quantity = 115 * 2
    }

    [Fact]
    public async Task AdditionalDiscountMethod_Type2_InclusiveDiscount_CalculatesCorrectly()
    {
        // Arrange: 2 items, total line total = 1150 + 230 = 1380
        var items = new List<TransactionDtls>
        {
            new TransactionDtls { Quantity = 10, ExRate = 100m, InRate = 115m, TaxPerc = 15m, LineTotal = 1150m },
            new TransactionDtls { Quantity = 2, ExRate = 100m, InRate = 115m, TaxPerc = 15m, LineTotal = 230m }
        };

        // Act: Apply 10% additional discount
        var (perc, amnt) = await TransactionHelper.AdditionalDiscountMethod(items, iType: 1, AdditionalDiscPertge: 10m, AdditionalDiscAmnt: null);

        // Assert
        Assert.Equal(10m, perc);
        Assert.Equal(138m, amnt); // 10% of 1380

        // Total of modified LineTotals should equal 1380 - 138 = 1242
        var sumLineTotals = items.Sum(x => x.LineTotal);
        Assert.Equal(1242m, Math.Round(sumLineTotals, 2));

        // Each line's tax and taxable base should sum up to line total
        foreach (var item in items)
        {
            Assert.Equal(item.LineTotal, Math.Round(item.TaxableAmnt + item.TaxAmount, 2));
        }
    }

    [Fact]
    public async Task AdditionalDiscountMethod_Type2_AmountBased_DerivesPercentageCorrectly()
    {
        // Arrange: 1 item, LineTotal 1000 SAR
        var items = new List<TransactionDtls>
        {
            new TransactionDtls { Quantity = 10, ExRate = 100m, InRate = 100m, TaxPerc = 0m, LineTotal = 1000m }
        };

        // Act: Apply 100 SAR discount (which is 10%)
        var (perc, amnt) = await TransactionHelper.AdditionalDiscountMethod(items, iType: 2, AdditionalDiscPertge: null, AdditionalDiscAmnt: 100m);

        // Assert
        Assert.Equal(10m, perc);
        Assert.Equal(100m, amnt);
        Assert.Equal(900m, items[0].LineTotal);
    }

    [Fact]
    public async Task AdditionalDiscountMethod_EmptyList_ReturnsNulls()
    {
        var items = new List<TransactionDtls>();
        var (perc, amnt) = await TransactionHelper.AdditionalDiscountMethod(items, 1, 10m, null);

        Assert.Null(perc);
        Assert.Null(amnt);
    }

    [Fact]
    public async Task AddDiscType2Method_TaxableBaseDiscount_DistributesPercentageProperly()
    {
        // Arrange
        var items = new List<TransactionDtls>
        {
            new TransactionDtls { Quantity = 5, ExRate = 100m, TaxPerc = 15m, GrossAmount = 500m },
            new TransactionDtls { Quantity = 5, ExRate = 100m, TaxPerc = 15m, GrossAmount = 500m }
        };

        // Act: Apply 5% discount across lines
        var (perc, amnt) = await TransactionHelper.AddDiscType2Method(items, iType: 1, AdditionalDiscPertge: 5m, AdditionalDiscAmnt: null, GrandTotal: 1000m);

        // Assert
        Assert.Equal(5m, perc);
        Assert.Equal(50m, amnt); // 5% of 1000 gross
        Assert.All(items, x => Assert.Equal(5m, x.DiscountPerc));
        Assert.All(items, x => Assert.Equal(25m, x.DiscountAmnt));
    }

    [Fact]
    public void AddDiscType3Method_HeaderDiscount_CalculatesGrandTotalCorrectly()
    {
        // Arrange: Net Amount = 1000 SAR
        decimal netAmount = 1000m;

        // Act: 15% discount
        var (perc, amnt, grandTotal) = TransactionHelper.AddDiscType3Method(iType: 1, AdditionalDiscPertge: 15m, AdditionalDiscAmnt: null, NetAmount: netAmount);

        // Assert
        Assert.Equal(15m, perc);
        Assert.Equal(150m, amnt);
        Assert.Equal(850m, grandTotal);
    }

    [Fact]
    public void AddDiscType3Method_AmountBased_DerivesPercentageAndGrandTotal()
    {
        // Arrange: Net Amount = 2000 SAR
        decimal netAmount = 2000m;

        // Act: 500 SAR discount (25%)
        var (perc, amnt, grandTotal) = TransactionHelper.AddDiscType3Method(iType: 2, AdditionalDiscPertge: null, AdditionalDiscAmnt: 500m, NetAmount: netAmount);

        // Assert
        Assert.Equal(25m, perc);
        Assert.Equal(500m, amnt);
        Assert.Equal(1500m, grandTotal);
    }

    [Fact]
    public void AddDiscType3Method_ZeroNetAmount_ReturnsNulls()
    {
        var (perc, amnt, grandTotal) = TransactionHelper.AddDiscType3Method(1, 10m, null, 0m);

        Assert.Null(perc);
        Assert.Null(amnt);
        Assert.Null(grandTotal);
    }
}

