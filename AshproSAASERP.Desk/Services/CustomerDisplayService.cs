using AshproSAASERP.Models.DTO;
using AshproSAASERP.Shared.Services;

namespace AshproSAASERP.Desk.Services;

public class CustomerDisplayService : ICustomerDisplayService
{
    private readonly IServiceProvider _services;

    private Window? _window;

    private CustomerDisplayViewModel? _viewModel;

    public bool IsOpen => _window != null;

    public CustomerDisplayService(IServiceProvider services)
    {
        _services = services;
    }

    public Task OpenAsync()
    {
        if (_window != null)
            return Task.CompletedTask;

        var page = _services.GetRequiredService<CustomerDisplayPage>();

        _viewModel = (CustomerDisplayViewModel)page.BindingContext;

        _window = new Window(page);

        Application.Current!.OpenWindow(_window);

        return Task.CompletedTask;
    }

    public Task CloseAsync()
    {
        if (_window != null)
        {
            Application.Current!.CloseWindow(_window);

            _window = null;
            _viewModel = null;
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(CustomerDisplayDto display)
    {
        _viewModel?.Update(display);

        return Task.CompletedTask;
    }
}