using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;
using System.Text.Json.Serialization;

namespace AshproSAASERP.Models.Transactions.Inventory;

[Table("InternalMaterialTransferHdr")]
public class InternalMaterialTransferHdr
{
    [Key]
    public int IntMatTransId { get; set; }

    [Required]
    public string VoucherNo { get; set; } = default!;

    [Required]
    public DateTime? VoucherDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Remarks { get; set; }

    [Required]
    public int FK_BranchId { get; set; } // The branch where transfer happens

    [Required]
    public int FK_FromStoreId { get; set; }

    [Required]
    public int FK_ToStoreId { get; set; }

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
    [ForeignKey("FK_BranchId")]
    [JsonIgnore]
    public virtual BranchMaster? Branch { get; set; }

    [ForeignKey("FK_FromStoreId")]
    [JsonIgnore]
    public virtual StoreMaster? FromStore { get; set; }

    [ForeignKey("FK_ToStoreId")]
    [JsonIgnore]
    public virtual StoreMaster? ToStore { get; set; }

    [ForeignKey("FK_UserId")]
    [JsonIgnore]
    public virtual UserDetails? User { get; set; }

    // Child Collection
    public virtual List<InternalMaterialTransferDtls> InternalMaterialTransferDetails { get; set; } = new();
}
