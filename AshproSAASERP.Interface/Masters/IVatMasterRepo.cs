namespace AshproSAASERP.Interface.Masters
{
    public interface IVatMasterRepo : ICommonInterface<AshproSAASERP.Models.Masters.VatMaster>
    {
        Task<List<AshproSAASERP.Models.Masters.VATCategoryDetails>> GetVatCategoryDetails(string code);
    }
}
