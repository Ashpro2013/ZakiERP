namespace ZakiSAASERP.Models.Masters;

[Table("ItemMaster")]
public class ItemMaster
{
    [Key]
    public int ItemId { get; set; }
    [Required(ErrorMessage = "Item Code is required")]
    public string? ItemCode { get; set; }
    [Required(ErrorMessage = "Item Name is required")]
    public string? ItemName { get; set; }
    public string? ItemNameArabic { get; set; }
    [Required(ErrorMessage = "Unit is required")]
    public int? FK_UnitId { get; set; }
    public decimal? SalesRate { get; set; }
    public decimal? PurchaseRate { get; set; }
    public decimal? WholeSaleRate { get; set; }
    public decimal? MinSaleRate { get; set; }
    public int? FK_BrandId { get; set; }
    public int? FK_CategoryId { get; set; }
    public string? Location { get; set; }
    public decimal? StandardCost { get; set; }
    public string? StockMode { get; set; } = "Inventory";
    public string? VatCategory { get; set; } = "S";
    public string? VatExcCode { get; set; }
    public string? VatExcName { get; set; }
    public string? Notes { get; set; }
    public string? Model { get; set; }
    public string? Fitted { get; set; }
    public string? ItemImageName { get; set; }
    public byte[]? ItemImage { get; set; }
    public string? ItemExpiry { get; set; }
    public string? ItemType { get; set; } = "Normal";
    public string? Barcode { get; set; }
    public string? WeightPerCarton { get; set; }
    public decimal? LandingCost { get; set; }
    public bool? IsActive { get; set; } = true;
    public decimal? InclusiveRate { get; set; }
    public int? FK_SubCategoryId { get; set; }
    public int? FK_SupplierId { get; set; }
    public int? FK_VatId { get; set; }
    public bool IsSaleable { get; set; } = true;

    [ForeignKey(nameof(FK_UnitId))]
    public virtual UnitMaster? Unit { get; set; }

    [ForeignKey(nameof(FK_CategoryId))]
    public virtual CategoryMaster? Category { get; set; }

    [ForeignKey(nameof(FK_SubCategoryId))]
    public virtual SubCategoryMaster? SubCategory { get; set; }

    [ForeignKey(nameof(FK_BrandId))]
    public virtual BrandMaster? Brand { get; set; }

    [ForeignKey(nameof(FK_VatId))]
    public virtual VatMaster? Vat { get; set; }

    [ForeignKey(nameof(FK_SupplierId))]
    public virtual SupplierMaster? Supplier { get; set; }

    [InverseProperty("ItemMaster")]
    public List<ItemMultiUnit> MultiUnits { get; set; } = new();


}
