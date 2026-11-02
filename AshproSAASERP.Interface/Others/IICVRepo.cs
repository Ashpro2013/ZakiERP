using AshproSAASERP.Models.Transactions.Sales;

namespace AshproSAASERP.Interface.Others;

public interface IICVRepo
{
    Task<(bool, string)> SaveInvoiceICV(InvoiceICV icv);
    Task<bool> ICVisExist(int iVoucherId, string VoucherType, int BranchId);
}
