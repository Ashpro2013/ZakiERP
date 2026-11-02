namespace AshproSAASERP.Repo.Masters
{
    public class BrandMasterRepo(IDbContextProvider contextProvider) : IBrandMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(BrandMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.BrandMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Brand Master already exists.");

                await db.BrandMasters.AddAsync(entity);
                await db.SaveChangesAsync();
                return new OutputResponse(entity.BrandId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.BrandMasters.FindAsync(id);
                if (existing is null) { return new OutputResponse(0, "Brand not found."); }

                db.BrandMasters.Remove(existing);
                await db.SaveChangesAsync();
                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<BrandMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.BrandMasters.AsNoTracking().ToListAsync();
        }

        public async Task<(List<BrandMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            int count = await db.BrandMasters.CountAsync();
            var list = await db.BrandMasters
                .AsNoTracking()
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<BrandMaster> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            var model = await db.BrandMasters.AsNoTracking().FirstOrDefaultAsync(x => x.BrandId == id);
            return model!;
        }

        public async Task<OutputResponse> UpdateAsync(BrandMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.BrandMasters.FirstOrDefaultAsync(x => x.BrandId == entity.BrandId);
                if (existing is null) { return new OutputResponse(0, "Brand not found."); }

                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.NameArabic = entity.NameArabic;
                existing.Remarks = entity.Remarks;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.BrandId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}



