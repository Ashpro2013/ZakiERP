using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.Settings;
using Microsoft.JSInterop;

namespace AshproSAASERP.Shared.Services;

public class ShiftService
{
    private readonly JSService _jsService;
    private readonly IJSRuntime _js;

    public ShiftService(JSService jsService, IJSRuntime js)
    {
        _jsService = jsService;
        _js = js;
    }

    private decimal ParseDecimal(string? val, decimal defaultVal = 0m)
    {
        if (string.IsNullOrWhiteSpace(val)) return defaultVal;
        if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var res)) return res;
        if (decimal.TryParse(val, out var res2)) return res2;
        return defaultVal;
    }

    private async Task<string?> GetFirstNonEmptyKey(params string[] keys)
    {
        foreach (var k in keys)
        {
            var val = await _jsService.GetLocal(k);
            if (!string.IsNullOrWhiteSpace(val)) return val;
        }
        return null;
    }

    public async Task<ShiftDetails> GetShiftDetailsAsync(string defaultTerminal = "Terminal 1", string defaultUser = "User")
    {
        ShiftDetails shift = new();

        // 1. Try reading JSON object first
        var jsonStr = await GetFirstNonEmptyKey("ShiftDetails", "Ashpro_POS_Shift_Details");
        if (!string.IsNullOrWhiteSpace(jsonStr))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<ShiftDetails>(jsonStr);
                if (parsed != null)
                {
                    shift = parsed;
                }
            }
            catch { }
        }

        // 2. Read individual keys from localStorage (supporting spaced and camelCase names)
        var totalSaleStr = await GetFirstNonEmptyKey("Total Sale", "TotalSale", "TotalSales");
        if (totalSaleStr != null) shift.TotalSale = ParseDecimal(totalSaleStr, shift.TotalSale);

        var totalSalesReturnStr = await GetFirstNonEmptyKey("Total Sales Return", "TotalSalesReturn", "SalesReturn");
        if (totalSalesReturnStr != null) shift.TotalSalesReturn = ParseDecimal(totalSalesReturnStr, shift.TotalSalesReturn);

        var totalPurchaseStr = await GetFirstNonEmptyKey("Total Purchase", "TotalPurchase");
        if (totalPurchaseStr != null) shift.TotalPurchase = ParseDecimal(totalPurchaseStr, shift.TotalPurchase);

        var totalCashSalesStr = await GetFirstNonEmptyKey("Total Cash Sales", "TotalCashSales", "CashSales");
        if (totalCashSalesStr != null) shift.TotalCashSales = ParseDecimal(totalCashSalesStr, shift.TotalCashSales);

        var totalCardSalesStr = await GetFirstNonEmptyKey("Total Card Sales", "TotalCardSales", "CardSales", "BankSales");
        if (totalCardSalesStr != null) shift.TotalCardSales = ParseDecimal(totalCardSalesStr, shift.TotalCardSales);

        var totalCreditSalesStr = await GetFirstNonEmptyKey("Total Credit Sales", "TotalCreditSales", "CreditSales");
        if (totalCreditSalesStr != null) shift.TotalCreditSales = ParseDecimal(totalCreditSalesStr, shift.TotalCreditSales);

        var totalPaymentStr = await GetFirstNonEmptyKey("Total Payment", "TotalPayment");
        if (totalPaymentStr != null) shift.TotalPayment = ParseDecimal(totalPaymentStr, shift.TotalPayment);

        var totalCashReceiptStr = await GetFirstNonEmptyKey("Total Cash Receipt", "TotalCashReceipt");
        if (totalCashReceiptStr != null) shift.TotalCashReceipt = ParseDecimal(totalCashReceiptStr, shift.TotalCashReceipt);

        var totalCardReceiptStr = await GetFirstNonEmptyKey("Total Card Receipt", "TotalCardReceipt");
        if (totalCardReceiptStr != null) shift.TotalCardReceipt = ParseDecimal(totalCardReceiptStr, shift.TotalCardReceipt);

        var openingBalanceStr = await GetFirstNonEmptyKey("Opening Balance", "OpeningBalance");
        if (openingBalanceStr != null) shift.OpeningBalance = ParseDecimal(openingBalanceStr, shift.OpeningBalance);

        var closingBalanceStr = await GetFirstNonEmptyKey("Closing Balance", "ClosingBalance");
        if (closingBalanceStr != null)
        {
            shift.ClosingBalance = ParseDecimal(closingBalanceStr, shift.ClosingBalance);
        }
        else if (shift.ClosingBalance == 0)
        {
            shift.ClosingBalance = shift.OpeningBalance + shift.TotalCashSales + shift.TotalCashReceipt - shift.TotalPayment;
        }

        var terminalStr = await GetFirstNonEmptyKey("Terminal", "TerminalName", "Terminal Name");
        shift.Terminal = !string.IsNullOrWhiteSpace(terminalStr) ? terminalStr : (!string.IsNullOrWhiteSpace(shift.Terminal) ? shift.Terminal : defaultTerminal);

        var userStr = await GetFirstNonEmptyKey("UserName", "User Name", "Username");
        shift.UserName = !string.IsNullOrWhiteSpace(userStr) ? userStr : (!string.IsNullOrWhiteSpace(shift.UserName) ? shift.UserName : defaultUser);

        var dateFromStr = await GetFirstNonEmptyKey("Date from", "DateFrom", "ShiftStartDate");
        if (!string.IsNullOrWhiteSpace(dateFromStr) && DateTime.TryParse(dateFromStr, out var dFrom))
        {
            shift.DateFrom = dFrom;
        }
        else if (!shift.DateFrom.HasValue)
        {
            shift.DateFrom = DateTime.Now;
        }

        var dateToStr = await GetFirstNonEmptyKey("Date to", "DateTo", "ShiftEndDate");
        if (!string.IsNullOrWhiteSpace(dateToStr) && DateTime.TryParse(dateToStr, out var dTo))
        {
            shift.DateTo = dTo;
        }
        else
        {
            shift.DateTo = DateTime.Now;
        }

        return shift;
    }

    public async Task SaveShiftDetailsAsync(ShiftDetails shift)
    {
        // 1. Save structured JSON
        var json = JsonSerializer.Serialize(shift);
        await _jsService.SetLocal(json, "ShiftDetails");
        await _jsService.SetLocal(json, "Ashpro_POS_Shift_Details");

        // 2. Save individual keys (both spaced and non-spaced versions)
        var sTotalSale = shift.TotalSale.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalSale, "Total Sale");
        await _jsService.SetLocal(sTotalSale, "TotalSale");

        var sTotalSalesReturn = shift.TotalSalesReturn.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalSalesReturn, "Total Sales Return");
        await _jsService.SetLocal(sTotalSalesReturn, "TotalSalesReturn");

        var sTotalPurchase = shift.TotalPurchase.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalPurchase, "Total Purchase");
        await _jsService.SetLocal(sTotalPurchase, "TotalPurchase");

        var sTotalCashSales = shift.TotalCashSales.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalCashSales, "Total Cash Sales");
        await _jsService.SetLocal(sTotalCashSales, "TotalCashSales");

        var sTotalCardSales = shift.TotalCardSales.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalCardSales, "Total Card Sales");
        await _jsService.SetLocal(sTotalCardSales, "TotalCardSales");

        var sTotalCreditSales = shift.TotalCreditSales.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalCreditSales, "Total Credit Sales");
        await _jsService.SetLocal(sTotalCreditSales, "TotalCreditSales");

        var sTotalPayment = shift.TotalPayment.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalPayment, "Total Payment");
        await _jsService.SetLocal(sTotalPayment, "TotalPayment");

        var sTotalCashReceipt = shift.TotalCashReceipt.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalCashReceipt, "Total Cash Receipt");
        await _jsService.SetLocal(sTotalCashReceipt, "TotalCashReceipt");

        var sTotalCardReceipt = shift.TotalCardReceipt.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sTotalCardReceipt, "Total Card Receipt");
        await _jsService.SetLocal(sTotalCardReceipt, "TotalCardReceipt");

        var sOpeningBalance = shift.OpeningBalance.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sOpeningBalance, "Opening Balance");
        await _jsService.SetLocal(sOpeningBalance, "OpeningBalance");

        var sClosingBalance = shift.ClosingBalance.ToString("0.00", CultureInfo.InvariantCulture);
        await _jsService.SetLocal(sClosingBalance, "Closing Balance");
        await _jsService.SetLocal(sClosingBalance, "ClosingBalance");

        await _jsService.SetLocal(shift.Terminal ?? "", "Terminal");
        await _jsService.SetLocal(shift.Terminal ?? "", "Terminal Name");
        await _jsService.SetLocal(shift.Terminal ?? "", "TerminalName");

        await _jsService.SetLocal(shift.UserName ?? "", "UserName");
        await _jsService.SetLocal(shift.UserName ?? "", "User Name");

        var sFrom = shift.DateFrom?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        await _jsService.SetLocal(sFrom, "Date from");
        await _jsService.SetLocal(sFrom, "DateFrom");

        var sTo = shift.DateTo?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        await _jsService.SetLocal(sTo, "Date to");
        await _jsService.SetLocal(sTo, "DateTo");
    }

    public async Task RecordSaleAsync(decimal amount, string paymentType, bool isReturn = false, string terminal = "Terminal 1", string userName = "User")
    {
        var shift = await GetShiftDetailsAsync(terminal, userName);

        if (isReturn || amount < 0)
        {
            var absAmount = Math.Abs(amount);
            shift.TotalSalesReturn += absAmount;
            if (paymentType == "Cash")
            {
                shift.TotalCashSales -= absAmount;
            }
            else if (paymentType == "Bank" || paymentType == "Card")
            {
                shift.TotalCardSales -= absAmount;
            }
            else if (paymentType == "Credit")
            {
                shift.TotalCreditSales -= absAmount;
            }
        }
        else
        {
            shift.TotalSale += amount;
            if (paymentType == "Cash")
            {
                shift.TotalCashSales += amount;
            }
            else if (paymentType == "Bank" || paymentType == "Card")
            {
                shift.TotalCardSales += amount;
            }
            else if (paymentType == "Credit")
            {
                shift.TotalCreditSales += amount;
            }
            else
            {
                shift.TotalCashSales += amount;
            }
        }

        shift.ClosingBalance = shift.OpeningBalance + shift.TotalCashSales + shift.TotalCashReceipt - shift.TotalPayment;
        shift.DateTo = DateTime.Now;
        shift.Terminal = terminal;
        shift.UserName = userName;

        await SaveShiftDetailsAsync(shift);
    }


    public async Task RecordPurchaseAsync(decimal amount, string? billType = "Cash", bool isReturn = false, string terminal = "Terminal 1", string userName = "User")
    {
        var shift = await GetShiftDetailsAsync(terminal, userName);

        if (isReturn || amount < 0)
        {
            var absAmount = Math.Abs(amount);
            shift.TotalPurchase -= absAmount;
            if (shift.TotalPurchase < 0) shift.TotalPurchase = 0;

            if (string.Equals(billType, "Cash", StringComparison.OrdinalIgnoreCase))
            {
                shift.TotalPayment -= absAmount;
                if (shift.TotalPayment < 0) shift.TotalPayment = 0;
            }
        }
        else
        {
            shift.TotalPurchase += amount;
            if (string.Equals(billType, "Cash", StringComparison.OrdinalIgnoreCase))
            {
                shift.TotalPayment += amount;
            }
        }

        shift.ClosingBalance = shift.OpeningBalance + shift.TotalCashSales + shift.TotalCashReceipt - shift.TotalPayment;
        shift.DateTo = DateTime.Now;
        shift.Terminal = terminal;
        shift.UserName = userName;

        await SaveShiftDetailsAsync(shift);
    }

    public async Task RecordPaymentAsync(decimal amount, string terminal = "Terminal 1", string userName = "User")
    {
        var shift = await GetShiftDetailsAsync(terminal, userName);
        shift.TotalPayment += amount;
        shift.ClosingBalance = shift.OpeningBalance + shift.TotalCashSales + shift.TotalCashReceipt - shift.TotalPayment;
        shift.DateTo = DateTime.Now;
        shift.Terminal = terminal;
        shift.UserName = userName;

        await SaveShiftDetailsAsync(shift);
    }

    public async Task RecordReceiptAsync(decimal amount, string paymentType = "Cash", string terminal = "Terminal 1", string userName = "User")
    {
        var shift = await GetShiftDetailsAsync(terminal, userName);
        if (string.Equals(paymentType, "Cash", StringComparison.OrdinalIgnoreCase))
        {
            shift.TotalCashReceipt += amount;
        }
        else
        {
            shift.TotalCardReceipt += amount;
        }

        shift.ClosingBalance = shift.OpeningBalance + shift.TotalCashSales + shift.TotalCashReceipt - shift.TotalPayment;
        shift.DateTo = DateTime.Now;
        shift.Terminal = terminal;
        shift.UserName = userName;

        await SaveShiftDetailsAsync(shift);
    }

    public async Task ResetShiftDetailsAsync(string defaultTerminal = "Terminal 1", string defaultUser = "User")
    {
        // 1. Make all values in the local storage as zero
        string[] zeroKeys = new[]
        {
            "Total Sale", "TotalSale", "TotalSales",
            "Total Sales Return", "TotalSalesReturn", "SalesReturn",
            "Total Purchase", "TotalPurchase",
            "Total Cash Sales", "TotalCashSales", "CashSales",
            "Total Card Sales", "TotalCardSales", "CardSales", "BankSales",
            "Total Credit Sales", "TotalCreditSales", "CreditSales",
            "Total Payment", "TotalPayment",
            "Total Cash Receipt", "TotalCashReceipt",
            "Total Card Receipt", "TotalCardReceipt",
            "Opening Balance", "OpeningBalance",
            "Closing Balance", "ClosingBalance"
        };

        foreach (var key in zeroKeys)
        {
            await _jsService.SetLocal("0", key);
        }

        var nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await _jsService.SetLocal(nowStr, "Date from");
        await _jsService.SetLocal(nowStr, "DateFrom");
        await _jsService.SetLocal(nowStr, "Date to");
        await _jsService.SetLocal(nowStr, "DateTo");

        var cleanShift = new ShiftDetails
        {
            TotalSale = 0,
            TotalSalesReturn = 0,
            TotalPurchase = 0,
            TotalCashSales = 0,
            TotalCardSales = 0,
            TotalCreditSales = 0,
            TotalPayment = 0,
            TotalCashReceipt = 0,
            TotalCardReceipt = 0,
            OpeningBalance = 0,
            ClosingBalance = 0,
            Terminal = defaultTerminal,
            UserName = defaultUser,
            DateFrom = DateTime.Now,
            DateTo = DateTime.Now
        };

        var json = JsonSerializer.Serialize(cleanShift);
        await _jsService.SetLocal(json, "ShiftDetails");
        await _jsService.SetLocal(json, "Ashpro_POS_Shift_Details");
    }

    public async Task PrintShiftDetailsAsync(ShiftDetails shift, CompanyDetails? company)
    {
        var companyName = company?.Name ?? "ASHPRO ERP";
        var companyVat = company?.VatNo ?? "";
        var companyPhone = company?.Telephone ?? company?.Mobile ?? "";

        var dateFrom = shift.DateFrom?.ToString("dd/MM/yyyy hh:mm tt") ?? DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
        var dateTo = shift.DateTo?.ToString("dd/MM/yyyy hh:mm tt") ?? DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title>Shift Closing Report</title>
    <style>
        @page {{
            size: 80mm auto;
            margin: 3mm;
        }}
        body {{
            font-family: 'Courier New', Courier, monospace, Tahoma, Arial, sans-serif;
            font-size: 12px;
            width: 74mm;
            margin: 0 auto;
            color: #000;
            background: #fff;
            line-height: 1.4;
        }}
        .header {{
            text-align: center;
            border-bottom: 1px dashed #000;
            padding-bottom: 6px;
            margin-bottom: 6px;
        }}
        .company-name {{
            font-size: 15px;
            font-weight: bold;
            margin-bottom: 2px;
        }}
        .report-title {{
            font-size: 14px;
            font-weight: bold;
            margin: 4px 0;
            text-transform: uppercase;
            border-top: 1px dashed #000;
            border-bottom: 1px dashed #000;
            padding: 3px 0;
        }}
        .info-section {{
            font-size: 11px;
            border-bottom: 1px dashed #000;
            padding-bottom: 6px;
            margin-bottom: 6px;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 2px;
        }}
        .table {{
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 6px;
        }}
        .table td {{
            padding: 2px 0;
        }}
        .table .label {{
            text-align: left;
            font-size: 11px;
        }}
        .table .val {{
            text-align: right;
            font-weight: bold;
            font-size: 12px;
        }}
        .divider {{
            border-top: 1px dashed #000;
            margin: 4px 0;
        }}
        .double-divider {{
            border-top: 2px solid #000;
            margin: 4px 0;
        }}
        .total-row td {{
            font-weight: bold;
            font-size: 13px;
            padding: 4px 0;
        }}
        .signatures {{
            display: flex;
            justify-content: space-between;
            margin-top: 25px;
            font-size: 11px;
        }}
        .sig-box {{
            text-align: center;
            border-top: 1px solid #000;
            width: 42%;
            padding-top: 3px;
        }}
        .footer {{
            text-align: center;
            font-size: 10px;
            margin-top: 10px;
            border-top: 1px dashed #000;
            padding-top: 4px;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <div class=""company-name"">{companyName}</div>
        {(string.IsNullOrEmpty(companyVat) ? "" : $"<div>VAT No: {companyVat}</div>")}
        {(string.IsNullOrEmpty(companyPhone) ? "" : $"<div>Tel: {companyPhone}</div>")}
        <div class=""report-title"">SHIFT CLOSING REPORT</div>
    </div>

    <div class=""info-section"">
        <div class=""info-row"">
            <span>Terminal:</span>
            <strong>{shift.Terminal}</strong>
        </div>
        <div class=""info-row"">
            <span>User Name:</span>
            <strong>{shift.UserName}</strong>
        </div>
        <div class=""info-row"">
            <span>Date From:</span>
            <span>{dateFrom}</span>
        </div>
        <div class=""info-row"">
            <span>Date To:</span>
            <span>{dateTo}</span>
        </div>
    </div>

    <table class=""table"">
        <tr>
            <td class=""label"">Opening Balance</td>
            <td class=""val"">{shift.OpeningBalance:N2}</td>
        </tr>
        <tr>
            <td colspan=""2"" class=""divider""></td>
        </tr>
        <tr>
            <td class=""label"">Total Sale</td>
            <td class=""val"">{shift.TotalSale:N2}</td>
        </tr>
        <tr>
            <td class=""label"" style=""padding-left: 8px;"">- Cash Sales</td>
            <td class=""val"">{shift.TotalCashSales:N2}</td>
        </tr>
        <tr>
            <td class=""label"" style=""padding-left: 8px;"">- Card Sales</td>
            <td class=""val"">{shift.TotalCardSales:N2}</td>
        </tr>
        <tr>
            <td class=""label"" style=""padding-left: 8px;"">- Credit Sales</td>
            <td class=""val"">{shift.TotalCreditSales:N2}</td>
        </tr>
        <tr>
            <td class=""label"">Total Sales Return</td>
            <td class=""val"">{shift.TotalSalesReturn:N2}</td>
        </tr>
        <tr>
            <td class=""label"">Total Purchase</td>
            <td class=""val"">{shift.TotalPurchase:N2}</td>
        </tr>
        <tr>
            <td class=""label"">Total Payment</td>
            <td class=""val"">{shift.TotalPayment:N2}</td>
        </tr>
        <tr>
            <td class=""label"">Total Cash Receipt</td>
            <td class=""val"">{shift.TotalCashReceipt:N2}</td>
        </tr>
        <tr>
            <td class=""label"">Total Card Receipt</td>
            <td class=""val"">{shift.TotalCardReceipt:N2}</td>
        </tr>
        <tr>
            <td colspan=""2"" class=""double-divider""></td>
        </tr>
        <tr class=""total-row"">
            <td class=""label"">Closing Balance</td>
            <td class=""val"">{shift.ClosingBalance:N2}</td>
        </tr>
        <tr>
            <td colspan=""2"" class=""double-divider""></td>
        </tr>
    </table>

    <div class=""signatures"">
        <div class=""sig-box"">Cashier Signature</div>
        <div class=""sig-box"">Supervisor Signature</div>
    </div>

    <div class=""footer"">
        <div>Printed: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</div>
        <div>*** End of Shift Report ***</div>
    </div>
</body>
</html>";

        await _js.InvokeVoidAsync("printHtmlContent", html);
    }
}

