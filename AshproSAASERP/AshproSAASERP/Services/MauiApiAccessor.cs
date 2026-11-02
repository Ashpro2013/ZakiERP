using AshproSAASERP.Shared.Services;
namespace AshproSAASERP.Services;

public class MauiApiAccessor() : IApiAccessor
{
    public IRefitService Api { get; private set; }

    public void SetApi(IRefitService api)
    {
        Api = api;
    }
}
