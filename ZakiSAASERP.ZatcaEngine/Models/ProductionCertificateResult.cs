using System.Text.Json.Serialization;

namespace ZakiSAASERP.ZatcaEngine.Models;

public class ProductionCertificateResult
{
    [JsonPropertyName("requestID")]
    public object? RawRequestID { get; set; }

    [JsonIgnore]
    public string RequestID => RawRequestID?.ToString() ?? "";

    [JsonPropertyName("binarySecurityToken")]
    public string? RawBinarySecurityToken { get; set; }

    [JsonPropertyName("issuedCertificate")]
    public string BinarySecurityToken
    {
        get => !string.IsNullOrEmpty(_binarySecurityToken) ? _binarySecurityToken : (RawBinarySecurityToken ?? "");
        set => _binarySecurityToken = value;
    }
    private string _binarySecurityToken = "";

    [JsonPropertyName("secret")]
    public string? RawSecret { get; set; }

    [JsonPropertyName("apiSecret")]
    public string Secret
    {
        get => !string.IsNullOrEmpty(_secret) ? _secret : (RawSecret ?? "");
        set => _secret = value;
    }
    private string _secret = "";

    [JsonPropertyName("dispositionStatus")]
    public string DispositionStatus { get; set; } = "";

    [JsonPropertyName("dispositionMessage")]
    public string DispositionMessage { get; set; } = "";

    public bool IsSuccess =>
        DispositionStatus.Equals("ISSUED", StringComparison.OrdinalIgnoreCase) ||
        DispositionMessage.Equals("ISSUED", StringComparison.OrdinalIgnoreCase) ||
        !string.IsNullOrEmpty(BinarySecurityToken);

    public string? ErrorMessage { get; set; }
}
