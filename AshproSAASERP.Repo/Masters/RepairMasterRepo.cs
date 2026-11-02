namespace AshproSAASERP.Repo.Masters
{
    public class RepairMasterRepo : IRepairMasterRepo
    {
        private readonly IDbContextProvider contextProvider;

        public RepairMasterRepo(IDbContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        public async Task<OutputResponse> CreateAsync(RepairMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.RepairMasters.AnyAsync(x => x.Code == entity.Code);
                if (exists) return new OutputResponse(0, "RepairAlreadyExists");

                await db.RepairMasters.AddAsync(entity);
                await db.SaveChangesAsync();

                return new OutputResponse(entity.RepairId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.RepairMasters.FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "RepairNotFound");

                db.RepairMasters.Remove(existing);
                await db.SaveChangesAsync();

                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<RepairMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.RepairMasters.AsNoTracking().ToListAsync();
        }

        public async Task<(List<RepairMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.RepairMasters.AsNoTracking();

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
                        _ => query.OrderByDescending(x => x.RepairId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        _ => query.OrderBy(x => x.RepairId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.RepairId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<RepairMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.RepairMasters.AsNoTracking().FirstOrDefaultAsync(x => x.RepairId == id);
        }

        public async Task<string> GetNextCode()
        {
            await using var db = contextProvider.CreateDbContext();
            var maxId = await db.RepairMasters.MaxAsync(x => (int?)x.RepairId) ?? 0;
            return (maxId + 1).ToString();
        }

        public async Task<OutputResponse> UpdateAsync(RepairMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.RepairMasters.FirstOrDefaultAsync(x => x.RepairId == entity.RepairId);
                if (existing is null) return new OutputResponse(0, "RepairNotFound");

                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.Remarks = entity.Remarks;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.RepairId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}
