using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace AshproSAASERP.POS.ViewModels;

public partial class PaymentViewModel : ObservableObject
{
    [ObservableProperty]
    private decimal totalAmount;

    [ObservableProperty]
    private decimal tenderedAmount;

    [ObservableProperty]
    private string tenderedBuffer = "";

    [ObservableProperty]
    private decimal changeAmount;

    [ObservableProperty]
    private decimal shortageAmount;

    [ObservableProperty]
    private bool isValid;

    [ObservableProperty]
    private string paymentType = "Cash";

    public bool DialogResult { get; private set; }

    public event Action? RequestClose;

    public void Initialize(decimal total)
    {
        TotalAmount = total;
        TenderedAmount = total;
        TenderedBuffer = total.ToString("F2", CultureInfo.InvariantCulture);
        CalculateChange();
    }

    [RelayCommand]
    private void SetExact()
    {
        TenderedAmount = TotalAmount;
        TenderedBuffer = TotalAmount.ToString("F2", CultureInfo.InvariantCulture);
        CalculateChange();
    }

    [RelayCommand]
    private void AddTendered(decimal amount)
    {
        TenderedAmount += amount;
        TenderedBuffer = TenderedAmount.ToString("F2", CultureInfo.InvariantCulture);
        CalculateChange();
    }

    [RelayCommand]
    private void SetPreset(decimal amount)
    {
        TenderedAmount = amount;
        TenderedBuffer = amount.ToString("F2", CultureInfo.InvariantCulture);
        CalculateChange();
    }

    [RelayCommand]
    private void HandleNumKey(string key)
    {
        switch (key)
        {
            case "Clear":
                TenderedBuffer = "";
                TenderedAmount = 0;
                break;
            case "Backspace":
                if (TenderedBuffer.Length > 0)
                {
                    TenderedBuffer = TenderedBuffer[..^1];
                }
                break;
            case "Exact":
                SetExact();
                return;
            case "Enter":
                if (IsValid)
                {
                    ConfirmPayment("Cash");
                }
                return;
            case ".":
                if (!TenderedBuffer.Contains('.'))
                {
                    TenderedBuffer += ".";
                }
                break;
            default:
                if (TenderedBuffer == "0" && key != ".")
                {
                    TenderedBuffer = "";
                }
                TenderedBuffer += key;
                break;
        }

        if (decimal.TryParse(TenderedBuffer, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed))
        {
            TenderedAmount = parsed;
        }
        else
        {
            TenderedAmount = 0;
        }

        CalculateChange();
    }

    [RelayCommand]
    private void ConfirmPayment(string type)
    {
        PaymentType = type;
        DialogResult = true;
        RequestClose?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke();
    }

    private void CalculateChange()
    {
        if (TenderedAmount >= TotalAmount)
        {
            ChangeAmount = TenderedAmount - TotalAmount;
            ShortageAmount = 0;
            IsValid = true;
        }
        else
        {
            ChangeAmount = 0;
            ShortageAmount = TotalAmount - TenderedAmount;
            IsValid = false;
        }
    }
}

