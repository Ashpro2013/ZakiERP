using Microsoft.EntityFrameworkCore;
using Xunit;
using ZakiSAASERP.Interface;
using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.User;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Repo.Others;
using ZakiSAASERP.Shared.Services;
using ZakiSAASERP.Tests.Common;

namespace ZakiSAASERP.Tests.Users;

public class UserPrivilegeMenuTests : IDisposable
{
    private readonly TestDbContextFactoryWrapper _dbWrapper;
    private readonly MockDbContextProvider _provider;

    public UserPrivilegeMenuTests()
    {
        _dbWrapper = new TestDbContextFactoryWrapper();
        _provider = new MockDbContextProvider(_dbWrapper);
    }

    public void Dispose()
    {
        _dbWrapper.Dispose();
    }

    private class MockDbContextProvider : IDbContextProvider
    {
        private readonly TestDbContextFactoryWrapper _wrapper;
        public MockDbContextProvider(TestDbContextFactoryWrapper wrapper) => _wrapper = wrapper;
        public TenantDBContext CreateDbContext(string? con = null) => _wrapper.CreateNewContext();
    }

    [Fact]
    public void AllSystemMenus_Contains_POSView_And_DBBackup()
    {
        var posMenu = MenuSeedHelper.AllSystemMenus.FirstOrDefault(m => m.Name == "POSView");
        Assert.NotNull(posMenu.Name);
        Assert.Equal("POS", posMenu.MenuCaption);
        Assert.Equal("نقطة البيع", posMenu.ArabicCaption);
        Assert.Equal("Sales", posMenu.ParentName);
        Assert.Equal("ZakiSAASERP.Shared.Pages.Transactions.Sales.POSView", posMenu.AssemblyName);
        Assert.Equal("POSView", posMenu.ClassName);

        var dbBackupMenu = MenuSeedHelper.AllSystemMenus.FirstOrDefault(m => m.Name == "DBBackup");
        Assert.NotNull(dbBackupMenu.Name);
        Assert.Equal("Database Backup", dbBackupMenu.MenuCaption);
        Assert.Equal("نسخ احتياطي لقاعدة البيانات", dbBackupMenu.ArabicCaption);
        Assert.Equal("Settings", dbBackupMenu.ParentName);
        Assert.Equal("ZakiSAASERP.Shared.Pages.Settings.DBBackupView", dbBackupMenu.AssemblyName);
        Assert.Equal("DBBackupView", dbBackupMenu.ClassName);
    }

    [Fact]
    public void AllSystemMenus_Contains_All_Reports()
    {
        var expectedReports = new[]
        {
            "Sales Report", "Sales Return Report", "Sales Details Report", "Sales Return Details Report",
            "Customer Wise Sales Report", "Category Wise Report", "Sales Quotation Register",
            "Customer Wise Quotation Report", "Sales Order Register", "Delivery Note Register",
            "Date Wise Profit Report", "Proforma Invoice Report", "Vat Report", "ShiftClosingReport", "PendingZatca",
            "Sales Monitor Report", "Purchase Monitor Report", "Transaction Monitor Report",
            "Supplier Statement", "Supplier Pending Invoices", "Supplier Summary", "Supplier List", "Supplier Payment Report",
            "Customer Statement", "Customer Summary", "Customer Pending Invoice Report", "Customer List",
            "Purchase Summary", "Purchase Details", "Purchase Order Report", "Purchase Return Summary", "Purchase Return Details",
            "Stock Report", "Stock Transfer Report", "Item Transaction History Report", "Stock Adjustment Report",
            "Journal Entry Report", "Trial Balance Report", "Expense Report", "Profit And Loss Report",
            "Balance Sheet Report", "Chart Of Account Report", "LedgerReport", "EnquiryRegistrationReport"
        };

        foreach (var rpt in expectedReports)
        {
            var found = MenuSeedHelper.AllSystemMenus.Any(m => m.Name == rpt || m.ClassName == rpt || m.MenuCaption == rpt);
            Assert.True(found, $"Expected report '{rpt}' not found in AllSystemMenus");
        }
    }

    [Fact]
    public async Task EnsureSeedDataAsync_Seeds_POSView_And_DBBackup_With_Valid_ParentIds()
    {
        await MenuSeedHelper.EnsureSeedDataAsync(_dbWrapper.DbContext);

        var salesMenu = await _dbWrapper.DbContext.MenuDetails.FirstOrDefaultAsync(m => m.Name == "Sales");
        Assert.NotNull(salesMenu);

        var settingsMenu = await _dbWrapper.DbContext.MenuDetails.FirstOrDefaultAsync(m => m.Name == "Settings");
        Assert.NotNull(settingsMenu);

        var pos = await _dbWrapper.DbContext.MenuDetails.FirstOrDefaultAsync(m => m.Name == "POSView");
        Assert.NotNull(pos);
        Assert.Equal(salesMenu.Id, pos.ParentId);
        Assert.True(pos.ParentId > 0);

        var dbBackup = await _dbWrapper.DbContext.MenuDetails.FirstOrDefaultAsync(m => m.Name == "DBBackup");
        Assert.NotNull(dbBackup);
        Assert.Equal(settingsMenu.Id, dbBackup.ParentId);
        Assert.True(dbBackup.ParentId > 0);
    }

    [Fact]
    public async Task UserPrivilegeRepo_GetAsync_NewPrivilege_Includes_POS_And_DBBackup()
    {
        var repo = new UserPrivilegeRepo(_provider);
        var result = await repo.GetAsync(0);

        Assert.NotNull(result);
        Assert.NotEmpty(result.entUserPrivilegeDtls);

        var posPrivilege = result.entUserPrivilegeDtls.FirstOrDefault(p => p.MenuCaption == "POS");
        Assert.NotNull(posPrivilege);
        Assert.Equal("Sales", posPrivilege.MenuTitle);
        Assert.Equal("نقطة البيع", posPrivilege.ArabicCaption);
        Assert.Equal("المبيعات", posPrivilege.ArabicTitle);

        var backupPrivilege = result.entUserPrivilegeDtls.FirstOrDefault(p => p.MenuCaption == "Database Backup");
        Assert.NotNull(backupPrivilege);
        Assert.Equal("Settings", backupPrivilege.MenuTitle);
        Assert.Equal("نسخ احتياطي لقاعدة البيانات", backupPrivilege.ArabicCaption);
        Assert.Equal("الإعدادات", backupPrivilege.ArabicTitle);
    }

    [Fact]
    public async Task UserPrivilegeRepo_GetAsync_ExistingPrivilege_Populates_Missing_Menus_As_Defaults()
    {
        // 1. Seed initial data
        await MenuSeedHelper.EnsureSeedDataAsync(_dbWrapper.DbContext);

        var repo = new UserPrivilegeRepo(_provider);

        // 2. Create a privilege header with a single detail (e.g. SalesInvoiceTitle only)
        var hdr = new UserPrivilegeHdr { Name = "Cashier Role", FK_BranchId = 1 };
        var salesInvoice = await _dbWrapper.DbContext.MenuDetails.FirstAsync(m => m.Name == "SalesInvoiceTitle");
        var dtls = new List<ExUserPrivilege>
        {
            new ExUserPrivilege
            {
                MenuId = salesInvoice.Id,
                FK_MenuDetailsId = salesInvoice.Id,
                Visible = true,
                SaveData = true,
                EditData = false,
                DeleteData = false
            }
        };

        var createResult = await repo.CreateAsync(new UserPrivilegeDTO(hdr, dtls, new List<ExFuncDetails>()));
        Assert.True(createResult.Id > 0);

        // 3. Load the saved privilege - it should load all menus, preserving saved ones and defaulting missing ones
        var loaded = await repo.GetAsync(createResult.Id);
        Assert.NotNull(loaded);
        Assert.NotNull(loaded.entUserPrivilegeHdr);
        Assert.Equal("Cashier Role", loaded.entUserPrivilegeHdr.Name);

        var loadedSalesInvoice = loaded.entUserPrivilegeDtls.FirstOrDefault(p => p.MenuId == salesInvoice.Id);
        Assert.NotNull(loadedSalesInvoice);
        Assert.True(loadedSalesInvoice.Visible);
        Assert.True(loadedSalesInvoice.SaveData);

        var loadedPos = loaded.entUserPrivilegeDtls.FirstOrDefault(p => p.MenuCaption == "POS");
        Assert.NotNull(loadedPos);
        Assert.False(loadedPos.Visible); // missing initially -> defaulted to false
        Assert.Equal("Sales", loadedPos.MenuTitle);

        var loadedBackup = loaded.entUserPrivilegeDtls.FirstOrDefault(p => p.MenuCaption == "Database Backup");
        Assert.NotNull(loadedBackup);
        Assert.False(loadedBackup.Visible);
        Assert.Equal("Settings", loadedBackup.MenuTitle);
    }

    [Theory]
    [InlineData("POS", true, "نقطة البيع")]
    [InlineData("POSView", true, "نقطة البيع")]
    [InlineData("Database Backup", true, "نسخ احتياطي لقاعدة البيانات")]
    [InlineData("DBBackup", true, "نسخ احتياطي لقاعدة البيانات")]
    [InlineData("POS", false, "POS")]
    [InlineData("Database Backup", false, "Database Backup")]
    public void ArabicLocalization_Translates_POS_And_DBBackup_Correctly(string key, bool isArabic, string expected)
    {
        AppData.IsArabic = isArabic;
        var localized = ArabicLocalization.Localizer(key);
        Assert.Equal(expected, localized);
    }
}
