namespace ZakiSAASERP.Repo.Masters
{
    public class PurchaserMasterRepo(IDbContextProvider contextProvider) : IPurchaserMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(PurchaserMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.PurchaserMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Purchaser Master already exists.");

                await db.PurchaserMasters.AddAsync(entity);
                await db.SaveChangesAsync();
                return new OutputResponse(entity.PurchaserId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.PurchaserMasters.FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "Purchaser not found.");

                db.PurchaserMasters.Remove(existing);
                await db.SaveChangesAsync();

                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<PurchaserMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.PurchaserMasters
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<(List<PurchaserMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.PurchaserMasters.AsNoTracking();

            if (!string.IsNullOrEmpty(pagination.search))
            {
                query = query.Where(x => x.Code.Contains(pagination.search) ||
                                         x.Name.Contains(pagination.search) ||
                                         x.NameArabic.Contains(pagination.search));
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderByDescending(x => x.Code),
                        "Name" => query.OrderByDescending(x => x.Name),
                        "NameArabic" => query.OrderByDescending(x => x.NameArabic),
                        "Remarks" => query.OrderByDescending(x => x.Remarks),
                        "CreditLimit" => query.OrderByDescending(x => x.CreditLimit),
                        _ => query.OrderByDescending(x => x.PurchaserId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        "NameArabic" => query.OrderBy(x => x.NameArabic),
                        "Remarks" => query.OrderBy(x => x.Remarks),
                        "CreditLimit" => query.OrderBy(x => x.CreditLimit),
                        _ => query.OrderBy(x => x.PurchaserId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.PurchaserId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<PurchaserMaster> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();

            var model = await db.PurchaserMasters
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.PurchaserId == id);

            return model!;
        }

        public async Task<OutputResponse> UpdateAsync(PurchaserMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.PurchaserMasters
                                       .FirstOrDefaultAsync(x => x.PurchaserId == entity.PurchaserId);

                if (existing is null)
                    return new OutputResponse(0, "Purchaser not found.");

                // Update fields
                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.NameArabic = entity.NameArabic;
                existing.Remarks = entity.Remarks;
                existing.CreditLimit = entity.CreditLimit;
                existing.Description = entity.Description;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.PurchaserId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}



