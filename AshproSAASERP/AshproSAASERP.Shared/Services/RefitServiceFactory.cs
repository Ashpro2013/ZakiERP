using Microsoft.Extensions.DependencyInjection;
using Refit;
namespace AshproSAASERP.Shared.Services;

public class RefitServiceFactory
{
    private readonly IServiceProvider _sp;

    public RefitServiceFactory(IServiceProvider sp)
    {
        _sp = sp;
    }

    public IRefitService Create(string baseUrl)
    {
        var httpClientFactory = _sp.GetRequiredService<IHttpClientFactory>();

        // Create pipeline with handler
        var httpClient = httpClientFactory.CreateClient("DynamicClient");

        httpClient.BaseAddress = new Uri(baseUrl);

        return RestService.For<IRefitService>(httpClient);
    }
}

