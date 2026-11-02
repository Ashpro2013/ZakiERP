using ZakiSAASERP.Models.Masters;
using System.Text.Json.Serialization;

namespace ZakiSAASERP.Models.Transactions.Workshop
{
    [Table("JobCardHdr")]
    public class JobCardHdr
    {
        [Key]
        public int JobCardId { get; set; }

        [Required]
        [StringLength(50)]
        public string VoucherNo { get; set; } = default!;

        [Required]
        public DateTime? VoucherDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime? PromisedDate { get; set; } = DateTime.Now;

        // Job Order Reference (Optional)
        public int? FK_JobOrderId { get; set; }

        public string? JobOrderNo { get; set; }

        // Vehicle Details (Stored for History/Snapshot)
        public int FK_VehicleId { get; set; }

        [StringLength(50)]
        public string? PlateNo { get; set; }

        [StringLength(100)]
        public string? ChassisNo { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        public int? Year { get; set; }

        [StringLength(50)]
        public string? Kilometer { get; set; }

        public string? VehicleInfo { get; set; }

        // Customer Details
        public int FK_CustomerId { get; set; }

        [StringLength(200)]
        public string? CustomerName { get; set; }

        [NotMapped]
        public string? CustomerCode { get; set; }

        [StringLength(50)]
        public string? ContactNo { get; set; }

        // Job Card Specifics
        [StringLength(100)]
        public string? DeliveredBy { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }

        public int? FK_SalesManId { get; set; }

        public DateTime? TimeIn { get; set; }

        [StringLength(100)]
        public string? QuotationNo { get; set; } // Reference to Quoation if any

        [StringLength(50)]
        public string? Status { get; set; } = "Pending";

        // Financials (Items Total)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountTotal { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalVat { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrandTotal { get; set; }

        // Audit & System
        public int FK_BranchId { get; set; }
        public int FK_FinancialYearId { get; set; }
        public int FK_UserId { get; set; }

        [StringLength(50)]
        public string? EnteredBy { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        [ForeignKey("FK_JobOrderId")]
        [JsonIgnore]
        public virtual JobOrderHdr? JobOrder { get; set; }

        [ForeignKey("FK_SalesManId")]
        [JsonIgnore]
        public virtual SalesManMaster? SalesMan { get; set; }

        public virtual List<JobCardDtls> JobCardDetails { get; set; } = new();
    }
}
