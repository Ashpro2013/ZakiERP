namespace ZakiSAASERP.Models.ViewModel;

public record CommonModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
}
