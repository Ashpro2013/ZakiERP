using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Accounts;

public partial class LedgerReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    private List<LedgerReportDtls> reportData = new();
    LedgerReportResponse? result;
    bool IsLoading = false;

    // Filters
    int BranchId = 0;
    int AccountId = 0;
    string AccountName = "";

    // Data Sources
    List<CommonModel> branchList = new();
    #endregion

    #region Properties
    [Parameter] public int TabId { get; set; }
    [Parameter] public LedgerReportRequest? req { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        branchList = await api.GetCombofill("Branch", BranchId);
        if (req is not null)
        {
            BranchId = req.BranchId ?? 0;
            FromDate = req.FromDate;
            ToDate = req.ToDate;
            AccountId = req.AccountId ?? 0;
            await ReportPdfGenerate(true);
        }
        else
        {
            ClearMethod();
        }
    }

    void ClearMethod()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        BranchId = 0;
        AccountId = 0;
        AccountName = "";
        reportData.Clear();
        result = null;
    }

    async Task AccountSearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<AccountSearchDialog>(Localizer("AccountSearch"), options);
        var dialogResult = await dialog.Result;
        if (!dialogResult!.Canceled && dialogResult.Data is AccountMaster selectedAccount)
        {
            AccountId = selectedAccount.AccountId;
            AccountName = selectedAccount.Name;
            StateHasChanged();
        }
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            if (AccountId == 0)
            {
                await Dialog.DialogMethod(Localizer("Please select an account"));
                return;
            }
            IsLoading = true;
            StateHasChanged();
            req = new LedgerReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate,
                AccountId = AccountId == 0 ? null : AccountId
            };
            result = await api.GetLedgerReport(req);
            if (result == null || !result.Items.Any())
            {
                IsLoading = false;
                StateHasChanged();
                await Dialog.DialogMethod(Localizer("No records found"));
                return;
            }
            if (IsPreview)
            {
                reportData = result?.Items ?? new List<LedgerReportDtls>();
            }
            else
            {
                var reportHeader = new LedgerReportHdr(
                        $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                        (result.TotalDebit ?? 0).ToString("N2"),
                        (result.TotalCredit ?? 0).ToString("N2"),
                        result.TotalBalance
                    );

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Ledger_Report"));
                if (pdto.hdr == null || !pdto.dtls.Any())
                {
                    IsLoading = false;
                    StateHasChanged();
                    await Dialog.DialogMethod("Print design 'Ledger_Report' not found. Please create design from designer.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);
            }

        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    private async Task RowClickEvent(TableRowClickEventArgs<LedgerReportDtls> args)
    {
        var item = args.Item;
        if (args.MouseEventArgs.Detail >= 2 && item is not null && !string.IsNullOrEmpty(item.VoucherType))
        {
            // Navigate based on transaction type
            switch (item.VoucherType)
            {
                case "CustomerReceipt":
                    await CrudHelper.Navigation(tabService, item.Id.Value, "CustomerReceipt");
                    break;
                case "Sales":
                    await CrudHelper.Navigation(tabService, item.Id.Value, "SalesInvoiceTitle");
                    break;
                case "SalesReturn":
                    await CrudHelper.Navigation(tabService, item.Id.Value, "SalesInvoiceRtnTitle");
                    break;
                case "SupplierPayment":
                    await CrudHelper.Navigation(tabService, item.Id.Value, "SupplierPayment");
                    break;
                case "Purchase":
                    await CrudHelper.Navigation(tabService, item.Id.Value, "PurchaseTitle");
                    break;
                case "PurchaseReturn":
                    await CrudHelper.Navigation(tabService, item.Id.Value, "Purchase Return");
                    break;
                case "JournalEntry":
                    await CrudHelper.Navigation(tabService, item.Id.Value, "JournalEntry");
                    break;
                case "ExpenseEntry":
                    await CrudHelper.Navigation(tabService, item.Id.Value, "ExpenseEntry");
                    break;

            }
        }
    }
    async Task ReportGenerate() => await ReportPdfGenerate(true);
    async Task PDFExportMethod() => await ReportPdfGenerate();

    async Task ExcelExportMethod()
    {
        try
        {
            if (AccountId == 0)
            {
                await Dialog.DialogMethod(Localizer("Please select an account"));
                return;
            }

            var req = new LedgerReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate,
                AccountId = AccountId == 0 ? null : AccountId
            };

            if (result == null)
            {
                result = await api.GetLedgerReport(req);
            }

            if (result is not null && result.Items.Any())
            {
                var report = result.Items;
                var jsonData = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
            }
            else
            {
                await Dialog.DialogMethod(Localizer("No records found"));
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion
}
