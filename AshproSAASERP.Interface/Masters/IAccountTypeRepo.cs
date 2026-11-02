namespace AshproSAASERP.Interface.Masters;

public interface IAccountTypeRepo
{
    Task<List<AccountType>> GetAllAsync();
}
