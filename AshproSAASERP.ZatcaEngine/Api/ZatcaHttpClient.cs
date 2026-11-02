using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AshproSAASERP.ZatcaEngine.Api;

public class ZatcaHttpClient
{
    private readonly HttpClient _client;

    public ZatcaHttpClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> PostAsync(
        string endpoint,
        string signedXml,
        string invoiceHash,
        string uuid,
        string binarySecurityToken,
        string secret,
        string? clearanceStatus = null)
    {
        var cleanToken = (binarySecurityToken ?? "").Trim().Trim('\"', '\'').Replace("\r", "").Replace("\n", "");
        var cleanSecret = (secret ?? "").Trim().Trim('\"', '\'').Replace("\r", "").Replace("\n", "");

        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{cleanToken}:{cleanSecret}"));

        var payload = new
        {
            invoiceHash = invoiceHash,
            uuid = uuid,
            invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml))
        };

        string jsonPayload = JsonSerializer.Serialize(payload);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint);

        request.Content = new StringContent(
            jsonPayload,
            Encoding.UTF8,
            "application/json");

        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        request.Headers.TryAddWithoutValidation("Accept-Version", "V2");
        request.Headers.TryAddWithoutValidation("accept-language", "en");
        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {authValue}");

        // Clearance-Status header must ONLY be passed to clearance or reporting endpoints
        if (endpoint.Contains("/compliance/", StringComparison.OrdinalIgnoreCase))
        {
            // Do NOT add Clearance-Status header for compliance endpoint!
        }
        else if (!string.IsNullOrEmpty(clearanceStatus))
        {
            request.Headers.TryAddWithoutValidation("Clearance-Status", clearanceStatus);
        }
        else if (endpoint.Contains("/clearance/", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.TryAddWithoutValidation("Clearance-Status", "1");
        }
        else if (endpoint.Contains("/reporting/", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.TryAddWithoutValidation("Clearance-Status", "0");
        }

        var response = await _client.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            string errorDetail = content;
            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;
                    var messages = new List<string>();

                    if (root.TryGetProperty("validationResults", out var valResults) &&
                        valResults.TryGetProperty("errorMessages", out var errArr) &&
                        errArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var err in errArr.EnumerateArray())
                        {
                            var msg = err.TryGetProperty("message", out var m) ? m.GetString() : null;
                            var code = err.TryGetProperty("code", out var c) ? c.GetString() : null;
                            if (!string.IsNullOrEmpty(msg))
                            {
                                messages.Add(string.IsNullOrEmpty(code) ? msg : $"[{code}] {msg}");
                            }
                        }
                    }

                    if (root.TryGetProperty("message", out var topMsg) && topMsg.ValueKind == JsonValueKind.String)
                    {
                        var tm = topMsg.GetString();
                        if (!string.IsNullOrEmpty(tm) && !messages.Contains(tm))
                        {
                            messages.Insert(0, tm);
                        }
                    }

                    if (messages.Count > 0)
                    {
                        errorDetail = string.Join("; ", messages);
                    }
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(errorDetail))
            {
                var authHeader = response.Headers.WwwAuthenticate.FirstOrDefault()?.ToString();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    errorDetail = "Unauthorized (401). Authentication failed against ZATCA API." +
                        (string.IsNullOrEmpty(authHeader) ? "" : $" (Server Header: {authHeader})") +
                        "\nPossible causes:\n" +
                        "1. Using a Compliance CSID (CCSID) on Clearance or Reporting endpoints. A Compliance CSID can only authenticate at the Compliance endpoint (/compliance/invoices).\n" +
                        "2. Environment mismatch: A Developer Portal CSID (issued by TSTZATCA) will be rejected by Simulation/Production servers. A Simulation CSID (PREZATCA) will be rejected by Developer Portal.\n" +
                        "3. VAT number mismatch: The Seller VAT in the invoice XML must match the VAT in the certificate Subject (UID).\n" +
                        "4. The CSID credentials (token or secret) are invalid or expired.";
                }
                else
                {
                    errorDetail = response.ReasonPhrase ?? response.StatusCode.ToString();
                }
            }

            throw new Exception($"ZATCA API Error ({(int)response.StatusCode}): {errorDetail}");
        }

        return content;
    }
}
