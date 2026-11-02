namespace ZakiSAASERP.Interface.Masters
{
    public interface ITechnicianMasterRepo
    {
        Task<List<TechnicianMaster>> GetAllAsync();
        Task<TechnicianMaster?> GetAsync(int id);
        Task<OutputResponse> CreateAsync(TechnicianMaster entity);
        Task<OutputResponse> UpdateAsync(TechnicianMaster entity);
        Task<OutputResponse> DeleteAsync(int id);
        Task<(List<TechnicianMaster>, int)> GetAllWithPagination(PaginationModel pagination);
        Task<string> GetNextCode();
    }
}
