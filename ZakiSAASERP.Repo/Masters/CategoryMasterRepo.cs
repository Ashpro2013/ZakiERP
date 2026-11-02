using ZakiSAASERP.Interface.Services;
using ZakiSAASERP.Repo.CodeGenerationServices;

namespace ZakiSAASERP.Repo.Masters
{
    public class CategoryMasterRepo : ICategoryMasterRepo
    {
        private readonly IDbContextProvider contextProvider;
        private readonly CodeSequenceService _codeService;
        private readonly ICommonImageRepo _imageRepo;
        private readonly IPermanentSettingsRepo _settingsRepo;

        public CategoryMasterRepo(
            IDbContextProvider contextProvider,
            CodeSequenceService codeService,
            ZakiSAASERP.Interface.Services.ICommonImageRepo imageRepo,
            IPermanentSettingsRepo settingsRepo)
        {
            this.contextProvider = contextProvider;
            _codeService = codeService;
            _imageRepo = imageRepo;
            _settingsRepo = settingsRepo;
        }

        public async Task<OutputResponse> CreateAsync(CategoryMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.CategoryMasters.AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);
                if (exists) return new OutputResponse(0, "Category Master already exists.");

                await db.CategoryMasters.AddAsync(entity);
                await db.SaveChangesAsync();

                // Increment code sequence if auto-code is enabled
                if (await _codeService.IsAutoCodeEnabledAsync(CodeKeys.Category, db))
                {
                    await _codeService.ConsumeCodeAsync(CodeKeys.Category, db);
                }

                return new OutputResponse(entity.CategoryId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CategoryMasters.FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "Category not found.");

                db.CategoryMasters.Remove(existing);
                await db.SaveChangesAsync();

                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<CategoryMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.CategoryMasters
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<CategoryMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.CategoryMasters.AsNoTracking();

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
                        _ => query.OrderByDescending(x => x.CategoryId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "Code" => query.OrderBy(x => x.Code),
                        "Name" => query.OrderBy(x => x.Name),
                        _ => query.OrderBy(x => x.CategoryId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CategoryId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .Select(x => new CategoryMaster
                {
                    CategoryId = x.CategoryId,
                    Code = x.Code,
                    Name = x.Name,
                    NameArabic = x.NameArabic,
                    Remarks = x.Remarks,
                    IsFixedAsset = x.IsFixedAsset,
                })
                .ToListAsync();
            return (list, count);
        }

        public async Task<CategoryMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();

            return await db.CategoryMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CategoryId == id);
        }

        public async Task<OutputResponse> UpdateAsync(CategoryMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.CategoryMasters
                                       .FirstOrDefaultAsync(x => x.CategoryId == entity.CategoryId);

                if (existing is null)
                    return new OutputResponse(0, "Category not found.");

                // Update fields
                existing.Code = entity.Code;
                existing.Name = entity.Name;
                existing.NameArabic = entity.NameArabic;
                existing.Remarks = entity.Remarks;
                existing.IsFixedAsset = entity.IsFixedAsset;

                existing.ItemImage = entity.ItemImage;
                await db.SaveChangesAsync();

                return new OutputResponse(existing.CategoryId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        /// <summary>
        /// Gets the next auto-generated category code (preview only - does not consume the number)
        /// Checks for existing codes and increments until finding a unique one
        /// </summary>
        public async Task<string> GetNextCategoryCodeAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            // Pass lambda to check if code already exists in CategoryMaster table
            return await _codeService.GetNextUniqueCodeAsync(
                CodeKeys.Category,
                async (code) => await db.CategoryMasters.AnyAsync(x => x.Code == code)
            );
        }
    }
}
