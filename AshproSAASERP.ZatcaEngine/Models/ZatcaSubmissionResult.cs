namespace AshproSAASERP.ZatcaEngine.Models;

public class ZatcaSubmissionResult
{
    public bool IsSuccess { get; set; }

    public string? InvoiceHash { get; set; }

    public string? QrCode { get; set; }

    public string? RawResponse { get; set; }
    public string? SignedXml { get; set; }

    public string? ClearanceStatus { get; set; }

    public string? ReportingStatus { get; set; }

    public string? ErrorMessage { get; set; }
}
