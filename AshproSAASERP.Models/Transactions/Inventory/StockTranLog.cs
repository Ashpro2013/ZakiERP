using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.Transactions.Inventory;

[Table("StockTranLog")]
public class StockTranLog
{
    [Key]
    public int STranId { get; set; }

    [Required]
    public int FK_ItemId { get; set; }

    [Required]
    public int VchrId { get; set; }

    [Required]
    [MaxLength(100)]
    public string VchrNo { get; set; } = string.Empty;

    public DateTime? VchrDate { get; set; }

    [MaxLength(50)]
    public string? VchrType { get; set; }

    [MaxLength(50)]
    public string? VchrFormType { get; set; }

    public int? AccountId { get; set; }

    public int? FK_StoreId { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal? InQuantity { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal? OutQuantity { get; set; }

    public int? FK_UnitId { get; set; }

    [MaxLength(400)]
    public string? Narration { get; set; }

    public int? FK_UserId { get; set; }

    public DateTime? TranDate { get; set; }

    public int? FK_BranchId { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal? InRate { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal? OutRate { get; set; }

    public int? FK_ActualUnitId { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal? ActualUnitQty { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal? ActualRate { get; set; }

    [NotMapped]
    public int? FK_RelatedId { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(FK_ItemId))]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey(nameof(FK_UnitId))]
    public virtual UnitMaster? Unit { get; set; }

    [ForeignKey(nameof(FK_ActualUnitId))]
    public virtual UnitMaster? ActualUnit { get; set; }

    [ForeignKey(nameof(FK_UserId))]
    public virtual UserDetails? User { get; set; }

    [ForeignKey(nameof(FK_BranchId))]
    public virtual BranchMaster? Branch { get; set; }

    [ForeignKey(nameof(FK_StoreId))]
    public virtual StoreMaster? Store { get; set; }
}
