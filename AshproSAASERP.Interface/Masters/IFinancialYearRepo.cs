namespace AshproSAASERP.Interface.Masters
{
    public interface IFinancialYearRepo : ICommonInterface<AshproSAASERP.Models.Masters.FinancialYear>
    {
        Task<int> GetMaxRefNo();
    }
}
