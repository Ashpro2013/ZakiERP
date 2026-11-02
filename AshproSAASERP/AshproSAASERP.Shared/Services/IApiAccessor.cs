namespace AshproSAASERP.Shared.Services;

public interface IApiAccessor
{
    IRefitService Api { get; }
    void SetApi(IRefitService api);
}
