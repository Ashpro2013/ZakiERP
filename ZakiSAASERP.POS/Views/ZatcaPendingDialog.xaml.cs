using ZakiSAASERP.POS.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ZakiSAASERP.POS.Views;

public partial class ZatcaPendingDialog : Window
{
    public ZatcaPendingDialog(ZatcaPendingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseAction = Close;

        MouseDown += (s, e) =>
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        };

        Loaded += async (_, _) =>
        {
            await viewModel.LoadPendingInvoicesCommand.ExecuteAsync(null);
        };
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

