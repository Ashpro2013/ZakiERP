using AshproSAASERP.Models.DTO;
using AshproSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AshproSAASERP.POS.ViewModels;

public partial class ItemSearchViewModel(IPosDataService posDataService) : ObservableObject
{
    [ObservableProperty]
    private string searchQuery = "";

    [ObservableProperty]
    private ObservableCollection<ItemDialogDTO> items = [];

    [ObservableProperty]
    private ItemDialogDTO? selectedItem;

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
            var list = await posDataService.SearchItemsAsync(SearchQuery, branchId);
            Items = new ObservableCollection<ItemDialogDTO>(list ?? []);
            if (Items.Any())
            {
                SelectedItem = Items.First();
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
        if (SelectedItem != null)
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

