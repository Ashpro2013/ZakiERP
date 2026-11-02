using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Sales;

public partial class VatReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[] pdfData;
    VatReportResponse result;

    // Filters
    int BranchId = 0;

    // Data Sources
    List<CommonModel> branchList = new();
    bool IsLoading = false;
    [Parameter] public int TabId { get; set; }
    #endregion

    #region Method
    protected override async Task OnInitializedAsync()
    {
        FromDate = DateTime.Today;
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
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        BranchId = 0;
        pdfData = null;
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            pdfData = null;
            IsLoading = true;
            var req = new VatReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate.ToDateTime(),
                ToDate = ToDate.ToDateTime()
            };

            result = await api.GetVatReport(req);

            if (result is not null && result.Items.Any())
            {
                var report = result.Items;
                // Add Total Row for Print is handled by Header usually, or added to list.
                // Here we usually pass Totals in Header or separate argument.
                // But ReportView expects Headers and Details.

                var reportHeader = new VatReportHdr(
                    $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                    DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    (result.TotalTaxable ?? 0).ToString("N2"),
                    (result.TotalVatAmount ?? 0).ToString("N2")
                );

                var dtls = new List<VatReportDtls>();
                foreach (var item in report)
                {
                    dtls.Add(item);
                }

                Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));

                var obj = dtls.Cast<object>().ToList();
                Dictionary<string, object> ReportDtls = new() { { "Details", obj } };

                var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Vat_Report"));
                if (pdto.hdr == null || !pdto.dtls.Any())
                {
                    Dialog.DialogMethod("Print design 'Vat_Report' not found.");
                    return;
                }

                SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
            }
            else
            {
                Dialog.DialogMethod(Localizer("No records found"));
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
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
            var req = new VatReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate.ToDateTime(),
                ToDate = ToDate.ToDateTime()
            };

            if (result == null)
            {
                result = await api.GetVatReport(req);
            }

            if (result is not null && result.Items.Any())
            {
                var report = result.Items;
                // Add Total Row
                report.Add(new VatReportDtls(
                    null,             // SL
                    "TOTAL",        // InvoiceType
                    null,             // TaxPercentage
                    result.TotalTaxable, // Taxable
                    result.TotalVatAmount // VatAmount
                ));

                var jsonData = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
            }
            else
            {
                Dialog.DialogMethod(Localizer("No records found"));
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion
}
