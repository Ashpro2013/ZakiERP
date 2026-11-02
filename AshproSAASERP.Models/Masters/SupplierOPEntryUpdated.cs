namespace AshproSAASERP.Models.Masters
{
    [Table("SupplierOPEntryUpdated")]
    public class SupplierOPEntryUpdated
    {
        [Key]
        public int SupplierOPEntryUpdatedId { get; set; }

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
    }
}
