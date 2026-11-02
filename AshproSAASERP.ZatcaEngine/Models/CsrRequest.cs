namespace AshproSAASERP.ZatcaEngine.Models;

public class CsrRequest
{
    public string OrganizationName { get; set; } = string.Empty;

    public string OrganizationIdentifier { get; set; } = string.Empty;

    public string CommonName { get; set; } = string.Empty;

    public string CountryCode { get; set; } = "SA";

    public string SerialNumber { get; set; } = string.Empty;

    public string OTP { get; set; } = string.Empty;

    public string OrganizationUnitName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string BusinessCategory { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = "1100";
    public string Environment { get; set; } = "Sandbox";
    // Sandbox / Simulation / Production

    public Party? Seller { get; set; }
    public Party? Buyer { get; set; }
}
