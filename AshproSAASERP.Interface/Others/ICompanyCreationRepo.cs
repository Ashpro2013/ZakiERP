using AshproSAASERP.Models.Company;

namespace AshproSAASERP.Interface.Others;

public interface ICompanyCreationRepo
{
    Task<LoginCompany> CreateAsync(LoginCompany entity);
    Task<bool> Exist(LoginCompany company);
    Task<LoginCompany> DeleteAsync(int id);
    Task<LoginCompany> GetAsync(LoginCompany entity);
    Task<LoginCompany> GetByCodeAsync(string code);
    Task<List<LoginCompany>> GetAllAsync();
    Task<LoginCompany> UpdateAsync(LoginCompany entity);
    Task<LoginCompanyPDTO> GetAllWithPagination(PaginationModel pagination);

}


