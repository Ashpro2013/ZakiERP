using ZakiSAASERP.Models.Transactions.Purchase;

namespace ZakiSAASERP.Models.DTO;

public record SupplierPaymentDTO(SupplierPaymentHdr Hdr, List<SupplierPaymentDtls> Dtls);
