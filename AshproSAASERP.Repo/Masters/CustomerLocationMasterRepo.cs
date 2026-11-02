namespace AshproSAASERP.Repo.Masters
{
    public class CustomerLocationMasterRepo(IDbContextProvider contextProvider) : ICustomerLocationMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(CustomerLocationMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.CustomerLocationMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Customer Location Master already exists.");

                await db.CustomerLocationMasters.AddAsync(entity);
                await db.SaveChangesAsync();
                return new OutputResponse(entity.LocationId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CustomerLocationMasters.FindAsync(id);
                if (existing is null) { return new OutputResponse(0, "Location not found."); }

                db.CustomerLocationMasters.Remove(existing);
                await db.SaveChangesAsync();
                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<CustomerLocationMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CustomerLocationMasters.AsNoTracking().ToListAsync();
        }

        public async Task<(List<CustomerLocationMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.CustomerLocationMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                query = query.Where(x => x.Name.Contains(pagination.search) || x.Code.Contains(pagination.search));
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderByDescending(x => x.Code),
                        "Name" => query.OrderByDescending(x => x.Name),
                        _ => query.OrderByDescending(x => x.LocationId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        _ => query.OrderBy(x => x.LocationId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.LocationId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<CustomerLocationMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CustomerLocationMasters.AsNoTracking().FirstOrDefaultAsync(x => x.LocationId == id);
        }

        public async Task<OutputResponse> UpdateAsync(CustomerLocationMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CustomerLocationMasters.FirstOrDefaultAsync(x => x.LocationId == entity.LocationId);
                if (existing is null) { return new OutputResponse(0, "Location not found."); }

                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.Remarks = entity.Remarks;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.LocationId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}



