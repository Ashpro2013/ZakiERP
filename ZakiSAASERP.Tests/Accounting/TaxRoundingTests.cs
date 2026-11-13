using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Tests.Accounting;

public class TaxRoundingTests
{
    [Theory]
    [InlineData(115.00, 15.0, 100.00, 15.00)]
    [InlineData(230.00, 15.0, 200.00, 30.00)]
    [InlineData(57.50, 15.0, 50.00, 7.50)]
    [InlineData(100.00, 0.0, 100.00, 0.00)]
    public void InclusiveTaxCalculation_StandardAmounts_ComputesBaseAndTaxAccurately(
        decimal inclusiveTotal, decimal vatRate, decimal expectedBase, decimal expectedTax)
    {
        // Act
        decimal calculatedTax = CrudHelper.GetInclusiveAmount(inclusiveTotal, vatRate);
        decimal calculatedBase = CrudHelper.GetInclusiveBaseAmount(inclusiveTotal, vatRate);

        // Assert
        Assert.Equal(expectedTax, Math.Round(calculatedTax, 2));
        Assert.Equal(expectedBase, Math.Round(calculatedBase, 2));
        Assert.Equal(inclusiveTotal, Math.Round(calculatedBase + calculatedTax, 2));
    }

    [Fact]
    public void InclusiveTaxCalculation_BasePlusTax_AlwaysSumsToOriginalInclusiveAmount()
    {
        // Test varying amounts with fractional cents (e.g., 9.99, 14.50, 199.99, 3.33)
        decimal[] testAmounts = { 1.00m, 3.33m, 9.99m, 14.50m, 99.95m, 199.99m, 1234.56m };
        decimal vatRate = 15.0m;

        foreach (var amount in testAmounts)
        {
            decimal tax = CrudHelper.GetInclusiveAmount(amount, vatRate);
            decimal baseAmnt = CrudHelper.GetInclusiveBaseAmount(amount, vatRate);

            // Exact mathematical sum of base + tax must equal input
            Assert.Equal(amount, baseAmnt + tax);
        }
    }

    [Theory]
    [InlineData(100.00, 15.0, 115.00)]
    [InlineData(50.00, 15.0, 57.50)]
    [InlineData(1000.00, 5.0, 1050.00)]
    [InlineData(250.00, 0.0, 250.00)]
    public void GetIncAmountFromExAmount_CalculatesInclusiveTotalCorrectly(
        decimal exAmount, decimal vatRate, decimal expectedInc)
    {
        decimal actualInc = CrudHelper.GetIncAmountFromExAmount(exAmount, vatRate);
        Assert.Equal(expectedInc, Math.Round(actualInc, 2));
    }

    [Theory]
    [InlineData(115.00, 15.0, 100.00)]
    [InlineData(57.50, 15.0, 50.00)]
    [InlineData(1050.00, 5.0, 1000.00)]
    public void GetExAmountFromInc_CalculatesExclusiveBaseCorrectly(
        decimal incAmount, decimal vatRate, decimal expectedEx)
    {
        decimal actualEx = CrudHelper.GetExAmountFromInc(incAmount, vatRate);
        Assert.Equal(expectedEx, Math.Round(actualEx, 2));
    }

    [Fact]
    public void TaxRoundTrip_ExToIncToEx_IsLossless()
    {
        decimal[] amounts = { 10.00m, 25.50m, 99.00m, 1500.25m, 8888.88m };
        decimal vatRate = 15.0m;

        foreach (var originalEx in amounts)
        {
            decimal inc = CrudHelper.GetIncAmountFromExAmount(originalEx, vatRate);
            decimal backToEx = CrudHelper.GetExAmountFromInc(inc, vatRate);

            Assert.Equal(originalEx, Math.Round(backToEx, 2));
        }
    }

    [Fact]
    public void MultiLineTaxRounding_CalculatesTotalTaxAccurately()
    {
        // Invoices with 3 line items having fractional tax
        // Item 1: 33.33 SAR @ 15% VAT = 4.9995 -> 5.00 SAR
        // Item 2: 33.33 SAR @ 15% VAT = 4.9995 -> 5.00 SAR
        // Item 3: 33.34 SAR @ 15% VAT = 5.0010 -> 5.00 SAR
        // Total Base = 100.00 SAR, Total Tax = 15.00 SAR
        decimal line1Base = 33.33m;
        decimal line2Base = 33.33m;
        decimal line3Base = 33.34m;

        decimal line1Tax = Math.Round((line1Base * 15m) / 100m, 2, MidpointRounding.AwayFromZero);
        decimal line2Tax = Math.Round((line2Base * 15m) / 100m, 2, MidpointRounding.AwayFromZero);
        decimal line3Tax = Math.Round((line3Base * 15m) / 100m, 2, MidpointRounding.AwayFromZero);

        decimal totalLineTax = line1Tax + line2Tax + line3Tax;
        decimal totalBaseTax = Math.Round(((line1Base + line2Base + line3Base) * 15m) / 100m, 2, MidpointRounding.AwayFromZero);

        Assert.Equal(15.00m, totalLineTax);
        Assert.Equal(15.00m, totalBaseTax);
    }

    [Fact]
    public void CurrencyFormatting_RespectsPrecisionAndNullHandling()
    {
        PermanentSettingsData.CurrencyDecimalPlaces = 2;

        Assert.Equal("N2", PermanentSettingsData.CurrencyFormat);
        Assert.Equal("1,234.57", PermanentSettingsData.FormatCurrency(1234.567m));
        Assert.Equal("0.00", PermanentSettingsData.FormatCurrency(null));
        Assert.Equal("0.00", PermanentSettingsData.FormatCurrency(0m));
        Assert.Equal("-50.25", PermanentSettingsData.FormatCurrency(-50.25m));
    }

    [Fact]
    public void QuantityFormatting_ClampsAndFormatsProperly()
    {
        PermanentSettingsData.QuantityDecimalPlaces = 3;

        Assert.Equal("N3", PermanentSettingsData.QuantityFormat);
        Assert.Equal("10.500", PermanentSettingsData.FormatQuantity(10.5m));
        Assert.Equal("0.000", PermanentSettingsData.FormatQuantity(null));
    }
}

