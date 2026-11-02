using ZakiSAASERP.POS.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ZakiSAASERP.POS.Views;

public partial class ItemSearchDialog : Window
{
    private readonly ItemSearchViewModel _viewModel;

    public ItemSearchDialog(ItemSearchViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.RequestClose += () =>
        {
            DialogResult = _viewModel.DialogResult;
            Close();
        };

        Loaded += async (_, _) =>
        {
            TxtSearch.Focus();
            await _viewModel.SearchCommand.ExecuteAsync(null);
        };

        KeyDown += ItemSearchDialog_KeyDown;
    }

    private void ItemSearchDialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.CancelCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && _viewModel.SelectedItem != null)
        {
            _viewModel.SelectCommand.Execute(null);
            e.Handled = true;
        }
    }

    private async void TxtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await _viewModel.SearchCommand.ExecuteAsync(null);
            e.Handled = true;
        }
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        _viewModel.SelectCommand.Execute(null);
    }
}

