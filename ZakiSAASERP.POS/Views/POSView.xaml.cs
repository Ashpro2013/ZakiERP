using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Transactions.Sales;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.POS.Services;
using ZakiSAASERP.POS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                GridCart.CommitEdit(DataGridEditingUnit.Row, true);
                TxtBarcode.Focus();
                TxtBarcode.SelectAll();
            });
        };

        viewModel.FocusCartCellHandler = (fieldName) =>
        {
            Dispatcher.Invoke(() =>
            {
                FocusCartGridCell(fieldName);
            });
        };

        viewModel.ActiveBufferUpdatedHandler = (buffer) =>
        {
            Dispatcher.Invoke(() =>
            {
                SyncActiveCellEditingText(buffer);
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
            dialog.Owner = Window.GetWindow(this);
            var vm = (PaymentViewModel)dialog.DataContext;
            vm.Initialize(total);

            bool? res = dialog.ShowDialog();
            return Task.FromResult((res == true && vm.DialogResult, vm.PaymentType, vm.TenderedAmount, vm.ChangeAmount));
        };

        viewModel.ShowCustomerSearchHandler = async () =>
        {
            var dialog = _serviceProvider.GetRequiredService<CustomerSearchDialog>();
            dialog.Owner = Window.GetWindow(this);
            var vm = (CustomerSearchViewModel)dialog.DataContext;
            await vm.SearchAsync();

            bool? res = dialog.ShowDialog();
            return res == true && vm.DialogResult ? vm.SelectedCustomer : null;
        };

        viewModel.ShowHeldInvoicesHandler = async () =>
        {
            var dialog = _serviceProvider.GetRequiredService<HeldInvoicesDialog>();
            dialog.Owner = Window.GetWindow(this);
            var vm = (HeldInvoicesViewModel)dialog.DataContext;
            await vm.LoadInvoicesAsync();

            bool? res = dialog.ShowDialog();
            return res == true && vm.DialogResult ? vm.SelectedHoldId : null;
        };

        viewModel.ShowBillSearchHandler = async () =>
        {
            var dialog = _serviceProvider.GetRequiredService<BillSearchDialog>();
            dialog.Owner = Window.GetWindow(this);
            var vm = (BillSearchViewModel)dialog.DataContext;
            await vm.LoadBillsAsync();

            bool? res = dialog.ShowDialog();
            return res == true && vm.DialogResult ? vm.SelectedInvoiceId : null;
        };

        viewModel.ShowItemSearchHandler = async (initialQuery) =>
        {
            var dialog = _serviceProvider.GetRequiredService<ItemSearchDialog>();
            dialog.Owner = Window.GetWindow(this);
            var vm = (ItemSearchViewModel)dialog.DataContext;
            await vm.InitializeAsync(initialQuery);

            bool? res = dialog.ShowDialog();
            return res == true && vm.DialogResult ? vm.SelectedItem : null;
        };

        viewModel.ShowDiscountHandler = (currentDisc, gross) =>
        {
            var dialog = _serviceProvider.GetRequiredService<DiscountDialog>();
            dialog.Owner = Window.GetWindow(this);
            var vm = (DiscountViewModel)dialog.DataContext;
            vm.Initialize(currentDisc, gross);

            bool? res = dialog.ShowDialog();
            return Task.FromResult<decimal?>(res == true && vm.DialogResult ? vm.DiscountAmount : null);
        };

        viewModel.ShowSettingsDialogHandler = () =>
        {
            var dialog = _serviceProvider.GetRequiredService<POSSettingsDialog>();
            dialog.Owner = Window.GetWindow(this);
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
            dialog.Owner = Window.GetWindow(this);
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
        string? key = e.Source as string
            ?? e.OriginalSource as string
            ?? (e.OriginalSource as FrameworkElement)?.Tag as string
            ?? (e.Source as FrameworkElement)?.Tag as string;

        if (string.IsNullOrEmpty(key) && e.OriginalSource is RoutedEventArgs inner)
        {
            key = inner.Source as string ?? (inner.Source as FrameworkElement)?.Tag as string;
        }

        if (!string.IsNullOrEmpty(key))
        {
            await ViewModel.HandleNumPadKeyCommand.ExecuteAsync(key);
        }
    }

    private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        if (e.Row.Item is TransactionDtls row && e.Column.Header is string header)
        {
            string field = header switch
            {
                "Qty" => "Qty",
                "Rate" or "Ex. Rate" => "Rate",
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
        if (e.EditAction == DataGridEditAction.Cancel) return;

        var row = e.Row.Item as TransactionDtls;
        var header = e.Column?.Header?.ToString();

        // Immediately push text from the editing element to the underlying data item
        if (e.EditingElement is TextBox tb)
        {
            var bindingExpression = tb.GetBindingExpression(TextBox.TextProperty);
            bindingExpression?.UpdateSource();
        }

        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (row != null && header != null)
            {
                ViewModel.OnRowFieldEdited(row, header);
            }
            else
            {
                ViewModel.RecalculateTotals();
            }
        }));
    }

    private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
        if (e.Row.Item is TransactionDtls row && e.Column.Header is string header && e.EditingElement is TextBox tb)
        {
            tb.SelectAll();

            TextChangedEventHandler handler = (s, args) =>
            {
                string text = tb.Text.Trim();
                if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal val) ||
                    decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out val))
                {
                    if (header is "Qty")
                    {
                        row.Quantity = val;
                        TransactionHelper.GetTransactionAfterEdit(row, ViewModel.Settings.ShowIncRate);
                        ViewModel.RecalculateTotals();
                    }
                    else if (header is "Ex. Rate" or "Rate")
                    {
                        row.ExRate = val;
                        ViewModel.UpdateInRate(row);
                        TransactionHelper.GetTransactionAfterEdit(row, ViewModel.Settings.ShowIncRate);
                        ViewModel.RecalculateTotals();
                    }
                    else if (header is "Inc. Rate" or "IncRate")
                    {
                        row.InRate = val;
                        ViewModel.UpdateExRate(row);
                        TransactionHelper.GetTransactionAfterEdit(row, true);
                        ViewModel.RecalculateTotals();
                    }
                    else if (header is "Disc (%)")
                    {
                        row.DiscountPerc = val;
                        TransactionHelper.GetTransactionAfterEdit(row, ViewModel.Settings.ShowIncRate);
                        ViewModel.RecalculateTotals();
                    }
                    else if (header is "Disc")
                    {
                        row.DiscountAmnt = val;
                        if (row.GrossAmount > 0)
                        {
                            row.DiscountPerc = Math.Round((val / row.GrossAmount) * 100m, 4, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            row.DiscountPerc = 0;
                        }
                        TransactionHelper.GetTransactionAfterEdit(row, ViewModel.Settings.ShowIncRate, true);
                        ViewModel.RecalculateTotals();
                    }
                }
            };

            tb.TextChanged += handler;

            RoutedEventHandler? unloadHandler = null;
            unloadHandler = (s, args) =>
            {
                tb.TextChanged -= handler;
                tb.Unloaded -= unloadHandler;
            };
            tb.Unloaded += unloadHandler;
        }
    }

    private void GridCart_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            GridCart.CommitEdit(DataGridEditingUnit.Row, true);
            e.Handled = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TxtBarcode.Focus();
                TxtBarcode.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
    }

    private void FocusCartGridCell(string fieldName)
    {
        if (GridCart.Items.Count == 0) return;

        var selectedItem = GridCart.SelectedItem as TransactionDtls
            ?? ViewModel.SelectedCartItem
            ?? (GridCart.Items.Count > 0 ? GridCart.Items[GridCart.Items.Count - 1] as TransactionDtls : null);

        if (selectedItem == null) return;

        GridCart.SelectedItem = selectedItem;
        ViewModel.SelectedCartItem = selectedItem;

        DataGridColumn? targetCol = null;
        if (fieldName == "Qty")
        {
            targetCol = GridCart.Columns.FirstOrDefault(c => c.Header?.ToString() == "Qty" && c.Visibility == Visibility.Visible);
        }
        else if (fieldName == "Rate")
        {
            if (ViewModel.Settings.ShowIncRate && ViewModel.Settings.AllowEditIncRate && (!ViewModel.Settings.ShowExRate || !ViewModel.Settings.AllowEditExcRate))
            {
                targetCol = GridCart.Columns.FirstOrDefault(c => c.Header?.ToString() == "Inc. Rate" && c.Visibility == Visibility.Visible);
            }
            if (targetCol == null)
            {
                targetCol = GridCart.Columns.FirstOrDefault(c => (c.Header?.ToString() == "Ex. Rate" || c.Header?.ToString() == "Rate") && c.Visibility == Visibility.Visible && !c.IsReadOnly);
            }
            if (targetCol == null)
            {
                targetCol = GridCart.Columns.FirstOrDefault(c => c.Header?.ToString() == "Inc. Rate" && c.Visibility == Visibility.Visible && !c.IsReadOnly);
            }
            if (targetCol == null)
            {
                targetCol = GridCart.Columns.FirstOrDefault(c => (c.Header?.ToString() == "Ex. Rate" || c.Header?.ToString() == "Rate" || c.Header?.ToString() == "Inc. Rate") && c.Visibility == Visibility.Visible);
            }
        }

        if (targetCol == null) return;

        GridCart.Focus();
        GridCart.CurrentCell = new DataGridCellInfo(selectedItem, targetCol);
        GridCart.ScrollIntoView(selectedItem, targetCol);
        GridCart.UpdateLayout();

        FocusAndEditCell(selectedItem, targetCol);
    }

    private void FocusAndEditCell(object item, DataGridColumn column)
    {
        var row = (DataGridRow)GridCart.ItemContainerGenerator.ContainerFromItem(item);
        if (row == null)
        {
            GridCart.UpdateLayout();
            GridCart.ScrollIntoView(item, column);
            row = (DataGridRow)GridCart.ItemContainerGenerator.ContainerFromItem(item);
        }

        if (row != null)
        {
            var presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter != null)
            {
                int colIndex = GridCart.Columns.IndexOf(column);
                if (colIndex >= 0)
                {
                    var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(colIndex);
                    if (cell != null)
                    {
                        cell.Focus();
                    }
                }
            }
        }

        GridCart.BeginEdit();

        Dispatcher.BeginInvoke(new Action(() =>
        {
            GridCart.BeginEdit();
            if (column.GetCellContent(item) is FrameworkElement element)
            {
                var textBox = element as TextBox ?? FindVisualChild<TextBox>(element);
                if (textBox != null)
                {
                    textBox.Focus();
                    textBox.SelectAll();
                }
            }
        }), System.Windows.Threading.DispatcherPriority.Input);
    }

    private void SyncActiveCellEditingText(string text)
    {
        if (GridCart.SelectedItem != null && GridCart.CurrentColumn != null)
        {
            var content = GridCart.CurrentColumn.GetCellContent(GridCart.SelectedItem);
            var tb = content as TextBox ?? FindVisualChild<TextBox>(content);
            if (tb != null)
            {
                tb.Text = text;
                tb.CaretIndex = tb.Text.Length;
            }
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild) return typedChild;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }
}

