namespace ZakiSAASERP.Interface.Masters
{
    public interface IVatMasterRepo : ICommonInterface<ZakiSAASERP.Models.Masters.VatMaster>
    {
        Task<List<ZakiSAASERP.Models.Masters.VATCategoryDetails>> GetVatCategoryDetails(string code);
    }
}
