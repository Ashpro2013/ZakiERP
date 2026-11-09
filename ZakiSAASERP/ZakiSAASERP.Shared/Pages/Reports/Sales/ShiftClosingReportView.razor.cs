using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Shared.Layout;
using ZakiSAASERP.Shared.SubWindows;

namespace ZakiSAASERP.Shared.Pages.Reports.Sales;

public partial class ShiftClosingReportView
{
    private DateTime? FromDate;
    private DateTime? ToDate;
    private int BranchId = 0;
    private string Terminal = "";
    private string UserName = "";
    private string searchString = "";

    private List<CommonModel> branchList = new();
    private ShiftClosingReportResponse? result;
    private bool IsLoading = false;
    private bool IsLoaded = false;

    [CascadingParameter] public MainLayout? Layout { get; set; }
    [Parameter] public int TabId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;

        try
        {
            branchList = await api.GetCombofill("Branch", BranchId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShiftClosingReportView] GetCombofill error: {ex.Message}");
        }

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var req = new ShiftClosingReportRequest(
                BranchId,
                FromDate ?? DateTime.Today,
                ToDate ?? DateTime.Today,
                string.IsNullOrWhiteSpace(Terminal) ? null : Terminal,
                string.IsNullOrWhiteSpace(UserName) ? null : UserName
            );

            result = await api.GetShiftClosingReportAsync(req);
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading shift report: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ClearMethod()
    {
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        BranchId = 0;
        Terminal = "";
        UserName = "";
        searchString = "";
        await LoadDataAsync();
    }

    private async Task ViewDetails(ShiftClosingRecord record)
    {
        var parameters = new DialogParameters<ShiftClosingDetailsDialog>
        {
            { x => x.Record, record }
        };
        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        await DialogService.ShowAsync<ShiftClosingDetailsDialog>("Shift Closing Details", parameters, options);
    }

    private async Task PrintTicket(ShiftClosingRecord record)
    {
        try
        {
            await ShiftService.PrintShiftClosingRecordAsync(record, AppData.CompanyDetails);
            Snackbar.Add("Shift receipt sent to printer.", Severity.Info);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Print error: {ex.Message}", Severity.Error);
        }
    }

    private async Task PrintReportAsync()
    {
        if (result == null || !result.Reports.Any())
        {
            Snackbar.Add("No records to print.", Severity.Warning);
            return;
        }

        try
        {
            var company = AppData.CompanyDetails;
            var cName = company?.Name ?? "ASHPRO ERP";
            var cVat = company?.VatNo ?? "";
            var fromStr = FromDate?.ToString("dd/MM/yyyy") ?? "";
            var toStr = ToDate?.ToString("dd/MM/yyyy") ?? "";

            var rowsHtml = string.Join("", result.Reports.Select(r => $@"
                <tr>
                    <td>{r.ShiftCode}</td>
                    <td>{(r.DateTo?.ToString("dd/MM/yyyy HH:mm") ?? r.CreatedAt.ToString("dd/MM/yyyy HH:mm"))}</td>
                    <td>{r.Terminal}</td>
                    <td>{r.UserName}</td>
                    <td class='num'>{r.OpeningBalance:N2}</td>
                    <td class='num text-green'>{r.TotalCashSales:N2}</td>
                    <td class='num'>{r.TotalCardSales:N2}</td>
                    <td class='num text-red'>{r.TotalSalesReturn:N2}</td>
                    <td class='num bold'>{r.NetSales:N2}</td>
                    <td class='num'>{r.ExpectedCash:N2}</td>
                    <td class='num bold'>{r.ActualCash:N2}</td>
                    <td class='num {(r.Difference < 0 ? "text-red" : r.Difference > 0 ? "text-blue" : "text-green")}'>
                        {(r.Difference >= 0 ? $"+{r.Difference:N2}" : r.Difference.ToString("N2"))}
                    </td>
                    <td class='num'>{r.NewOpeningBalance:N2}</td>
                </tr>
            "));

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>Shift Closing Report</title>
    <style>
        @page {{ size: A4 landscape; margin: 10mm; }}
        body {{ font-family: 'Segoe UI', Arial, sans-serif; font-size: 11px; color: #1E293B; margin: 0; }}
        .header {{ text-align: center; border-bottom: 2px solid #0F172A; padding-bottom: 8px; margin-bottom: 12px; }}
        .company-name {{ font-size: 18px; font-weight: bold; color: #0F172A; }}
        .title {{ font-size: 14px; font-weight: bold; margin-top: 4px; text-transform: uppercase; color: #2563EB; }}
        .meta {{ font-size: 11px; color: #64748B; margin-top: 4px; }}
        .summary-cards {{ display: flex; gap: 10px; margin-bottom: 12px; }}
        .card {{ flex: 1; border: 1px solid #CBD5E1; border-radius: 6px; padding: 8px; text-align: center; background: #F8FAFC; }}
        .card-lbl {{ font-size: 9px; font-weight: bold; color: #64748B; }}
        .card-val {{ font-size: 14px; font-weight: bold; color: #0F172A; margin-top: 2px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 8px; font-size: 10px; }}
        th {{ background: #0F172A; color: #FFF; padding: 6px 4px; text-align: left; font-size: 10px; }}
        td {{ padding: 5px 4px; border-bottom: 1px solid #E2E8F0; }}
        tr:nth-child(even) {{ background: #F8FAFC; }}
        .num {{ text-align: right; }}
        .bold {{ font-weight: bold; }}
        .text-green {{ color: #16A34A; }}
        .text-blue {{ color: #2563EB; }}
        .text-red {{ color: #DC2626; }}
        .footer {{ margin-top: 20px; font-size: 9px; color: #64748B; display: flex; justify-content: space-between; }}
    </style>
</head>
<body>
    <div class='header'>
        <div class='company-name'>{cName}</div>
        {(string.IsNullOrEmpty(cVat) ? "" : $"<div>VAT No: {cVat}</div>")}
        <div class='title'>Shift Closing Summary Report</div>
        <div class='meta'>Period: {fromStr} to {toStr} | Generated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</div>
    </div>

    <div class='summary-cards'>
        <div class='card'>
            <div class='card-lbl'>TOTAL SHIFTS</div>
            <div class='card-val'>{result.TotalShifts}</div>
        </div>
        <div class='card'>
            <div class='card-lbl'>TOTAL NET SALES</div>
            <div class='card-val text-green'>{result.TotalNetSales:N2} SAR</div>
        </div>
        <div class='card'>
            <div class='card-lbl'>CASH SALES</div>
            <div class='card-val'>{result.TotalCashSales:N2} SAR</div>
        </div>
        <div class='card'>
            <div class='card-lbl'>CARD SALES</div>
            <div class='card-val'>{result.TotalCardSales:N2} SAR</div>
        </div>
        <div class='card'>
            <div class='card-lbl'>COUNTED CASH</div>
            <div class='card-val text-blue'>{result.TotalActualCash:N2} SAR</div>
        </div>
        <div class='card'>
            <div class='card-lbl'>TOTAL VARIANCE</div>
            <div class='card-val {(result.TotalDifference < 0 ? "text-red" : "text-green")}'>{result.TotalDifference:N2} SAR</div>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th>Shift #</th>
                <th>Closed At</th>
                <th>Terminal</th>
                <th>Cashier</th>
                <th class='num'>Opening</th>
                <th class='num'>Cash Sales</th>
                <th class='num'>Card Sales</th>
                <th class='num'>Returns</th>
                <th class='num'>Net Sales</th>
                <th class='num'>Expected</th>
                <th class='num'>Counted</th>
                <th class='num'>Variance</th>
                <th class='num'>Next Float</th>
            </tr>
        </thead>
        <tbody>
            {rowsHtml}
        </tbody>
        <tfoot>
            <tr style='background: #E2E8F0; font-weight: bold;'>
                <td colspan='4'>TOTALS</td>
                <td class='num'>-</td>
                <td class='num text-green'>{result.TotalCashSales:N2}</td>
                <td class='num'>{result.TotalCardSales:N2}</td>
                <td class='num text-red'>{result.TotalReturns:N2}</td>
                <td class='num'>{result.TotalNetSales:N2}</td>
                <td class='num'>{result.TotalExpectedCash:N2}</td>
                <td class='num text-blue'>{result.TotalActualCash:N2}</td>
                <td class='num {(result.TotalDifference < 0 ? "text-red" : "text-green")}'>{result.TotalDifference:N2}</td>
                <td class='num'>-</td>
            </tr>
        </tfoot>
    </table>

    <div class='footer'>
        <div>Zaki ERP - Shift Closing Report</div>
        <div>Page 1 of 1</div>
    </div>
</body>
</html>";

            await JS.InvokeVoidAsync("printHtmlContent", html);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Printing report error: {ex.Message}", Severity.Error);
        }
    }

    private async Task ExcelExportAsync()
    {
        if (result == null || !result.Reports.Any())
        {
            Snackbar.Add("No records to export.", Severity.Warning);
            return;
        }

        try
        {
            var exportList = result.Reports.Select(r => new
            {
                r.ShiftCode,
                ClosedAt = r.DateTo?.ToString("yyyy-MM-dd HH:mm") ?? r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                r.Terminal,
                Cashier = r.UserName,
                r.OpeningBalance,
                r.TotalCashSales,
                r.TotalCardSales,
                r.TotalCreditSales,
                r.TotalSalesReturn,
                r.NetSales,
                r.ExpectedCash,
                ActualCountedCash = r.ActualCash,
                Variance = r.Difference,
                r.Status,
                HandoverFloat = r.NewOpeningBalance,
                r.Remarks
            }).ToList();

            var json = JsonSerializer.Serialize(exportList, new JsonSerializerOptions { WriteIndented = true });
            await JS.InvokeVoidAsync("exportJsonToExcel", json);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Excel export error: {ex.Message}", Severity.Error);
        }
    }

    private bool FilterFunc(ShiftClosingRecord record)
    {
        if (string.IsNullOrWhiteSpace(searchString)) return true;
        var s = searchString.Trim();
        if (!string.IsNullOrEmpty(record.ShiftCode) && record.ShiftCode.Contains(s, StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.IsNullOrEmpty(record.UserName) && record.UserName.Contains(s, StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.IsNullOrEmpty(record.Terminal) && record.Terminal.Contains(s, StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.IsNullOrEmpty(record.Status) && record.Status.Contains(s, StringComparison.OrdinalIgnoreCase)) return true;
        if (!string.IsNullOrEmpty(record.Remarks) && record.Remarks.Contains(s, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private Color GetRowStatusColor(decimal diff)
    {
        if (diff == 0) return Color.Success;
        if (diff > 0) return Color.Info;
        return Color.Error;
    }

    private string GetVarianceBorder(decimal diff)
    {
        if (diff == 0) return "border-left: 4px solid #16A34A;";
        if (diff > 0) return "border-left: 4px solid #2563EB;";
        return "border-left: 4px solid #DC2626;";
    }

    private string GetVarianceTextStyle(decimal diff)
    {
        if (diff == 0) return "font-weight: 700; color: #16A34A;";
        if (diff > 0) return "font-weight: 700; color: #2563EB;";
        return "font-weight: 700; color: #DC2626;";
    }
}

