using ZakiSAASERP.ZatcaEngine.Constants;
using ZakiSAASERP.ZatcaEngine.Enums;
using ZakiSAASERP.ZatcaEngine.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ZakiSAASERP.ZatcaEngine.Onboarding;

public class ProductionCertificateClient
{
    private readonly HttpClient _httpClient;
    private readonly EnvironmentType _environment;

    public ProductionCertificateClient(
        HttpClient httpClient,
        EnvironmentType environment)
    {
        _httpClient = httpClient;
        _environment = environment;
    }

    public async Task<ProductionCertificateResult> RequestProductionCertificateAsync(
        string complianceRequestId,
        string binarySecurityToken,
        string secret)
    {
        var endpoint = ZatcaEndpoints.GetProductionCsid(_environment);

        object requestBody;
        if (long.TryParse(complianceRequestId, out var numericId))
        {
            requestBody = new { compliance_request_id = numericId };
        }
        else
        {
            requestBody = new { compliance_request_id = complianceRequestId };
        }

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var cleanToken = (binarySecurityToken ?? "").Trim().Replace("\r", "").Replace("\n", "");
        var cleanSecret = (secret ?? "").Trim().Replace("\r", "").Replace("\n", "");

        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{cleanToken}:{cleanSecret}"));

        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("Accept-Version", "V2");
        request.Headers.TryAddWithoutValidation("accept-language", "en");
        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {authValue}");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new ProductionCertificateResult
            {
                ErrorMessage = $"Production Certificate API Failed ({(int)response.StatusCode}): {content}"
            };
        }

        try
        {
            var result = JsonSerializer.Deserialize<ProductionCertificateResult>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new ProductionCertificateResult { ErrorMessage = "Failed to deserialize production CSID response." };
        }
        catch (Exception ex)
        {
            return new ProductionCertificateResult
            {
                ErrorMessage = $"Failed to parse production CSID response: {ex.Message}"
            };
        }
    }
}
