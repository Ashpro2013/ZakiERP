namespace ZakiSAASERP.Models.Masters
{
    [Table("SupplierMaster")]
    public class SupplierMaster
    {
        [Key]
        public int SupplierId { get; set; }

        // ------------------------------
        // BASIC INFORMATION
        // ------------------------------
        [Required(ErrorMessage = "Supplier code is required.")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Supplier Name is required.")]
        [StringLength(1000)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? NameArabic { get; set; }

        // ------------------------------
        // ADDRESS
        // ------------------------------
        [StringLength(2000)]
        public string? Address { get; set; }

        [StringLength(2000)]
        public string? AddressArabic { get; set; }

        // ------------------------------
        // CONTACT INFO
        // ------------------------------
        [StringLength(100)]
        public string? Contact { get; set; }

        [StringLength(100)]
        public string? ContactArabic { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^\d{0,15}$", ErrorMessage = "Telephone must be numeric.")]
        public string? Telephone { get; set; }

        [StringLength(300)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [StringLength(200)]
        [RegularExpression(@"^\d{0,15}$", ErrorMessage = "Mobile must be numeric.")]
        public string? Mobile { get; set; }

        [StringLength(100)]
        public string? PhoneArabic { get; set; }

        [StringLength(100)]
        public string? MobileArabic { get; set; }

        // ------------------------------
        // VAT
        // ------------------------------
        [StringLength(30)]
        [RegularExpression(@"^\d{15}$", ErrorMessage = "VAT No must be exactly 15 digits.")]
        public string? VatNo { get; set; }

        [StringLength(50)]
        public string? VatNoArabic { get; set; }

        // ------------------------------
        // FINANCIALS
        // ------------------------------
        public bool? NeedsFill { get; set; } = true;

        [StringLength(100)]
        public string? SupStatus { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Credit Limit must be a positive number.")]
        public decimal? CreditLimit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Credit Days must be a positive number.")]
        public decimal? CreditDays { get; set; }

        // ------------------------------
        // ZATCA / ADDRESS DETAILS
        // ------------------------------
        [StringLength(100)]
        public string? BuildingNo { get; set; }

        [StringLength(500)]
        public string? StreetName { get; set; }

        [StringLength(500)]
        public string? District { get; set; }

        [StringLength(500)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? AdditionalNo { get; set; }

        [StringLength(100)]
        public string? OtherSellerNo { get; set; }

        // Arabic Fields for ZATCA
        [StringLength(100)]
        public string? BuildingNoArabic { get; set; }

        [StringLength(500)]
        public string? StreetNameArabic { get; set; }

        [StringLength(500)]
        public string? DistrictArabic { get; set; }

        [StringLength(500)]
        public string? CityArabic { get; set; }

        [StringLength(100)]
        public string? PostalCodeArabic { get; set; }

        [StringLength(100)]
        public string? AdditionalNoArabic { get; set; }

        [StringLength(100)]
        public string? OtherSellerNoArabic { get; set; }

        // ------------------------------
        // FOREIGN KEYS
        // ------------------------------
        public int? FK_CountryId { get; set; }
        public int? FK_PurchaserId { get; set; }

        // ------------------------------
        // NAVIGATION PROPERTIES
        // ------------------------------
        [ForeignKey(nameof(FK_CountryId))]
        public virtual CountryMaster? Country { get; set; }

        [ForeignKey(nameof(FK_PurchaserId))]
        public virtual PurchaserMaster? Purchaser { get; set; }

        public int? FK_AccountId { get; set; }

        [ForeignKey(nameof(FK_AccountId))]
        public virtual AccountMaster? Account { get; set; }

        // ------------------------------
        // IDENTIFICATION
        // ------------------------------
        [StringLength(100)]
        public string? IdentificationNo { get; set; }

        [StringLength(100)]
        public string? Identification { get; set; } = "CRN";

        [StringLength(100)]
        public string? IdentificationNoArabic { get; set; }

        // ------------------------------
        // EXTRA
        // ------------------------------
        [StringLength(30)]
        public string? GroupVat { get; set; }

        [StringLength(500)]
        public string? Neighbourhood { get; set; }

        [StringLength(500)]
        public string? NeighbourhoodArabic { get; set; }

        [StringLength(30)]
        public string? SuppBillType { get; set; }

        [StringLength(100)]
        public string? SuppType { get; set; }

        public bool IsCustomer { get; set; } = false;
        public bool IsActive { get; set; } = true;

    }
}
