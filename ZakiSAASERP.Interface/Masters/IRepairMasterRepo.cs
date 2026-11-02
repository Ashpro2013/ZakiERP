namespace ZakiSAASERP.Interface.Masters
{
    public interface IRepairMasterRepo
    {
        Task<List<RepairMaster>> GetAllAsync();
        Task<RepairMaster?> GetAsync(int id);
        Task<OutputResponse> CreateAsync(RepairMaster entity);
        Task<OutputResponse> UpdateAsync(RepairMaster entity);
        Task<OutputResponse> DeleteAsync(int id);
        Task<(List<RepairMaster>, int)> GetAllWithPagination(PaginationModel pagination);
        Task<string> GetNextCode();
    }
}
