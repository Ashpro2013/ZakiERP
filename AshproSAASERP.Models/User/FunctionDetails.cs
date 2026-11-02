namespace AshproSAASERP.Models.User;

[Table("FunctionDetails")]
public class FunctionDetails
{
    [Key]
    public int Id { get; set; }
    public int FK_MenuDetailsId { get; set; }
    public string? EventName { get; set; }
    public string? EventCaption { get; set; }

    [ForeignKey(nameof(FK_MenuDetailsId))]
    public virtual MenuDetails? MenuDetails { get; set; }
}
