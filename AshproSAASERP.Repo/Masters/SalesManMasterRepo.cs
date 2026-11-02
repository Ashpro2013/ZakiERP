namespace AshproSAASERP.Repo.Masters
{
    public class SalesManMasterRepo(IDbContextProvider contextProvider) : ISalesManMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(SalesManMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.SalesManMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "SalesMan Master already exists.");

                await db.SalesManMasters.AddAsync(entity);
                await db.SaveChangesAsync();
                return new OutputResponse(entity.SalesmanId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.SalesManMasters.FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "Salesman not found.");

                db.SalesManMasters.Remove(existing);
                await db.SaveChangesAsync();

                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
        public async Task<List<SalesManMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.SalesManMasters
                .AsNoTracking()
                .Select(x => new SalesManMaster
                {
                    SalesmanId = x.SalesmanId,
                    Code = x.Code,
                    Name = x.Name,
                    Mobile = x.Mobile,
                    Email = x.Email,
                    Designation = x.Designation,

                    // DO NOT LOAD IMAGES
                    Remarks = x.Remarks,
                    NameArabic = x.NameArabic,

                    SignatureImage = null,
                    SealImage = null,
                    SalesmanImage = null
                })
                .ToListAsync();
        }

        public async Task<(List<SalesManMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.SalesManMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                var term = pagination.search.ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(term) || x.Name.ToLower().Contains(term) || x.Mobile.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderByDescending(x => x.Code),
                        "Name" => query.OrderByDescending(x => x.Name),
                        "Mobile" => query.OrderByDescending(x => x.Mobile),
                        "Designation" => query.OrderByDescending(x => x.Designation),
                        _ => query.OrderByDescending(x => x.SalesmanId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        "Mobile" => query.OrderBy(x => x.Mobile),
                        "Designation" => query.OrderBy(x => x.Designation),
                        _ => query.OrderBy(x => x.SalesmanId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.SalesmanId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .Select(x => new SalesManMaster
                {
                    SalesmanId = x.SalesmanId,
                    Code = x.Code,
                    Name = x.Name,
                    Mobile = x.Mobile,
                    Email = x.Email,
                    Designation = x.Designation,
                    Remarks = x.Remarks,
                    NameArabic = x.NameArabic,
                    SignatureImage = null,
                    SealImage = null,
                    SalesmanImage = null
                })
                .ToListAsync();
            return (list, count);
        }

        public async Task<SalesManMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();

            return await db.SalesManMasters
                           .AsNoTracking()
                           .FirstOrDefaultAsync(x => x.SalesmanId == id);
        }

        public async Task<OutputResponse> UpdateAsync(SalesManMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.SalesManMasters
                                       .FirstOrDefaultAsync(x => x.SalesmanId == entity.SalesmanId);

                if (existing is null)
                    return new OutputResponse(0, "Salesman not found.");

                // Update fields
                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.Remarks = entity.Remarks;
                existing.NameArabic = entity.NameArabic;
                existing.Designation = entity.Designation;
                existing.Mobile = entity.Mobile;
                existing.Email = entity.Email;
                existing.SignatureImage = entity.SignatureImage;
                existing.SealImage = entity.SealImage;
                existing.SalesmanImage = entity.SalesmanImage;

                await db.SaveChangesAsync();

                return new OutputResponse(existing.SalesmanId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}



