using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.Models.DTO
{
    public class OpeningStockEntryDTO
    {
        public int OpeningStockEntryId { get; set; }
        public string? DocumentNo { get; set; }
        public DateTime EntryDate { get; set; }
        public string? EnteredBy { get; set; }
        public string? Remarks { get; set; }
        public int FK_BranchId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public int? FK_UserId { get; set; }

        public List<OpeningStockEntryItem> EntryDetails { get; set; } = new();
    }
}
