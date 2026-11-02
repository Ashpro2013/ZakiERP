using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Web.Client.Services;

public class WasmApiAccessor(IRefitService refitApi) : IApiAccessor
{
    public IRefitService Api => refitApi;

    public void SetApi(IRefitService api)
    {
        throw new NotImplementedException();
    }
}
