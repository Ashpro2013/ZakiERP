using AshproSAASERP.Models.Transactions.Purchase;

namespace AshproSAASERP.Models.DTO;

public record PurchaseReturnDTO(PurchaseReturnHdr PurchaseReturnHdr, List<TransactionDtls> PurchaseReturnDtls);
