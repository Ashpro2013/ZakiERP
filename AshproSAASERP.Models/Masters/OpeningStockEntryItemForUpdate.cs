namespace AshproSAASERP.Models.Masters
{
    [Table("OpeningStockEntryItemForUpdate")]
    public class OpeningStockEntryItemForUpdate
    {
        [Key]
        public int OpeningStockEntryItemForUpdateId { get; set; }

        public int FK_OpeningStockEntryId { get; set; }

        public int FK_ItemId { get; set; }

        public int FK_StoreId { get; set; }

        public int FK_BranchId { get; set; }

        public int FK_UnitId { get; set; }

        public int SlNo { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? QtyPerUnit { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? VatPer { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? VatAmt { get; set; }

        [StringLength(500)]
        public string? LocName { get; set; }

        [ForeignKey(nameof(FK_OpeningStockEntryId))]
        public OpeningStockEntry? Header { get; set; }

        [ForeignKey(nameof(FK_ItemId))]
        public ItemMaster? Item { get; set; }

        [ForeignKey(nameof(FK_StoreId))]
        public StoreMaster? Store { get; set; }

        [ForeignKey(nameof(FK_BranchId))]
        public BranchMaster? Branch { get; set; }

        [ForeignKey(nameof(FK_UnitId))]
        public UnitMaster? Unit { get; set; }
    }
}
