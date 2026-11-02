namespace ZakiSAASERP.Models.Masters;

[Table("AccountGroup")]
public class AccountGroup
{
    [Key]
    public int AccountGroupId { get; set; }
    [Required(ErrorMessage = "Account Code is required. *")]
    [MaxLength(100)]
    public string? Code { get; set; }

    [Required(ErrorMessage = "Account Name is required. *")]
    [MaxLength(500)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? NameArabic { get; set; }

    [MaxLength(2000)]
    public string? Remarks { get; set; }

    [Required(ErrorMessage = "Account head is required. *")]
    public int? FK_AcHeadId { get; set; }

    public bool IsReserved { get; set; } = false;

    [ForeignKey("FK_AcHeadId")]
    public virtual AccountHeadMaster? AccountHead { get; set; }
}
