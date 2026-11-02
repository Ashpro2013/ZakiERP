using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Models.Transactions.Accounts
{
    [Table("TblAccounting")]
    public class TblAccounting
    {
        [Key]
        public int TblAccountId { get; set; }
        public DateTime? EntryDate { get; set; }

        [Column("VoucherId")]
        public int? VoucherId { get; set; }

        [Column("VoucherNo")]
        [StringLength(50)]
        public string? VoucherNo { get; set; }

        public int? FK_AccountId { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal? Amount { get; set; }

        [StringLength(2000)]
        public string? Narration { get; set; }

        public int? FK_UserId { get; set; }

        [StringLength(500)]
        public string? Type { get; set; }

        [StringLength(500)]
        public string? BillType { get; set; }

        public int? FK_BranchId { get; set; }

        [ForeignKey(nameof(FK_AccountId))]
        public AccountMaster? Account { get; set; }

        [ForeignKey(nameof(FK_BranchId))]
        public BranchMaster? Branch { get; set; }

        [ForeignKey(nameof(FK_UserId))]
        public UserDetails? User { get; set; }

        public int? FK_SupplierId { get; set; }
        public int? FK_CustomerId { get; set; }
    }
}
