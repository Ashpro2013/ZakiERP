namespace ZakiSAASERP.Models.DTO;

public record DeliveryNoteRtnDTO(DeliveryNoteRtnHdr DeliveryNoteRtnHdr, List<TransactionDtls> DeliveryNoteRtnDtls);
