using ZakiSAASERP.POS.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ZakiSAASERP.POS.Views;

public partial class HeldInvoicesDialog : Window
{
    private readonly HeldInvoicesViewModel _viewModel;

    public HeldInvoicesDialog(HeldInvoicesViewModel viewModel)
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
            await _viewModel.LoadInvoicesCommand.ExecuteAsync(null);
        };

        KeyDown += HeldInvoicesDialog_KeyDown;
    }

    private void HeldInvoicesDialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.CancelCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && _viewModel.SelectedHeldInvoice != null)
        {
            _viewModel.RetrieveCommand.Execute(null);
            e.Handled = true;
        }
    }

    private async void TxtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await _viewModel.LoadInvoicesCommand.ExecuteAsync(null);
            e.Handled = true;
        }
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        _viewModel.RetrieveCommand.Execute(null);
    }
}

