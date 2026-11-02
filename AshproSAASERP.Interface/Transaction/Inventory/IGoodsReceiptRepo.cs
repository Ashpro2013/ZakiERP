namespace AshproSAASERP.Interface.Transaction.Inventory;

public interface IGoodsReceiptRepo
{
    Task<string> GetNewDocNo(int branchId);
    Task<string> GetNextGoodsReceiptNoAsync();
    Task<GoodsReceiptDTO> Create(GoodsReceiptDTO master);
    Task<GoodsReceiptDTO> Update(GoodsReceiptDTO master);
    Task<GoodsReceiptDTO> Delete(int id);
    Task<GoodsReceiptDTO> GetById(int id);
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
}
