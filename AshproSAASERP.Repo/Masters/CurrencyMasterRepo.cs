namespace AshproSAASERP.Repo.Masters
{
    public class CurrencyMasterRepo(IDbContextProvider contextProvider) : ICurrencyMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(CurrencyMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.CurrencyMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Currency Master already exists.");

                await db.CurrencyMasters.AddAsync(entity);
                await db.SaveChangesAsync();
                return new OutputResponse(entity.CurrencyId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CurrencyMasters.FindAsync(id);
                if (existing is null) { return new OutputResponse(0, "Currency not found."); }

                db.CurrencyMasters.Remove(existing);
                await db.SaveChangesAsync();
                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<CurrencyMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CurrencyMasters.AsNoTracking().ToListAsync();
        }

        public async Task<(List<CurrencyMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            int count = await db.CurrencyMasters.CountAsync();
            var list = await db.CurrencyMasters
                .AsNoTracking()
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<CurrencyMaster> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            var model = await db.CurrencyMasters
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.CurrencyId == id);
            return model!;
        }

        public async Task<OutputResponse> UpdateAsync(CurrencyMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CurrencyMasters
                                       .FirstOrDefaultAsync(x => x.CurrencyId == entity.CurrencyId);

                if (existing is null) { return new OutputResponse(0, "Currency not found."); }

                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.Rate = entity.Rate;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.CurrencyId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}



