namespace AshproSAASERP.Interface.Transaction.Inventory;

public interface IDamageStockRepo
{
    Task<string> GetNextVoucherNo(int branchId);
    Task<DamageStockDTO> Create(DamageStockDTO master);
    Task<DamageStockDTO> Update(DamageStockDTO master);
    Task<DamageStockDTO> Delete(int id);
    Task<DamageStockDTO> GetById(int id);
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
}
