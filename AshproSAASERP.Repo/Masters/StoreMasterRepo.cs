
namespace AshproSAASERP.Repo.Masters
{
    public class StoreMasterRepo : IStoreMasterRepo
    {
        private readonly IDbContextProvider contextProvider;

        public StoreMasterRepo(IDbContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        public async Task<List<StoreMaster>> GetAllAsync()
        {
            await using var _context = contextProvider.CreateDbContext();
            return await _context.StoreMasters.Include(x => x.Branch).ToListAsync();
        }

        public async Task<OutputResponse> CreateAsync(StoreMaster store)
        {
            await using var _context = contextProvider.CreateDbContext();
            try
            {
                // Logic: Check if Code or Name already exists
                if (await _context.StoreMasters.AnyAsync(x => x.Code == store.Code))
                {
                    return new OutputResponse(0, "StoreCodeExists");
                }
                if (await _context.StoreMasters.AnyAsync(x => x.Name == store.Name && x.FK_BranchId == store.FK_BranchId))
                {
                    return new OutputResponse(0, "StoreNameExists");
                }

                // Logic: Only one default store allowed
                if (store.IsDefaultStore)
                {
                    var exists = await CheckIfDefaultStoreExists(_context, 0, store.FK_BranchId);
                    if (exists)
                    {
                        return new OutputResponse(0, "DefaultStoreExists");
                    }
                }

                _context.StoreMasters.Add(store);
                await _context.SaveChangesAsync();
                return new OutputResponse(store.StoreId, null);
            }
            catch (Exception ex)
            {
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<OutputResponse> UpdateAsync(StoreMaster store)
        {
            await using var _context = contextProvider.CreateDbContext();
            try
            {
                // Logic: Check if Code or Name already exists
                if (await _context.StoreMasters.AnyAsync(x => x.Code == store.Code && x.StoreId != store.StoreId))
                {
                    return new OutputResponse(0, "StoreCodeExists");
                }
                if (await _context.StoreMasters.AnyAsync(x => x.Name == store.Name && x.StoreId != store.StoreId && x.FK_BranchId == store.FK_BranchId))
                {
                    return new OutputResponse(0, "StoreNameExists");
                }

                // Logic: Only one default store allowed
                if (store.IsDefaultStore)
                {
                    var exists = await CheckIfDefaultStoreExists(_context, store.StoreId, store.FK_BranchId);
                    if (exists)
                    {
                        return new OutputResponse(0, "DefaultStoreExists");
                    }
                }

                _context.StoreMasters.Update(store);
                await _context.SaveChangesAsync();
                return new OutputResponse(store.StoreId, null);
            }
            catch (Exception ex)
            {
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var _context = contextProvider.CreateDbContext();
            try
            {
                var store = await _context.StoreMasters.FindAsync(id);
                if (store != null)
                {
                    _context.StoreMasters.Remove(store);
                    await _context.SaveChangesAsync();
                    return new OutputResponse(id, null);
                }
                return new OutputResponse(0, "RecordNotFound");
            }
            catch (Exception ex)
            {
                return new OutputResponse(0, ex.Message);
            }
        }

        public async Task<StoreMaster?> GetAsync(int id)
        {
            await using var _context = contextProvider.CreateDbContext();
            return await _context.StoreMasters.Include(x => x.Branch).FirstOrDefaultAsync(x => x.StoreId == id);
        }

        public async Task<(List<StoreMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var _context = contextProvider.CreateDbContext();
            var query = _context.StoreMasters.Include(x => x.Branch).AsQueryable();

            if (!string.IsNullOrEmpty(pagination.search))
            {
                query = query.Where(x => x.Name.Contains(pagination.search) || x.Code.Contains(pagination.search));
            }

            // Simple sorting logic
            if (!string.IsNullOrEmpty(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    switch (pagination.sortColumn.ToLower())
                    {
                        case "name": query = query.OrderByDescending(x => x.Name); break;
                        case "code": query = query.OrderByDescending(x => x.Code); break;
                        default: query = query.OrderByDescending(x => x.StoreId); break;
                    }
                }
                else
                {
                    switch (pagination.sortColumn.ToLower())
                    {
                        case "name": query = query.OrderBy(x => x.Name); break;
                        case "code": query = query.OrderBy(x => x.Code); break;
                        default: query = query.OrderBy(x => x.StoreId); break;
                    }
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.StoreId);
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pagination.page - 1) * pagination.pageSize).Take(pagination.pageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> CheckIfDefaultStoreExists(int excludeStoreId = 0, int? branchId = null)
        {
            await using var db = contextProvider.CreateDbContext();
            return await CheckIfDefaultStoreExists(db, excludeStoreId, branchId);
        }

        public async Task<bool> CheckIfDefaultStoreExists(TenantDBContext _context, int excludeStoreId = 0, int? branchId = null)
        {
            return await _context.StoreMasters
                .AnyAsync(x => x.IsDefaultStore && x.StoreId != excludeStoreId && x.FK_BranchId == branchId);
        }
    }
}
