using System.Text.Json;
namespace AshproSAASERP.Shared.Pages.Reports.Sales;

public partial class SalesQuotationRegisterView
{
    #region Variables
    byte[]? pdfData;
    bool IsLoading = false;
    SalesQuotationRegisterResponse? result;
    List<CommonModel> branchList = new();
    SalesQuotationRegisterRequest req = new();
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        try
        {
            branchList = await api.GetCombofill("Branch", 0);
            ClearMethod();
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    void ClearMethod()
    {
        req = new SalesQuotationRegisterRequest { FromDate = DateTime.Now, ToDate = DateTime.Now, BranchId = AppData.BranchId };
        pdfData = null;
        result = null;
    }
    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            pdfData = null;
            IsLoading = true;
            if (AppData.IsWindows)
            {
                result = await api.GetSalesQuotationRegisterAsync(req);
                if (result is not null && result.Items.Any())
                {
                    var reportHeader = new SalesQuotationRegisterHdr(
                        $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                        (result.TotalVat ?? 0).ToString("N2"),
                        (result.TotalNet ?? 0).ToString("N2")
                    );
                    Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                    Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };
                    var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Sales_Quotation_Register"));
                    if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
                    {
                        await Dialog.DialogMethod("Print design 'Sales Quotation Register' not found.");
                        return;
                    }
                    SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                    pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
                }
                else
                {
                    await Dialog.DialogMethod(Localizer("No records found"));
                }
            }
            else
            {
                var data = await api.GetSalesQuotationRegisterReportPDF(req);
                if (data is not null && data.IsSuccessStatusCode)
                {
                    using var stream = data.Content;
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    if (IsPreview)
                        pdfData = memoryStream.ToArray();
                    else
                        await downloader.DownloadFromStream(memoryStream.ToArray(), "SalesQuotationRegister.pdf");
                }
                else
                {
                    await Dialog.DialogMethod("Failed to generate report");
                }
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
            if (result == null)
            {
                result = await api.GetSalesQuotationRegisterAsync(req);
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
                await Dialog.DialogMethod(Localizer("No records found"));
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion
}
