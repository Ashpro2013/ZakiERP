namespace AshproSAASERP.Models.Masters;

[Table("AccountSubGroup")]
public class AccountSubGroup
{
    [Key]
    public int SubGroupId { get; set; }

    [Required]
    public int? FK_AcGroupId { get; set; }

    [ForeignKey("FK_AcGroupId")]
    public virtual AccountGroup? AccountGroup { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public string NameArabic { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    [Required]
    public int FK_AcHeadId { get; set; }

    public bool IsReserved { get; set; } = false;

    [ForeignKey("FK_AcHeadId")]
    public virtual AccountHeadMaster? AccountHead { get; set; }
}
