namespace ZakiSAASERP.Repo.Masters
{
    public class FinancialYearRepo(IDbContextProvider contextProvider) : IFinancialYearRepo
    {
        public async Task<OutputResponse> CreateAsync(FinancialYear entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.FinancialYears.AnyAsync(x => x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Financial Years Master already exists.");

                await db.FinancialYears.AddAsync(entity);
                await db.SaveChangesAsync();
                return new OutputResponse(entity.FinancialYearId, null);
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
                var existing = await db.FinancialYears.FindAsync(id);
                if (existing is null) return new OutputResponse(0, "Financial year not found.");
                db.Set<FinancialYear>().Remove(existing);
                await db.SaveChangesAsync();
                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<FinancialYear>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.FinancialYears.AsNoTracking().ToListAsync();
        }

        public async Task<(List<FinancialYear>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            int count = await db.FinancialYears.CountAsync();
            var list = await db.FinancialYears
                .AsNoTracking()
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<FinancialYear> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            if (id is 0) return null!;
            var model = await db.FinancialYears.FirstOrDefaultAsync(x => x.FinancialYearId == id);
            return model!;
        }
        public async Task<int> GetMaxRefNo()
        {
            await using var db = contextProvider.CreateDbContext();
            var maxRefNo = await db.FinancialYears.MaxAsync(x => (int?)x.RefNo) ?? 0;
            return maxRefNo + 1;
        }
        public async Task<OutputResponse> UpdateAsync(FinancialYear entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.FinancialYears.FirstOrDefaultAsync(x => x.FinancialYearId == entity.FinancialYearId);
                if (existing is null) return new OutputResponse(0, "Financial year not found.");
                existing.RefNo = entity.RefNo;
                existing.FromDate = entity.FromDate;
                existing.ToDate = entity.ToDate;
                existing.Name = entity.Name;
                existing.IsClosed = entity.IsClosed;
                await db.SaveChangesAsync();
                return new OutputResponse(existing.FinancialYearId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}
