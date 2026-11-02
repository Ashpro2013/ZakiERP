using AshproSAASERP.ZatcaEngine.Enums;

namespace AshproSAASERP.ZatcaEngine.Core;

public class ZatcaOptions
{
    public EnvironmentType Environment { get; set; }
    public string VatNumber { get; set; } = "";
    public string PrivateKeyPem { get; set; } = "";
    public string CertificatePem { get; set; } = "";
    public string BinarySecurityToken { get; set; } = "";
    public string Secret { get; set; } = "";
    public int Icv { get; set; }
    public string PreviousInvoiceHash { get; set; } = "";

}

