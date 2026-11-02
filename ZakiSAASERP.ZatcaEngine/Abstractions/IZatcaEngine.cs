using ZakiSAASERP.ZatcaEngine.Models;

namespace ZakiSAASERP.ZatcaEngine.Abstractions;

public interface IZatcaEngine
{
    Task<CsrResult> GenerateCsrAsync(CsrRequest request);

    Task<ProductionCertificateResult> RequestProductionCsidAsync(
        string complianceRequestId,
        string complianceBinarySecurityToken,
        string complianceSecret,
        string environment);

    Task<ZatcaSubmissionResult> SubmitInvoiceAsync(
        ZatcaInvoiceRequest request);
}