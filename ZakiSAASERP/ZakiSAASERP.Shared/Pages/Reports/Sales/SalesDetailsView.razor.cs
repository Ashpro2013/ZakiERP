using ZakiSAASERP.Shared.Layout;
using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Sales
{
    public partial class SalesDetailsView : ComponentBase
    {
        // NO injections here, all in razor file. 
        // ISnackbar removed to avoid conflicts and errors, using Dialog.DialogMethod instead.

        #region Variables
        DateTime? FromDate;
        DateTime? ToDate;
        byte[]? pdfData;
        SalesDetailsReportResponse? result;

        // Filters
        int BranchId { get; set; } = 0;
        int SalesManId { get; set; } = 0;
        int CustomerId { get; set; } = 0;
        string CustomerName { get; set; } = "";
        string InvoiceType { get; set; } = "All";
        string BillType { get; set; } = "All";
        string ItemName { get; set; } = "";
        bool IsLoading = false;
        [CascadingParameter] public MainLayout Layout { get; set; }

        // Data Sources
        List<CommonModel> branchList = new();
        List<CommonModel> salesmanList = new();
        bool IsResultLoaded = false;
        bool IsLoaded => IsResultLoaded;
        string searchString = "";
        [Parameter] public int TabId { get; set; }
        #endregion

        protected override async Task OnInitializedAsync()
        {
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
            BranchId = 0;

            try
            {
                branchList = await api.GetCombofill("Branch", BranchId);
                salesmanList = await api.GetCombofill("SalesMan", BranchId);
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
            SalesManId = 0;
            CustomerId = 0;
            CustomerName = "";
            InvoiceType = "All";
            BillType = "All";
            ItemName = "";
            pdfData = null;
            result = null;
            IsResultLoaded = false;
            searchString = "";
        }

        async Task PartySearchMethod()
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Large
            };
            var dialog = await DialogService.ShowAsync<CustomerSearchDialog>(Localizer("CustomerSearch"), options);
            var resultDialog = await dialog.Result;
            if (!resultDialog!.Canceled && resultDialog.Data is CustomerMaster selectedCustomer)
            {
                CustomerId = selectedCustomer.CustomerId;
                CustomerName = selectedCustomer.Name;
                StateHasChanged();
            }
        }

        protected async Task GenerateReport() => await ReportPdfGenerate(false, false); // Separate logic: Generate Grid only

        protected async Task PrintMethod() => await ReportPdfGenerate(false, true); // Print

        protected async Task PreviewMethod() => await ReportPdfGenerate(true, true); // Preview

        async Task ReportPdfGenerate(bool IsPreview = false, bool IsPrint = false)
        {
            try
            {
                IsLoading = true;
                bool Iswindows = AppData.IsWindows;

                if (IsPrint)
                {
                    if (Iswindows)
                    {
                        if (result == null || !result.Reports.Any())
                        {
                            await FetchData();
                            if (result == null || !result.Reports.Any()) return;
                        }
                        await PrintPreviewLogic(IsPreview);
                    }
                    else
                    {
                        var req = new SalesDetailsReportRequest(
                             BranchId,
                             FromDate.Value,
                             ToDate.Value,
                             CustomerId == 0 ? null : CustomerId,
                             SalesManId == 0 ? null : SalesManId,
                             InvoiceType,
                             BillType,
                             ItemName
                         );

                        var data = await api.GetSalesDetailsReportPDF(req);
                        if (data is not null && data.IsSuccessStatusCode)
                        {
                            using var stream = data.Content;

                            using var memoryStream = new MemoryStream();
                            await stream.CopyToAsync(memoryStream);

                            if (IsPreview)
                                pdfData = memoryStream.ToArray();
                            else
                                await downloader.DownloadFromStream(memoryStream.ToArray(), "SalesDetailsReport.pdf");
                        }
                        else
                        {
                            await Dialog.DialogMethod("Failed to generate report");
                        }
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

        async Task FetchData()
        {
            try
            {
                IsResultLoaded = false;
                pdfData = null;

                if (FromDate == null || ToDate == null)
                {
                    await Dialog.DialogMethod(Localizer("Please select a date range."));
                    return;
                }

                var req = new SalesDetailsReportRequest(
                    BranchId,
                    FromDate.Value,
                    ToDate.Value,
                    CustomerId == 0 ? null : CustomerId,
                    SalesManId == 0 ? null : SalesManId,
                    InvoiceType,
                    BillType,
                    ItemName
                );

                result = await api.GetSalesDetailsReport(req);

                if (result is not null && result.Reports.Any())
                {
                    IsResultLoaded = true;
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

        async Task PrintPreviewLogic(bool IsPreview)
        {
            var reportHeader = new SalesReportHdr(
               $"{FromDate:dd-MM-yyyy} - {ToDate:dd-MM-yyyy}",
               DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
               (result.TotalGross ?? 0).ToString("N2"),
               (result.TotalDiscount ?? 0).ToString("N2"),
               (result.TotalAmount ?? 0).ToString("N2"),
               (result.TotalTax ?? 0).ToString("N2"),
               (result.TotalAmount ?? 0).ToString("N2")
           );

            Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
            Dictionary<string, object> ReportDtls = new() { { "Details", result.Reports } };

            var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "SalesDetails_Report"));

            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Sales_Report"));
            }

            if (pdto.hdr == null || !pdto.dtls.Any())
            {
                await Dialog.DialogMethod("Print design 'SalesDetails_Report' not found.");
                return;
            }

            SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
            pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr, ReportDtls, null, IsPreview);
            IsResultLoaded = false;
        }

        protected async Task DownloadExcel()
        {
            try
            {
                if (FromDate == null || ToDate == null) return;

                var req = new SalesDetailsReportRequest(
                   BranchId,
                   FromDate.Value,
                   ToDate.Value,
                   CustomerId == 0 ? null : CustomerId,
                   SalesManId == 0 ? null : SalesManId,
                   InvoiceType,
                   BillType,
                   ItemName
               );

                if (result == null)
                {
                    result = await api.GetSalesDetailsReport(req);
                }

                if (result is not null && result.Reports.Any())
                {
                    var report = result.Reports;

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

        async Task RowClickEvent(TableRowClickEventArgs<SalesDetailsReportDtls> args)
        {
            if (args.MouseEventArgs.Detail >= 2 && args.Item.Id.HasValue && args.Item.Id > 0 && !string.IsNullOrEmpty(args.Item.InvType))
            {
                if (args.Item.InvType.Equals("Sales", StringComparison.OrdinalIgnoreCase))
                {
                    await CrudHelper.Navigation(tabService, args.Item.Id.Value, "SalesInvoiceTitle");
                }
                else if (args.Item.InvType.Contains("Return", StringComparison.OrdinalIgnoreCase))
                {
                    await CrudHelper.Navigation(tabService, args.Item.Id.Value, "SalesInvoiceRtnTitle");
                }
            }
        }
    }
}
