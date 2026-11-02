using ZakiSAASERP.Models.Transactions.Purchase;

namespace ZakiSAASERP.Models.DTO;

public class PurchaseOrderDTO
{
    public PurchaseOrderHdr PurchaseOdrHdr { get; set; }
    public List<TransactionDtls> PurchaseOdrDtls { get; set; }

    public PurchaseOrderDTO()
    {
        PurchaseOdrHdr = new PurchaseOrderHdr();
        PurchaseOdrDtls = new List<TransactionDtls>();
    }

    public PurchaseOrderDTO(PurchaseOrderHdr purchaseOdrHdr, List<TransactionDtls> purchaseOdrDtls)
    {
        PurchaseOdrHdr = purchaseOdrHdr;
        PurchaseOdrDtls = purchaseOdrDtls;
    }
}
