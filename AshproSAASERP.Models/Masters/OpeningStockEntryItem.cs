namespace AshproSAASERP.Models.Masters
{
    [Table("OpeningStockEntryItem")]
    public class OpeningStockEntryItem
    {
        [Key]
        public int OpeningStockEntryItemId { get; set; }

        [StringLength(100)]
        public string DocumentNo { get; set; } = string.Empty;

        public int FK_OpeningStockEntryId { get; set; }

        public int FK_ItemId { get; set; }

        public int FK_StoreId { get; set; }

        public int FK_UnitId { get; set; }

        public int SlNo { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal QtyPerUnit { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? LocName { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? VatPer { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? VatAmt { get; set; }

        [ForeignKey(nameof(FK_OpeningStockEntryId))]
        public OpeningStockEntry? Header { get; set; }

        [ForeignKey(nameof(FK_ItemId))]
        public ItemMaster? Item { get; set; }

        [ForeignKey(nameof(FK_StoreId))]
        public StoreMaster? Store { get; set; }

        [ForeignKey(nameof(FK_UnitId))]
        public UnitMaster? Unit { get; set; }

        private string? _itemCode;
        [NotMapped]
        public string? ItemCode
        {
            get => _itemCode ?? Item?.ItemCode;
            set => _itemCode = value;
        }

        private string? _barcode;
        [NotMapped]
        public string? Barcode
        {
            get => _barcode ?? Item?.Barcode;
            set => _barcode = value;
        }

        private string? _itemName;
        [NotMapped]
        public string? ItemName
        {
            get => _itemName ?? Item?.ItemName;
            set => _itemName = value;
        }

        [NotMapped]
        public string? ArabicName
        {
            get => Item?.ItemNameArabic;
            set { }
        }

        [NotMapped]
        public string? UnitName
        {
            get => Unit?.Name;
            set { }
        }

        [NotMapped]
        [System.Text.Json.Serialization.JsonIgnore]
        public List<UnitMaster> AvailableUnits { get; set; } = new();

        [NotMapped]
        [System.Text.Json.Serialization.JsonIgnore]
        public List<ItemMultiUnit> MultiUnitDetails { get; set; } = new();
    }
}
