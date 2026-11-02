using AshproSAASERP.Models.DTO;

namespace AshproSAASERP.POS.Services;

public interface ICustomerDisplayService
{
    Task OpenAsync();
    Task CloseAsync();
    Task ToggleAsync();
    Task UpdateAsync(CustomerDisplayDto display);
    bool IsOpen { get; }
}

