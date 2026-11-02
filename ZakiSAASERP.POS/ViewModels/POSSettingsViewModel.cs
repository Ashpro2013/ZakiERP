using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZakiSAASERP.POS.ViewModels;

public partial class POSSettingsViewModel : ObservableObject
{
    private readonly POSSettingsService _settingsService;

    [ObservableProperty]
    private POSSettings model = new();

    public bool DialogResult { get; private set; }
    public Action? CloseAction { get; set; }

    public POSSettingsViewModel(POSSettingsService settingsService)
    {
        _settingsService = settingsService;
        Initialize();
    }

    public void Initialize()
    {
        // Work on a deep clone of the current settings
        Model = _settingsService.CurrentSettings.Clone();
    }

    [RelayCommand]
    private void SelectAllColumns()
    {
        Model.ShowItemCode = true;
        Model.ShowBarcode = true;
        Model.ShowItemName = true;
        Model.ShowArabicName = true;
        Model.ShowExRate = true;
        Model.ShowIncRate = true;
        Model.ShowGross = true;
        Model.ShowDiscPercentage = true;
        Model.ShowDiscAmount = true;
        Model.ShowTaxableAmount = true;
        Model.ShowTaxPercentage = true;
        Model.ShowTaxAmount = true;
        Model.ShowUnit = true;
        Model.ShowAmount = true;

        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private void ResetDefaultColumns()
    {
        Model.ShowItemCode = true;
        Model.ShowBarcode = true;
        Model.ShowItemName = true;
        Model.ShowArabicName = false;
        Model.ShowExRate = true;
        Model.ShowIncRate = false;
        Model.ShowGross = true;
        Model.ShowDiscPercentage = false;
        Model.ShowDiscAmount = true;
        Model.ShowTaxableAmount = false;
        Model.ShowTaxPercentage = false;
        Model.ShowTaxAmount = true;
        Model.ShowUnit = true;
        Model.ShowAmount = true;

        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private void ResetAllDefaults()
    {
        Model = new POSSettings();
        OnPropertyChanged(nameof(Model));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _settingsService.SaveSettingsAsync(Model);
        DialogResult = true;
        CloseAction?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        CloseAction?.Invoke();
    }
}

