namespace AshproSAASERP.Models.User;

[Table("MenuDetails")]
public class MenuDetails
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MenuCaption { get; set; } = string.Empty;
    public string ArabicCaption { get; set; } = string.Empty;
    public int? ParentId { get; set; }  // Nullable for top-level menus
    public string? AssemblyName { get; set; }
    public string? ClassName { get; set; }
    public bool Active { get; set; }
    public int Position { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsFavorite2 { get; set; }
    public int FilterOneOrder { get; set; }
    public int FilterTwoOrder { get; set; }
}
