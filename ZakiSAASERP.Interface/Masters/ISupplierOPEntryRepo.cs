namespace ZakiSAASERP.Interface.Masters
{
    public interface ISupplierOPEntryRepo
    {
        Task<string> GetNewDocNo();
        Task<OutputResponse> CreateAsync(SupplierOPEntry header, List<SupplierOPEntryDetails> details);
        Task<OutputResponse> UpdateAsync(SupplierOPEntry header, List<SupplierOPEntryDetails> details);
        Task<OutputResponse> DeleteAsync(int id);
        Task<List<SupplierOPEntry>> GetAllAsync();
        Task<SupplierOPEntry> GetAsync(int id);
        Task<SupplierOPEntryDTO> GetFullAsync(int id);
        Task<(List<SupplierOPEntry>, int)> GetAllWithPagination(PaginationModel pagination);
    }
}
