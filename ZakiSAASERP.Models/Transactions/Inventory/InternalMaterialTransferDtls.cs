using ZakiSAASERP.Models.Masters;
using System.Text.Json.Serialization;

namespace ZakiSAASERP.Models.Transactions.Inventory;

[Table("InternalMaterialTransferDtls")]
public class InternalMaterialTransferDtls
{
    [Key]
    public int IntMatTransDtlsId { get; set; }

    public int FK_IntMatTransId { get; set; }

    public int FK_ItemId { get; set; }

    public int FK_UnitId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Rate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    // Navigation Properties
    [ForeignKey("FK_IntMatTransId")]
    [JsonIgnore]
    public virtual InternalMaterialTransferHdr? Header { get; set; }

    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    [NotMapped] public int SL { get; set; } = 1;
    [NotMapped] public string? Barcode { get; set; }
    [NotMapped] public string? ItemName { get; set; }
    [NotMapped] public string? UnitName { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal QtyPerUnit { get; set; }
    [NotMapped] public decimal FromStoreCurrentQty { get; set; }
    [NotMapped] public decimal ToStoreCurrentQty { get; set; }
}
