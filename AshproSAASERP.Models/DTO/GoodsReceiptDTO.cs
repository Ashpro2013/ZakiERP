using AshproSAASERP.Models.Transactions.Purchase;

namespace AshproSAASERP.Models.DTO;

public class GoodsReceiptDTO
{
    public GoodsReceiptHdr Header { get; set; }
    public List<GoodsReceiptDtls> Details { get; set; }
    public string Message { get; set; }
    public int Id { get; set; }

    public GoodsReceiptDTO()
    {
        Header = new GoodsReceiptHdr();
        Details = new List<GoodsReceiptDtls>();
    }

    public GoodsReceiptDTO(GoodsReceiptHdr header, List<GoodsReceiptDtls> details)
    {
        Header = header;
        Details = details;
    }
}
