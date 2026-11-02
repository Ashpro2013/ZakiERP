namespace ZakiSAASERP.Interface.Masters;

public interface IAccountTypeRepo
{
    Task<List<AccountType>> GetAllAsync();
}
