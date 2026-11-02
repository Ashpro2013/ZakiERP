using AshproSAASERP.Models.Transactions.Sales;

namespace AshproSAASERP.Repo.Others;

public class ICVRepo(IDbContextProvider contextProvider) : IICVRepo
{

    public async Task<bool> ICVisExist(int iVoucherId, string VoucherType, int BranchId)
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.InvoiceICVs.AnyAsync(x => x.VoucherType == VoucherType && x.VoucherId == iVoucherId && x.FK_BranchId == BranchId);
    }
    public async Task<(bool, string)> SaveInvoiceICV(InvoiceICV icv)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            await db.InvoiceICVs.AddAsync(icv);
            await db.SaveChangesAsync();
            return new(true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}



