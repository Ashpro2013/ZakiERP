using AshproSAASERP.Models.Company;

namespace AshproSAASERP.Interface.Others;

public interface IAdminUserRepo
{
    Task<AdminUser> CreateAsync(AdminUser entity);
    Task<bool> Exist(AdminUser company);
    Task<AdminUser> DeleteAsync(int id);
    Task<AdminUser> GetForLoginAsync(string username, string password);
    Task<List<AdminUser>> GetAllAsync();
    Task<AdminUser> UpdateAsync(AdminUser entity);
}


