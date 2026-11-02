namespace ZakiSAASERP.Interface.Masters
{
    public interface IFinancialYearRepo : ICommonInterface<ZakiSAASERP.Models.Masters.FinancialYear>
    {
        Task<int> GetMaxRefNo();
    }
}
