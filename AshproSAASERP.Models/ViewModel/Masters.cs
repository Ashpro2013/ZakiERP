namespace AshproSAASERP.Models.ViewModel;

public class Masters
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsSelected { get; set; }
}
