namespace AshproSAASERP.Models.Masters
{
    [Table("PurchaserMaster")]
    public class PurchaserMaster
    {
        [Key]
        public int PurchaserId { get; set; }

        [Required(ErrorMessage = "Purchaser Code is required. *")]
        [StringLength(50)]
        public string Code { get; set; } = default!;
        [Required(ErrorMessage = "Purchaser Name is required. *")]
        [StringLength(500)]
        public string Name { get; set; } = default!;

        [StringLength(500)]
        public string? NameArabic { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal CreditLimit { get; set; } = 0;
        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
