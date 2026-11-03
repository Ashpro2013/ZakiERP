using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.POS.ViewModels;
using ZakiSAASERP.POS.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ZakiSAASERP.POS.Services;

public class CustomerDisplayService : ICustomerDisplayService
{
    private readonly IServiceProvider _serviceProvider;
    private CustomerDisplayWindow? _window;
    private CustomerDisplayViewModel? _viewModel;

    public bool IsOpen => _window != null && _window.IsLoaded && _window.IsVisible;

    public CustomerDisplayService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        if (Application.Current != null)
        {
            Application.Current.Exit += (s, e) =>
            {
                if (_window != null)
                {
                    _window.AllowClose = true;
                    _window.Close();
                }
            };
        }
    }

    public Task OpenAsync()
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            if (_window != null && !_window.IsLoaded)
            {
                _window = null;
                _viewModel = null;
            }

            if (_window == null)
            {
                _viewModel = _serviceProvider.GetRequiredService<CustomerDisplayViewModel>();
                _window = new CustomerDisplayWindow(_viewModel);

                _window.Closed += (s, e) =>
                {
                    _window = null;
                    _viewModel = null;
                };

                // Detect secondary monitor
                if (SystemParameters.VirtualScreenWidth > SystemParameters.PrimaryScreenWidth)
                {
                    _window.WindowStartupLocation = WindowStartupLocation.Manual;
                    _window.Left = SystemParameters.PrimaryScreenWidth + 20;
                    _window.Top = 50;
                    _window.WindowState = WindowState.Maximized;
                }
            }

            _window.Show();
            if (_window.WindowState == WindowState.Minimized)
            {
                _window.WindowState = WindowState.Normal;
            }
            _window.Activate();
        });

        return Task.CompletedTask;
    }

    public Task CloseAsync()
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            if (_window != null)
            {
                _window.Hide();
            }
        });

        return Task.CompletedTask;
    }

    public Task ToggleAsync()
    {
        if (IsOpen)
            return CloseAsync();
        else
            return OpenAsync();
    }

    public Task UpdateAsync(CustomerDisplayDto display)
    {
        _viewModel?.Update(display);
        return Task.CompletedTask;
    }
}
