namespace AshproSAASERP.Shared.Services;

public class IndexedDbFactory(IJSRuntime _js)
{
    public IndexedDbRepository<T> Create<T>(string store)
        where T : class
    {
        return new IndexedDbRepository<T>(_js, store);
    }
    public IndexedDbRepository<T> CreateMenuDetails<T>(string store)
       where T : class
    {
        return new IndexedDbRepository<T>(_js, store);
    }
}
