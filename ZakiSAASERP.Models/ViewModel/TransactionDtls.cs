using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZakiSAASERP.Models.ViewModel;

public class TransactionDtls : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private int _sl;
    public int SL
    {
        get => _sl;
        set { if (_sl != value) { _sl = value; OnPropertyChanged(); } }
    }

    public int DtlsId { get; set; }
    public int HdrId { get; set; }
    public int FK_ItemId { get; set; }

    private string? _itemCode;
    public string? ItemCode
    {
        get => !string.IsNullOrEmpty(_itemCode) ? _itemCode : (FK_ItemId > 0 ? FK_ItemId.ToString("D5") : "");
        set { if (_itemCode != value) { _itemCode = value; OnPropertyChanged(); } }
    }

    private string? _itemName;
    public string? ItemName
    {
        get => _itemName;
        set { if (_itemName != value) { _itemName = value; OnPropertyChanged(); } }
    }

    private string? _itemArabicName;
    public string? ItemArabicName
    {
        get => _itemArabicName;
        set { if (_itemArabicName != value) { _itemArabicName = value; OnPropertyChanged(); } }
    }

    private string? _barcode;
    public string? Barcode
    {
        get => _barcode;
        set { if (_barcode != value) { _barcode = value; OnPropertyChanged(); } }
    }

    public int FK_UnitId { get; set; }

    private string? _unitName;
    public string? UnitName
    {
        get => _unitName;
        set { if (_unitName != value) { _unitName = value; OnPropertyChanged(); } }
    }

    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set { if (_quantity != value) { _quantity = value; OnPropertyChanged(); } }
    }

    public decimal RequestedQty { get; set; }  // For approval/rejection reports
    public decimal QtyPerUnit { get; set; }

    private decimal _exRate;
    public decimal ExRate
    {
        get => _exRate;
        set { if (_exRate != value) { _exRate = value; OnPropertyChanged(); } }
    }

    private decimal _inRate;
    public decimal InRate
    {
        get => _inRate;
        set { if (_inRate != value) { _inRate = value; OnPropertyChanged(); } }
    }

    private decimal _grossAmount;
    public decimal GrossAmount
    {
        get => _grossAmount;
        set { if (_grossAmount != value) { _grossAmount = value; OnPropertyChanged(); } }
    }

    private decimal _discountPerc;
    public decimal DiscountPerc
    {
        get => _discountPerc;
        set { if (_discountPerc != value) { _discountPerc = value; OnPropertyChanged(); } }
    }

    private decimal _discountAmnt;
    public decimal DiscountAmnt
    {
        get => _discountAmnt;
        set { if (_discountAmnt != value) { _discountAmnt = value; OnPropertyChanged(); } }
    }

    private decimal _taxableAmnt;
    public decimal TaxableAmnt
    {
        get => _taxableAmnt;
        set
        {
            if (_taxableAmnt != value)
            {
                _taxableAmnt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TaxableAmount));
            }
        }
    }

    public decimal TaxableAmount
    {
        get => TaxableAmnt;
        set => TaxableAmnt = value;
    }

    private decimal _taxPerc;
    public decimal TaxPerc
    {
        get => _taxPerc;
        set { if (_taxPerc != value) { _taxPerc = value; OnPropertyChanged(); } }
    }

    private decimal _taxAmount;
    public decimal TaxAmount
    {
        get => _taxAmount;
        set { if (_taxAmount != value) { _taxAmount = value; OnPropertyChanged(); } }
    }

    private decimal _lineTotal;
    public decimal LineTotal
    {
        get => _lineTotal;
        set { if (_lineTotal != value) { _lineTotal = value; OnPropertyChanged(); } }
    }

    public bool IsDeleted { get; set; } = false;
    public int FK_ItemBaseUnit { get; set; }
    public int FK_StoreId { get; set; }
    public string StoreName { get; set; }
    public string FromStoreName { get; set; }
    public string ToStoreName { get; set; }
    public List<CommonModel> AvailableUnits { get; set; } = new();
    public string? Description { get; set; }
    public int? RefDtlsId { get; set; }
    public string? RefType { get; set; }
    public int? FK_DeliveryNoteDtlsId { get; set; }
    public int? FK_SalesInvoiceDtlsId { get; set; }
    public decimal DeliveredQty { get; set; } // Quantity that has been delivered via Delivery Note
}
