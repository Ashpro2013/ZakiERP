namespace AshproSAASERP.Interface.Transaction.Sales;

public interface IDeliveryNoteRepo : ICommonTranInterface<DeliveryNoteDTO>
{
    Task<string> GetNextDeliveryNoteNoAsync();
    Task<List<PendingDeliveryNoteDTO>> GetPendingDeliveryNotes(int customerId);
}
