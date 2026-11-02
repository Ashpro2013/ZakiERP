namespace ZakiSAASERP.Models.Masters;

[Table("AccountHeadMaster")]
public class AccountHeadMaster
{
    [Key]
    public int AccountHeadId { get; set; }
    [Required(ErrorMessage = "Account Head Code is required. *")]
    public string Code { get; set; }
    [Required(ErrorMessage = "Account Head Name is required. *")]
    public string Name { get; set; }
    public string? NameArabic { get; set; }
    [Required(ErrorMessage = "Account Head Type is required. *")]
    public int FK_AcTypeId { get; set; }

    [ForeignKey("FK_AcTypeId")]
    public virtual AccountType? AccountType { get; set; }

    public bool IsReserved { get; set; } = false;
}
