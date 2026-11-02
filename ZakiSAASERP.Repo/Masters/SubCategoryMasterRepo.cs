using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Masters
{
    public class SubCategoryMasterRepo(IDbContextProvider contextProvider, CodeSequenceService _codeService) : ISubCategoryMasterRepo
    {

        public async Task<OutputResponse> CreateAsync(SubCategoryMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                // Check for existing code within the same category (unique per category)
                var exists = await db.SubCategoryMasters.AnyAsync(x =>
                    (x.Code == entity.Code && x.FK_CategoryId == entity.FK_CategoryId) ||
                    x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Sub Category Master already exists.");

                await db.SubCategoryMasters.AddAsync(entity);
                await db.SaveChangesAsync();

                // Consume code sequence after successful save
                await _codeService.ConsumeSubCategoryCodeAsync(entity.FK_CategoryId, db);

                return new OutputResponse(entity.SubCategoryId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }


        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.SubCategoryMasters.FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "Sub Category not found.");

                db.SubCategoryMasters.Remove(existing);
                await db.SaveChangesAsync();

                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<SubCategoryMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.SubCategoryMasters
                .AsNoTracking()
                .Select(x => new SubCategoryMaster
                {
                    SubCategoryId = x.SubCategoryId,
                    Code = x.Code,
                    Name = x.Name,
                    NameArabic = x.NameArabic,
                    FK_CategoryId = x.FK_CategoryId,
                    Remarks = x.Remarks,
                    IsFixedAsset = x.IsFixedAsset,
                    ItemImageName = x.ItemImageName,

                    // Do not load heavy image
                    ItemImage = null
                })
                .ToListAsync();
        }

        public async Task<(List<SubCategoryMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.SubCategoryMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                var term = pagination.search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(term) || x.Code.ToLower().Contains(term));
            }

            if (pagination.FilterId.HasValue)
            {
                query = query.Where(x => x.FK_CategoryId == pagination.FilterId.Value);
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderByDescending(x => x.Code),
                        "Name" => query.OrderByDescending(x => x.Name),
                        _ => query.OrderByDescending(x => x.SubCategoryId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        _ => query.OrderBy(x => x.SubCategoryId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.SubCategoryId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .Select(x => new SubCategoryMaster
                {
                    SubCategoryId = x.SubCategoryId,
                    Code = x.Code,
                    Name = x.Name,
                    NameArabic = x.NameArabic,
                    FK_CategoryId = x.FK_CategoryId,
                    Remarks = x.Remarks,
                    IsFixedAsset = x.IsFixedAsset,
                    ItemImageName = x.ItemImageName,
                    ItemImage = null,
                    Category = x.FK_CategoryId == null || x.Category == null ? null : new CategoryMaster
                    {
                        // Removed CategoryId assignment to prevent SqlNullValueException from GetInt32
                        Code = x.Category.Code,
                        Name = x.Category.Name,
                        IsFixedAsset = x.Category.IsFixedAsset
                    }
                })
                .ToListAsync();
            return (list, count);
        }

        public async Task<SubCategoryMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();

            return await db.SubCategoryMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SubCategoryId == id);
        }

        public async Task<OutputResponse> UpdateAsync(SubCategoryMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.SubCategoryMasters
                    .FirstOrDefaultAsync(x => x.SubCategoryId == entity.SubCategoryId);

                if (existing is null)
                    return new OutputResponse(0, "Sub Category not found.");

                // Update fields
                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.NameArabic = entity.NameArabic;

                existing.FK_CategoryId = entity.FK_CategoryId;

                existing.Remarks = entity.Remarks;

                existing.IsFixedAsset = entity.IsFixedAsset;

                existing.ItemImageName = entity.ItemImageName;
                existing.ItemImage = entity.ItemImage;

                await db.SaveChangesAsync();

                return new OutputResponse(existing.SubCategoryId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        /// <summary>
        /// Gets the next auto-generated subcategory code for the specified category.
        /// SubCategory codes are unique per category, not globally.
        /// </summary>
        public async Task<string> GetNextSubCategoryCodeAsync(int? categoryId, string? categoryCode)
        {
            await using var db = contextProvider.CreateDbContext();
            return await _codeService.GetNextSubCategoryCodeAsync(categoryId, categoryCode, db);
        }
    }
}




