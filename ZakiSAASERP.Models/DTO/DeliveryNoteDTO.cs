namespace ZakiSAASERP.Models.DTO;

public record DeliveryNoteDTO(DeliveryNoteHdr DeliveryNoteHdr, List<TransactionDtls> DeliveryNoteDtls);
