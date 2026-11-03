using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Mob.Services;

public class CustomerDisplayService : ICustomerDisplayService
{
    private readonly IServiceProvider _services;

    private Window? _window;

    private CustomerDisplayViewModel? _viewModel;

    public bool IsOpen => _window != null && Application.Current?.Windows.Contains(_window) == true;

    public CustomerDisplayService(IServiceProvider services)
    {
        _services = services;
    }

    public Task OpenAsync()
    {
        // If already open and active in MAUI windows list, nothing to do
        if (_window != null && Application.Current?.Windows.Contains(_window) == true)
            return Task.CompletedTask;

        // Clean up any stale window reference if closed by user via 'X'
        _window = null;
        _viewModel = null;

        var page = _services.GetRequiredService<CustomerDisplayPage>();

        _viewModel = (CustomerDisplayViewModel)page.BindingContext;

        _window = new Window(page);

        _window.Destroying += (s, e) =>
        {
            _window = null;
            _viewModel = null;
        };

        Application.Current!.OpenWindow(_window);

        return Task.CompletedTask;
    }

    public Task CloseAsync()
    {
        if (_window != null)
        {
            try
            {
                if (Application.Current?.Windows.Contains(_window) == true)
                {
                    Application.Current.CloseWindow(_window);
                }
            }
            catch { }
            finally
            {
                _window = null;
                _viewModel = null;
            }
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(CustomerDisplayDto display)
    {
        if (_window != null && Application.Current?.Windows.Contains(_window) != true)
        {
            _window = null;
            _viewModel = null;
        }

        _viewModel?.Update(display);

        return Task.CompletedTask;
    }
}
