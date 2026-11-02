namespace ZakiSAASERP.Repo.Masters
{
    public class TechnicianMasterRepo : ITechnicianMasterRepo
    {
        private readonly IDbContextProvider contextProvider;

        public TechnicianMasterRepo(IDbContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        public async Task<OutputResponse> CreateAsync(TechnicianMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.Set<TechnicianMaster>().AnyAsync(x => x.Code == entity.Code);
                if (exists) return new OutputResponse(0, "Technician Code Already Exists");

                await db.Set<TechnicianMaster>().AddAsync(entity);
                await db.SaveChangesAsync();

                return new OutputResponse(entity.TechnicianId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.Set<TechnicianMaster>().FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "Technician Not Found");

                db.Set<TechnicianMaster>().Remove(existing);
                await db.SaveChangesAsync();

                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<TechnicianMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.Set<TechnicianMaster>().AsNoTracking().ToListAsync();
        }

        public async Task<(List<TechnicianMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.Set<TechnicianMaster>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                var term = pagination.search.ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(term) || x.Name.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderByDescending(x => x.Code),
                        "Name" => query.OrderByDescending(x => x.Name),
                        _ => query.OrderByDescending(x => x.TechnicianId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        _ => query.OrderBy(x => x.TechnicianId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.TechnicianId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<TechnicianMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.Set<TechnicianMaster>().AsNoTracking().FirstOrDefaultAsync(x => x.TechnicianId == id);
        }

        public async Task<string> GetNextCode()
        {
            await using var db = contextProvider.CreateDbContext();
            var maxId = await db.Set<TechnicianMaster>().MaxAsync(x => (int?)x.TechnicianId) ?? 0;
            return (maxId + 1).ToString();
        }

        public async Task<OutputResponse> UpdateAsync(TechnicianMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.Set<TechnicianMaster>().FirstOrDefaultAsync(x => x.TechnicianId == entity.TechnicianId);
                if (existing is null) return new OutputResponse(0, "Technician Not Found");

                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.Position = entity.Position;
                existing.Mobile = entity.Mobile;
                existing.Remarks = entity.Remarks;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.TechnicianId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}
