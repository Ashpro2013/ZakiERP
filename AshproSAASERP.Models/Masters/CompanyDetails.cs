namespace AshproSAASERP.Models.Masters
{
    [Table("CompanyDetails")]
    public class CompanyDetails
    {
        [Key]
        public int CompanyDetailId { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Branch is required.")]
        public int FK_BranchId { get; set; }

        // ------------------------------
        // BASIC INFORMATION
        // ------------------------------
        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(2000)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string? NameArabic { get; set; }

        // ------------------------------
        // CONTACT INFO
        // ------------------------------
        [StringLength(100)]
        public string? Contact { get; set; }

        [StringLength(100)]
        public string? ContactArabic { get; set; }

        [StringLength(100)]
        public string? Telephone { get; set; }

        [StringLength(100)]
        public string? TelephoneArabic { get; set; }

        [StringLength(100)]
        public string? Fax { get; set; }

        [StringLength(100)]
        public string? FaxArabic { get; set; }

        [StringLength(100)]
        public string? Mobile { get; set; }

        [StringLength(100)]
        public string? MobileArabic { get; set; }

        [StringLength(500)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        // ------------------------------
        // VAT INFORMATION
        // ------------------------------
        [StringLength(15)]
        [RegularExpression(@"^3\d{13}3$", ErrorMessage = "VAT No must be 15 digits, starting and ending with 3.")]
        public string? VatNo { get; set; }

        [StringLength(100)]
        public string? VatNoArabic { get; set; }

        // ------------------------------
        // ZATCA ADDRESS FIELDS
        // ------------------------------
        [StringLength(4)]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Building No must be exactly 4 digits.")]
        public string? BuildingNo { get; set; }

        [StringLength(100)]
        public string? BuildingNoArabic { get; set; }

        [StringLength(300)]
        public string? StreetName { get; set; }

        [StringLength(300)]
        public string? StreetNameArabic { get; set; }

        [StringLength(300)]
        public string? District { get; set; }

        [StringLength(300)]
        public string? DistrictArabic { get; set; }

        [StringLength(300)]
        public string? City { get; set; }

        [StringLength(300)]
        public string? CityArabic { get; set; }

        [StringLength(5)]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Postal Code must be exactly 5 digits.")]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? PostalCodeArabic { get; set; }

        [StringLength(4)]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Additional No must be exactly 4 digits.")]
        public string? AdditionalNo { get; set; }

        [StringLength(200)]
        public string? AdditionalNoArabic { get; set; }
        [StringLength(200)]
        public string? CountrySubentity { get; set; }
        [StringLength(200)]
        public string? CountrySubentityArabic { get; set; }

        // ------------------------------
        // COUNTRY INFORMATION
        // ------------------------------
        public int? FK_CountryId { get; set; }

        // ------------------------------
        // OTHER SELLER INFORMATION
        // ------------------------------
        [StringLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Other Seller ID must be exactly 10 digits.")]
        public string? OtherSellerId { get; set; }

        [StringLength(200)]
        public string? OtherSellerIdArabic { get; set; }

        // ------------------------------
        // IDENTIFICATION
        // ------------------------------
        [StringLength(50)]
        public string? IdentificationType { get; set; } = "CRN";

        [StringLength(200)]
        public string? IdentificationNo { get; set; }

        [StringLength(200)]
        public string? IdentificationNoArabic { get; set; }

        [StringLength(50)]
        public string? GroupVat { get; set; }
        public byte[]? HeaderImage { get; set; }
        public byte[]? FooterImage { get; set; }
        // ------------------------------
        // NAVIGATION PROPERTIES
        // ------------------------------
        [ForeignKey(nameof(FK_BranchId))]
        public virtual BranchMaster? Branch { get; set; }

        [ForeignKey(nameof(FK_CountryId))]
        public virtual CountryMaster? Country { get; set; }
    }
}
