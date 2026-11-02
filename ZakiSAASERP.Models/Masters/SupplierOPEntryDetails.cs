namespace ZakiSAASERP.Models.Masters
{
    [Table("SupplierOPEntryDetails")]
    public class SupplierOPEntryDetails
    {
        [Key]
        public int SupplierOPEntryDetailsId { get; set; }

        public int FK_SupplierOPEntryId { get; set; }

        public int? FK_SupplierId { get; set; }

        [StringLength(50)]
        public string? DocNo { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? PurchaseNo { get; set; }
        public DateTime? PurchaseDate { get; set; }

        public decimal? PurchaseAmt { get; set; }
        public decimal? ReceivedAmt { get; set; }
        public decimal? DiscountAmt { get; set; }
        public decimal? ReturnedAmt { get; set; }
        public decimal? BalanceAmt { get; set; }

        public int? FK_BranchId { get; set; }

        [ForeignKey(nameof(FK_SupplierId))]
        public SupplierMaster? Supplier { get; set; }

        [ForeignKey(nameof(FK_BranchId))]
        public BranchMaster? Branch { get; set; }

        private string? _supplierName;
        [NotMapped]
        public string? SupplierName
        {
            get => _supplierName ?? Supplier?.Name;
            set => _supplierName = value;
        }

        private string? _supplierCode;
        [NotMapped]
        public string? SupplierCode
        {
            get => _supplierCode ?? Supplier?.Code;
            set => _supplierCode = value;
        }
    }
}
