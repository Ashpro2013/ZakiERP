namespace ZakiSAASERP.Models.DTO;

public record ProformaInvoiceDTO(ProformaInvoiceHdr Hdr, List<TransactionDtls> Dtls);
