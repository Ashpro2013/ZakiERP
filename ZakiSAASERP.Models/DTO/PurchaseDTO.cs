using ZakiSAASERP.Models.Transactions.Purchase;

namespace ZakiSAASERP.Models.DTO;

public record PurchaseDTO(PurchaseHdr Hdr, List<TransactionDtls> Dtls);
