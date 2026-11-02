using ZakiSAASERP.Models.Masters;
using System.Text.Json.Serialization;

namespace ZakiSAASERP.Models.Transactions.Workshop
{
    [Table("JobOrderHdr")]
    public class JobOrderHdr
    {
        [Key]
        public int JobOrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string VoucherNo { get; set; } = default!;

        [Required]
        public DateTime? VoucherDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime? PromisedDate { get; set; } = DateTime.Now;

        // Vehicle Details
        public int FK_VehicleId { get; set; }

        [NotMapped]
        public string? PlateNo { get; set; } // Searchable

        [NotMapped]
        public string? ChassisNo { get; set; }

        [NotMapped]
        public string? Model { get; set; }

        [NotMapped]
        public int? Year { get; set; }

        [StringLength(50)]
        public string? Kilometer { get; set; }

        // Customer Details
        public int FK_CustomerId { get; set; }

        [NotMapped]
        public string? CustomerName { get; set; }

        [NotMapped]
        public string? CustomerCode { get; set; }

        [StringLength(50)]
        public string? ContactNo { get; set; }

        // Job Details
        [StringLength(100)]
        public string? DeliveredBy { get; set; }

        public DateTime? TimeIn { get; set; } // Can use DateTime or TimeSpan, UI shows 11:17

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } = "Opened";

        public int? FK_SalesManId { get; set; }

        [StringLength(500)]
        public string? VehicleInfo { get; set; }

        public string? Comments { get; set; }

        [StringLength(500)]
        public string? IstimaraPath { get; set; }

        // Financials
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

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
        [ForeignKey("FK_VehicleId")]
        [JsonIgnore]
        public virtual VehicleMaster? Vehicle { get; set; }

        [ForeignKey("FK_CustomerId")]
        [JsonIgnore]
        public virtual CustomerMaster? Customer { get; set; }

        [ForeignKey("FK_SalesManId")]
        [JsonIgnore]
        public virtual SalesManMaster? SalesMan { get; set; }

        public virtual List<JobOrderDtls> JobOrderDetails { get; set; } = new();
    }
}
