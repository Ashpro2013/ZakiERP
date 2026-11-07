using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace ZakiSAASERP.POS.ViewModels;

public partial class BillSearchViewModel(IPosDataService posDataService) : ObservableObject
{
    [ObservableProperty]
    private string searchQuery = "";

    [ObservableProperty]
    private ObservableCollection<ExMasters> bills = [];

    [ObservableProperty]
    private ExMasters? selectedBill;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = "";

    public bool DialogResult { get; private set; }
    public int? SelectedInvoiceId { get; private set; }
    public event Action? RequestClose;

    public async Task LoadBillsAsync(string? search = null)
    {
        SearchQuery = search ?? "";
        await SearchAsync();
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            var list = await posDataService.SearchInvoicesAsync(SearchQuery, branchId);
            var newBills = new ObservableCollection<ExMasters>(list ?? []);

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Bills = newBills;
                    SelectedBill = Bills.FirstOrDefault();
                });
            }
            else
            {
                Bills = newBills;
                SelectedBill = Bills.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load invoices: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[BillSearchViewModel] SearchAsync error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Select()
    {
        if (SelectedBill != null && SelectedBill.Id > 0)
        {
            SelectedInvoiceId = SelectedBill.Id;
            DialogResult = true;
            RequestClose?.Invoke();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        SelectedInvoiceId = null;
        RequestClose?.Invoke();
    }
}
