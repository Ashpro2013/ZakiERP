using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Mob.Services;

public class MauiApiAccessor() : IApiAccessor
{
    public IRefitService Api { get; private set; } = default!;

    public void SetApi(IRefitService api)
    {
        Api = api;
    }
}
