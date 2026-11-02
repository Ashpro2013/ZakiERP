using AshproSAASERP.Models.DTO;
using AshproSAASERP.Shared.Services;

namespace AshproSAASERP.Web.Client.Services;

public class CustomerDisplayService : ICustomerDisplayService
{
    public bool IsOpen => throw new NotImplementedException();

    public Task CloseAsync()
    {
        throw new NotImplementedException();
    }

    public Task OpenAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(CustomerDisplayDto display)
    {
        throw new NotImplementedException();
    }
}
