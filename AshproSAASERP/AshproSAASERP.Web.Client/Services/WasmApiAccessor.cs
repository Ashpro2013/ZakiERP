using AshproSAASERP.Shared.Services;

namespace AshproSAASERP.Web.Client.Services;

public class WasmApiAccessor(IRefitService refitApi) : IApiAccessor
{
    public IRefitService Api => refitApi;

    public void SetApi(IRefitService api)
    {
        throw new NotImplementedException();
    }
}
