namespace AshproSAASERP.Models.Masters
{
    [Table("SalesManMaster")]
    public class SalesManMaster
    {
        [Key]
        public int SalesmanId { get; set; }

        [Required(ErrorMessage = "Salesman Code is required. *")]
        [StringLength(15)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Salesman Name is required. *")]
        [StringLength(500)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [StringLength(1000)]
        public string? NameArabic { get; set; }

        [StringLength(50)]
        public string? Designation { get; set; }

        [StringLength(100)]
        public string? Mobile { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        public byte[]? SignatureImage { get; set; }

        public byte[]? SealImage { get; set; }
        public byte[]? SalesmanImage { get; set; }
    }
}
