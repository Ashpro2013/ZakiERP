namespace ZakiSAASERP.Interface.Masters
{
    public interface ICategoryMasterRepo : ICommonInterface<CategoryMaster>
    {
        /// <summary>
        /// Gets the next auto-generated category code based on permanent settings
        /// </summary>
        Task<string> GetNextCategoryCodeAsync();
    }
}
