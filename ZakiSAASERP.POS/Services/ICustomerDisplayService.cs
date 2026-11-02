using ZakiSAASERP.Models.DTO;

namespace ZakiSAASERP.POS.Services;

public interface ICustomerDisplayService
{
    Task OpenAsync();
    Task CloseAsync();
    Task ToggleAsync();
    Task UpdateAsync(CustomerDisplayDto display);
    bool IsOpen { get; }
}

