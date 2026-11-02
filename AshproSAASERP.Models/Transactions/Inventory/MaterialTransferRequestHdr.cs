using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;
using System.Text.Json.Serialization;

namespace AshproSAASERP.Models.Transactions.Inventory;

[Table("MaterialTransferRequestHdr")]
public class MaterialTransferRequestHdr
{
    [Key]
    public int MatTransReqId { get; set; }

    [Required]
    public string VoucherNo { get; set; } = default!;

    [Required]
    public DateTime? VoucherDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Remarks { get; set; }

    public int FK_FromBranchId { get; set; }
    public int FK_ToBranchId { get; set; }

    public string? Status { get; set; } = "Opened"; // Opened, Approved, Rejected, Processed

    public string? EnteredBy { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.Now;

    // Foreign Keys
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalQty { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalApprovedAmount { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Approval Tracking Fields
    public int? FK_ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public int? FK_RejectedBy { get; set; }
    public DateTime? RejectedDate { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation Properties
    [ForeignKey("FK_FromBranchId")]
    [JsonIgnore]
    public virtual BranchMaster? FromBranch { get; set; }

    [ForeignKey("FK_ToBranchId")]
    [JsonIgnore]
    public virtual BranchMaster? ToBranch { get; set; }

    [ForeignKey("FK_UserId")]
    [JsonIgnore]
    public virtual UserDetails? User { get; set; }

    [ForeignKey("FK_ApprovedBy")]
    [JsonIgnore]
    public virtual UserDetails? ApproverUser { get; set; }

    [ForeignKey("FK_RejectedBy")]
    [JsonIgnore]
    public virtual UserDetails? RejectorUser { get; set; }

    // Child Collection
    public virtual List<MaterialTransferRequestDtls> MaterialTransferRequestDetails { get; set; } = new();
}
