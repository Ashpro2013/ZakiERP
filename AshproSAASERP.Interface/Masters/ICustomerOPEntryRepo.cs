namespace AshproSAASERP.Interface.Masters
{
    public interface ICustomerOPEntryRepo
    {
        Task<List<CustomerOPEntry>> GetAllAsync();
        Task<CustomerOPEntry> GetAsync(int id);
        Task<OutputResponse> CreateAsync(CustomerOPEntry value, List<CustomerOPEntryDetails> details);
        Task<OutputResponse> UpdateAsync(CustomerOPEntry value, List<CustomerOPEntryDetails> details);
        Task<OutputResponse> DeleteAsync(int id);
        Task<(List<CustomerOPEntry>, int)> GetAllWithPagination(PaginationModel pagination);
        Task<string> GetNewDocNo();
        Task<CustomerOPEntryDTO> GetFullAsync(int id);
    }
}
