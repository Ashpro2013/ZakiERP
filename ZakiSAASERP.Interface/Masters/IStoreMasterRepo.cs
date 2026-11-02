namespace ZakiSAASERP.Interface.Masters
{
    public interface IStoreMasterRepo : ICommonInterface<StoreMaster>
    {
        Task<bool> CheckIfDefaultStoreExists(int excludeStoreId = 0, int? branchId = null);
    }
}
