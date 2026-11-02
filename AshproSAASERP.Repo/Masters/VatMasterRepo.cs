namespace AshproSAASERP.Repo.Masters
{
    public class VatMasterRepo(IDbContextProvider contextProvider) : IVatMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(VatMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.VatMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Vat Master already exists.");

                await db.VatMasters.AddAsync(entity);
                await db.SaveChangesAsync();
                return new OutputResponse(entity.VatId, null);
            }
            catch (DbUpdateException ex)
            {
                return new OutputResponse(0, ex.GetBaseException().Message);
            }
            catch (Exception ex)
            {
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.VatMasters.FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "VAT entry not found.");

                db.VatMasters.Remove(existing);
                await db.SaveChangesAsync();
                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex)
            {
                return new OutputResponse(0, ex.GetBaseException().Message);
            }
            catch (Exception ex)
            {
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<List<VatMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.VatMasters.AsNoTracking().ToListAsync();
        }

        public async Task<(List<VatMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            int count = await db.VatMasters.CountAsync();
            var list = await db.VatMasters
                .AsNoTracking()
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<VatMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.VatMasters.AsNoTracking().FirstOrDefaultAsync(x => x.VatId == id);
        }

        public async Task<OutputResponse> UpdateAsync(VatMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.VatMasters.FirstOrDefaultAsync(x => x.VatId == entity.VatId);

                if (existing is null)
                    return new OutputResponse(0, "VAT entry not found.");

                existing.Code = entity.Code;
                existing.Percentage = entity.Percentage;
                existing.Name = entity.Name;
                existing.IsDefault = entity.IsDefault;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.VatId, null);
            }
            catch (DbUpdateException ex)
            {
                return new OutputResponse(0, ex.GetBaseException().Message);
            }
            catch (Exception ex)
            {
                return new OutputResponse(0, ex.Message);
            }
        }
        public async Task<List<Models.Masters.VATCategoryDetails>> GetVatCategoryDetails(string code)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.VATCategoryDetails.AsNoTracking().Where(x => x.Code == code).ToListAsync();
        }
    }
}



