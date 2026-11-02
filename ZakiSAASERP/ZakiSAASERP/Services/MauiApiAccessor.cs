using ZakiSAASERP.Shared.Services;
namespace ZakiSAASERP.Services;

public class MauiApiAccessor() : IApiAccessor
{
    public IRefitService Api { get; private set; }

    public void SetApi(IRefitService api)
    {
        Api = api;
    }
}
