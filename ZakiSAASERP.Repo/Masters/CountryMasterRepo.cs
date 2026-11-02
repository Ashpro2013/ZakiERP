namespace ZakiSAASERP.Repo.Masters
{
    public class CountryMasterRepo(IDbContextProvider contextProvider) : ICountryMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(CountryMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.CountryMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Country Master already exists.");

                await db.CountryMasters.AddAsync(entity);
                await db.SaveChangesAsync();
                return new OutputResponse(entity.CountryId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CountryMasters.FindAsync(id);
                if (existing is null) { return new OutputResponse(0, "Country not found."); }

                db.CountryMasters.Remove(existing);
                await db.SaveChangesAsync();
                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<CountryMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CountryMasters.AsNoTracking().ToListAsync();
        }

        public async Task<(List<CountryMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.CountryMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                var term = pagination.search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(term) || x.Code.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderByDescending(x => x.Code),
                        "Name" => query.OrderByDescending(x => x.Name),
                        _ => query.OrderByDescending(x => x.CountryId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        _ => query.OrderBy(x => x.CountryId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CountryId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<CountryMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CountryMasters.AsNoTracking().FirstOrDefaultAsync(x => x.CountryId == id);
        }

        public async Task<OutputResponse> UpdateAsync(CountryMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CountryMasters.FirstOrDefaultAsync(x => x.CountryId == entity.CountryId);
                if (existing is null) { return new OutputResponse(0, "Country not found."); }

                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.NameArabic = entity.NameArabic;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.CountryId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}



