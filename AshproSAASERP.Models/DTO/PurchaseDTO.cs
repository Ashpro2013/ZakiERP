using AshproSAASERP.Models.Transactions.Purchase;

namespace AshproSAASERP.Models.DTO;

public record PurchaseDTO(PurchaseHdr Hdr, List<TransactionDtls> Dtls);
