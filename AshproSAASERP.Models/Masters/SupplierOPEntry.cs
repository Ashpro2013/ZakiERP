using AshproSAASERP.Models.User;

namespace AshproSAASERP.Models.Masters
{
    [Table("SupplierOPEntry")]
    public class SupplierOPEntry
    {
        [Key]
        public int SupplierOPEntryId { get; set; }

        [StringLength(50)]
        public string? DocNo { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? EnteredBy { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public int FK_BranchId { get; set; }

        [ForeignKey(nameof(FK_BranchId))]
        public BranchMaster? Branch { get; set; }

        public int? FK_UserId { get; set; }

        [ForeignKey(nameof(FK_UserId))]
        public UserDetails? User { get; set; }
    }
}
