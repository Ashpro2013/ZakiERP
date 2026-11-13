using Xunit;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Tests.AI;

public class InvoiceOcrTests
{
    [Fact]
    public void GetSampleInvoiceData_ReturnsValidOcrResultWithMatchingTotals()
    {
        var service = new InvoiceOcrService();
        var sample = service.GetSampleInvoiceData();

        Assert.NotNull(sample);
        Assert.False(string.IsNullOrWhiteSpace(sample.SupplierName));
        Assert.Equal("300123456789013", sample.SupplierVatNo);
        Assert.Equal("INV-2026-9842", sample.InvoiceNo);
        Assert.NotEmpty(sample.LineItems);
        Assert.Equal(4, sample.LineItems.Count);

        decimal expectedGross = 0;
        decimal expectedTax = 0;
        foreach (var item in sample.LineItems)
        {
            Assert.True(item.Quantity > 0);
            Assert.True(item.UnitPrice > 0);
            Assert.False(string.IsNullOrWhiteSpace(item.ItemName));
            Assert.False(string.IsNullOrWhiteSpace(item.ItemArabicName));
            Assert.True(item.IsSelected);

            decimal lineSubtotal = item.Quantity * item.UnitPrice;
            expectedGross += lineSubtotal;
            expectedTax += item.TaxAmount;
            Assert.Equal(item.LineTotal, Math.Round(lineSubtotal + item.TaxAmount, 2));
        }

        Assert.Equal(sample.GrossAmount, expectedGross);
        Assert.Equal(sample.TaxAmount, expectedTax);
        Assert.Equal(sample.GrandTotal, Math.Round(sample.GrossAmount - sample.DiscountAmount + sample.TaxAmount, 2));
    }

    [Fact]
    public async Task ParseInvoiceAsync_EmptyFileBytes_ThrowsArgumentException()
    {
        var service = new InvoiceOcrService();
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await service.ParseInvoiceAsync(Array.Empty<byte>(), "test.pdf", customApiKey: "dummy-key");
        });
    }

    [Fact]
    public async Task ParseInvoiceAsync_MissingApiKey_ThrowsInvalidOperationException()
    {
        // Ensure no permanent setting or env key interferes with this test
        string? originalApiKey = PermanentSettingsData.AiApiKey;
        PermanentSettingsData.AiApiKey = string.Empty;
        string? originalEnvKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", null);

        try
        {
            var service = new InvoiceOcrService();
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.ParseInvoiceAsync(new byte[] { 1, 2, 3 }, "test.pdf", customApiKey: "");
            });

            Assert.Contains("Gemini API Key", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            PermanentSettingsData.AiApiKey = originalApiKey;
            Environment.SetEnvironmentVariable("GEMINI_API_KEY", originalEnvKey);
        }
    }

    [Fact]
    public void PurchaseOcrDTO_LineItem_PropertiesHoldValuesProperly()
    {
        var lineItem = new PurchaseInvoiceOcrLineItem
        {
            SL = 1,
            ItemName = "Apple iPhone 15",
            ItemArabicName = "أبل آيفون 15",
            Barcode = "194253000000",
            Quantity = 2,
            UnitName = "PCS",
            UnitPrice = 3200m,
            TaxPerc = 15m,
            TaxAmount = 960m,
            LineTotal = 7360m,
            MatchedItemId = 42,
            MatchedItemName = "iPhone 15 128GB",
            MatchedUnitId = 1,
            IsSelected = true
        };

        Assert.Equal(1, lineItem.SL);
        Assert.Equal("Apple iPhone 15", lineItem.ItemName);
        Assert.Equal("أبل آيفون 15", lineItem.ItemArabicName);
        Assert.Equal("194253000000", lineItem.Barcode);
        Assert.Equal(2, lineItem.Quantity);
        Assert.Equal(3200m, lineItem.UnitPrice);
        Assert.Equal(15m, lineItem.TaxPerc);
        Assert.Equal(960m, lineItem.TaxAmount);
        Assert.Equal(7360m, lineItem.LineTotal);
        Assert.Equal(42, lineItem.MatchedItemId);
        Assert.Equal(1, lineItem.MatchedUnitId);
        Assert.True(lineItem.IsSelected);
    }

    [Fact]
    public void PermanentSettings_ShowAIInvoiceOCR_DefaultAndLoadBehavior()
    {
        var settingsModel = new ZakiSAASERP.Models.Settings.SettingsModel();
        Assert.True(settingsModel.ShowAIInvoiceOCR);

        // Test Load with enabled
        var dbSettingsEnabled = new List<ZakiSAASERP.Models.Masters.PermanentSettings>
        {
            new() { SettingKey = "ShowAIInvoiceOCR", SettingValue = "1" },
            new() { SettingKey = "AiProvider", SettingValue = "Gemini" },
            new() { SettingKey = "AiApiKey", SettingValue = "test-key" },
            new() { SettingKey = "AiModelName", SettingValue = "gemini-2.0-flash" }
        };
        PermanentSettingsData.Load(dbSettingsEnabled);

        Assert.True(PermanentSettingsData.ShowAIInvoiceOCR);
        Assert.Equal("Gemini", PermanentSettingsData.AiProvider);
        Assert.Equal("test-key", PermanentSettingsData.AiApiKey);
        Assert.Equal("gemini-2.0-flash", PermanentSettingsData.AiModelName);

        // Test Load with disabled
        var dbSettingsDisabled = new List<ZakiSAASERP.Models.Masters.PermanentSettings>
        {
            new() { SettingKey = "ShowAIInvoiceOCR", SettingValue = "0" }
        };
        PermanentSettingsData.Load(dbSettingsDisabled);

        Assert.False(PermanentSettingsData.ShowAIInvoiceOCR);

        // Reset to defaults
        PermanentSettingsData.Clear();
        Assert.True(PermanentSettingsData.ShowAIInvoiceOCR);
        Assert.Equal("gemini-2.5-flash", PermanentSettingsData.AiModelName);
    }
}

