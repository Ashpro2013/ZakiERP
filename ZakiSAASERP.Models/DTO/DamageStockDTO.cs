using ZakiSAASERP.Models.Transactions.Inventory;

namespace ZakiSAASERP.Models.DTO;

public class DamageStockDTO
{
    public DamageStockHdr Header { get; set; }
    public List<DamageStockDtls> Details { get; set; }
    public string? Message { get; set; }
    public int Id { get; set; }

    public DamageStockDTO()
    {
        Header = new DamageStockHdr();
        Details = new List<DamageStockDtls>();
    }

    public DamageStockDTO(DamageStockHdr header, List<DamageStockDtls> details)
    {
        Header = header;
        Details = details;
    }
}
