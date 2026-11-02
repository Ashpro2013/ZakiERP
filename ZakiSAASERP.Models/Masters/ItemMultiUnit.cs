namespace ZakiSAASERP.Models.Masters;

[Table("ItemMultiUnit")]
public class ItemMultiUnit
{
    [Key]
    public int MultiUnitId { get; set; }
    public int FK_ItemId { get; set; }
    public int FK_UnitId { get; set; }
    public decimal? QTYPerUnit { get; set; }
    public decimal? Rate { get; set; }
    public decimal? IncRate { get; set; }
    public string? Barcode { get; set; }
    public decimal? Cost { get; set; }
    public string? MultiUnitName { get; set; }
    public string? MultiUnitArabicName { get; set; }

    [ForeignKey(nameof(FK_ItemId))]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ItemMaster? ItemMaster { get; set; }

    [ForeignKey(nameof(FK_UnitId))]
    public virtual UnitMaster? Unit { get; set; }

    [NotMapped]
    public string? ItemCode { get; set; }
    [NotMapped]
    public string? ItemName { get; set; }
    [NotMapped]
    public string? ItemNameArabic { get; set; }
    [NotMapped]
    public string? CategoryName { get; set; }
    [NotMapped]
    public string? UnitName { get; set; }
}
