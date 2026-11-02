using ZakiSAASERP.POS.Services;
using ZakiSAASERP.POS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ZakiSAASERP.POS.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private bool _isSyncingPassword;

    public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = viewModel;

        _viewModel.OnLoginSuccess = () =>
        {
            Dispatcher.Invoke(() =>
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                Close();
            });
        };

        _viewModel.OnOpenDatabaseConfig = () =>
        {
            Dispatcher.Invoke(() =>
            {
                var splash = _serviceProvider.GetRequiredService<SplashWindow>();
                var splashVm = (SplashViewModel)splash.DataContext;
                splashVm.ShowDbConfig = true;
                splashVm.IsLoading = false;
                splashVm.ConnectionString = RegistryHelper.GetData("ConString", "");
                splashVm.SelectedProvider = RegistryHelper.GetData("DBProvider", "MSSQL");
                splash.Show();
                Close();
            });
        };

        Loaded += async (s, e) =>
        {
            await _viewModel.InitializeAsync();
            if (!string.IsNullOrEmpty(_viewModel.Password))
            {
                _isSyncingPassword = true;
                TxtPasswordBox.Password = _viewModel.Password;
                TxtPasswordText.Text = _viewModel.Password;
                _isSyncingPassword = false;
            }

            // Set initial focus to UserName field
            TxtUser.Focus();
            if (!string.IsNullOrEmpty(TxtUser.Text))
            {
                TxtUser.SelectAll();
            }
        };
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            // Do not call DragMove if clicking on an interactive control (TextBox, PasswordBox, ComboBox, etc.)
            if (e.OriginalSource is Visual visual)
            {
                DependencyObject? current = visual;
                while (current != null && current != this)
                {
                    if (current is TextBox || current is PasswordBox || current is ComboBox ||
                        current is ComboBoxItem || current is Button || current is CheckBox)
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

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // Alt+F3 opens database configuration (matching .Desk)
        if (e.Key == Key.F3 && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            _viewModel.OpenDatabaseConfig();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            if (!_viewModel.IsLoading)
            {
                _ = _viewModel.LoginCommand.ExecuteAsync(null);
                e.Handled = true;
            }
        }
    }

    private void TxtUser_LostFocus(object sender, RoutedEventArgs e)
    {
        _ = _viewModel.LoadPreLoginDetailsCommand.ExecuteAsync(null);
    }

    private void TxtPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSyncingPassword) return;
        _isSyncingPassword = true;
        _viewModel.Password = TxtPasswordBox.Password;
        TxtPasswordText.Text = TxtPasswordBox.Password;
        _isSyncingPassword = false;
    }

    private void TxtPasswordText_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isSyncingPassword) return;
        _isSyncingPassword = true;
        _viewModel.Password = TxtPasswordText.Text;
        TxtPasswordBox.Password = TxtPasswordText.Text;
        _isSyncingPassword = false;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
