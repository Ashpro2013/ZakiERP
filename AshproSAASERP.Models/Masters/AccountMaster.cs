
namespace AshproSAASERP.Models.Masters;

[Table("AccountMaster")]
public class AccountMaster
{
    [Key]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Account Code is required. *")]
    [StringLength(100)]
    public string Code { get; set; }

    [Required(ErrorMessage = "Account Name is required. *")]
    [StringLength(500)]
    public string Name { get; set; }

    [StringLength(500)]
    public string? NameArabic { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Account Head is required. *")]
    public int FK_AccountHeadId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Account Group is required. *")]
    public int FK_AcGroupId { get; set; }

    public double Opening { get; set; }

    [StringLength(20)]
    public string? DbCr { get; set; }

    public int? FK_AcTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Account Sub Group is required. *")]
    public int FK_AccountSubGroupId { get; set; }

    [StringLength(20)]
    public string? Flag { get; set; }

    public bool IsReserved { get; set; } = false;
}
