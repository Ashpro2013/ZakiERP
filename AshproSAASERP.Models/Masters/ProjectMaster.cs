namespace AshproSAASERP.Models.Masters
{
    [Table("ProjectMaster")]
    public class ProjectMaster
    {
        [Key]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Code is required.")]
        [StringLength(100)]
        public string Code { get; set; } = default!;
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(500)]
        public string Name { get; set; } = default!;

        // --- Foreign Keys ---
        public int? FK_CustomerId { get; set; }
        public int? FK_SalesmanId { get; set; }
        public int? FK_PurchaserId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("FK_CustomerId")]
        public virtual CustomerMaster? Customer { get; set; }

        [ForeignKey("FK_SalesmanId")]
        public virtual SalesManMaster? Salesman { get; set; }

        [ForeignKey("FK_PurchaserId")]
        public virtual PurchaserMaster? Purchaser { get; set; }

        // --- Amounts ---
        public decimal? Amount { get; set; }
        public decimal? ContractValue { get; set; }

        // --- Description ---
        [StringLength(1000)]
        public string? Description { get; set; }

        // --- Dates ---
        public DateTime? ExpectedStrtDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualStrtDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        // --- Status ---
        [StringLength(10)]
        public string? Status { get; set; }
    }
}
