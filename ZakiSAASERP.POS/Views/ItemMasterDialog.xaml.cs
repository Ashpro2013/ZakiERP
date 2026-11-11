using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZakiSAASERP.POS.ViewModels;

namespace ZakiSAASERP.POS.Views;

public partial class ItemMasterDialog : Window
{
    private readonly ItemMasterViewModel _viewModel;

    public ItemMasterDialog(ItemMasterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.RequestClose += () =>
        {
            DialogResult = _viewModel.DialogResult;
            Close();
        };

        MouseDown += (_, e) =>
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try { DragMove(); } catch { }
            }
        };

        Loaded += (_, _) =>
        {
            if (_viewModel.IsAutoCode)
            {
                TxtItemName.Focus();
                TxtItemName.SelectAll();
            }
            else
            {
                TxtItemCode.Focus();
                TxtItemCode.SelectAll();
            }
        };

        KeyDown += ItemMasterDialog_KeyDown;
    }

    private void ItemMasterDialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.CancelCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SelectAllText_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.Dispatcher.BeginInvoke(new Action(tb.SelectAll));
        }
    }
}

