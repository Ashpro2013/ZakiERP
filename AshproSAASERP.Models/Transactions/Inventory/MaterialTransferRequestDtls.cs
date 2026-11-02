using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.Transactions.Inventory;

[Table("MaterialTransferRequestDtls")]
public class MaterialTransferRequestDtls
{
    [Key]
    public int MatTransReqDtlsId { get; set; }

    public int FK_MatTransReqId { get; set; }

    public int FK_ItemId { get; set; }

    public int FK_UnitId { get; set; }

    public int FK_FromStoreId { get; set; } // Store to take stock from (if applicable, or just preferred store)

    public int? FK_ToStoreId { get; set; } // Destination store (assigned during approval)

    public decimal Quantity { get; set; }

    public decimal ApprovedQuantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ApprovedAmount { get; set; }

    public decimal Rate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string? ApproverComment { get; set; }

    // Navigation Properties
    [ForeignKey("FK_MatTransReqId")]
    public virtual MaterialTransferRequestHdr? Header { get; set; }

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
    [NotMapped] public decimal CurrentQty { get; set; } // Stock at From Branch
}
