using Xunit;
using ZakiSAASERP.Models.User;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Pages.Reports.Transaction;
using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Tests.Zatca;

public class ZatcaPendingNavigationTests
{
    [Theory]
    [InlineData("PASS", false)]
    [InlineData("REPORTED", false)]
    [InlineData("CLEARED", false)]
    [InlineData("SUCCESS", false)]
    [InlineData("Success", false)]
    [InlineData("WARNING", false)]
    [InlineData("FAILED", true)]
    [InlineData("Failed", true)]
    [InlineData("ERROR", true)]
    [InlineData("REJECTED", true)]
    public void IsFailedInvoice_EvaluatesDetailedStatusCorrectly_WhenNotExplicitlyShowingFailed(string status, bool expectedFailed)
    {
        var view = new ZatcaPendingView { ShowingFailed = false };
        var item = new ExInvoiceICV
        {
            VoucherId = 101,
            DetailedStatus = status
        };

        bool result = view.IsFailedInvoice(item);

        Assert.Equal(expectedFailed, result);
    }

    [Fact]
    public void IsFailedInvoice_AlwaysReturnsTrue_WhenShowingFailedIsTrue()
    {
        var view = new ZatcaPendingView { ShowingFailed = true };
        var item = new ExInvoiceICV
        {
            VoucherId = 102,
            DetailedStatus = "PASS" // even if status was pass previously, on failed grid view it's treated as failed
        };

        bool result = view.IsFailedInvoice(item);

        Assert.True(result);
    }

    [Fact]
    public void IsFailedInvoice_ReturnsFalse_ForNullItem()
    {
        var view = new ZatcaPendingView { ShowingFailed = true };

        bool result = view.IsFailedInvoice(null);

        Assert.False(result);
    }

    [Theory]
    [InlineData("Standard Invoice", false)]
    [InlineData("Simplified Invoice", false)]
    [InlineData("Standered Invoice", false)]
    [InlineData("Sales Invoice", false)]
    [InlineData("Standard Credit Note", true)]
    [InlineData("Simplified Credit Note", true)]
    [InlineData("Standard Credit Invoice", true)]
    [InlineData("Credit Note", true)]
    [InlineData("Sales Return", true)]
    public void IsCreditNote_IdentifiesInvoicesAndCreditNotesCorrectly(string voucherType, bool expectedIsCredit)
    {
        var view = new ZatcaPendingView();
        var item = new ExInvoiceICV
        {
            VoucherId = 201,
            VoucherType = voucherType
        };

        bool result = view.IsCreditNote(item);

        Assert.Equal(expectedIsCredit, result);
    }

    [Fact]
    public void IsCreditNote_FallsBackToIsInvoice_WhenVoucherTypeIsNullOrEmpty()
    {
        var viewInvoice = new ZatcaPendingView { IsInvoice = true };
        var viewReturn = new ZatcaPendingView { IsInvoice = false };
        var emptyItem = new ExInvoiceICV { VoucherId = 202, VoucherType = "" };

        Assert.False(viewInvoice.IsCreditNote(emptyItem));
        Assert.True(viewReturn.IsCreditNote(emptyItem));
    }

    [Fact]
    public async Task CrudHelper_Navigation_SetsIsFromZatcaReportParameter()
    {
        // Arrange
        AppData.menuList = new List<MenuDetails>
        {
            new MenuDetails
            {
                Id = 462,
                Name = "SalesInvoiceTitle",
                ClassName = "SalesInvoiceView",
                MenuCaption = "Sales Invoice",
                ArabicCaption = "فاتورة مبيعات",
                AssemblyName = "ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceView"
            },
            new MenuDetails
            {
                Id = 463,
                Name = "SalesInvoiceRtnTitle",
                ClassName = "SalesInvoiceRtnView",
                MenuCaption = "Sales Invoice Return",
                ArabicCaption = "مرتجع فاتورة مبيعات",
                AssemblyName = "ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceRtnView"
            }
        };

        var tabService = new TabService();
        TabRequest? receivedRequest = null;
        tabService.OnOpenTab += req =>
        {
            receivedRequest = req;
            return Task.CompletedTask;
        };

        // Act - Navigate to Sales Invoice with isFromZatcaReport = true
        await CrudHelper.Navigation(tabService, 555, "SalesInvoiceTitle", null, isFromZatcaReport: true);

        // Assert
        Assert.NotNull(receivedRequest);
        Assert.Equal(462, receivedRequest.Id);
        Assert.Equal("ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceView", receivedRequest.View);
        Assert.NotNull(receivedRequest.Parameters);
        Assert.Equal(555, receivedRequest.Parameters["Id"]);
        Assert.True((bool)receivedRequest.Parameters["IsFromZatcaReport"]);

        // Act - Navigate to Sales Return with isFromZatcaReport = true
        receivedRequest = null;
        await CrudHelper.Navigation(tabService, 777, "SalesInvoiceRtnTitle", null, isFromZatcaReport: true);

        // Assert
        Assert.NotNull(receivedRequest);
        Assert.Equal(463, receivedRequest.Id);
        Assert.Equal("ZakiSAASERP.Shared.Pages.Transactions.Sales.SalesInvoiceRtnView", receivedRequest.View);
        Assert.NotNull(receivedRequest.Parameters);
        Assert.Equal(777, receivedRequest.Parameters["Id"]);
        Assert.True((bool)receivedRequest.Parameters["IsFromZatcaReport"]);
    }
}

