namespace AshproSAASERP.ZatcaEngine.Models;

public class ZatcaResult
{
    public string Status { get; set; } = "";
    public string InvoiceHash { get; set; } = "";
    public string SignedXml { get; set; } = "";
    public string QrBase64 { get; set; } = "";
    public string RawResponse { get; set; } = "";
}
