using AshproSAASERP.Models.Company;

namespace AshproSAASERP.Repo.LoginLogic;

public class CompanyCreationRepo(MasterDBContext db) : ICompanyCreationRepo
{
    public async Task<LoginCompany> CreateAsync(LoginCompany entity)
    {
        await db.LoginCompanies.AddAsync(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<LoginCompany> DeleteAsync(int id)
    {
        var existing = await db.LoginCompanies.FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return null!;
        db.LoginCompanies.Remove(existing);
        await db.SaveChangesAsync();
        return existing;
    }

    public Task<bool> Exist(LoginCompany company)
    {
        var result = db.LoginCompanies.AnyAsync(x => x.CompanyCode == company.CompanyCode || x.CompanyName == company.CompanyName);
        return result;
    }
    public async Task<List<LoginCompany>> GetAllAsync() => await db.LoginCompanies.AsNoTracking().ToListAsync();
    public async Task<LoginCompanyPDTO> GetAllWithPagination(PaginationModel pagination)
    {
        int count = await db.LoginCompanies.CountAsync();
        var list = await db.LoginCompanies.Skip((pagination.page - 1) * pagination.pageSize)
            .Take(pagination.pageSize).AsNoTracking().ToListAsync();
        return new LoginCompanyPDTO(list, count);
    }
    public async Task<LoginCompany> GetAsync(LoginCompany entity)
        => await db.LoginCompanies.FindAsync(entity);

    public async Task<LoginCompany?> GetByCodeAsync(string code)
    {
        return await db.LoginCompanies.FirstOrDefaultAsync(x => x.CompanyCode == code);
    }

    public async Task<LoginCompany> UpdateAsync(LoginCompany entity)
    {
        var existing = await db.LoginCompanies.FirstOrDefaultAsync(x => x.Id == entity.Id);
        if (existing is null) return null!;

        existing.CompanyName = entity.CompanyName;
        existing.CompanyCode = entity.CompanyCode;
        existing.SubscriptionType = entity.SubscriptionType;
        existing.CreatedDate = entity.CreatedDate;
        existing.SubscriptionStartedAt = entity.SubscriptionStartedAt;
        existing.SubscriptionEndingDate = entity.SubscriptionEndingDate;
        existing.IsExpired = entity.IsExpired;
        existing.ContactNo = entity.ContactNo;
        existing.Email = entity.Email;
        existing.CompanyAddress = entity.CompanyAddress;

        await db.SaveChangesAsync();
        return existing;
    }
}


