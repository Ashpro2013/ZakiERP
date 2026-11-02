using ZakiSAASERP.Models.Masters;
using System.Text.Json.Serialization;

namespace ZakiSAASERP.Models.Transactions.Inventory;

[Table("MaterialTransferInDtls")]
public class MaterialTransferInDtls
{
    [Key]
    public int MatTransInDtlsId { get; set; }

    public int FK_MatTransInId { get; set; }

    public int FK_ItemId { get; set; }

    public int FK_UnitId { get; set; }

    public int FK_FromStoreId { get; set; }

    public int FK_ToStoreId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Rate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    // Navigation Properties
    [ForeignKey("FK_MatTransInId")]
    [JsonIgnore]
    public virtual MaterialTransferInHdr? Header { get; set; }

    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    [ForeignKey("FK_FromStoreId")]
    public virtual StoreMaster? FromStore { get; set; }

    [ForeignKey("FK_ToStoreId")]
    public virtual StoreMaster? ToStore { get; set; }

    [NotMapped] public int SL { get; set; } = 1;
    [NotMapped] public string? Barcode { get; set; }
    [NotMapped] public string? ItemName { get; set; }
    [NotMapped] public string? UnitName { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal QtyPerUnit { get; set; }
    [NotMapped] public decimal FromBranchCurrentQty { get; set; }
    [NotMapped] public decimal ToBranchCurrentQty { get; set; }
}
