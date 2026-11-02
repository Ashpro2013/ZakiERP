using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Sales;

public partial class DateWiseProfitReportView
{
    DateWiseProfitReportRequest req = new() { FromDate = DateTime.Now.Date, ToDate = DateTime.Now.Date };
    DateWiseProfitReportResponse? result;
    List<ExMasters> branches = new();
    byte[]? pdfData;
    bool IsLoading = false;
    [Parameter] public int TabId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        branches = await api.GetMasters("BRANCHMASTER", 0);
    }

    async Task ClearMethod()
    {
        req = new() { FromDate = DateTime.Now.Date, ToDate = DateTime.Now.Date };
        pdfData = null;
        result = null;
    }

    async Task ReportGenerate()
    {
        await ReportPdfGenerate(true);
    }

    async Task PDFExportMethod()
    {
        await ReportPdfGenerate(false);
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            pdfData = null;
            IsLoading = true;
            if (AppData.IsWindows)
            {
                result = await api.GetDateWiseProfitReport(req);

                if (result is not null && result.Items.Any())
                {
                    var reportHeader = new DateWiseProfitReportHdr(
                        $"{req.FromDate:dd-MM-yyyy} - {req.ToDate:dd-MM-yyyy}",
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                        (result.TotalSalesAmount ?? 0).ToString("N2"),
                        (result.TotalSalesReturn ?? 0).ToString("N2"),
                        (result.TotalNetSales ?? 0).ToString("N2"),
                        (result.TotalCost ?? 0).ToString("N2"),
                        (result.TotalProfit ?? 0).ToString("N2"),
                        (result.TotalProfitPercentage ?? 0).ToString("N2")
                    );

                    Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                    Dictionary<string, object> ReportDtls = new() { { "Details", result.Items } };

                    var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Date_Wise_Profit_Report"));
                    if (pdto == null || pdto.hdr == null || !pdto.dtls.Any())
                    {
                        await Dialog.DialogMethod("Print design 'Date_Wise_Profit_Report' not found.");
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
                var data = await api.GetDateWiseProfitReportPDF(req);
                if (data is not null && data.IsSuccessStatusCode)
                {
                    using var stream = data.Content;
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    if (IsPreview)
                        pdfData = memoryStream.ToArray();
                    else
                        await downloader.DownloadFromStream(memoryStream.ToArray(), "DateWiseProfitReport.pdf");
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

    async Task ExcelExportMethod()
    {
        try
        {
            if (result == null)
            {
                result = await api.GetDateWiseProfitReport(req);
            }

            if (result is not null && result.Items.Any())
            {
                var jsonData = JsonSerializer.Serialize(result.Items, new JsonSerializerOptions
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
}
