using ZakiSAASERP.Interface.Services;

namespace ZakiSAASERP.Repo.Masters
{
    public class VehicleMasterRepo : IVehicleMasterRepo
    {
        private readonly IDbContextProvider contextProvider;
        private readonly ICommonImageRepo _imageRepo;
        private readonly IPermanentSettingsRepo _settingsRepo;

        public VehicleMasterRepo(
            IDbContextProvider contextProvider,
            ICommonImageRepo imageRepo,
            IPermanentSettingsRepo settingsRepo)
        {
            this.contextProvider = contextProvider;
            _imageRepo = imageRepo;
            _settingsRepo = settingsRepo;
        }

        public async Task<OutputResponse> CreateAsync(VehicleMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var exists = await db.VehicleMasters.AnyAsync(x => x.PlateNo == entity.PlateNo);
                if (exists) return new OutputResponse(0, "VehicleAlreadyExists");

                await db.VehicleMasters.AddAsync(entity);
                await db.SaveChangesAsync();

                return new OutputResponse(entity.VehicleId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<OutputResponse> DeleteAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.VehicleMasters.FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "VehicleNotFound");

                // Delete associated image if exists
                if (!string.IsNullOrWhiteSpace(existing.ImagePath))
                {
                    try
                    {
                        var basePath = await _settingsRepo.GetSetting("ImageLocalBasePath");
                        var clientFolder = await _settingsRepo.GetSetting("ImageClientFolder");
                        await _imageRepo.DeleteImageAsync(basePath, existing.ImagePath, clientFolder);
                    }
                    catch
                    {
                        // Ignore image deletion errors
                    }
                }

                db.VehicleMasters.Remove(existing);
                await db.SaveChangesAsync();

                return new OutputResponse(id, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }

        public async Task<List<VehicleMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.VehicleMasters.AsNoTracking().ToListAsync();
        }

        public async Task<(List<VehicleMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            var query = db.VehicleMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pagination.search))
            {
                var term = pagination.search.ToLower();
                query = query.Where(x => x.PlateNo.ToLower().Contains(term)
                                      || x.ChassisNo.ToLower().Contains(term)
                                      || (x.Model != null && x.Model.ToLower().Contains(term))
                                      || (x.CustomerName != null && x.CustomerName.ToLower().Contains(term))
                                      || x.Year.ToString().Contains(term)
                );
            }

            if (!string.IsNullOrWhiteSpace(pagination.sortColumn))
            {
                if (pagination.sortDirection == "desc")
                {
                    query = pagination.sortColumn switch
                    {
                        "PlateNo" => query.OrderByDescending(x => x.PlateNo),
                        "ChassisNo" => query.OrderByDescending(x => x.ChassisNo),
                        "Model" => query.OrderByDescending(x => x.Model),
                        "Year" => query.OrderByDescending(x => x.Year),
                        _ => query.OrderByDescending(x => x.VehicleId)
                    };
                }
                else
                {
                    query = pagination.sortColumn switch
                    {
                        "PlateNo" => query.OrderBy(x => x.PlateNo),
                        "ChassisNo" => query.OrderBy(x => x.ChassisNo),
                        "Model" => query.OrderBy(x => x.Model),
                        "Year" => query.OrderBy(x => x.Year),
                        _ => query.OrderBy(x => x.VehicleId)
                    };
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.VehicleId);
            }

            int count = await query.CountAsync();
            var list = await query
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .ToListAsync();
            return (list, count);
        }

        public async Task<VehicleMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.VehicleMasters.AsNoTracking().FirstOrDefaultAsync(x => x.VehicleId == id);
        }

        public async Task<OutputResponse> UpdateAsync(VehicleMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.VehicleMasters.FirstOrDefaultAsync(x => x.VehicleId == entity.VehicleId);
                if (existing is null) return new OutputResponse(0, "VehicleNotFound");

                existing.PlateNo = entity.PlateNo;
                existing.ChassisNo = entity.ChassisNo;
                existing.Model = entity.Model;
                existing.Year = entity.Year;
                existing.FK_CustomerId = entity.FK_CustomerId;
                existing.CustomerName = entity.CustomerName;

                existing.ItemImageName = entity.ItemImageName;
                existing.ImagePath = entity.ImagePath;
                existing.StorageType = entity.StorageType;

                await db.SaveChangesAsync();
                return new OutputResponse(existing.VehicleId, null);
            }
            catch (DbUpdateException ex) { return new OutputResponse(0, ex.GetBaseException().Message); }
            catch (Exception ex) { return new OutputResponse(0, ex.Message); }
        }
    }
}
