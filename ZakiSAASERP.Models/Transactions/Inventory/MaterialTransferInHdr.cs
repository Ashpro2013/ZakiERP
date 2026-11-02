using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;
using System.Text.Json.Serialization;

namespace ZakiSAASERP.Models.Transactions.Inventory;

[Table("MaterialTransferInHdr")]
public class MaterialTransferInHdr
{
    [Key]
    public int MatTransInId { get; set; }

    [Required]
    public string VoucherNo { get; set; } = default!;

    [Required]
    public DateTime? VoucherDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Remarks { get; set; }

    public int FK_FromBranchId { get; set; }
    public int FK_ToBranchId { get; set; }

    public string? EnteredBy { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.Now;

    // Foreign Keys
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalQty { get; set; }

    public bool IsDeleted { get; set; } = false;

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

    // Child Collection
    public virtual List<MaterialTransferInDtls> MaterialTransferInDetails { get; set; } = new();
}
