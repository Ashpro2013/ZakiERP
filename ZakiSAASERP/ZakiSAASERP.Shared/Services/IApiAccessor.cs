namespace ZakiSAASERP.Shared.Services;

public interface IApiAccessor
{
    IRefitService Api { get; }
    void SetApi(IRefitService api);
}
