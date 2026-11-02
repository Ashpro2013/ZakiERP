namespace ZakiSAASERP.Models.DTO
{
    public class CustomerListDTO
    {
        public int SL { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameArabic { get; set; }
        public string? Address { get; set; }
        public string? AddressArabic { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? CreditDays { get; set; }
        public string? Contact { get; set; }
        public string? ContactArabic { get; set; }
        public string? Telephone { get; set; }
        public string? PhoneArabic { get; set; }
        public string? Mobile { get; set; }
        public string? MobileArabic { get; set; }
        public string? Mail { get; set; }
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
        public string? OtherSellerNo { get; set; }
        public string? OtherSellerNoArabic { get; set; }
        public string? CountrySubentity { get; set; }
        public string? CountrySubentityArabic { get; set; }
        public string? IdentificationNo { get; set; }
        public string? IdentificationNoArabic { get; set; }
        public string? Identification { get; set; }
        public string? CusBillType { get; set; }
        public bool? NeedsFill { get; set; }
        public bool IsSupplier { get; set; }
        public bool IsActive { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? Neighbourhood { get; set; }
        public string? NeighbourhoodArabic { get; set; }
        public string? GroupVat { get; set; }

        // Related Data Names
        public string? CustomerGroupName { get; set; }
        public string? CountryName { get; set; }
        public string? SalesmanName { get; set; }
    }
}
