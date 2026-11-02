namespace ZakiSAASERP.Repo.Masters;

public class AccountSubGroupRepo(IDbContextProvider contextProvider) : IAccountSubGroupRepo
{
    public async Task<OutputResponse> CreateAsync(AccountSubGroup entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var exists = await db.AccountSubGroups.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
            if (exists) return new OutputResponse(0, "Account SubGroup already exists.");
            await db.AccountSubGroups.AddAsync(entity);
            await db.SaveChangesAsync();
            return new OutputResponse(entity.SubGroupId, null);
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
            var existing = await db.AccountSubGroups.FindAsync(id);
            if (existing is null) return new OutputResponse(0, "Account SubGroup not found.");

            // Check if reserved - cannot delete built-in sub groups
            if (existing.IsReserved)
                return new OutputResponse(0, "Cannot delete this record because it is reserved.");

            db.AccountSubGroups.Remove(existing);
            await db.SaveChangesAsync();
            return new OutputResponse(id, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }

    public async Task<List<AccountSubGroup>> GetAllAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.AccountSubGroups.Include(x => x.AccountGroup).Include(x => x.AccountHead).AsNoTracking().ToListAsync();
    }

    public async Task<(List<AccountSubGroup>, int)> GetAllWithPagination(PaginationModel pagination)
    {
        await using var db = contextProvider.CreateDbContext();
        var query = db.AccountSubGroups.Include(x => x.AccountGroup).Include(x => x.AccountHead).AsQueryable();

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

    public async Task<AccountSubGroup> GetAsync(int id)
    {
        await using var db = contextProvider.CreateDbContext();
        if (id is 0) return null!;
        var model = await db.AccountSubGroups.Include(x => x.AccountGroup).Include(x => x.AccountHead).AsNoTracking().FirstOrDefaultAsync(x => x.SubGroupId == id);
        return model!;
    }

    public async Task<OutputResponse> UpdateAsync(AccountSubGroup entity)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var existing = await db.AccountSubGroups.FirstOrDefaultAsync(x => x.SubGroupId == entity.SubGroupId);
            if (existing is null) return new OutputResponse(0, "Account SubGroup not found.");

            // Check if reserved - cannot update built-in sub groups
            if (existing.IsReserved)
                return new OutputResponse(0, "Cannot update this record because it is reserved.");

            existing.Code = entity.Code;
            existing.Name = entity.Name;
            existing.NameArabic = entity.NameArabic;
            existing.Remarks = entity.Remarks;
            existing.FK_AcGroupId = entity.FK_AcGroupId;
            existing.FK_AcHeadId = entity.FK_AcHeadId;

            await db.SaveChangesAsync();
            return new OutputResponse(existing.SubGroupId, null);
        }
        catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
        catch (Exception ex) { return new OutputResponse(0, ex.Message); }
    }
    public async Task<OutputResponse> GetNextCode(int accountGroupId)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var group = await db.AccountGroups.Include(x => x.AccountHead).AsNoTracking().FirstOrDefaultAsync(x => x.AccountGroupId == accountGroupId);
            if (group == null) return new OutputResponse(0, "Account Group not found");

            var head = group?.AccountHead;
            if (head == null) return new OutputResponse(0, "Account Head not found in Group");

            int typeId = head.FK_AcTypeId;

            var subGroups = await db.AccountSubGroups.Where(x => x.FK_AcGroupId == accountGroupId).AsNoTracking().ToListAsync();
            int maxCode = 0;

            if (subGroups.Any())
            {
                foreach (var sub in subGroups)
                {
                    string codePart = sub.Code;
                    if (string.IsNullOrWhiteSpace(codePart)) continue;

                    string seqStr = codePart;
                    // Format: "TypeId-HeadCode-GroupCode-Seq"
                    // We simply assume the last part is the sequence number for finding max.
                    if (codePart.Contains("-"))
                    {
                        var parts = codePart.Split('-');
                        if (parts.Length > 0)
                        {
                            seqStr = parts.Last();
                        }
                    }
                    // Support legacy simple format if present
                    else if (codePart.StartsWith("G", StringComparison.OrdinalIgnoreCase) || codePart.StartsWith("S", StringComparison.OrdinalIgnoreCase))
                    {
                        // Basic heuristic, but existing logic for Group used implicit assumption. 
                        // For sub groups, if they were simple like "S01", remove S.
                        seqStr = System.Text.RegularExpressions.Regex.Replace(codePart, "[^0-9]", "");
                    }

                    if (int.TryParse(seqStr, out int currentCode))
                    {
                        if (currentCode > maxCode) maxCode = currentCode;
                    }
                }
            }

            // Requested Format: TypeId-AccountHeadCode-AccountGroupCode-AccountSubGrpCode
            // Note: group.Code is used as the 'AccountGroupCode' component.
            // We use D3 for the sequence to allow for more subgroups and cleaner sorting, common in ERPs. 
            // If the user prefers simpler "1", they can adjust, but D3 is safer for "001" style mentioned in samples.
            // Using maxCode + 1 without padding to match AccountGroupRepo exactly would be: $"{maxCode + 1}"
            // But user sample "1007-001" sequence part was "001". I'll use D3.

            string nextCode = $"{group.Code}-{maxCode + 1:D3}";

            return new OutputResponse(1, nextCode);
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message);
        }
    }
}



