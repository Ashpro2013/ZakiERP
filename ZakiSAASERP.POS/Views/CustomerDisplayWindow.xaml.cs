using ZakiSAASERP.POS.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ZakiSAASERP.POS.Views;

public partial class CustomerDisplayWindow : Window
{
    public bool AllowClose { get; set; } = false;

    public CustomerDisplayWindow(CustomerDisplayViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        MouseDown += (s, e) =>
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        };
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (AllowClose)
        {
            base.OnClosing(e);
            return;
        }

        // Don't destroy window on user close, hide it so service can re-show cleanly
        e.Cancel = true;
        Hide();
    }
}
