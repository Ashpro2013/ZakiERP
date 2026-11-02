namespace AshproSAASERP.Models.Masters
{
    [Table("CustomerOPEntryDetails")]
    public class CustomerOPEntryDetails
    {
        [Key]
        public int CustomerOPEntryDetailsId { get; set; }

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

        [ForeignKey(nameof(FK_CustomerOPEntryId))]
        public CustomerOPEntry? Header { get; set; }

        [ForeignKey(nameof(FK_CustomerId))]
        public CustomerMaster? Customer { get; set; }

        [ForeignKey(nameof(FK_BranchId))]
        public BranchMaster? Branch { get; set; }

        private string? _customerCode;
        [NotMapped]
        public string? CustomerCode
        {
            get => _customerCode ?? Customer?.Code;
            set => _customerCode = value;
        }

        [NotMapped]
        public string? CustomerNameHeader => Customer?.Name;

        private string? _customerName;
        [NotMapped]
        public string? CustomerName
        {
            get => _customerName ?? Customer?.Name;
            set => _customerName = value;
        }
        [NotMapped]
        public string? ArabicName
        {
            get => Customer?.NameArabic;
            set { }
        }
    }
}
