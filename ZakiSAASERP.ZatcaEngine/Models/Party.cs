namespace ZakiSAASERP.ZatcaEngine.Models;

public class Party
{
    public string Name { get; set; } = "";
    public string VatNumber { get; set; } = "";

    public string IdentificationNumber { get; set; } = "";
    public string IdentificationScheme { get; set; } = "CRN"; // CRN, NAT, Iqama, etc.

    public string Street { get; set; } = "";
    public string BuildingNumber { get; set; } = "";
    public string City { get; set; } = "";
    public string District { get; set; } = "";
    public string Subdivision { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string CountryCode { get; set; } = "SA";
}
