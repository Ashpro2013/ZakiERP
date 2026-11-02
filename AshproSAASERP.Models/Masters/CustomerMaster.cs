namespace AshproSAASERP.Models.Masters
{
    [Table("CustomerMaster")]
    public class CustomerMaster
    {
        [Key]
        public int CustomerId { get; set; }

        // ------------------------------
        // BASIC INFORMATION
        // ------------------------------
        [Required(ErrorMessage = "Customer code is required.")]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(2000)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? NameArabic { get; set; }

        // ------------------------------
        // ADDRESS
        // ------------------------------
        [StringLength(2000)]
        public string? Address { get; set; }

        [StringLength(2000)]
        public string? AddressArabic { get; set; }

        // ------------------------------
        // FINANCIALS
        // ------------------------------
        [Range(0, double.MaxValue, ErrorMessage = "Credit Limit must be a positive number.")]
        public decimal? CreditLimit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Credit Days must be a positive number.")]
        public decimal? CreditDays { get; set; }

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

        [StringLength(100)]
        public string? PhoneArabic { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^\d{0,15}$", ErrorMessage = "Mobile must be numeric.")]
        public string? Mobile { get; set; }

        [StringLength(100)]
        public string? MobileArabic { get; set; }

        [StringLength(500)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        // ------------------------------
        // VAT
        // ------------------------------
        [StringLength(500)]
        [RegularExpression(@"^\d{15}$", ErrorMessage = "VAT No must be exactly 15 digits.")]
        public string? VatNo { get; set; }

        [StringLength(100)]
        public string? VatNoArabic { get; set; }

        // ------------------------------
        // ZATCA ADDRESS FIELDS
        // ------------------------------
        [StringLength(100)]
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

        [StringLength(100)]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Postal Code must be exactly 5 digits.")]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? PostalCodeArabic { get; set; }

        [StringLength(200)]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Additional No must be exactly 4 digits.")]
        public string? AdditionalNo { get; set; }

        [StringLength(200)]
        public string? AdditionalNoArabic { get; set; }

        [StringLength(200)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Other Buyer ID must be exactly 10 digits.")]
        public string? OtherSellerNo { get; set; }

        [StringLength(200)]
        public string? OtherSellerNoArabic { get; set; }
        [StringLength(200)]
        public string? CountrySubentity { get; set; }
        [StringLength(200)]
        public string? CountrySubentityArabic { get; set; }

        // ------------------------------
        // IDENTIFICATION
        // ------------------------------
        [StringLength(200)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Identification No must be exactly 10 digits.")]
        public string? IdentificationNo { get; set; }

        [StringLength(200)]
        public string? IdentificationNoArabic { get; set; }

        [StringLength(200)]
        public string? Identification { get; set; } = "CRN";

        // ------------------------------
        // FOREIGN KEYS
        // ------------------------------
        public int? FK_CustomerGroupId { get; set; }
        public int? FK_CountryId { get; set; }
        public int? FK_SalesmanId { get; set; }
        public int? FK_AccountId { get; set; }

        // ------------------------------
        // NAVIGATION PROPERTIES
        // ------------------------------
        [ForeignKey(nameof(FK_CustomerGroupId))]
        public virtual CustomerGroupMaster? CustomerGroup { get; set; }

        [ForeignKey(nameof(FK_CountryId))]
        public virtual CountryMaster? Country { get; set; }

        [ForeignKey(nameof(FK_SalesmanId))]
        public virtual SalesManMaster? Salesman { get; set; }

        [ForeignKey(nameof(FK_AccountId))]
        public virtual AccountMaster? Account { get; set; }


        // ------------------------------
        // BILLING
        // ------------------------------
        [StringLength(50)]
        public string? CusBillType { get; set; }

        // ------------------------------
        // MISC FIELDS
        // ------------------------------
        public bool? NeedsFill { get; set; } = true;

        public bool IsSupplier { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public decimal? DiscountPercentage { get; set; }

        // ------------------------------
        // ZATCA EXTRA FIELDS
        // ------------------------------
        [StringLength(300)]
        public string? Neighbourhood { get; set; }

        [StringLength(300)]
        public string? NeighbourhoodArabic { get; set; }

        [StringLength(50)]
        public string? GroupVat { get; set; }

        [NotMapped]
        public List<CustomerLocationDetails> LocationDetails { get; set; } = new();

    }
}
