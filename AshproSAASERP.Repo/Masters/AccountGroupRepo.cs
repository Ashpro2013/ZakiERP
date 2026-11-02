namespace AshproSAASERP.Repo.Masters;

public class AccountGroupRepo(IDbContextProvider contextProvider) : IAccountGroupRepo
{
    public async Task<OutputResponse> CreateAsync(AccountGroup entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var exists = await db.AccountGroups.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
            if (exists) return new OutputResponse(0, "Account Group already exists.");
            await db.AccountGroups.AddAsync(entity);
            await db.SaveChangesAsync();
            return new OutputResponse(entity.AccountGroupId, null);
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
            var existing = await db.AccountGroups.FindAsync(id);
            if (existing is null) return new OutputResponse(0, "Account Group not found.");

            // Check if reserved - cannot delete built-in groups
            if (existing.IsReserved)
                return new OutputResponse(0, "Cannot delete this record because it is reserved.");

            db.AccountGroups.Remove(existing);
            await db.SaveChangesAsync();
            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }

    public async Task<List<AccountGroup>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.AccountGroups.Include(x => x.AccountHead).AsNoTracking().ToListAsync();
    }

    public async Task<(List<AccountGroup>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.AccountGroups.Include(x => x.AccountHead).AsQueryable();

        if (!string.IsNullOrEmpty(pagination.search))
        {
            query = query.Where(x => x.Name.Contains(pagination.search) || x.Code.Contains(pagination.search));
        }

        int count = await query.CountAsync();
        var list = await query
            .AsNoTracking()
            .Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize)
            .ToListAsync();
        return (list, count);
    }

    public async Task<AccountGroup> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        if (id is 0) return null!;
        var model = await db.AccountGroups.Include(x => x.AccountHead).AsNoTracking().FirstOrDefaultAsync(x => x.AccountGroupId == id);
        return model!;
    }

    public async Task<OutputResponse> UpdateAsync(AccountGroup entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.AccountGroups.FirstOrDefaultAsync(x => x.AccountGroupId == entity.AccountGroupId);
            if (existing is null) return new OutputResponse(0, "Account Group not found.");

            // Check if reserved - cannot update built-in groups
            if (existing.IsReserved)
                return new OutputResponse(0, "Cannot update this record because it is reserved.");

            existing.Code = entity.Code;
            existing.Name = entity.Name;
            existing.NameArabic = entity.NameArabic;
            existing.Remarks = entity.Remarks;
            existing.FK_AcHeadId = entity.FK_AcHeadId;

            await db.SaveChangesAsync();
            return new OutputResponse(existing.AccountGroupId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> GetNextCode(int accountHeadId)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var head = await db.AccountHeadMasters.FindAsync(accountHeadId);
            if (head == null) return new OutputResponse(0, "Account Head not found");

            int typeId = head.FK_AcTypeId;
            var type = await db.AccountTypes.FindAsync(typeId);
            if (type == null) return new OutputResponse(0, "Account Type not found");

            var groups = await db.AccountGroups.Where(x => x.FK_AcHeadId == accountHeadId).AsNoTracking().ToListAsync();
            int maxCode = 0;

            if (groups.Any())
            {
                foreach (var group in groups)
                {
                    string codePart = group.Code;
                    if (string.IsNullOrWhiteSpace(codePart)) continue;

                    string seqStr = codePart;
                    // Support "TypeId-HeadId-Seq" format
                    if (codePart.Contains("-"))
                    {
                        var parts = codePart.Split('-');
                        if (parts.Length > 0)
                        {
                            seqStr = parts.Last();
                        }
                    }
                    // Handle "G01" legacy format
                    else if (codePart.StartsWith("G", StringComparison.OrdinalIgnoreCase))
                    {
                        seqStr = codePart.Substring(1);
                    }

                    if (int.TryParse(seqStr, out int currentCode))
                    {
                        if (currentCode > maxCode) maxCode = currentCode;
                    }
                }
            }

            string nextCode = $"{head.Code}-{maxCode + 1}";
            return new OutputResponse(1, nextCode);
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message);
        }
    }
}
