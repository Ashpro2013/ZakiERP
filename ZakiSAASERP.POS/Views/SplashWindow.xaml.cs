using ZakiSAASERP.POS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ZakiSAASERP.POS.Views;

public partial class SplashWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SplashViewModel _viewModel;

    public SplashWindow(SplashViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = viewModel;

        _viewModel.OnReadyToLogin = () =>
        {
            Dispatcher.Invoke(() =>
            {
                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                Close();
            });
        };

        Loaded += async (s, e) =>
        {
            await _viewModel.StartStartupSequenceAsync();
        };
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            if (e.OriginalSource is Visual visual)
            {
                DependencyObject? current = visual;
                while (current != null && current != this)
                {
                    if (current is TextBox || current is PasswordBox || current is Button)
                    {
                        return;
                    }
                    current = VisualTreeHelper.GetParent(current);
                }
            }

            try
            {
                DragMove();
            }
            catch { }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}

