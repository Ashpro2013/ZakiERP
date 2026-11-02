using ZakiSAASERP.Models.Transactions.Purchase;

namespace ZakiSAASERP.Models.DTO;

public record PurchaseReturnDTO(PurchaseReturnHdr PurchaseReturnHdr, List<TransactionDtls> PurchaseReturnDtls);
