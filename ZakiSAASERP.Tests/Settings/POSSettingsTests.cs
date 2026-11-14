using Xunit;
using ZakiSAASERP.Models.Settings;

namespace ZakiSAASERP.Tests.Settings;

public class POSSettingsTests
{
    [Fact]
    public void POSSettings_Default_BeepOnBarcodeNotFoundIsTrue()
    {
        var settings = new POSSettings();
        Assert.True(settings.BeepOnBarcodeNotFound, "BeepOnBarcodeNotFound must be true by default.");
    }

    [Fact]
    public void POSSettings_Clone_PreservesBeepOnBarcodeNotFound()
    {
        var original = new POSSettings { BeepOnBarcodeNotFound = false };
        var clone = original.Clone();

        Assert.False(clone.BeepOnBarcodeNotFound);
        Assert.NotSame(original, clone);

        original.BeepOnBarcodeNotFound = true;
        var clone2 = original.Clone();
        Assert.True(clone2.BeepOnBarcodeNotFound);
    }
}

