namespace AshproSAASERP.Models.Settings;

public class MenuVisibilityDTO
{
    public int ERPTypeId { get; set; }
    public string ERPTypeName { get; set; } = string.Empty;
    public bool IsCustomType { get; set; }
    public List<MenuVisibilityItem> MenuItems { get; set; } = new();
}

public class MenuVisibilityItem
{
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MenuCaption { get; set; } = string.Empty;
    public string ArabicCaption { get; set; } = string.Empty;
    public int? ParentId { get; set; }  // Nullable to match MenuDetails
    public bool IsVisible { get; set; }
    public bool CanToggle { get; set; } = true; // False for Dashboard, Settings
    public int Position { get; set; }
    public string? AssemblyName { get; set; }
    public string? ClassName { get; set; }
}
