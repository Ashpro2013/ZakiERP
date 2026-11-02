using AshproSAASERP.Models.Company;
using AshproSAASERP.Models.Settings;
using System.IO;
using System.Text.Json;

namespace AshproSAASERP.POS.Services;

public class ShiftService
{
    private ShiftDetails _currentShift = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public ShiftDetails CurrentShift => _currentShift;

    public ShiftService()
    {
        TryLoadFromDisk();
    }

    private string GetLocalFilePath()
    {
        try
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AshproSAAS");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "pos_shift.json");
        }
        catch
        {
            return string.Empty;
        }
    }

    private void TryLoadFromDisk()
    {
        try
        {
            var path = GetLocalFilePath();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                var json = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var loaded = JsonSerializer.Deserialize<ShiftDetails>(json, JsonOptions);
                    if (loaded is not null)
                    {
                        _currentShift = loaded;
                        return;
                    }
                }
            }
        }
        catch { }

        _currentShift = new ShiftDetails
        {
            DateFrom = DateTime.Now,
            DateTo = DateTime.Now,
            Terminal = "Terminal 1",
            UserName = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "Cashier"
        };
    }

    public Task<ShiftDetails> GetShiftDetailsAsync(string defaultTerminal = "Terminal 1", string defaultUser = "Cashier")
    {
        TryLoadFromDisk();
        if (string.IsNullOrWhiteSpace(_currentShift.Terminal)) _currentShift.Terminal = defaultTerminal;
        if (string.IsNullOrWhiteSpace(_currentShift.UserName)) _currentShift.UserName = defaultUser;
        if (!_currentShift.DateFrom.HasValue) _currentShift.DateFrom = DateTime.Now;
        _currentShift.DateTo = DateTime.Now;

        return Task.FromResult(_currentShift);
    }

    public async Task SaveShiftDetailsAsync(ShiftDetails shift)
    {
        _currentShift = shift;
        var json = JsonSerializer.Serialize(_currentShift, JsonOptions);
        var path = GetLocalFilePath();
        if (!string.IsNullOrEmpty(path))
        {
            await File.WriteAllTextAsync(path, json);
        }
    }

    public async Task RecordSaleAsync(decimal amount, string paymentType, bool isReturn = false, string terminal = "Terminal 1", string userName = "Cashier")
    {
        var shift = await GetShiftDetailsAsync(terminal, userName);

        if (isReturn || amount < 0)
        {
            var absAmount = Math.Abs(amount);
            shift.TotalSalesReturn += absAmount;
            if (string.Equals(paymentType, "Cash", StringComparison.OrdinalIgnoreCase))
                shift.TotalCashSales -= absAmount;
            else if (string.Equals(paymentType, "Bank", StringComparison.OrdinalIgnoreCase) || string.Equals(paymentType, "Card", StringComparison.OrdinalIgnoreCase))
                shift.TotalCardSales -= absAmount;
            else if (string.Equals(paymentType, "Credit", StringComparison.OrdinalIgnoreCase))
                shift.TotalCreditSales -= absAmount;
            else
                shift.TotalCashSales -= absAmount;
        }
        else
        {
            shift.TotalSale += amount;
            if (string.Equals(paymentType, "Cash", StringComparison.OrdinalIgnoreCase))
                shift.TotalCashSales += amount;
            else if (string.Equals(paymentType, "Bank", StringComparison.OrdinalIgnoreCase) || string.Equals(paymentType, "Card", StringComparison.OrdinalIgnoreCase))
                shift.TotalCardSales += amount;
            else if (string.Equals(paymentType, "Credit", StringComparison.OrdinalIgnoreCase))
                shift.TotalCreditSales += amount;
            else
                shift.TotalCashSales += amount;
        }

        shift.ClosingBalance = shift.OpeningBalance + shift.TotalCashSales + shift.TotalCashReceipt - shift.TotalPayment;
        shift.DateTo = DateTime.Now;
        shift.Terminal = terminal;
        shift.UserName = userName;

        await SaveShiftDetailsAsync(shift);
    }

    public async Task ResetShiftDetailsAsync(string defaultTerminal = "Terminal 1", string defaultUser = "Cashier", decimal openingBalance = 0m)
    {
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
            OpeningBalance = openingBalance,
            ClosingBalance = openingBalance,
            Terminal = defaultTerminal,
            UserName = defaultUser,
            DateFrom = DateTime.Now,
            DateTo = DateTime.Now
        };

        await SaveShiftDetailsAsync(cleanShift);
    }
}

