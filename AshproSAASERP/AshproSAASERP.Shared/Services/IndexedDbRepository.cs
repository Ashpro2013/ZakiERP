namespace AshproSAASERP.Shared.Services;

public class IndexedDbRepository<T>(IJSRuntime _js, string _store) where T : class
{
    private IJSObjectReference? _module;
    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>(
     "import",
     "/_content/AshproSAASERP.Shared/js/indexedDb.js"
 );


        return _module;
    }
    public async Task<T?> GetAsync(string Id)
    {
        var module = await GetModuleAsync();

        return await module.InvokeAsync<T?>(
            "getById",
            _store,
            Id
        );
    }

    public async Task<List<T>> GetAllAsync()
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<List<T>>("getAll", _store);
    }

    public async Task SaveAsync(T entity)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("upsert", _store, entity);
    }
    public async Task SaveAsync(List<T> entities)
    {
        var module = await GetModuleAsync();
        foreach (var menu in entities)
        {
            await module.InvokeVoidAsync("upsert", _store, menu);
        }
    }
    public async Task DeleteAsync(string Id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("remove", _store, Id);
    }
    public async Task ClearAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("clear", _store);
    }
}
