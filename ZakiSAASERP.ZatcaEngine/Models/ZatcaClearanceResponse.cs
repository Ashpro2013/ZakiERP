namespace ZakiSAASERP.ZatcaEngine.Models;

public class ZatcaApiStructuredResponse
{
    public string? clearanceStatus { get; set; }

    public string? reportingStatus { get; set; }

    public string? invoiceHash { get; set; }

    public string? uuid { get; set; }
    public string? clearedInvoice { get; set; }
    public ValidationResults? validationResults { get; set; }
}

public class ValidationResults
{
    public string? status { get; set; }

    public List<ZatcaMessage>? infoMessages { get; set; }

    public List<ZatcaMessage>? warningMessages { get; set; }

    public List<ZatcaMessage>? errorMessages { get; set; }
}

public class ZatcaMessage
{
    public string? code { get; set; }

    public string? message { get; set; }

    public string? status { get; set; }
}
