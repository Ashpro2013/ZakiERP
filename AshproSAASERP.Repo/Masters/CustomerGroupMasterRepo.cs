namespace AshproSAASERP.Repo.Masters
{
    public class CustomerGroupMasterRepo(IDbContextProvider contextProvider) : ICustomerGroupMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(CustomerGroupMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.CustomerGroupMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Customer Group Master already exists.");

                await db.CustomerGroupMasters.AddAsync(entity);
                await db.SaveChangesAsync();

                return new OutputResponse(entity.CustomerGroupId, null);
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
                var existing = await db.CustomerGroupMasters.FindAsync(id);
                if (existing is null)
                {
                    return new OutputResponse(0, "Customer Group not found.");
                }

                db.CustomerGroupMasters.Remove(existing);
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

        public async Task<List<CustomerGroupMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CustomerGroupMasters
                           .AsNoTracking()
                           .ToListAsync();
        }

        public async Task<(List<CustomerGroupMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.CustomerGroupMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                var term = pagination.search.ToLower();
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Contains(term)) || (x.Code != null && x.Code.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderByDescending(x => x.Code),
                        "Name" => query.OrderByDescending(x => x.Name),
                        _ => query.OrderByDescending(x => x.CustomerGroupId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        _ => query.OrderBy(x => x.CustomerGroupId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CustomerGroupId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<CustomerGroupMaster> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();

            var model = await db.CustomerGroupMasters
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.CustomerGroupId == id);

            return model!;
        }

        public async Task<OutputResponse> UpdateAsync(CustomerGroupMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CustomerGroupMasters
                                       .FirstOrDefaultAsync(x => x.CustomerGroupId == entity.CustomerGroupId);

                if (existing is null)
                {
                    return new OutputResponse(0, "Customer Group not found.");
                }

                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.NameArabic = entity.NameArabic;
                existing.Remarks = entity.Remarks;

                await db.SaveChangesAsync();

                return new OutputResponse(existing.CustomerGroupId, null);
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
    }
}



