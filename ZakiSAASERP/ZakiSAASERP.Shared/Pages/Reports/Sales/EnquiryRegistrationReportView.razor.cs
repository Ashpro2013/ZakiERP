using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Sales;

public partial class EnquiryRegistrationReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[] pdfData;
    bool IsLoading = false;
    EnquiryRegistrationReportResponse result;

    // Filters
    int BranchId = 0;
    int SalesManId = 0;
    int CustomerId = 0;
    string CustomerName = "";

    // Data Sources
    List<CommonModel> branchList = new();
    List<CommonModel> salesmanList = new();
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        FromDate = new DateTime(DateTime.Today.Year, 1, 1);
        ToDate = DateTime.Today;
        BranchId = 0; // Default All

        try
        {
            // Load Combos
            branchList = await api.GetCombofill("Branch", BranchId);
            salesmanList = await api.GetCombofill("SalesMan", BranchId);
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }

    void ClearMethod()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        BranchId = 0;
        SalesManId = 0;
        CustomerId = 0;
        CustomerName = "";
        pdfData = null;
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

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            IsLoading = true;
            var req = new EnquiryRegistrationReportRequest(
                BranchId,
                FromDate.ToDateTime(),
                ToDate.ToDateTime(),
                CustomerId == 0 ? null : CustomerId,
                SalesManId == 0 ? null : SalesManId
            );

            result = await api.GetEnquiryRegistrationReport(req);

            if (result is not null && result.Items.Any())
            {
                var reportHeader = new EnquiryRegistrationReportHdr(
                    DateTime.Now.ToString("dd-MM-yyyy"),
                    DateTime.Now.ToString("HH:mm:ss")
                );

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                var obj = result.Items.Cast<object>().ToList();
                Dictionary<string, object> ReportDtls = new() { { "Details", obj } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "EnquiryRegistration_Report"));
                if (pdto.hdr == null || !pdto.dtls.Any())
                {
                    await Dialog.DialogMethod("Print design 'EnquiryRegistration_Report' not found.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintReportMethod(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
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
        finally
        {
            IsLoading = false;
        }
    }

    async Task ReportGenerate() => await ReportPdfGenerate(true);
    async Task PDFExportMethod() => await ReportPdfGenerate();

    async Task ExcelExportMethod()
    {
        try
        {
            var req = new EnquiryRegistrationReportRequest(
               BranchId,
               FromDate.ToDateTime(),
               ToDate.ToDateTime(),
               CustomerId == 0 ? null : CustomerId,
               SalesManId == 0 ? null : SalesManId
           );

            if (result == null)
            {
                result = await api.GetEnquiryRegistrationReport(req);
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
