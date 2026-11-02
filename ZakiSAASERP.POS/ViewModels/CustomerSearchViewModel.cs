using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ZakiSAASERP.POS.ViewModels;

public partial class CustomerSearchViewModel(IPosDataService posDataService) : ObservableObject
{
    [ObservableProperty]
    private string searchQuery = "";

    [ObservableProperty]
    private ObservableCollection<CustomerMaster> customers = [];

    [ObservableProperty]
    private CustomerMaster? selectedCustomer;

    [ObservableProperty]
    private bool isBusy;

    public bool DialogResult { get; private set; }
    public event Action? RequestClose;

    [RelayCommand]
    public async Task SearchAsync()
    {
        IsBusy = true;
        try
        {
            var branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            var list = await posDataService.SearchCustomersAsync(SearchQuery, branchId);
            Customers = new ObservableCollection<CustomerMaster>(list ?? []);
            if (Customers.Any())
            {
                SelectedCustomer = Customers.First();
            }
        }
        catch { }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Select()
    {
        if (SelectedCustomer != null)
        {
            DialogResult = true;
            RequestClose?.Invoke();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke();
    }
}

