using System.Text.Json;

namespace ZakiSAASERP.Shared.Pages.Reports.Accounts;

public partial class ProfitAndLossReportView
{
    #region Variables
    DateTime? FromDate;
    DateTime? ToDate;
    byte[]? pdfData;
    ProfitAndLossReportResponse? result;

    // Filters
    int BranchId = 0;

    // Data Sources
    List<CommonModel> branchList = new();
    private List<ProfitAndLossReportDtls> reportData = new();
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
            var req = new ProfitAndLossReportRequest(BranchId, FromDate ?? DateTime.Today, ToDate ?? DateTime.Today);

            result = await api.GetProfitAndLossReport(req);

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
                    var reportHeader = new ProfitAndLossReportHdr
                    (
                        $"{FromDate.ToDateTime():dd-MM-yyyy} - {ToDate.ToDateTime():dd-MM-yyyy}",
                        DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                        result.TotalIncome.ToString("N2"),
                        result.TotalCOGS.ToString("N2"),
                        result.TotalExpense.ToString("N2"),
                        result.GrossProfit.ToString("N2"),
                        result.NetProfit.ToString("N2")
                    );

                    Dictionary<string, object?> ReportHdr = reportHeader.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(reportHeader));
                    foreach (var item in finalReportItems)
                    {
                        item.DrAccount = item.DrAccount ?? "";
                        item.DrAmount = item.DrAmount ?? "";
                        item.CrAccount = item.CrAccount ?? "";
                        item.CrAmount = item.CrAmount ?? "";
                    }
                    Dictionary<string, object> ReportDtls = new() { { "Details", finalReportItems } };

                    var pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, "Profit_And_Loss_Report"));
                    if (pdto.hdr == null || !pdto.dtls.Any())
                    {
                        IsLoading = false;
                        StateHasChanged();
                        await Dialog.DialogMethod("Print design 'Profit And Loss Report' not found. Please create design from designer.");
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
                var req = new ProfitAndLossReportRequest(BranchId, FromDate ?? DateTime.Today, ToDate ?? DateTime.Today);
                result = await api.GetProfitAndLossReport(req);
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
    List<ProfitAndLossReportDtls> PrepareReportData(ProfitAndLossReportResponse data)
    {
        List<ProfitAndLossReportDtls> finalReportItems = new();

        // 1. Trading Account
        var directExpense = data.Items.Where(x => x.Category == "COGS").ToList();
        var directIncome = data.Items.Where(x => x.Category == "Income").ToList();

        var debitList = new List<(string Name, decimal Amount)>();
        var creditList = new List<(string Name, decimal Amount)>();

        // Opening Stock (Debit)
        if (data.OpeningStock != 0)
        {
            debitList.Add(("Opening Stock", data.OpeningStock));
        }

        // Direct Expenses
        debitList.AddRange(directExpense.Select(x => (x.AccountName, x.Amount)));

        // Direct Incomes
        creditList.AddRange(directIncome.Select(x => (x.AccountName, x.Amount)));

        // Closing Stock (Credit)
        if (data.ClosingStock != 0)
        {
            creditList.Add(("Closing Stock", data.ClosingStock));
        }

        var tradeMaxCount = Math.Max(debitList.Count, creditList.Count);
        decimal totalDr = 0;
        decimal totalCr = 0;

        for (int i = 0; i < tradeMaxCount; i++)
        {
            string drName = "", drAmt = "", crName = "", crAmt = "";

            if (i < debitList.Count)
            {
                drName = debitList[i].Name;
                var amt = debitList[i].Amount;
                drAmt = amt.ToString("N2");
                totalDr += amt;
            }

            if (i < creditList.Count)
            {
                crName = creditList[i].Name;
                var amt = creditList[i].Amount;
                crAmt = amt.ToString("N2");
                totalCr += amt;
            }

            finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = drName, DrAmount = drAmt, CrAccount = crName, CrAmount = crAmt });
        }

        // Trading Gross Profit / Loss
        decimal grossProfit = 0;
        decimal grossLoss = 0;

        // If Credit side > Debit side => Gross Profit (balancing figure on Debit side)
        // If Debit side > Credit side => Gross Loss (balancing figure on Credit side)

        // However, conventionally:
        // Gross Profit = NetIncome - COGS. If +ve, it's Profit.
        // existing logic: result.GrossProfit = totalIncome - totalCOGS

        // Let's rely on calculated totals for the T-Format balancing
        if (totalCr > totalDr)
        {
            grossProfit = totalCr - totalDr;
            finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = "Gross Profit c/d", DrAmount = grossProfit.ToString("N2"), CrAccount = "", CrAmount = "" });
            totalDr += grossProfit; // Balance it
        }
        else if (totalDr > totalCr)
        {
            grossLoss = totalDr - totalCr;
            finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = "", DrAmount = "", CrAccount = "Gross Loss c/d", CrAmount = grossLoss.ToString("N2") });
            totalCr += grossLoss; // Balance it
        }

        // Total Line for Trading
        finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = "Total", DrAmount = totalDr.ToString("N2"), CrAccount = "Total", CrAmount = totalCr.ToString("N2") });

        // Spacer
        finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = "", DrAmount = "", CrAccount = "", CrAmount = "" });

        // 2. Initial P&L (Bring down Gross Profit/Loss)
        // Gross Profit b/d goes to Credit Side
        // Gross Loss b/d goes to Debit Side

        // P&L Lists
        var indirectExpense = data.Items.Where(x => x.Category == "Expense").ToList();
        // var indirectIncome = result.Items.Where(x => x.Category == "OtherIncome").ToList(); // If any

        // We need to insert the b/d row first or manage it in the loop? 
        // Better to create lists first.

        var drList = new List<(string Name, decimal Amount)>();
        var crList = new List<(string Name, decimal Amount)>();

        if (grossLoss > 0) drList.Add(("Gross Loss b/d", grossLoss));
        if (grossProfit > 0) crList.Add(("Gross Profit b/d", grossProfit));

        drList.AddRange(indirectExpense.Select(x => (x.AccountName, x.Amount)));
        // crList.AddRange(indirectIncome.Select(x => (x.AccountName, x.Amount))); // Add other incomes if any

        var pnlMaxCount = Math.Max(drList.Count, crList.Count);
        decimal pnlTotalDr = 0;
        decimal pnlTotalCr = 0;

        for (int i = 0; i < pnlMaxCount; i++)
        {
            string drName = "", drAmt = "", crName = "", crAmt = "";

            if (i < drList.Count)
            {
                drName = drList[i].Name;
                var amt = drList[i].Amount;
                drAmt = amt.ToString("N2");
                pnlTotalDr += amt;
            }

            if (i < crList.Count)
            {
                crName = crList[i].Name;
                var amt = crList[i].Amount;
                crAmt = amt.ToString("N2");
                pnlTotalCr += amt;
            }

            finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = drName, DrAmount = drAmt, CrAccount = crName, CrAmount = crAmt });
        }

        // Net Profit / Loss
        decimal netProfit = 0;
        decimal netLoss = 0;

        if (pnlTotalCr > pnlTotalDr)
        {
            netProfit = pnlTotalCr - pnlTotalDr;
            finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = "Net Profit", DrAmount = netProfit.ToString("N2"), CrAccount = "", CrAmount = "" });
            pnlTotalDr += netProfit;
        }
        else if (pnlTotalDr > pnlTotalCr)
        {
            netLoss = pnlTotalDr - pnlTotalCr;
            finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = "", DrAmount = "", CrAccount = "Net Loss", CrAmount = netLoss.ToString("N2") });
            pnlTotalCr += netLoss;
        }

        // Total Line for P&L
        finalReportItems.Add(new ProfitAndLossReportDtls { DrAccount = "Total", DrAmount = pnlTotalDr.ToString("N2"), CrAccount = "Total", CrAmount = pnlTotalCr.ToString("N2") });

        return finalReportItems;
    }

    #endregion
}
