namespace ZakiSAASERP.Interface.Masters
{
    public interface IOpeningStockEntryRepo
    {
        Task<List<OpeningStockEntry>> GetAllAsync();
        Task<OpeningStockEntry> GetAsync(int id);
        Task<OutputResponse> CreateAsync(OpeningStockEntry value, List<OpeningStockEntryItem> details);
        Task<OutputResponse> UpdateAsync(OpeningStockEntry value, List<OpeningStockEntryItem> details);
        Task<OutputResponse> DeleteAsync(int id);
        Task<(List<OpeningStockEntry>, int)> GetAllWithPagination(PaginationModel pagination);
        Task<string> GetNewDocNo();
        Task<List<OpeningStockEntryItemForUpdate>> GetExistingStockForItem(int itemId);
        Task<OpeningStockEntryDTO> GetFullAsync(int id);
    }
}
