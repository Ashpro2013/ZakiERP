namespace AshproSAASERP.Interface.Settings;

public interface IMenuVisibilityRepo
{
    Task<MenuVisibilityDTO> GetMenuConfigurationAsync();
    Task<List<ERPType>> GetAllERPTypesAsync();
    Task<List<MenuVisibilityItem>> GetERPTypePresetAsync(int erpTypeId);
    Task<OutputResponse> UpdateClientERPTypeAsync(int erpTypeId, string modifiedBy);
    Task<OutputResponse> SaveCustomMenuVisibilityAsync(List<MenuVisibilityItem> items);
    Task<List<MenuDetails>> GetVisibleMenusAsync();
    Task<List<MenuDetails>> GetAllMenuDetailsAsync();
}
