namespace AshproSAASERP.Repo.Masters
{
    public class ProjectMasterRepo(IDbContextProvider contextProvider) : IProjectMasterRepo
    {
        public async Task<OutputResponse> CreateAsync(ProjectMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                // Check for duplicate Code or Name
                var exists = await db.ProjectMasters
                    .AnyAsync(x => x.Code == entity.Code || x.Name == entity.Name);

                if (exists)
                    return new OutputResponse(0, "Project Master already exists.");

                await db.ProjectMasters.AddAsync(entity);
                await db.SaveChangesAsync();

                return new OutputResponse(entity.ProjectId, null);
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
                var existing = await db.ProjectMasters.FindAsync(id);

                if (existing is null)
                    return new OutputResponse(0, "Project not found.");

                db.ProjectMasters.Remove(existing);
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

        public async Task<List<ProjectMaster>> GetAllAsync()
        {
            await using var db = contextProvider.CreateDbContext();
            return await db.ProjectMasters
                .AsNoTracking()
                .Select(x => new ProjectMaster
                {
                    ProjectId = x.ProjectId,
                    Code = x.Code,
                    Name = x.Name,

                    FK_CustomerId = x.FK_CustomerId,
                    FK_SalesmanId = x.FK_SalesmanId,
                    FK_PurchaserId = x.FK_PurchaserId,

                    Amount = x.Amount,
                    ContractValue = x.ContractValue,

                    Description = x.Description,

                    ExpectedStrtDate = x.ExpectedStrtDate,
                    ExpectedEndDate = x.ExpectedEndDate,
                    ActualStrtDate = x.ActualStrtDate,
                    ActualEndDate = x.ActualEndDate,

                    Status = x.Status
                })
                .ToListAsync();
        }

        public async Task<(List<ProjectMaster>, int)> GetAllWithPagination(PaginationModel pagination)
        {
            await using var db = contextProvider.CreateDbContext();
            int count = await db.ProjectMasters.CountAsync();

            var list = await db.ProjectMasters
                .AsNoTracking()
                .Skip((pagination.page - 1) * pagination.pageSize)
                .Take(pagination.pageSize)
                .Select(x => new ProjectMaster
                {
                    ProjectId = x.ProjectId,
                    Code = x.Code,
                    Name = x.Name,

                    FK_CustomerId = x.FK_CustomerId,
                    FK_SalesmanId = x.FK_SalesmanId,
                    FK_PurchaserId = x.FK_PurchaserId,

                    Amount = x.Amount,
                    ContractValue = x.ContractValue,
                    Description = x.Description,

                    ExpectedStrtDate = x.ExpectedStrtDate,
                    ExpectedEndDate = x.ExpectedEndDate,
                    ActualStrtDate = x.ActualStrtDate,
                    ActualEndDate = x.ActualEndDate,

                    Status = x.Status
                })
                .ToListAsync();

            return (list, count);
        }

        public async Task<ProjectMaster?> GetAsync(int id)
        {
            await using var db = contextProvider.CreateDbContext();

            return await db.ProjectMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectId == id);
        }

        public async Task<OutputResponse> UpdateAsync(ProjectMaster entity)
        {
            await using var db = contextProvider.CreateDbContext();
            try
            {
                var existing = await db.ProjectMasters
                    .FirstOrDefaultAsync(x => x.ProjectId == entity.ProjectId);

                if (existing is null)
                    return new OutputResponse(0, "Project not found.");

                // Update fields
                existing.Code = entity.Code;
                existing.Name = entity.Name;

                existing.FK_CustomerId = entity.FK_CustomerId;
                existing.FK_SalesmanId = entity.FK_SalesmanId;
                existing.FK_PurchaserId = entity.FK_PurchaserId;

                existing.Amount = entity.Amount;
                existing.ContractValue = entity.ContractValue;

                existing.Description = entity.Description;

                existing.ExpectedStrtDate = entity.ExpectedStrtDate;
                existing.ExpectedEndDate = entity.ExpectedEndDate;
                existing.ActualStrtDate = entity.ActualStrtDate;
                existing.ActualEndDate = entity.ActualEndDate;

                existing.Status = entity.Status;

                await db.SaveChangesAsync();

                return new OutputResponse(existing.ProjectId, null);
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



