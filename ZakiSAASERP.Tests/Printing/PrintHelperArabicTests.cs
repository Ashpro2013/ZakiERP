using Xunit;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Tests.Printing;

public class PrintHelperArabicTests
{
    [Fact]
    public void TransactionDtls_ItemArabicName_SetsAndGetsProperly()
    {
        var dtls = new TransactionDtls
        {
            ItemName = "101 Toilet Cleaner-PCs",
            ItemArabicName = "101 منظف المرحاض-PCs"
        };

        Assert.Equal("101 Toilet Cleaner-PCs", dtls.ItemName);
        Assert.Equal("101 منظف المرحاض-PCs", dtls.ItemArabicName);
        Assert.Equal("101 منظف المرحاض-PCs", dtls.ItemNameArabic);
    }

    [Fact]
    public void ContainsArabic_DetectsArabicCharactersCorrectly()
    {
        Assert.True(SyncfusionPrintHelperThermal.ContainsArabic("منظف المرحاض"));
        Assert.True(SyncfusionPrintHelperThermal.ContainsArabic("101 منظف المرحاض-PCs"));
        Assert.False(SyncfusionPrintHelperThermal.ContainsArabic("Toilet Cleaner"));
        Assert.False(SyncfusionPrintHelperThermal.ContainsArabic("12345!@#"));
        Assert.False(SyncfusionPrintHelperThermal.ContainsArabic(null));
        Assert.False(SyncfusionPrintHelperThermal.ContainsArabic(""));
    }
}

