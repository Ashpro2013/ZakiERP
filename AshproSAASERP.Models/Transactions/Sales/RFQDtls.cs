using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.Transactions.Sales;

[Table("RFQDtls")]
public class RFQDtls
{
    [Key]
    public int RFQDtlsId { get; set; }

    public int FK_RFQId { get; set; }
    public int FK_ItemId { get; set; }
    public int? FK_UnitId { get; set; }
    public int? FK_StoreId { get; set; }

    public decimal Quantity { get; set; }
    public decimal QtyPerUnit { get; set; } = 1;

    [ForeignKey("FK_RFQId")]
    public virtual RFQHdr? RFQHdr { get; set; }

    [ForeignKey("FK_ItemId")]
    public virtual ItemMaster? Item { get; set; }

    [ForeignKey("FK_UnitId")]
    public virtual UnitMaster? Unit { get; set; }

    [ForeignKey("FK_StoreId")]
    public virtual StoreMaster? Store { get; set; }

    [NotMapped]
    public string? ItemName { get; set; }
    [NotMapped]
    public string? ItemCode { get; set; }
    [NotMapped]
    public string? ItemArabicName { get; set; }
}
