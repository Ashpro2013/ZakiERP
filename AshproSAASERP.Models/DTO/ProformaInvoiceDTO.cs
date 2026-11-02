namespace AshproSAASERP.Models.DTO;

public record ProformaInvoiceDTO(ProformaInvoiceHdr Hdr, List<TransactionDtls> Dtls);
