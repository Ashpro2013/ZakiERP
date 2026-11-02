namespace ZakiSAASERP.ZatcaEngine.Models;

public class OnboardingResult
{
    public string PrivateKeyPem { get; set; } = "";

    public string ComplianceBinarySecurityToken { get; set; } = "";
    public string ComplianceSecret { get; set; } = "";

    public string ProductionBinarySecurityToken { get; set; } = "";
    public string ProductionSecret { get; set; } = "";

    public string ProductionCertificate { get; set; } = "";
}
