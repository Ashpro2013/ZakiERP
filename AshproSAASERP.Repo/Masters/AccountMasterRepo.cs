
using System.Linq.Expressions;

namespace AshproSAASERP.Repo.Masters;

public class AccountMasterRepo(IDbContextProvider contextProvider) : IAccountMasterRepo
{
    public async Task<OutputResponse> CreateAsync(AccountMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var exists = await db.AccountMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
            if (exists) return new OutputResponse(0, "Account master already exists.");

            // Ensure Flag is set to "Acc" if missing
            if (string.IsNullOrEmpty(entity.Flag)) entity.Flag = "Acc";

            await db.AccountMasters.AddAsync(entity);
            await db.SaveChangesAsync();
            return new OutputResponse(entity.AccountId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> DeleteAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.AccountMasters.FindAsync(id);
            if (existing is null) { return new OutputResponse(0, "Account not found."); }

            // Check if reserved - cannot delete built-in accounts
            if (existing.IsReserved)
                return new OutputResponse(0, "Cannot delete this record because it is reserved.");

            db.AccountMasters.Remove(existing);
            await db.SaveChangesAsync();
            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<List<AccountMaster>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.AccountMasters.AsNoTracking().ToListAsync();
    }

    // Interface implementation
    public async Task<(List<AccountMaster>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        IQueryable<AccountMaster> query = db.AccountMasters.AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(pagination.search))
        {
            var search = pagination.search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) ||
                                     x.Code.ToLower().Contains(search) ||
                                     (x.NameArabic != null && x.NameArabic.ToLower().Contains(search)));
        }

        // Apply sorting
        var sortExpr = pagination.sortColumn switch
        {
            "Code" => (Expression<Func<AccountMaster, object>>)(x => x.Code),
            "Name" => x => x.Name,
            "Type" => x => x.IsReserved, // Map "Type" column to IsReserved
            "IsReserved" => x => x.IsReserved, // Explicit mapping for IsReserved
            "NameArabic" => x => x.NameArabic,
            "DbCr" => x => x.DbCr,
            _ => x => x.Code  // Default to Code
        };

        // Apply generic filter (e.g., Account Flag)
        if (pagination.FilterId > 0)
        {
            if (pagination.FilterId == 1) // Customer
            {
                query = query.Where(x => x.Flag == "Cus");
            }
            else if (pagination.FilterId == 2) // Supplier
            {
                query = query.Where(x => x.Flag == "Sup");
            }
            else if (pagination.FilterId == 3) // General (Acc or null for built-in)
            {
                query = query.Where(x => x.Flag == "Acc" || x.Flag == null || x.Flag == "");
            }
        }

        if ((pagination.sortColumn == "Code" || string.IsNullOrEmpty(pagination.sortColumn)) && pagination.sortDirection != "desc")
        {
            // Special case: Default view (Built-in first, then Code)
            query = query.OrderByDescending(x => x.IsReserved).ThenBy(x => x.Code);
        }
        else
        {
            query = pagination.sortDirection == "desc"
                ? query.OrderByDescending(sortExpr).ThenBy(x => x.Code)
                : query.OrderBy(sortExpr).ThenBy(x => x.Code);
        }

        int count = await query.CountAsync();

        var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
        return (list, count);
    }

    public async Task<AccountMaster> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        var model = await db.AccountMasters.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == id);
        return model!;
    }

    public async Task<OutputResponse> UpdateAsync(AccountMaster entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.AccountMasters.FirstOrDefaultAsync(x => x.AccountId == entity.AccountId);
            if (existing is null) { return new OutputResponse(0, "Account not found."); }

            // Check if reserved - cannot update built-in accounts
            if (existing.IsReserved)
                return new OutputResponse(0, "Cannot update this record because it is reserved.");

            existing.Code = entity.Code;
            existing.Name = entity.Name;
            existing.NameArabic = entity.NameArabic;
            existing.FK_AccountHeadId = entity.FK_AccountHeadId;
            existing.FK_AcGroupId = entity.FK_AcGroupId;
            existing.FK_AccountSubGroupId = entity.FK_AccountSubGroupId;
            existing.FK_AcTypeId = entity.FK_AcTypeId;
            existing.DbCr = entity.DbCr;
            existing.Opening = entity.Opening;
            existing.Flag = entity.Flag;

            await db.SaveChangesAsync();
            return new OutputResponse(existing.AccountId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> GetNextCode(int subGroupId)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            // Fetch validation hierarchy
            var subGroup = await db.AccountSubGroups
                .Include(x => x.AccountGroup)
                .ThenInclude(x => x.AccountHead)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SubGroupId == subGroupId);

            if (subGroup == null) return new OutputResponse(0, "SubGroup not found");

            var group = subGroup.AccountGroup;
            if (group == null) return new OutputResponse(0, "Account Group not found in SubGroup");

            var head = group.AccountHead;
            if (head == null) return new OutputResponse(0, "Account Head not found in Group");

            // Fetch TypeId
            // var type = await db.AccountTypes.FirstOrDefaultAsync(x => x.AcTypeName == head.GroupCategory); // OLD
            // int typeId = type?.AcTypeId ?? 0;
            int typeId = head.FK_AcTypeId; // NEW: Direct access

            // Determine SubGroup Sequence
            string subGroupSeqStr = "001"; // Default
            if (!string.IsNullOrWhiteSpace(subGroup.Code))
            {
                if (subGroup.Code.Contains("-"))
                {
                    subGroupSeqStr = subGroup.Code.Split('-').Last();
                }
                else
                {
                    string digits = System.Text.RegularExpressions.Regex.Replace(subGroup.Code, "[^0-9]", "");
                    if (!string.IsNullOrEmpty(digits)) subGroupSeqStr = digits;
                }
            }
            if (int.TryParse(subGroupSeqStr, out int subGroupSeqInt))
            {
                subGroupSeqStr = subGroupSeqInt.ToString("D3");
            }


            // Determine Account Sequence
            var accounts = await db.AccountMasters.Where(x => x.FK_AccountSubGroupId == subGroupId).AsNoTracking().ToListAsync();
            int maxCode = 0;

            if (accounts.Any())
            {
                foreach (var acc in accounts)
                {
                    string codePart = acc.Code;
                    if (string.IsNullOrWhiteSpace(codePart)) continue;

                    string seqStr = codePart;
                    if (codePart.Contains("-"))
                    {
                        var parts = codePart.Split('-');
                        if (parts.Length > 0)
                        {
                            seqStr = parts.Last();
                        }
                    }
                    else
                    {
                        seqStr = System.Text.RegularExpressions.Regex.Replace(codePart, "[^0-9]", "");
                    }

                    if (int.TryParse(seqStr, out int currentCode))
                    {
                        if (currentCode > maxCode) maxCode = currentCode;
                    }
                }
            }

            // Format: TypeId-AccountHeadCode-AccountGroupCode-SubGroupSequence-AccountSequence
            string nextCode = $"{subGroup.Code}-{maxCode + 1:D3}";
            return new OutputResponse(1, nextCode);
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message);
        }
    }

    public async Task<List<AccountMaster>> GetCashAndBankAccountsAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var cashBankGroup = await db.Set<AccountGroup>()
                .Where(g => g.Code == "110001")
                .FirstOrDefaultAsync();

            if (cashBankGroup == null) return new List<AccountMaster>();

            return await db.Set<AccountMaster>()
                .Where(a => a.FK_AcGroupId == cashBankGroup.AccountGroupId)
                .OrderBy(a => a.Code)
                .ToListAsync();
        }
        catch (Exception)
        {
            return new List<AccountMaster>();
        }
    }
}



