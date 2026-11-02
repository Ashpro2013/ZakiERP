namespace AshproSAASERP.ZatcaEngine.Models;

public class CsrResult
{
    public bool IsSuccess { get; set; }

    public string? CsrPem { get; set; }

    public string? CsrBase64 { get; set; }

    public string? PrivateKeyPem { get; set; }

    public string? BinarySecurityToken { get; set; }

    public string? Secret { get; set; }

    public string? RequestId { get; set; }

    public string? ErrorMessage { get; set; }
}
