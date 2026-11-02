namespace ZakiSAASERP.Models.DTO;

/// <summary>
/// DTO for ItemSearchDialog that includes current stock balance
/// </summary>
public class ItemDialogDTO
{
    public int ItemId { get; set; }
    public string? Barcode { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? ItemNameArabic { get; set; }
    public decimal? SalesRate { get; set; }
    public decimal? PurchaseRate { get; set; }
    public string? StockMode { get; set; }
    public int? FK_UnitId { get; set; }
    public decimal? InclusiveRate { get; set; }
    public decimal? StandardCost { get; set; }
    /// <summary>
    /// Current stock quantity in base unit for the specified branch
    /// </summary>
    public decimal CurrentStock { get; set; }
}

/// <summary>
/// DTO for ProductSearchDialog that includes current stock balance
/// </summary>
public class ItemMultiUnitDialogDTO
{
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
    /// <summary>
    /// Current stock quantity in base unit for the specified branch
    /// </summary>
    public decimal CurrentStock { get; set; }
}
