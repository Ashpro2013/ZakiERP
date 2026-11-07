using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace ZakiSAASERP.POS.ViewModels;

public partial class ItemSearchViewModel(IPosDataService posDataService) : ObservableObject
{
    private CancellationTokenSource? _debounceCts;
    private string? _lastSearchedQuery = null;

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
        await ExecuteSearchAsync(SearchQuery);
    }

    partial void OnSearchQueryChanged(string value)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, token);
                if (token.IsCancellationRequested) return;

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    if (!token.IsCancellationRequested)
                    {
                        await ExecuteSearchAsync(value);
                    }
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ItemSearchViewModel] Debounce search error: {ex}");
            }
        }, token);
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        _debounceCts?.Cancel();
        await ExecuteSearchAsync(SearchQuery);
    }

    public async Task HandleEnterInSearchAsync()
    {
        if (_lastSearchedQuery != SearchQuery)
        {
            _debounceCts?.Cancel();
            await ExecuteSearchAsync(SearchQuery);
            if (Items.Count == 1)
            {
                Select();
            }
        }
        else if (SelectedItem != null)
        {
            Select();
        }
    }

    private async Task ExecuteSearchAsync(string query)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            var list = await posDataService.SearchItemsAsync(query ?? "", branchId);
            var resultList = list ?? [];

            Items = new ObservableCollection<ItemDialogDTO>(resultList);
            SelectedItem = Items.FirstOrDefault();
            _lastSearchedQuery = query;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load products: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[ItemSearchViewModel] ExecuteSearchAsync error: {ex}");
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
