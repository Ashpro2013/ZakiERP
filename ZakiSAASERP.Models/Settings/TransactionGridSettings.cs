namespace ZakiSAASERP.Models.Settings;

public class TransactionGridSettings
{
    #region Grid Columns Visibility
    public bool ShowItemCode { get; set; } = false;
    public bool ShowBarcode { get; set; } = true;
    public bool ShowItemName { get; set; } = true;
    public bool ShowArabicName { get; set; } = false;
    public bool ShowStore { get; set; } = false;
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

    #region Grid Inline Field Editing
    public bool AllowEditExcRate { get; set; } = true;
    public bool AllowEditIncRate { get; set; } = true;
    public bool AllowEditDiscPer { get; set; } = true;
    public bool AllowEditDiscAmount { get; set; } = true;
    #endregion

    #region Transaction Operations / Permissions
    public bool EnableUpdate { get; set; } = false;
    public bool EnableCancel { get; set; } = false;
    #endregion

    public TransactionGridSettings Clone()
    {
        return (TransactionGridSettings)this.MemberwiseClone();
    }

    public void CopyFrom(TransactionGridSettings other)
    {
        if (other == null) return;
        ShowItemCode = other.ShowItemCode;
        ShowBarcode = other.ShowBarcode;
        ShowItemName = other.ShowItemName;
        ShowArabicName = other.ShowArabicName;
        ShowStore = other.ShowStore;
        ShowUnit = other.ShowUnit;
        ShowExRate = other.ShowExRate;
        ShowIncRate = other.ShowIncRate;
        ShowGross = other.ShowGross;
        ShowDiscPercentage = other.ShowDiscPercentage;
        ShowDiscAmount = other.ShowDiscAmount;
        ShowTaxableAmount = other.ShowTaxableAmount;
        ShowTaxPercentage = other.ShowTaxPercentage;
        ShowTaxAmount = other.ShowTaxAmount;
        ShowAmount = other.ShowAmount;

        AllowEditExcRate = other.AllowEditExcRate;
        AllowEditIncRate = other.AllowEditIncRate;
        AllowEditDiscPer = other.AllowEditDiscPer;
        AllowEditDiscAmount = other.AllowEditDiscAmount;

        EnableUpdate = other.EnableUpdate;
        EnableCancel = other.EnableCancel;
    }

    public void ResetToDefaults()
    {
        ShowItemCode = false;
        ShowBarcode = true;
        ShowItemName = true;
        ShowArabicName = false;
        ShowStore = false;
        ShowUnit = true;
        ShowExRate = true;
        ShowIncRate = false;
        ShowGross = true;
        ShowDiscPercentage = true;
        ShowDiscAmount = true;
        ShowTaxableAmount = false;
        ShowTaxPercentage = true;
        ShowTaxAmount = true;
        ShowAmount = true;

        AllowEditExcRate = true;
        AllowEditIncRate = true;
        AllowEditDiscPer = true;
        AllowEditDiscAmount = true;

        EnableUpdate = false;
        EnableCancel = false;
    }

    public void SelectAllColumns()
    {
        ShowItemCode = true;
        ShowBarcode = true;
        ShowItemName = true;
        ShowArabicName = true;
        ShowStore = true;
        ShowUnit = true;
        ShowExRate = true;
        ShowIncRate = true;
        ShowGross = true;
        ShowDiscPercentage = true;
        ShowDiscAmount = true;
        ShowTaxableAmount = true;
        ShowTaxPercentage = true;
        ShowTaxAmount = true;
        ShowAmount = true;
    }
}
