using AshproSAASERP.Models.Transactions.Sales;
using AshproSAASERP.POS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace AshproSAASERP.POS.ViewModels;

public partial class HeldInvoicesViewModel(IPosDataService posDataService) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<SalesInvoiceHoldHdr> heldInvoices = [];

    [ObservableProperty]
    private SalesInvoiceHoldHdr? selectedHeldInvoice;

    [ObservableProperty]
    private string searchQuery = "";

    [ObservableProperty]
    private bool isBusy;

    public bool DialogResult { get; private set; }
    public int SelectedHoldId { get; private set; }
    public string CurrentTerminal => AppData.Terminal;
    public event Action? RequestClose;

    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        IsBusy = true;
        try
        {
            var branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            var list = await posDataService.GetHeldInvoicesAsync(branchId, AppData.Terminal);
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                list = list.Where(x => (x.SalesInvoiceNo != null && x.SalesInvoiceNo.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                                    || (x.CustomerName != null && x.CustomerName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            HeldInvoices = new ObservableCollection<SalesInvoiceHoldHdr>(list ?? []);
            if (HeldInvoices.Any())
            {
                SelectedHeldInvoice = HeldInvoices.First();
            }
        }
        catch { }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Retrieve()
    {
        if (SelectedHeldInvoice != null && SelectedHeldInvoice.SalesInvoiceHoldHdrId > 0)
        {
            SelectedHoldId = SelectedHeldInvoice.SalesInvoiceHoldHdrId;
            DialogResult = true;
            RequestClose?.Invoke();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(SalesInvoiceHoldHdr? item)
    {
        var target = item ?? SelectedHeldInvoice;
        if (target != null && target.SalesInvoiceHoldHdrId > 0)
        {
            var confirm = MessageBox.Show(
                $"Are you sure you want to delete held invoice '{target.SalesInvoiceNo}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                await posDataService.DeleteHeldInvoiceAsync(target.SalesInvoiceHoldHdrId);
                HeldInvoices.Remove(target);
                SelectedHeldInvoice = HeldInvoices.FirstOrDefault();
            }
        }
        else
        {
            MessageBox.Show("Please select a held invoice to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private async Task DeleteAllAsync()
    {
        if (!HeldInvoices.Any())
        {
            MessageBox.Show("No held invoices to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            "Are you sure you want to delete ALL held invoices? This action cannot be undone.",
            "Confirm Delete All",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            var branchId = AppData.BranchId > 0 ? AppData.BranchId : 1;
            await posDataService.DeleteAllHeldInvoicesAsync(branchId, AppData.Terminal);
            HeldInvoices.Clear();
            SelectedHeldInvoice = null;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke();
    }
}

