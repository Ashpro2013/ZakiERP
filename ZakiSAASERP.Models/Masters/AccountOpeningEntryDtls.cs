namespace ZakiSAASERP.Models.Masters;

[Table("AccountOpeningEntryDetails")]
public class AccountOpeningEntryDtls
{
    [Key]
    public int AccountOpeningEntryDetailsId { get; set; }
    public int? FK_AccountOpeningEntryId { get; set; }
    public int? FK_AccountId { get; set; }
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
    public string? Narration { get; set; }
    public bool IsDeleted { get; set; }

    [ForeignKey("FK_AccountId")]
    public virtual AccountMaster? Account { get; set; }

    [NotMapped]
    public bool IsChanged { get; set; }
}
