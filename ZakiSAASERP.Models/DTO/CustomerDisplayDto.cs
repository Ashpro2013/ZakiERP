namespace ZakiSAASERP.Models.DTO;

public class CustomerDisplayDto
{
    public List<SaleItem> Items { get; set; } = new();

    public decimal Total => Items.Sum(item => item.Total);
}
