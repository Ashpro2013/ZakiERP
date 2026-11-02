using AshproSAASERP.Models.Transactions.Purchase;

namespace AshproSAASERP.Models.DTO;

public record SupplierPaymentDTO(SupplierPaymentHdr Hdr, List<SupplierPaymentDtls> Dtls);
