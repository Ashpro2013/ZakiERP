using ZakiSAASERP.POS.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ZakiSAASERP.POS.Views;

public partial class BillSearchDialog : Window
{
    private readonly BillSearchViewModel _viewModel;

    public BillSearchDialog(BillSearchViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.RequestClose += () =>
        {
            DialogResult = _viewModel.DialogResult;
            Close();
        };

        Loaded += (_, _) =>
        {
            TxtSearch.Focus();
            TxtSearch.SelectAll();
        };

        KeyDown += BillSearchDialog_KeyDown;
    }

    private void BillSearchDialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.CancelCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && _viewModel.SelectedBill != null && !TxtSearch.IsFocused)
        {
            _viewModel.SelectCommand.Execute(null);
            e.Handled = true;
        }
    }

    private async void TxtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await _viewModel.SearchAsync();
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            GridBills.Focus();
            if (GridBills.SelectedIndex < 0 && GridBills.Items.Count > 0)
            {
                GridBills.SelectedIndex = 0;
            }
            e.Handled = true;
        }
    }

    private void GridBills_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _viewModel.SelectedBill != null)
        {
            _viewModel.SelectCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        _viewModel.SelectCommand.Execute(null);
    }
}

