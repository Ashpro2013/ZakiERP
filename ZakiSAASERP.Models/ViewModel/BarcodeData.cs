
namespace ZakiSAASERP.Models.ViewModel;

public class BarcodeData
{
    public int ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? ItemNameArabic { get; set; }
    public decimal? SalesRate { get; set; }
    public decimal? PurchaseRate { get; set; }
    public decimal? WholeSaleRate { get; set; }
    public decimal? MinSaleRate { get; set; }
    public string? Location { get; set; }
    public decimal? StandardCost { get; set; }
    public string? StockMode { get; set; } = "Inventory";
    public string? Notes { get; set; }
    public string? Model { get; set; }
    public string? Fitted { get; set; }
    public string? ItemImageName { get; set; }
    public string? ItemExpiry { get; set; }
    public string? ItemType { get; set; } = "Normal";
    public string? Barcode { get; set; }
    public string? WeightPerCarton { get; set; }
    public decimal? LandingCost { get; set; }
    public decimal? InclusiveRate { get; set; }
    public string? BrandName { get; set; }
    public string? UnitName { get; set; }
    public string? CategoryName { get; set; }
    public string? SubCategoryName { get; set; }
    public string? SupplierName { get; set; }
    [NotMapped]
    public int Quantity { get; set; }
    [NotMapped]
    public string? Label { get; set; }
    [NotMapped]
    public string? BarcodeLabel
    {
        get => Barcode;
        set => Barcode = value;
    }
}

