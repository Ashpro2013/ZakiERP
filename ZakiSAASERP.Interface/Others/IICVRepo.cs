using ZakiSAASERP.Models.Transactions.Sales;

namespace ZakiSAASERP.Interface.Others;

public interface IICVRepo
{
    Task<(bool, string)> SaveInvoiceICV(InvoiceICV icv);
    Task<bool> ICVisExist(int iVoucherId, string VoucherType, int BranchId);
}
