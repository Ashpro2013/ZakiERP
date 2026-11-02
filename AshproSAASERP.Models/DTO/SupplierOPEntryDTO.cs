using AshproSAASERP.Models.Masters;

namespace AshproSAASERP.Models.DTO
{
    public class SupplierOPEntryDTO
    {
        public int SupplierOPEntryId { get; set; }
        public string? DocNo { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Now;
        public string? EnteredBy { get; set; }
        public string? Remarks { get; set; }
        public int FK_BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? FK_UserId { get; set; }

        public List<SupplierOPEntryDetails> EntryDetails { get; set; } = new();
    }
}
