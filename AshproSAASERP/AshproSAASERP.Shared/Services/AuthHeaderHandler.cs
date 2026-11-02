using System.Net.Http.Headers;
namespace AshproSAASERP.Shared.Services;

public class AuthHeaderHandler(TokenService tokenService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = AppData.tocken;
        if (string.IsNullOrEmpty(token))
        {
            token = await tokenService.GetToken();
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
