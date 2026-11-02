using ZakiSAASERP.POS.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ZakiSAASERP.POS.Views;

public partial class PaymentDialog : Window
{
    private readonly PaymentViewModel _viewModel;

    public PaymentDialog(PaymentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.RequestClose += OnRequestClose;

        Loaded += (_, _) =>
        {
            TxtTenderInput.Focus();
            TxtTenderInput.SelectAll();
        };

        KeyDown += PaymentDialog_KeyDown;
    }

    private void OnRequestClose()
    {
        DialogResult = _viewModel.DialogResult;
        Close();
    }

    private void PaymentDialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.CancelCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && _viewModel.IsValid)
        {
            _viewModel.ConfirmPaymentCommand.Execute("Cash");
            e.Handled = true;
        }
    }

    private void TxtTenderInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _viewModel.IsValid)
        {
            _viewModel.ConfirmPaymentCommand.Execute("Cash");
            e.Handled = true;
        }
    }
}

