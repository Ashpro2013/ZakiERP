namespace ZakiSAASERP.Models.Transactions.Accounts;

public class JournalEntryDTO
{
    public int JournalId { get; set; }
    public string JournalNo { get; set; } = string.Empty;
    public DateTime JournalDate { get; set; } = DateTime.Now;
    public int FK_BranchId { get; set; }
    public int? FK_UserId { get; set; }
    public string? Narration { get; set; }
    public string? ReferenceNo { get; set; }
    public int? FK_ProjectId { get; set; }
    public int? FK_SalesmanId { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Status { get; set; } = "Posted";

    public List<JournalEntryDtls> Details { get; set; } = new();
}
