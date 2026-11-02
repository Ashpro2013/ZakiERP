namespace AshproSAASERP.Repo.Masters;

using AshproSAASERP.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class AccountHeadRepo(IDbContextProvider contextProvider) : IAccountHeadRepo
{
    public async Task<OutputResponse> CreateAsync(AccountHeadMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var exists = await db.AccountHeadMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
            if (exists) return new OutputResponse(0, "Account head already exists.");
            await db.AccountHeadMasters.AddAsync(entity);
            await db.SaveChangesAsync();
            return new OutputResponse(entity.AccountHeadId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            if (id is 0) return new OutputResponse(0, "Invalid identifier.");

            var existing = await db.AccountHeadMasters.FindAsync(id);
            if (existing is null) return new OutputResponse(0, "Account head not found.");

            if (existing.IsReserved) return new OutputResponse(0, "Cannot delete this record because it is reserved.");

            db.AccountHeadMasters.Remove(existing);
            await db.SaveChangesAsync();
            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("REFERENCE constraint") == true || ex.InnerException?.Message.Contains("FK_") == true)
            {
                return new OutputResponse(0, "Cannot delete this record because it is used in other transactions or masters.");
            }
            return new OutputResponse(0, ex.GetBaseException().Message);
        }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<List<AccountHeadMaster>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.AccountHeadMasters.AsNoTracking().ToListAsync();
    }

    public async Task<(List<AccountHeadMaster>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();

        IQueryable<AccountHeadMaster> query;
        var sortExpr = pagination.sortColumn switch
        {
            "Code" => (Expression<Func<AccountHeadMaster, object>>)(x => x.Code),
            "Name" => x => x.Name,
            _ => x => x.AccountHeadId
        };
        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            query = pagination.sortDirection == "desc" ? db.AccountHeadMasters.AsNoTracking().Where(x => x.Name.Contains(pagination.search) || x.Code.Contains(pagination.search))
             .OrderByDescending(sortExpr)
           : db.AccountHeadMasters.AsNoTracking().Where(x => x.Name.Contains(pagination.search) || x.Code.Contains(pagination.search)).OrderBy(sortExpr);
        }
        else
        {
            query = pagination.sortDirection == "desc" ? db.AccountHeadMasters.AsNoTracking().OrderByDescending(sortExpr) :
                db.AccountHeadMasters.AsNoTracking().OrderBy(sortExpr);
        }

        int count = await query.CountAsync();
        // APPLY ORDER

        var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
        return (list, count);
    }

    public async Task<AccountHeadMaster> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        if (id is 0) return null!;
        var model = await db.AccountHeadMasters.AsNoTracking().FirstOrDefaultAsync(x => x.AccountHeadId == id);
        return model!;
    }
    public async Task<OutputResponse> GetNextCode(int typeId)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var type = await db.AccountTypes.FindAsync(typeId);
            if (type == null) return new OutputResponse(0, "Account Type not found");

            var heads = await db.AccountHeadMasters.Where(x => x.FK_AcTypeId == typeId).AsNoTracking().ToListAsync();
            int maxCode = 1000;

            if (heads.Any())
            {
                foreach (var head in heads)
                {
                    string codePart = head.Code;
                    if (string.IsNullOrWhiteSpace(codePart)) continue;

                    if (codePart.Contains("-"))
                    {
                        var parts = codePart.Split('-');
                        if (parts.Length > 1) codePart = parts[1];
                    }

                    if (int.TryParse(codePart, out int currentCode))
                    {
                        if (currentCode > maxCode) maxCode = currentCode;
                    }
                }
            }

            string nextCode = $"{type.AcTypeId:D2}-{maxCode + 1}";
            return new OutputResponse(1, nextCode); // Success with code in Message
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message);
        }
    }

    public async Task<OutputResponse> UpdateAsync(AccountHeadMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.AccountHeadMasters.FirstOrDefaultAsync(x => x.AccountHeadId == entity.AccountHeadId);
            if (existing is null) return new OutputResponse(0, "Account head not found.");

            // Check if reserved - cannot update built-in accounts
            if (existing.IsReserved)
                return new OutputResponse(0, "Cannot update this record because it is reserved.");

            existing.Code = entity.Code;
            existing.Name = entity.Name;
            existing.NameArabic = entity.NameArabic;
            existing.FK_AcTypeId = entity.FK_AcTypeId;
            // Removed: GroupCategory, GroupCategoryArabic
            existing.IsReserved = entity.IsReserved;
            await db.SaveChangesAsync();
            return new OutputResponse(existing.AccountHeadId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
}
