namespace AshproSAASERP.Models.Masters;

[Table("AccountType")]
public class AccountType
{
    [Key]
    public int AcTypeId { get; set; }

    [StringLength(100)]
    public string AcTypeName { get; set; }
}
