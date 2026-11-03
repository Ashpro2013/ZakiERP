using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.DTO
{
    public class CSRRequest
    {
        [Required(ErrorMessage = "OPT Required")]
        public string OTP { get; set; } = string.Empty;
        [Required(ErrorMessage = "Common Name Required")]
        public string CommonName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Organization Required")]
        public string Organization { get; set; } = string.Empty;
        [Required(ErrorMessage = "Unit Name Required")]
        public string UnitName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Country Code Required")]
        public string CountryCode { get; set; } = "SA";
        [Required(ErrorMessage = "Serial Number Required")]
        public string SerialNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Organization Identifier Required")]
        public string OrganizationIdentifier { get; set; } = string.Empty;
        [Required(ErrorMessage = "Location Required")]
        public string Location { get; set; } = string.Empty;
        [Required(ErrorMessage = "Business Category Required")]
        public string BusinessCategory { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        [Required(ErrorMessage = "Zatca Mode Required")]
        public string CSRGenerationType { get; set; } = "Production";
        public CompanyDetails company { get; set; }
    }

}
