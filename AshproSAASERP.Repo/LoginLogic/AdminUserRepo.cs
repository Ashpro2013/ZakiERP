using AshproSAASERP.Models.Company;

namespace AshproSAASERP.Repo.LoginLogic;

public class AdminUserRepo(MasterDBContext db) : IAdminUserRepo
{
    public async Task<AdminUser> CreateAsync(AdminUser entity)
    {
        try
        {
            // Check if username already exists
            var exists = await db.AdminUsers.AnyAsync(x => x.UserName == entity.UserName);
            if (exists)
                throw new InvalidOperationException("Username already exists.");

            await db.AdminUsers.AddAsync(entity);
            await db.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Database error: {ex.GetBaseException().Message}");
        }
    }
    public async Task<AdminUser> DeleteAsync(int id)
    {
        try
        {
            var existing = await db.AdminUsers.FindAsync(id);
            if (existing is null)
                throw new InvalidOperationException("User not found.");

            db.AdminUsers.Remove(existing);
            await db.SaveChangesAsync();
            return existing;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Database error: {ex.GetBaseException().Message}");
        }
    }
    public async Task<bool> Exist(AdminUser company) => await db.AdminUsers.AnyAsync(x => x.UserName == company.UserName);
    public async Task<List<AdminUser>> GetAllAsync() => await db.AdminUsers.AsNoTracking().ToListAsync();
    public async Task<AdminUser> GetForLoginAsync(string username, string password)
    {
        var user = await db.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == username && x.Password == password && x.IsActive);

        return user!;
    }
    public async Task<AdminUser> UpdateAsync(AdminUser entity)
    {
        try
        {
            var existing = await db.AdminUsers.FirstOrDefaultAsync(x => x.UserId == entity.UserId);
            if (existing is null)
                throw new InvalidOperationException("User not found.");

            // Check if new username already exists (excluding current user)
            var usernameExists = await db.AdminUsers.AnyAsync(x =>
                x.UserName == entity.UserName && x.UserId != entity.UserId);
            if (usernameExists)
                throw new InvalidOperationException("Username already exists.");

            existing.UserName = entity.UserName;
            existing.Password = entity.Password;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;
            await db.SaveChangesAsync();
            return existing;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Database error: {ex.GetBaseException().Message}");
        }
    }
}


