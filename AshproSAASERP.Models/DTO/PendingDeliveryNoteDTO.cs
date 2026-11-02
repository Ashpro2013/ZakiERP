namespace AshproSAASERP.Models.DTO;

public class PendingDeliveryNoteDTO
{
    public int DeliveryNoteId { get; set; }
    public string DeliveryNoteNo { get; set; }
    public DateTime? DeliveryNoteDate { get; set; }
    public List<PendingDeliveryNoteItemDTO> Items { get; set; } = new();
    public bool IsSelected { get; set; }
}

public class PendingDeliveryNoteItemDTO : TransactionDtls
{
    public decimal PendingQty { get; set; }
    public string UnitName { get; set; }
    public decimal OriginalQty { get; set; }
}
