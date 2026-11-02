using AshproSAASERP.Models.Others;

namespace AshproSAASERP.Interface.Others;

public interface IBarcodeDesignRepo
{
    Task<OutputResponse> CreateAsync(BarcodeDesignDTO entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<BarcodeDesignDTO> GetAsync(int Id);
    Task<OutputResponse> UpdateAsync(BarcodeDesignDTO entity);
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
    Task<BarcodeDesignDTO> GetAsync();
    Task<List<BarcodeDesignHdr>> GetAllDesignsAsync();
}
