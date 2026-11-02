namespace AshproSAASERP.Shared.Services;

public interface ICustomerDisplayService
{
    Task OpenAsync();

    Task CloseAsync();

    Task UpdateAsync(CustomerDisplayDto display);

    bool IsOpen { get; }
}
