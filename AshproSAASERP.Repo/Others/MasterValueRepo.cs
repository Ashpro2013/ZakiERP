using AshproSAASERP.Models.Others;

namespace AshproSAASERP.Repo.Others;

public class MasterValueRepo(IDbContextProvider contextProvider) : IMasterValueRepo
{
    public async Task<OutputResponse> AddAsync(MasterValue value)
    {
        await using var db = contextProvider.CreateDbContext();
        try
        {
            var exist = await db.MasterValues.FirstOrDefaultAsync(x => x.MasterType == value.MasterType && x.BranchId == value.BranchId);
            if(exist != null)
            {
                exist.FieldOne = value.FieldOne;
                exist.FieldTwo = value.FieldTwo;
                exist.FieldThree = value.FieldThree;
                exist.FieldFour = value.FieldFour;
                exist.Name = value.Name;
                exist.Description = value.Description;
                db.MasterValues.Update(exist);
                await db.SaveChangesAsync();
                return new OutputResponse(exist.Id, "", exist);
            }
            else
            {
                await db.MasterValues.AddAsync(value);
                await db.SaveChangesAsync();
                return new OutputResponse(value.Id, "", value);
            }
        }
        catch (Exception ex)
        {
            return new OutputResponse(0, ex.Message, null);
        }
    }
    public async Task<MasterValue?> GetAsync(string key, int? BranchId)
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.MasterValues
            .Where(x => x.MasterType == key && (BranchId == null || x.BranchId == BranchId))
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(MasterValue value)
    {
        await using var db = contextProvider.CreateDbContext();
        db.MasterValues.Update(value);
        await db.SaveChangesAsync();
    }

    public async Task UpdatePreviousHashAsync(string hash, int BranchId)
    {
        await using var db = contextProvider.CreateDbContext();
        await db.MasterValues.Where(x => x.MasterType == "CSRDetails" && x.BranchId == BranchId).ExecuteUpdateAsync(u => u
         .SetProperty(p => p.FieldFour, hash));
    }
}



