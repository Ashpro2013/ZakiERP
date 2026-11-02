using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.Transactions.Inventory;

[Table("StockAdjustmentHdr")]
public class StockAdjustmentHdr
{
    [Key]
    public int StockAdjId { get; set; }

    [Required]
    public string VoucherNo { get; set; } = default!;

    [Required]
    public DateTime? VoucherDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Remarks { get; set; }

    public string? EnteredBy { get; set; }

    // Foreign Keys
    public int FK_BranchId { get; set; }
    public int FK_UserId { get; set; }
    public int FK_FinancialYearId { get; set; }


    public DateTime EntryDate { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalQty { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    [ForeignKey("FK_BranchId")]
    public virtual BranchMaster? Branch { get; set; }

    [ForeignKey("FK_UserId")]
    public virtual UserDetails? User { get; set; }



    // Child Collection
    public virtual List<StockAdjustmentDtls> StockAdjustmentDetails { get; set; } = new();
}
