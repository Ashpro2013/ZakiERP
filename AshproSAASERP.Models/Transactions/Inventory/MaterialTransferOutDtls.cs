using AshproSAASERP.Models.Masters;
using System.Text.Json.Serialization;

namespace AshproSAASERP.Models.Transactions.Inventory;

[Table("MaterialTransferOutDtls")]
public class MaterialTransferOutDtls
{
    [Key]
    public int MatTransOutDtlsId { get; set; }

    public int FK_MatTransOutId { get; set; }

    [NotMapped]
    public int SL { get; set; }

    public int FK_ItemId { get; set; }

    [NotMapped]
    public string? ItemName { get; set; }

    [NotMapped]
    public string? Barcode { get; set; }

    public int FK_FromStoreId { get; set; }

    public int FK_ToStoreId { get; set; }

    public int FK_UnitId { get; set; }

    [NotMapped]
    public string? UnitName { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal QtyPerUnit { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Rate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    // Navigation Properties
    [ForeignKey("FK_MatTransOutId")]
    [JsonIgnore]
    public virtual MaterialTransferOutHdr? Header { get; set; }

    [ForeignKey("FK_ItemId")]
    [JsonIgnore]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey("FK_UnitId")]
    [JsonIgnore]
    public virtual UnitMaster? Unit { get; set; }

    [ForeignKey("FK_FromStoreId")]
    [JsonIgnore]
    public virtual StoreMaster? FromStore { get; set; }

    [ForeignKey("FK_ToStoreId")]
    [JsonIgnore]
    public virtual StoreMaster? ToStore { get; set; }

    // Helper Properties for UI display (Non-Mapped)
    [NotMapped]
    public decimal FromBranchCurrentQty { get; set; }

    [NotMapped]
    public decimal ToBranchCurrentQty { get; set; }
}
