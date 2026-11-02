namespace ZakiSAASERP.Models.Masters
{
    [Table("CustomerOPEntryUpdated")]
    public class CustomerOPEntryUpdated
    {
        [Key]
        public int CustomerOPEntryUpdatedId { get; set; }

        public int FK_CustomerOPEntryId { get; set; }

        public int? FK_CustomerId { get; set; }

        [StringLength(50)]
        public string? DocNo { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? InvoiceNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? InvoiceAmt { get; set; } = 0;

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? ReceivedAmt { get; set; } = 0;

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? DiscountAmt { get; set; } = 0;

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? ReturnedAmt { get; set; } = 0;

        [Column(TypeName = "decimal(18, 4)")]
        public decimal? BalanceAmt { get; set; } = 0;

        public int? FK_BranchId { get; set; }

        [StringLength(100)]
        public string? PONo { get; set; }

        [ForeignKey("FK_CustomerOPEntryId")]
        public CustomerOPEntry? Header { get; set; }

        [ForeignKey("FK_CustomerId")]
        public CustomerMaster? Customer { get; set; }

        [ForeignKey("FK_BranchId")]
        public BranchMaster? Branch { get; set; }
    }
}
