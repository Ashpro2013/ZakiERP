using System.Text.Json.Serialization;

namespace ZakiSAASERP.ZatcaEngine.Models;

public class ComplianceResult
{
    [JsonPropertyName("requestID")]
    public object? RawRequestID { get; set; }

    [JsonIgnore]
    public string RequestID => RawRequestID?.ToString() ?? "";

    [JsonPropertyName("binarySecurityToken")]
    public string BinarySecurityToken { get; set; } = "";

    [JsonPropertyName("issuedCertificate")]
    public string? IssuedCertificate
    {
        get => BinarySecurityToken;
        set { if (!string.IsNullOrEmpty(value)) BinarySecurityToken = value; }
    }

    [JsonPropertyName("secret")]
    public string Secret { get; set; } = "";

    [JsonPropertyName("apiSecret")]
    public string? ApiSecret
    {
        get => Secret;
        set { if (!string.IsNullOrEmpty(value)) Secret = value; }
    }

    [JsonPropertyName("dispositionMessage")]
    public string DispositionMessage { get; set; } = "";

    [JsonPropertyName("dispositionStatus")]
    public string DispositionStatus { get; set; } = "";

    public bool IsSuccess =>
        DispositionStatus.Equals("ISSUED", StringComparison.OrdinalIgnoreCase) ||
        DispositionMessage.Equals("ISSUED", StringComparison.OrdinalIgnoreCase) ||
        !string.IsNullOrEmpty(BinarySecurityToken);

    public string? ErrorMessage { get; set; }
}
