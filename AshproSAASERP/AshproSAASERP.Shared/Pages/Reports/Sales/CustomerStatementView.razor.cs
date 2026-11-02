using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Sales;

public partial class CustomerStatementView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    CustomerStatementResponse result;
    bool IsLoaded = false;

    // Filters
    int BranchId = 0;
    int CustomerId = 0;
    string CustomerName = "";

    // Data Sources
    List<CommonModel> branchList = new();
    [Parameter] public int TabId { get; set; }

    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        FromDate = DateTime.Today.AddDays(-30);
        ToDate = DateTime.Today;
        BranchId = 0; // Default All

        try
        {
            // Load Combos
            branchList = await api.GetCombofill("Branch", BranchId);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    void ClearMethod()
    {
        FromDate = DateTime.Today.AddDays(-30);
        ToDate = DateTime.Today;
        BranchId = 0;
        CustomerId = 0;
        CustomerName = "";
        pdfData = null;
        IsLoaded = false;
        result = null;
    }

    async Task PartySearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
        var res = await dialog.Result;
        if (!res!.Canceled && res.Data is CustomerMaster selectedCustomer)
        {
            CustomerId = selectedCustomer.CustomerId;
            CustomerName = selectedCustomer.Name;
            StateHasChanged();
        }
    }

    async Task LoadData()
    {
        try
        {
            if (CustomerId == 0)
            {
                Dialog.DialogMethod(Localizer("Please select a customer"));
                return;
            }

            var req = new CustomerStatementRequest(
                FromDate.ToDateTime(),
                ToDate.ToDateTime(),
                CustomerId,
                BranchId
            );

            result = await api.GetCustomerStatementReport(req);

            if (result is not null && (result.Details.Any() || result.OpeningBalance != 0))
            {
                // Insert Opening Balance Row ONLY if it's not zero
                if (result.OpeningBalance != 0)
                {
                    result.Details.Insert(0, new CustomerStatementReportDtls(
                        0,
                        FromDate.ToDateTime().ToString("dd-MM-yyyy"),
                        "",
                        "",
                        "OPENING BALANCE",
                        null,
                        null,
                        result.OpeningBalance,
                        "" // RefNo
                    ));
                }

                IsLoaded = true;
                pdfData = null;
            }
            else
            {
                IsLoaded = false;
                Dialog.DialogMethod(Localizer("No records found"));
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task ReportGenerate() => await LoadData();


    async Task PDFExportMethod()
    {
        try
        {
            if (result == null || (!result.Details.Any() && result.OpeningBalance == 0))
            {
                await LoadData();
                if (result == null || (!result.Details.Any() && result.OpeningBalance == 0)) return;
            }

            // Prepare Header Data
            var reportHeader = new
            {
                FromDate = FromDate.ToDateTime().ToString("dd-MM-yyyy"),
                ToDate = ToDate.ToDateTime().ToString("dd-MM-yyyy"),
                PrintDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                CustomerName = result.Customer?.Name ?? CustomerName,
                CustomerCode = result!.Customer?.Code ?? "",
                OpeningBalance = result.OpeningBalance.ToString("N2"),
                TotalDebit = result.TotalDebit.ToString("N2"),
                TotalCredit = result.TotalCredit.ToString("N2"),
                ClosingBalance = result.ClosingBalance.ToString("N2")
            };

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));

            // Map DTO Description to Narration for Print Design
            // Exclude the artificially added Opening Balance row for PDF if the header already has it, 
            // BUT standard practice often includes it in the grid too. 
            // However, to be safe and consistent with previous "Standard" request, let's keep it in the list.
            var obj = result.Details.Select(d => new
            {
                d.SL,
                d.Date,
                d.VoucherNo,
                d.Type,
                Narration = d.Description,
                d.Debit,
                d.Credit,
                d.Balance,
                d.RefNo
            }).Cast<object>().ToList();

            Dictionary<string, object> ReportDtls = new() { { "Details", obj } };

            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Customer_Statement"));

            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                Dialog.DialogMethod(Localizer("Print Design 'Customer_Statement' not found. Please contact admin."));
                return;
            }

            SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
            // isPreview = false triggers download
            await pHelper.PrintReportMethod(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, false);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    async Task ExcelExportMethod()
    {
        try
        {
            if (result == null || (!result.Details.Any() && result.OpeningBalance == 0))
            {
                await LoadData();
                if (result == null || (!result.Details.Any() && result.OpeningBalance == 0)) return;
            }

            if (result is not null && (result.Details.Any() || result.OpeningBalance != 0))
            {
                // Create a copy list to add Total row without affecting the UI result
                var report = result.Details.ToList();

                // Add Total Row
                report.Add(new CustomerStatementReportDtls(
                    0,
                    "",
                    "",
                    "",
                    "TOTAL",
                    result.TotalDebit,
                    result.TotalCredit,
                    result.ClosingBalance,
                    ""
                ));

                var jsonData = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    private async Task RowClickEvent(TableRowClickEventArgs<CustomerStatementReportDtls> args)
    {
        var item = args.Item;
        if (item == null || string.IsNullOrEmpty(item.Type) || item.VoucherId == 0) return;

        // Navigate based on transaction type (Matching Ledger Report Logic)
        switch (item.Type)
        {
            case "CustomerReceipt":
                await CrudHelper.Navigation(TabService, item.VoucherId, "CustomerReceipt");
                break;
            case "Sales":
                await CrudHelper.Navigation(TabService, item.VoucherId, "SalesInvoiceTitle"); // Check Title Key
                break;
            case "SalesReturn":
                await CrudHelper.Navigation(TabService, item.VoucherId, "SalesInvoiceRtnTitle");
                break;
            case "CustomerOPEntry":
                await CrudHelper.Navigation(TabService, item.VoucherId, "CustomerOpeningEntry");
                break;
            case "JournalEntry":
                await CrudHelper.Navigation(TabService, item.VoucherId, "JournalEntry");
                break;
                // Add other types as needed
        }
    }
    #endregion
}
