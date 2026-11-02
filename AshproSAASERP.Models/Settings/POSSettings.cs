namespace AshproSAASERP.Models.Settings;

public class POSSettings
{
    #region Grid Columns Visibility
    public bool ShowItemCode { get; set; } = true;
    public bool ShowBarcode { get; set; } = true;
    public bool ShowItemName { get; set; } = true;
    public bool ShowArabicName { get; set; } = false;
    public bool ShowUnit { get; set; } = true;
    public bool ShowExRate { get; set; } = true;
    public bool ShowIncRate { get; set; } = false;
    public bool ShowGross { get; set; } = true;
    public bool ShowDiscPercentage { get; set; } = true;
    public bool ShowDiscAmount { get; set; } = true;
    public bool ShowTaxableAmount { get; set; } = false;
    public bool ShowTaxPercentage { get; set; } = true;
    public bool ShowTaxAmount { get; set; } = true;
    public bool ShowAmount { get; set; } = true;
    #endregion

    #region Layout & Sizing
    // Category Panel
    public int CategoryPanelWidth { get; set; } = 108; // px (range: 70 - 240)
    public int CategoryButtonHeight { get; set; } = 64; // px (range: 45 - 120)
    public int CategoryIconSize { get; set; } = 36; // px (range: 20 - 60)
    public int CategoryFontSize { get; set; } = 12; // px (range: 9 - 16)
    public bool ShowCategoryImages { get; set; } = true;

    // Split / Heights
    public int ProductAreaHeight { get; set; } = 45; // % of POS left area (range: 25 - 75)

    // Product Buttons / Cards
    public int ProductButtonWidth { get; set; } = 136; // px min width (range: 90 - 260)
    public int ProductButtonHeight { get; set; } = 148; // px min height (range: 90 - 260)
    public int ProductFontSize { get; set; } = 13; // px (range: 10 - 18)
    public int ProductPriceFontSize { get; set; } = 13; // px (range: 10 - 18)
    public bool ShowProductImages { get; set; } = true;

    // Product Pagination
    public bool EnableProductPagination { get; set; } = true;
    private int _productPageSize = 12;
    public int ProductPageSize
    {
        get => _productPageSize <= 0 ? 12 : _productPageSize;
        set => _productPageSize = value <= 0 ? 12 : value;
    }
    #endregion

    #region Grid Inline Field Editing
    public bool AllowEditExcRate { get; set; } = true;
    public bool AllowEditIncRate { get; set; } = true;
    public bool AllowEditDiscPer { get; set; } = true;
    public bool AllowEditDiscAmount { get; set; } = true;

    // Backward-compatible aliases
    [System.Text.Json.Serialization.JsonIgnore]
    public bool AllowEditExRate { get => AllowEditExcRate; set => AllowEditExcRate = value; }
    [System.Text.Json.Serialization.JsonIgnore]
    public bool AllowDirectEditRate { get => AllowEditExcRate; set => AllowEditExcRate = value; }
    [System.Text.Json.Serialization.JsonIgnore]
    public bool AllowDirectEditDiscount { get => AllowEditDiscPer; set => AllowEditDiscPer = value; }
    #endregion

    #region Behavior & General Settings
    public bool ShowNumPadByDefault { get; set; } = true;
    public bool AutoFocusBarcode { get; set; } = true;
    public bool AutoPrintBill { get; set; } = true;
    public bool ReadonlyInvoiceNumber { get; set; } = true;
    public string DefaultPaymentMode { get; set; } = "Cash"; // "Cash" or "Bank"
    public string GridRowDensity { get; set; } = "Dense"; // "Dense" or "Normal"

    // Backward-compatible alias
    [System.Text.Json.Serialization.JsonIgnore]
    public bool ReadOnlyInvoiceNumber { get => ReadonlyInvoiceNumber; set => ReadonlyInvoiceNumber = value; }
    #endregion

    public POSSettings Clone()
    {
        return (POSSettings)this.MemberwiseClone();
    }
}
