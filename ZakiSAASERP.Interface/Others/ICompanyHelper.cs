using ZakiSAASERP.Models.Company;

namespace ZakiSAASERP.Interface.Others;

public interface ICompanyHelper
{
    Task<(bool, LoginCompany)> CreateCompanyDatabase(ExLCompany exlcompany);
    Task<LoginCompany?> UpdateDatabase(LoginCompany company);
}
