using ZakiSAASERP.POS.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ZakiSAASERP.POS.Views;

public partial class DiscountDialog : Window
{
    private readonly DiscountViewModel _viewModel;

    public DiscountDialog(DiscountViewModel viewModel)
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
            TxtDiscount.Focus();
            TxtDiscount.SelectAll();
        };

        KeyDown += DiscountDialog_KeyDown;
    }

    private void DiscountDialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.CancelCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            _viewModel.ApplyCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void TxtDiscount_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.ApplyCommand.Execute(null);
            e.Handled = true;
        }
    }
}

