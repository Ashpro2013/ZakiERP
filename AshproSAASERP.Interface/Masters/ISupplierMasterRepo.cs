namespace AshproSAASERP.Interface.Masters
{
    public interface ISupplierMasterRepo : ICommonInterface<SupplierMaster>
    {
        Task<string> GetNextSupplierCodeAsync();
    }
}
