using AshproSAASERP.ZatcaEngine.Constants;
using AshproSAASERP.ZatcaEngine.Enums;
using AshproSAASERP.ZatcaEngine.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AshproSAASERP.ZatcaEngine.Onboarding;

public class ComplianceClient
{
    private readonly HttpClient _httpClient;
    private readonly EnvironmentType _environment;

    public ComplianceClient(HttpClient httpClient, EnvironmentType environment)
    {
        _httpClient = httpClient;
        _environment = environment;
    }

    public async Task<ComplianceResult> RequestComplianceAsync(
        string csrBase64,
        string otp)
    {
        var endpoint = ZatcaEndpoints.GetCompliance(_environment);

        var requestBody = new
        {
            csr = csrBase64
        };

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        if (!string.IsNullOrWhiteSpace(otp))
        {
            request.Headers.TryAddWithoutValidation("OTP", otp.Trim());
        }
        request.Headers.TryAddWithoutValidation("Accept-Version", "V2");
        request.Headers.TryAddWithoutValidation("accept-language", "en");
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[ZATCA Compliance API Failed] Status: {(int)response.StatusCode}, Endpoint: {endpoint}, Response: {content}");
            string errorDetail = content;
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array && errors.GetArrayLength() > 0)
                {
                    var first = errors[0];
                    var msg = first.TryGetProperty("message", out var m) ? m.GetString() : null;
                    var code = first.TryGetProperty("code", out var c) ? c.GetString() : null;
                    errorDetail = $"{code}: {msg}";
                }
                else if (root.TryGetProperty("errorMessage", out var errMsg))
                {
                    errorDetail = errMsg.GetString() ?? content;
                }
            }
            catch { }

            return new ComplianceResult
            {
                ErrorMessage = $"ZATCA Error ({(int)response.StatusCode}): {errorDetail}"
            };
        }

        try
        {
            var result = JsonSerializer.Deserialize<ComplianceResult>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                });

            return result ?? new ComplianceResult { ErrorMessage = "Failed to deserialize compliance response." };
        }
        catch (Exception ex)
        {
            return new ComplianceResult
            {
                ErrorMessage = $"Failed to parse ZATCA compliance response: {ex.Message}. Raw: {content}"
            };
        }
    }
}
