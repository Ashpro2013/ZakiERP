using AshproSAASERP.Models.Company;

namespace AshproSAASERP.Interface.Others;

public interface ICompanyHelper
{
    Task<(bool, LoginCompany)> CreateCompanyDatabase(ExLCompany exlcompany);
    Task<LoginCompany?> UpdateDatabase(LoginCompany company);
}
