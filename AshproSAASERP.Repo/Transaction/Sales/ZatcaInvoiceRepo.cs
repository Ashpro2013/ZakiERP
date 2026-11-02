using AshproSAASERP.Interface.Transaction.Sales;
using AshproSAASERP.Models.Zatca;

namespace AshproSAASERP.Repo.Transaction.Sales;

public class ZatcaInvoiceRepo(IDbContextProvider _contextProvider) : IZatcaInvoiceRepo
{
    public async Task<(bool, string)> DeleteZatcaInvoice(int id)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            var existing = await db.ZatcaInvoices.FindAsync(id);
            if (existing is null) { return (false, "Zatca invoice not found."); }

            db.ZatcaInvoices.Remove(existing);
            await db.SaveChangesAsync();
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<ZatcaInvoices>> GetAllZatcaInvoiceByBranchId(int BranchId)
    {
        await using var db = _contextProvider.CreateDbContext();
        return await db.ZatcaInvoices.AsNoTracking().Where(x => x.FK_BranchId == BranchId).ToListAsync();
    }

    public async Task<ZatcaInvoiceDTO> GetAllZatcaInvoiceWithPagination(PaginationModel pagination)
    {
        await using var db = _contextProvider.CreateDbContext();
        var query = db.ZatcaInvoices.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var term = pagination.search.ToLower();
            query = query.Where(x => x.InvNo.ToLower().Contains(term)
            || x.Code.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
        {
            if (pagination.sortDirection == "desc")
            {
                query = pagination.sortColumn switch
                {
                    "Code" => query.OrderByDescending(x => x.Code),
                    "Name" => query.OrderByDescending(x => x.InvoiceType),
                    _ => query.OrderByDescending(x => x.InvNo)
                };
            }
            else
            {
                query = pagination.sortColumn switch
                {
                    "Code" => query.OrderBy(x => x.Code),
                    "Name" => query.OrderBy(x => x.InvoiceType),
                    _ => query.OrderBy(x => x.InvNo)
                };
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        int count = await query.CountAsync();
        var list = await query
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        return new ZatcaInvoiceDTO(list, count);
    }

    public async Task<(bool, string)> SaveZatcaInvoice(ZatcaInvoices zatcaInvoices)
    {
        try
        {
            await using var db = _contextProvider.CreateDbContext();
            await db.ZatcaInvoices.AddAsync(zatcaInvoices);
            await db.SaveChangesAsync();
            return (true, "Success");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
