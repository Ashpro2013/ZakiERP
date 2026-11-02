using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Sales;

public partial class ProformaInvoiceReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    bool IsLoading = false;
    ProformaInvoiceReportResponse? result;

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
            await Dialog.DialogMethod(ex.Message);
        }
    }

    void ClearMethod()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        BranchId = 0;
        CustomerId = 0;
        CustomerName = "";
        pdfData = null;
        result = null;
    }

    async Task CustomerSearchMethod()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large
        };
        var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
        var dialogResult = await dialog.Result;
        if (!dialogResult!.Canceled && dialogResult.Data is CustomerMaster selectedCustomer)
        {
            CustomerId = selectedCustomer.CustomerId;
            CustomerName = selectedCustomer.Name!;
            StateHasChanged();
        }
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            pdfData = null;
            IsLoading = true;
            var req = new ProformaInvoiceReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate
            };
            if (AppData.IsWindows)
            {
                result = await api.GetProformaInvoiceReport(req);

                if (result is not null && result.Items.Any())
                {
                    var filteredItems = result.Items;
                    if (CustomerId > 0)
                    {
                        filteredItems = result.Items.Where(x => string.IsNullOrEmpty(CustomerName) || (x.CustomerName?.Contains(CustomerName, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

                        // Recalculate totals for filtered items
                        var totalDiscount = filteredItems.Sum(x => x.Discount ?? 0);
                        var totalVat = filteredItems.Sum(x => x.VatAmount ?? 0);
                        var totalNet = filteredItems.Sum(x => x.TotalAmount ?? 0);

                        // Update result with filtered items
                        result = new ProformaInvoiceReportResponse(filteredItems, totalDiscount, totalVat, totalNet);
                    }

                    if (result.Items.Any())
                    {
                        var reportHeader = new ProformaInvoiceReportHdr(
                            $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                            DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                            (result.TotalDiscount ?? 0).ToString("N2"),
                            (result.TotalVat ?? 0).ToString("N2"),
                            (result.TotalNet ?? 0).ToString("N2")
                        );

                        Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                        Dictionary<string, object> ReportDtls = new() { { "Details", filteredItems } };

                        var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Proforma_Invoice_Report"));
                        if (pdto.hdr == null || !pdto.dtls.Any())
                        {
                            await Dialog.DialogMethod("Print design 'Proforma_Invoice_Report' not found.");
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
                    await Dialog.DialogMethod(Localizer("No records found"));
                }
            }
            else
            {
                var data = await api.GetProformaInvoiceReportPDF(req);
                if (data is not null && data.IsSuccessStatusCode)
                {
                    using var stream = data.Content;
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    if (IsPreview)
                        pdfData = memoryStream.ToArray();
                    else
                        await downloader.DownloadFromStream(memoryStream.ToArray(), "ProformaInvoiceReport.pdf");
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
            var req = new ProformaInvoiceReportRequest
            {
                BranchId = BranchId,
                FromDate = FromDate,
                ToDate = ToDate
            };

            if (result == null)
            {
                result = await api.GetProformaInvoiceReport(req);
                // Apply filtering if needed (same logic as above)
                if (result is not null && result.Items.Any() && CustomerId > 0)
                {
                    var filteredItems = result.Items.Where(x => string.IsNullOrEmpty(CustomerName) || (x.CustomerName?.Contains(CustomerName, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                    var totalDiscount = filteredItems.Sum(x => x.Discount ?? 0);
                    var totalVat = filteredItems.Sum(x => x.VatAmount ?? 0);
                    var totalNet = filteredItems.Sum(x => x.TotalAmount ?? 0);
                    result = new ProformaInvoiceReportResponse(filteredItems, totalDiscount, totalVat, totalNet);
                }
            }

            if (result is not null && result.Items.Any())
            {
                var report = result.Items.ToList(); // Clone list
                // Add Total Row
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
