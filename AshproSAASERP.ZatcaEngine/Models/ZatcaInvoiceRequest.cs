namespace AshproSAASERP.ZatcaEngine.Models;

public class ZatcaInvoiceRequest
{
    public Invoice Invoice { get; set; } = default!;

    public string PrivateKeyPem { get; set; } = string.Empty;

    public string CertificateBase64 { get; set; } = string.Empty;

    public string BinarySecurityToken { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public string Environment { get; set; } = "Sandbox";

    public string InvoiceTypeName { get; set; } = "0100000";
    public bool blnUploadXML { get; set; }
    public bool IsCompliance { get; set; }
}
