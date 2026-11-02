namespace ZakiSAASERP.Models.Masters;

[Table("AccountOpeningEntryHdr")]
public class AccountOpeningEntryHdr
{
    [Key]
    public int AccountOpeningEntryId { get; set; }
    public DateTime? EntryDate { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string? VoucherNo { get; set; }

    public DateTime? VoucherDate { get; set; } = DateTime.Now;
    public int? FK_BranchId { get; set; }
    public decimal? DebitTotal { get; set; }
    public decimal? CreditTotal { get; set; }
    public string? Remarks { get; set; }
    public int? FK_FinancialYearId { get; set; }
    public int? FK_UserId { get; set; }
    public bool IsDeleted { get; set; }
}
