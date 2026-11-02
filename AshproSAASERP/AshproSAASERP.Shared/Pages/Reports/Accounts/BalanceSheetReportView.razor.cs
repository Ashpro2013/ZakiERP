using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Reports.Accounts;

public partial class BalanceSheetReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    BalanceSheetReportResponse? result;

    // Filters
    int BranchId = 0;

    // Data Sources
    List<CommonModel> branchList = new();
    private List<BalanceSheetReportDtls> reportData = new();
    bool IsLoading = false;
    #endregion

    #region Properties
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
        pdfData = null;
        result = null;
    }

    async Task ReportPdfGenerate(bool IsPreview = false)
    {
        try
        {
            IsLoading = true;
            StateHasChanged();
            var req = new BalanceSheetReportRequest(BranchId, FromDate ?? DateTime.Today, ToDate ?? DateTime.Today);

            result = await api.GetBalanceSheetReport(req);

            if (result is not null && result.Items.Any())
            {
                var finalReportItems = PrepareReportData(result);
                if (IsPreview)
                {
                    reportData = finalReportItems;
                }
                else
                {
                    // Create Header
                    var reportHeader = new BalanceSheetReportHdr
                    (
                        $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                        result.TotalLiabilities.ToString("N2"),
                        result.TotalAssets.ToString("N2")
                    );

                    Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                    foreach (var item in finalReportItems)
                    {
                        item.LiabilityAccount = item.LiabilityAccount ?? "";
                        item.LiabilityAmount = item.LiabilityAmount ?? "";
                        item.AssetAccount = item.AssetAccount ?? "";
                        item.AssetAmount = item.AssetAmount ?? "";
                    }
                    Dictionary<string, object> ReportDtls = new() { { "Details", finalReportItems } };

                    var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Balance_Sheet_Report"));
                    if (pdto.hdr == null || !pdto.dtls.Any())
                    {
                        IsLoading = false;
                        StateHasChanged();
                        await Dialog.DialogMethod("Print design 'Balance Sheet Report' not found. Please create design from designer.");
                        return;
                    }

                    SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
                    pdfData = await pHelper.PrintReportMethodNew(downloader, pdto.hdr, pdto.dtls, ReportHdr!, ReportDtls, null, IsPreview);
                }
            }
            else
            {
                await Dialog.DialogMethod("No records found");
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

    async Task ReportGenerate() => await ReportPdfGenerate(true);
    async Task PDFExportMethod() => await ReportPdfGenerate();

    async Task ExcelExportMethod()
    {
        try
        {
            if (result == null)
            {
                var req = new BalanceSheetReportRequest(BranchId, FromDate ?? DateTime.Today, ToDate ?? DateTime.Today);
                result = await api.GetBalanceSheetReport(req);
            }

            if (result is not null && result.Items.Any())
            {
                var finalReportItems = PrepareReportData(result);

                var jsonData = JsonSerializer.Serialize(finalReportItems, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await JS.InvokeVoidAsyncIgnoreErrors("exportJsonToExcel", jsonData);
            }
            else
            {
                await Dialog.DialogMethod("No records found");
            }
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }

    List<BalanceSheetReportDtls> PrepareReportData(BalanceSheetReportResponse data)
    {
        List<BalanceSheetReportDtls> finalReportItems = new();

        var assets = data.Items.Where(x => x.Category == "Asset").ToList();
        var liabilities = data.Items.Where(x => x.Category == "Liability").ToList();
        var capital = data.Items.Where(x => x.Category == "Capital").ToList();

        // Left Side: Liabilities + Capital (+ Net Profit/Loss)
        var leftList = new List<(string Name, decimal Amount)>();

        // Capital Section
        leftList.Add(("CAPITAL ACCOUNT", 0)); // Header
        leftList.AddRange(capital.Select(x => (x.AccountName, x.Amount)));

        // Net Profit/Loss
        if (data.NetProfitLoss > 0)
        {
            leftList.Add(("Net Profit", data.NetProfitLoss));
        }
        else if (data.NetProfitLoss < 0)
        {
            // Loss reduces capital usually, but here we list it? 
            // Or show it on Asset side? 
            // Conventional: Capital - Loss. 
            // If we just list it as negative? 
            // Let's show "Net Loss" on Asset side (Right side) or as negative on Liability side.
            // Let's put it on Asset side for balancing if we treat it as accumulation?
            // Actually, usually: Capital + Net Profit OR Capital - Net Loss. 
            // Let's add "Less: Net Loss" on Capital side (Liability side) with negative amount for display?
            // Or just put "Net Loss" on Liability side with negative value. 
            // But to make explicit:
            leftList.Add(("Net Loss", data.NetProfitLoss));
        }

        // Liabilities Section
        leftList.Add(("", 0)); // Spacer
        leftList.Add(("LIABILITIES", 0)); // Header
        leftList.AddRange(liabilities.Select(x => (x.AccountName, x.Amount)));


        // Right Side: Assets
        var rightList = new List<(string Name, decimal Amount)>();
        rightList.Add(("ASSETS", 0)); // Header
        rightList.AddRange(assets.Select(x => (x.AccountName, x.Amount)));

        if (data.AccountBalanceDifference != 0)
        {
            var diff = Math.Abs(data.AccountBalanceDifference);
            if (data.AccountBalanceDifference > 0)
            {
                leftList.Add(("AccountBalanceDifference", diff));
            }
            else
            {
                rightList.Add(("AccountBalanceDifference", diff));
            }
        }

        // Closing Stock is already in Assets list from Repo? 
        // Yes: reportItems.Add(new BalanceSheetPrintItem("STK", "Closing Stock", closingStockVal, "Asset"));


        var maxCount = Math.Max(leftList.Count, rightList.Count);
        decimal totalLeft = 0;
        decimal totalRight = 0;

        for (int i = 0; i < maxCount; i++)
        {
            string lName = "", lAmt = "", rName = "", rAmt = "";

            if (i < leftList.Count)
            {
                lName = leftList[i].Name;
                var amt = leftList[i].Amount;
                if (lName == "CAPITAL ACCOUNT" || lName == "LIABILITIES" || lName == "")
                {
                    lAmt = "";
                }
                else
                {
                    lAmt = amt.ToString("N2");
                    totalLeft += amt;
                }
            }

            if (i < rightList.Count)
            {
                rName = rightList[i].Name;
                var amt = rightList[i].Amount;
                if (rName == "ASSETS" || rName == "")
                {
                    rAmt = "";
                }
                else
                {
                    rAmt = amt.ToString("N2");
                    totalRight += amt;
                }
            }

            finalReportItems.Add(new BalanceSheetReportDtls { LiabilityAccount = lName, LiabilityAmount = lAmt, AssetAccount = rName, AssetAmount = rAmt });
        }

        // Total Line
        finalReportItems.Add(new BalanceSheetReportDtls { LiabilityAccount = "Total", LiabilityAmount = totalLeft.ToString("N2"), AssetAccount = "Total", AssetAmount = totalRight.ToString("N2") });

        return finalReportItems;
    }

    #endregion
}
