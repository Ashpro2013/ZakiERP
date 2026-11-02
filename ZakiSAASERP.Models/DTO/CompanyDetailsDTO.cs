namespace ZakiSAASERP.Models.DTO
{
    public class CompanyDetailsDTO
    {
        public int CompanyDetailId { get; set; }
        public int FK_BranchId { get; set; }
        public required string Name { get; set; }
        public string? NameArabic { get; set; }
        public string? Contact { get; set; }
        public string? ContactArabic { get; set; }
        public string? Telephone { get; set; }
        public string? TelephoneArabic { get; set; }
        public string? Fax { get; set; }
        public string? FaxArabic { get; set; }
        public string? Mobile { get; set; }
        public string? MobileArabic { get; set; }
        public string? Email { get; set; }
        public string? VatNo { get; set; }
        public string? VatNoArabic { get; set; }
        public string? BuildingNo { get; set; }
        public string? BuildingNoArabic { get; set; }
        public string? StreetName { get; set; }
        public string? StreetNameArabic { get; set; }
        public string? District { get; set; }
        public string? DistrictArabic { get; set; }
        public string? City { get; set; }
        public string? CityArabic { get; set; }
        public string? PostalCode { get; set; }
        public string? PostalCodeArabic { get; set; }
        public string? AdditionalNo { get; set; }
        public string? AdditionalNoArabic { get; set; }
        public int? FK_CountryId { get; set; }
        public string? OtherSellerId { get; set; }
        public string? OtherSellerIdArabic { get; set; }
        public string? IdentificationType { get; set; }
        public string? IdentificationNo { get; set; }
        public string? IdentificationNoArabic { get; set; }
        public string? GroupVat { get; set; }
    }
}
