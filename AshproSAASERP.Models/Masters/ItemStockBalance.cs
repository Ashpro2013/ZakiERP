namespace AshproSAASERP.Models.Masters;

[Table("ItemStockBalance")]
public class ItemStockBalance
{
    [Key]
    public int ItemStockBalanceId { get; set; }

    [Required]
    public int FK_ItemId { get; set; }

    [Required]
    public int FK_BranchId { get; set; }

    [Column(TypeName = "decimal(24, 4)")]
    public decimal CurrentQty { get; set; } = 0;

    public DateTime LastUpdated { get; set; } = DateTime.Now;

    // Navigation Properties
    [ForeignKey(nameof(FK_ItemId))]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey(nameof(FK_BranchId))]
    public virtual BranchMaster? Branch { get; set; }
}
