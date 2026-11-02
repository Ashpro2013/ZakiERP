using AshproSAASERP.POS.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace AshproSAASERP.POS.Views;

public partial class POSSettingsDialog : Window
{
    public POSSettingsDialog(POSSettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseAction = Close;

        MouseDown += (s, e) =>
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        };
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

