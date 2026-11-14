using Xunit;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Models.Settings;

namespace ZakiSAASERP.Tests.Settings;

public class TransactionGridSettingsTests
{
    [Fact]
    public void TransactionGridSettings_Default_ShowStoreIsFalse()
    {
        var settings = new TransactionGridSettings();
        Assert.False(settings.ShowStore, "ShowStore must be false by default.");
    }

    [Fact]
    public void TransactionGridSettings_ResetToDefaults_ShowStoreIsFalse()
    {
        var settings = new TransactionGridSettings { ShowStore = true };
        settings.ResetToDefaults();
        Assert.False(settings.ShowStore, "ShowStore must reset to false.");
    }

    [Fact]
    public void TransactionGridSettings_SelectAllColumns_ShowStoreIsTrue()
    {
        var settings = new TransactionGridSettings { ShowStore = false };
        settings.SelectAllColumns();
        Assert.True(settings.ShowStore, "SelectAllColumns should enable ShowStore.");
    }

    [Fact]
    public void TransactionGridSettings_CopyFrom_CopiesShowStore()
    {
        var source = new TransactionGridSettings { ShowStore = true };
        var target = new TransactionGridSettings { ShowStore = false };

        target.CopyFrom(source);
        Assert.True(target.ShowStore, "CopyFrom should copy ShowStore from source.");

        source.ShowStore = false;
        target.CopyFrom(source);
        Assert.False(target.ShowStore, "CopyFrom should copy false ShowStore from source.");
    }

    [Fact]
    public void TransactionGridSettings_Clone_PreservesShowStore()
    {
        var original = new TransactionGridSettings { ShowStore = true };
        var clone = original.Clone();

        Assert.True(clone.ShowStore);
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void PermanentSettingsData_Load_ParsesShowStoreCorrectly()
    {
        try
        {
            var settingsList = new List<PermanentSettings>
            {
                new() { SettingKey = "SalesInvoice_ShowStore", SettingValue = "1" },
                new() { SettingKey = "Purchase_ShowStore", SettingValue = "0" },
                new() { SettingKey = "SalesReturn_ShowStore", SettingValue = "true" }
            };

            PermanentSettingsData.Load(settingsList);

            Assert.True(PermanentSettingsData.SalesInvoiceSettings.ShowStore);
            Assert.False(PermanentSettingsData.PurchaseSettings.ShowStore);
            Assert.True(PermanentSettingsData.SalesReturnSettings.ShowStore);
            // Default when not in settings list should be false
            Assert.False(PermanentSettingsData.SalesOrderSettings.ShowStore);
        }
        finally
        {
            PermanentSettingsData.Clear();
        }
    }

    [Fact]
    public void PermanentSettingsData_Clear_ResetsShowStoreToFalse()
    {
        var settingsList = new List<PermanentSettings>
        {
            new() { SettingKey = "SalesInvoice_ShowStore", SettingValue = "1" }
        };

        PermanentSettingsData.Load(settingsList);
        Assert.True(PermanentSettingsData.SalesInvoiceSettings.ShowStore);

        PermanentSettingsData.Clear();
        Assert.False(PermanentSettingsData.SalesInvoiceSettings.ShowStore);
    }
}

