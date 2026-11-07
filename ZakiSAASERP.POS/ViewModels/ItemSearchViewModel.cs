using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace ZakiSAASERP.POS.ViewModels;

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

    [ObservableProperty]
    private string errorMessage = "";

    public bool DialogResult { get; private set; }
    public event Action? RequestClose;

    public async Task InitializeAsync(string? initialSearch = null)
    {
        SearchQuery = initialSearch ?? "";
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
        var branchId = AppData.BranchId > 0
            ? AppData.BranchId
            : 1;

        var newItems = Items
            .Where(i =>
                i.ItemName?.Contains(SearchQuery ?? "", StringComparison.OrdinalIgnoreCase) == true ||
                i.ItemCode?.Contains(SearchQuery ?? "", StringComparison.OrdinalIgnoreCase) == true ||
                i.Barcode?.Contains(SearchQuery ?? "", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        if (newItems.Count == 0)
        {
            var list = await posDataService.SearchItemsAsync(
                SearchQuery, branchId);

            newItems = list?.ToList() ?? [];
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            Items = new ObservableCollection<ItemDialogDTO>(newItems);
            SelectedItem = Items.FirstOrDefault();
        });
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Failed to load products: {ex.Message}";

        System.Diagnostics.Debug.WriteLine(
            $"[ItemSearchViewModel] SearchAsync error: {ex}");
    }
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
