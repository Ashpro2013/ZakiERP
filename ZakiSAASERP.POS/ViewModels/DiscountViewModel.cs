using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZakiSAASERP.POS.ViewModels;

public partial class DiscountViewModel : ObservableObject
{
    [ObservableProperty]
    private decimal discountAmount;

    [ObservableProperty]
    private decimal discountPercentage;

    [ObservableProperty]
    private decimal grossAmount;

    [ObservableProperty]
    private bool isPercentage;

    public bool DialogResult { get; private set; }
    public event Action? RequestClose;

    public void Initialize(decimal currentDisc, decimal gross)
    {
        DiscountAmount = currentDisc;
        GrossAmount = gross;
        if (gross > 0 && currentDisc > 0)
        {
            DiscountPercentage = Math.Round((currentDisc / gross) * 100m, 2);
        }
    }

    [RelayCommand]
    private void Apply()
    {
        if (IsPercentage && GrossAmount > 0)
        {
            DiscountAmount = Math.Round((GrossAmount * DiscountPercentage) / 100m, 2);
        }
        DialogResult = true;
        RequestClose?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke();
    }
}

