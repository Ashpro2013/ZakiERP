using AshproSAASERP.ZatcaEngine.Constants;
using AshproSAASERP.ZatcaEngine.Enums;

namespace AshproSAASERP.ZatcaEngine.Api;

public class ReportingClient
{
    private readonly ZatcaHttpClient _client;
    private readonly EnvironmentType _environment;

    public ReportingClient(
        ZatcaHttpClient client,
        EnvironmentType environment)
    {
        _client = client;
        _environment = environment;
    }

    public async Task<string> SubmitAsync(
        string signedXml,
        string invoiceHash,
        string uuid,
        string token,
        string secret)
    {
        var endpoint =
            ZatcaEndpoints.GetReporting(_environment);

        return await _client.PostAsync(
            endpoint,
            signedXml,
            invoiceHash,
            uuid,
            token,
            secret,
            "0");
    }
}
