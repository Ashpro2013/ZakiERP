namespace ZakiSAASERP.Interface.Masters
{
    public interface ISubCategoryMasterRepo : ICommonInterface<SubCategoryMaster>
    {
        /// <summary>
        /// Gets the next auto-generated subcategory code for the specified category.
        /// SubCategory codes are unique per category, not globally.
        /// </summary>
        /// <param name="categoryId">The category ID to generate code for</param>
        /// <param name="categoryCode">The category code to use as prefix</param>
        /// <returns>The next unique subcategory code</returns>
        Task<string> GetNextSubCategoryCodeAsync(int? categoryId, string? categoryCode);
    }
}

