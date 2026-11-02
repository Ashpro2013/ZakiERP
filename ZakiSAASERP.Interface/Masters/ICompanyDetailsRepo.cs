namespace ZakiSAASERP.Interface.Masters
{
    public interface ICompanyDetailsRepo
    {
        Task<CompanyDetails?> GetByBranchIdAsync(int branchId);
        Task<OutputResponse> CreateAsync(CompanyDetails details);
        Task<OutputResponse> UpdateAsync(CompanyDetails details);
        Task<bool> ExistsForBranchAsync(int branchId);
    }
}
