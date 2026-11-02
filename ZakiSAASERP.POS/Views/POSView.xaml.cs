using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.POS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZakiSAASERP.POS.Views;

public partial class POSView : UserControl
{
    private readonly IServiceProvider _serviceProvider;
    private POSViewModel ViewModel => (POSViewModel)DataContext;

    public POSView(POSViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        DataContext = viewModel;

        // Wire up UI interaction delegates
        viewModel.FocusBarcodeHandler = () =>
        {
            Dispatcher.Invoke(() =>
            {
                TxtBarcode.Focus();
                TxtBarcode.SelectAll();
            });
        };

        viewModel.ShowNotificationHandler = (title, message) =>
        {
            Dispatcher.Invoke(() =>
            {
                MessageBoxImage icon = title == "Error" ? MessageBoxImage.Error :
                                      title == "Warning" ? MessageBoxImage.Warning :
                                      MessageBoxImage.Information;
                MessageBox.Show(Window.GetWindow(this), message, title, MessageBoxButton.OK, icon);
            });
        };

        viewModel.ShowPaymentDialogHandler = (total) =>
        {
            var dialog = _serviceProvider.GetRequiredService<PaymentDialog>();
            var vm = (PaymentViewModel)dialog.DataContext;
            vm.Initialize(total);

            bool? res = dialog.ShowDialog();
            return Task.FromResult((res == true && vm.DialogResult, vm.PaymentType, vm.TenderedAmount, vm.ChangeAmount));
        };

        viewModel.ShowCustomerSearchHandler = () =>
        {
            var dialog = _serviceProvider.GetRequiredService<CustomerSearchDialog>();
            var vm = (CustomerSearchViewModel)dialog.DataContext;

            bool? res = dialog.ShowDialog();
            return Task.FromResult(res == true && vm.DialogResult ? vm.SelectedCustomer : null);
        };

        viewModel.ShowHeldInvoicesHandler = () =>
        {
            var dialog = _serviceProvider.GetRequiredService<HeldInvoicesDialog>();
            var vm = (HeldInvoicesViewModel)dialog.DataContext;

            bool? res = dialog.ShowDialog();
            return Task.FromResult<int?>(res == true && vm.DialogResult ? vm.SelectedHoldId : null);
        };

        viewModel.ShowItemSearchHandler = () =>
        {
            var dialog = _serviceProvider.GetRequiredService<ItemSearchDialog>();
            var vm = (ItemSearchViewModel)dialog.DataContext;

            bool? res = dialog.ShowDialog();
            return Task.FromResult(res == true && vm.DialogResult ? vm.SelectedItem : null);
        };

        viewModel.ShowDiscountHandler = (currentDisc, gross) =>
        {
            var dialog = _serviceProvider.GetRequiredService<DiscountDialog>();
            var vm = (DiscountViewModel)dialog.DataContext;
            vm.Initialize(currentDisc, gross);

            bool? res = dialog.ShowDialog();
            return Task.FromResult<decimal?>(res == true && vm.DialogResult ? vm.DiscountAmount : null);
        };

        viewModel.ShowSettingsDialogHandler = () =>
        {
            var dialog = _serviceProvider.GetRequiredService<POSSettingsDialog>();
            var vm = (POSSettingsViewModel)dialog.DataContext;
            vm.Initialize();

            bool? res = dialog.ShowDialog();
            return Task.FromResult(res == true && vm.DialogResult);
        };

        viewModel.ShowZatcaPendingHandler = () =>
        {
            // No dialog on click per user requirement - only refresh count

            return Task.CompletedTask;
        };

        viewModel.ShowShiftDialogHandler = () =>
        {
            var dialog = _serviceProvider.GetRequiredService<ShiftDialog>();
            var vm = (ShiftViewModel)dialog.DataContext;

            bool? res = dialog.ShowDialog();
            return Task.FromResult(res == true && vm.ShiftChangedResult);
        };

        Loaded += async (_, _) =>
        {
            await ViewModel.InitializeCommand.ExecuteAsync(null);
            TxtBarcode.Focus();
        };
    }

    private async void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await ViewModel.ProcessBarcodeCommand.ExecuteAsync(null);
            e.Handled = true;
        }
    }

    private void TxtBarcode_GotFocus(object sender, RoutedEventArgs e)
    {
        ViewModel.SetActiveField("BarcodeSearch");
    }

    private async void POSNumPad_KeyClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is RoutedEventArgs routed && routed.Source is string key)
        {
            await ViewModel.HandleNumPadKeyCommand.ExecuteAsync(key);
        }
        else if (e.Source is RoutedEventArgs re && re.Source is string k)
        {
            await ViewModel.HandleNumPadKeyCommand.ExecuteAsync(k);
        }
    }

    private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        if (e.Row.Item is TransactionDtls row && e.Column.Header is string header)
        {
            string field = header switch
            {
                "Qty" => "Qty",
                "Rate" => "Rate",
                "Inc. Rate" => "IncRate",
                "Disc (%)" => "Disc%",
                "Disc" => "DiscAmt",
                _ => "BarcodeSearch"
            };
            ViewModel.SetActiveCell(row, field);
        }
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            ViewModel.RecalculateTotals();
        }));
    }
}

