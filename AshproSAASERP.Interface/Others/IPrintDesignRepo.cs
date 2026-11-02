namespace AshproSAASERP.Interface.Others;

public interface IPrintDesignRepo
{
    Task<OutputResponse> CreateAsync(PrintDesignDTO entity);
    Task<OutputResponse> DeleteAsync(int id);
    Task<PrintDesignDTO> GetAsync(PrintDesignReq req);
    Task<OutputResponse> UpdateAsync(PrintDesignDTO entity);
    Task<TransactionPDTO> GetAllWithPagination(PaginationModel pagination);
}
