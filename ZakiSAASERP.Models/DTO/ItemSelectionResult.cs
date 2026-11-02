using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.DTO;

/// <summary>
/// Result DTO from item selection dialogs (ItemSearchDialog or ProductSearchDialog).
/// Contains all necessary data including pre-populated available units based on search mode.
/// </summary>
public class ItemSelectionResult
{
    // Item identification
    public int FK_ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? ItemNameArabic { get; set; }
    public string? Barcode { get; set; }
    public string? Location { get; set; }

    // Selected unit info
    public int FK_UnitId { get; set; }
    public string? UnitName { get; set; }
    public decimal QtyPerUnit { get; set; } = 1;
    public decimal? Rate { get; set; }
    public decimal? IncRate { get; set; }
    public decimal? Cost { get; set; }
    public decimal? StandardCost { get; set; }

    /// <summary>
    /// Available units for this item (pre-populated based on search mode).
    /// ProductSearchDialog: Only the selected unit.
    /// ItemSearchDialog: All units for the item.
    /// </summary>
    public List<CommonModel> AvailableUnits { get; set; } = new();

    /// <summary>
    /// MultiUnit details for advanced usage (e.g., switching units with full rate info).
    /// Only populated when ItemSearchMode == 1 (ItemCode mode).
    /// </summary>
    public List<ItemMultiUnit>? MultiUnitDetails { get; set; }

    #region Backward Compatibility Properties
    // These alias properties maintain compatibility with code that was using ItemMultiUnit

    /// <summary>
    /// Alias for ItemName - backward compatibility with ItemMultiUnit.MultiUnitName
    /// </summary>
    public string? MultiUnitName => ItemName;

    /// <summary>
    /// Alias for ItemNameArabic - backward compatibility with ItemMultiUnit.MultiUnitArabicName
    /// </summary>
    public string? MultiUnitArabicName => ItemNameArabic;

    /// <summary>
    /// Alias for QtyPerUnit - backward compatibility with ItemMultiUnit.QTYPerUnit
    /// </summary>
    public decimal? QTYPerUnit => QtyPerUnit;

    #endregion
}
