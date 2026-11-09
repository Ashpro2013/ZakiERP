using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZakiSAASERP.POS.ViewModels;

public partial class ShiftViewModel : ObservableObject
{
    private readonly ShiftService _shiftService;
    private readonly PrintHelper _printHelper;
    private readonly IShiftClosingRepo _shiftClosingRepo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpectedCash))]
    [NotifyPropertyChangedFor(nameof(Difference))]
    [NotifyPropertyChangedFor(nameof(DifferenceStatus))]
    [NotifyPropertyChangedFor(nameof(NetSales))]
    private ShiftDetails currentShift = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Difference))]
    [NotifyPropertyChangedFor(nameof(DifferenceStatus))]
    private decimal actualCash;

    [ObservableProperty]
    private decimal newOpeningBalance;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "";

    public decimal ExpectedCash => CurrentShift.OpeningBalance + CurrentShift.TotalCashSales + CurrentShift.TotalCashReceipt - CurrentShift.TotalPayment;

    public decimal Difference => ActualCash - ExpectedCash;

    public string DifferenceStatus => Difference == 0 ? "Balanced" : Difference > 0 ? $"Overage (+{Difference:N2} SAR)" : $"Shortage ({Difference:N2} SAR)";

    public decimal NetSales => CurrentShift.TotalSale - CurrentShift.TotalSalesReturn;

    public bool ShiftChangedResult { get; private set; }
    public Action? CloseAction { get; set; }

    public ShiftViewModel(ShiftService shiftService, PrintHelper printHelper, IShiftClosingRepo shiftClosingRepo)
    {
        _shiftService = shiftService;
        _printHelper = printHelper;
        _shiftClosingRepo = shiftClosingRepo;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "Cashier";
            CurrentShift = await _shiftService.GetShiftDetailsAsync("Terminal 1", user);
            ActualCash = ExpectedCash; // Pre-populate with expected cash
            NewOpeningBalance = 0;
            StatusMessage = "Shift active.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CloseAndChangeShiftAsync()
    {
        IsBusy = true;
        StatusMessage = "Closing shift and recording handover...";

        try
        {
            var user = !string.IsNullOrWhiteSpace(AppData.UserName) ? AppData.UserName : "Cashier";

            // Print summary report
            await PrintShiftReportAsync();

            // Save shift closing record to DB
            try
            {
                var closingRecord = new ShiftClosingRecord
                {
                    ShiftCode = "SC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
                    UserName = user,
                    Terminal = CurrentShift.Terminal,
                    DateFrom = CurrentShift.DateFrom ?? DateTime.Now,
                    DateTo = DateTime.Now,
                    OpeningBalance = CurrentShift.OpeningBalance,
                    TotalSale = CurrentShift.TotalSale,
                    TotalSalesReturn = CurrentShift.TotalSalesReturn,
                    NetSales = NetSales,
                    TotalCashSales = CurrentShift.TotalCashSales,
                    TotalCardSales = CurrentShift.TotalCardSales,
                    TotalCreditSales = CurrentShift.TotalCreditSales,
                    TotalPayment = CurrentShift.TotalPayment,
                    TotalCashReceipt = CurrentShift.TotalCashReceipt,
                    TotalCardReceipt = CurrentShift.TotalCardReceipt,
                    ExpectedCash = ExpectedCash,
                    ActualCash = ActualCash,
                    Difference = Difference,
                    NewOpeningBalance = NewOpeningBalance,
                    Status = Difference == 0 ? "Balanced" : Difference > 0 ? "Overage" : "Shortage",
                    CreatedAt = DateTime.Now
                };
                await _shiftClosingRepo.SaveShiftClosingAsync(closingRecord);
            }
            catch (Exception dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"[ShiftViewModel] Error saving shift closing to DB: {dbEx.Message}");
            }

            // Reset shift with new opening balance
            await _shiftService.ResetShiftDetailsAsync("Terminal 1", user, NewOpeningBalance);

            ShiftChangedResult = true;
            CloseAction?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Shift handover error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public Task PrintShiftReportAsync()
    {
        try
        {
            var company = AppData.CompanyDetails;
            var cName = company?.Name ?? "ZAKI ERP POS";
            var dateFromStr = CurrentShift.DateFrom?.ToString("dd/MM/yyyy hh:mm tt") ?? DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
            var dateToStr = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine($"         {cName}         ");
            sb.AppendLine("          SHIFT CLOSING REPORT          ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Terminal : {CurrentShift.Terminal}");
            sb.AppendLine($"Cashier  : {CurrentShift.UserName}");
            sb.AppendLine($"Start    : {dateFromStr}");
            sb.AppendLine($"End      : {dateToStr}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Opening Balance     : {CurrentShift.OpeningBalance,10:N2} SAR");
            sb.AppendLine($"Total Sales         : {CurrentShift.TotalSale,10:N2} SAR");
            sb.AppendLine($"  - Cash Sales      : {CurrentShift.TotalCashSales,10:N2} SAR");
            sb.AppendLine($"  - Card/Bank Sales : {CurrentShift.TotalCardSales,10:N2} SAR");
            sb.AppendLine($"  - Credit Sales    : {CurrentShift.TotalCreditSales,10:N2} SAR");
            sb.AppendLine($"Total Sales Return  : {CurrentShift.TotalSalesReturn,10:N2} SAR");
            sb.AppendLine($"Net Sales Total     : {NetSales,10:N2} SAR");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Cash Receipts       : {CurrentShift.TotalCashReceipt,10:N2} SAR");
            sb.AppendLine($"Cash Payments       : {CurrentShift.TotalPayment,10:N2} SAR");
            sb.AppendLine($"Expected in Drawer  : {ExpectedCash,10:N2} SAR");
            sb.AppendLine($"Actual Counted      : {ActualCash,10:N2} SAR");
            sb.AppendLine($"Difference          : {Difference,10:N2} SAR ({DifferenceStatus})");
            sb.AppendLine("========================================");
            sb.AppendLine("          HANDOVER COMPLETED            ");
            sb.AppendLine("========================================");

            _printHelper.PrintText(sb.ToString());
            StatusMessage = "Shift report printed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Print failed: {ex.Message}";
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    public void Cancel()
    {
        ShiftChangedResult = false;
        CloseAction?.Invoke();
    }
}
