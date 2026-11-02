namespace AshproSAASERP.Interface.Masters
{
    public interface ICustomerMasterRepo : ICommonInterface<CustomerMaster>
    {
        Task<string> GetNextCustomerCodeAsync();
        Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId);
    }
}
