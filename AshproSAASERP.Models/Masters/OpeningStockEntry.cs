using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.Masters
{
    [Table("OpeningStockEntry")]
    public class OpeningStockEntry
    {
        [Key]
        public int OpeningStockEntryId { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string DocumentNo { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EnteredBy { get; set; }

        public int FK_BranchId { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(FK_BranchId))]
        public BranchMaster? Branch { get; set; }

        public int? FK_UserId { get; set; }

        [ForeignKey(nameof(FK_UserId))]
        public UserDetails? User { get; set; }
    }
}
