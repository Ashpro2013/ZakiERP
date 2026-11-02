namespace AshproSAASERP.Models.DTO;

public record DeliveryNoteDTO(DeliveryNoteHdr DeliveryNoteHdr, List<TransactionDtls> DeliveryNoteDtls);
