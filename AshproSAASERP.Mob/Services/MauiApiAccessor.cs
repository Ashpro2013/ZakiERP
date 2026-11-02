using AshproSAASERP.Shared.Services;

namespace AshproSAASERP.Mob.Services;

public class MauiApiAccessor() : IApiAccessor
{
    public IRefitService Api { get; private set; } = default!;

    public void SetApi(IRefitService api)
    {
        Api = api;
    }
}
