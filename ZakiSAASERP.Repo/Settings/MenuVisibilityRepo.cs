using ZakiSAASERP.Interface.Settings;
using ZakiSAASERP.Models.Settings;
using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Repo.Settings;

public class MenuVisibilityRepo(IDbContextProvider contextProvider) : IMenuVisibilityRepo
{
    public async Task<MenuVisibilityDTO> GetMenuConfigurationAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        if (!await db.MenuDetails.AnyAsync(m => m.Name == "PrintingBarcode"))
        {
            await ZakiSAASERP.Interface.Others.MenuSeedHelper.EnsureSeedDataAsync(db);
        }

        var clientSettings = await db.ClientERPSettings.FirstOrDefaultAsync() ?? new ClientERPSettings { ERPTypeId = 1 };
        var erpType = await db.ERPTypes.FindAsync(clientSettings.ERPTypeId) ?? await db.ERPTypes.FirstAsync();
        var allMenus = await db.MenuDetails.Where(m => m.Active).OrderBy(m => m.Position).ToListAsync();
        var settingsMenuId = allMenus.FirstOrDefault(p => p.Name == "Settings")?.Id;
        List<MenuVisibilityItem> menuItems;
        if (erpType.IsCustom)
        {
            var customVisibility = await db.MenuVisibilities.ToListAsync();
            menuItems = allMenus.Select(m => new MenuVisibilityItem { MenuId = m.Id, Name = m.Name, MenuCaption = m.MenuCaption, ArabicCaption = m.ArabicCaption, ParentId = m.ParentId, Position = m.Position, IsVisible = customVisibility.FirstOrDefault(cv => cv.MenuDetailId == m.Id)?.IsVisible ?? true, CanToggle = m.Name != "Dashboard" && m.Name != "Settings", AssemblyName = m.AssemblyName, ClassName = m.ClassName }).ToList();
        }
        else
        {
            var presetItems = await db.ERPTypePresets.Where(p => p.ERPTypeId == erpType.Id).ToListAsync();
            menuItems = allMenus.Select(m => new MenuVisibilityItem { MenuId = m.Id, Name = m.Name, MenuCaption = m.MenuCaption, ArabicCaption = m.ArabicCaption, ParentId = m.ParentId, Position = m.Position, IsVisible = presetItems.Any() ? (presetItems.FirstOrDefault(p => p.MenuDetailId == m.Id)?.IsVisible ?? (m.ParentId == settingsMenuId)) : true, CanToggle = false, AssemblyName = m.AssemblyName, ClassName = m.ClassName }).ToList();
        }
        return new MenuVisibilityDTO { ERPTypeId = erpType.Id, ERPTypeName = erpType.TypeName, IsCustomType = erpType.IsCustom, MenuItems = menuItems };
    }

    public async Task<List<ERPType>> GetAllERPTypesAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.ERPTypes.OrderBy(t => t.Id).ToListAsync();
    }

    public async Task<List<MenuVisibilityItem>> GetERPTypePresetAsync(int erpTypeId)
    {
        await using var db = contextProvider.CreateDbContext();
        var erpType = await db.ERPTypes.FindAsync(erpTypeId);
        if (erpType == null)
            return new List<MenuVisibilityItem>();

        var allMenus = await db.MenuDetails
            .Where(m => m.Active)
            .OrderBy(m => m.Position)
            .ToListAsync();

        if (erpType.IsCustom)
        {
            // For custom, return all as visible by default
            return allMenus.Select(m => new MenuVisibilityItem
            {
                MenuId = m.Id,
                Name = m.Name,
                MenuCaption = m.MenuCaption,
                ArabicCaption = m.ArabicCaption,
                ParentId = m.ParentId,
                Position = m.Position,
                IsVisible = true,
                CanToggle = m.Name != "Dashboard" && m.Name != "Settings",
                AssemblyName = m.AssemblyName,
                ClassName = m.ClassName
            }).ToList();
        }

        var presetItems = await db.ERPTypePresets
            .Where(p => p.ERPTypeId == erpTypeId)
            .ToListAsync();

        return allMenus.Select(m => new MenuVisibilityItem
        {
            MenuId = m.Id,
            Name = m.Name,
            MenuCaption = m.MenuCaption,
            ArabicCaption = m.ArabicCaption,
            ParentId = m.ParentId,
            Position = m.Position,
            IsVisible = presetItems.Any()
                ? (presetItems.FirstOrDefault(p => p.MenuDetailId == m.Id)?.IsVisible ?? false)
                : true,
            CanToggle = false,
            AssemblyName = m.AssemblyName,
            ClassName = m.ClassName
        }).ToList();
    }

    public async Task<OutputResponse> UpdateClientERPTypeAsync(int erpTypeId, string modifiedBy)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            var clientSettings = await db.ClientERPSettings.FirstOrDefaultAsync();

            if (clientSettings == null)
            {
                clientSettings = new ClientERPSettings
                {
                    ERPTypeId = erpTypeId,
                    ModifiedBy = modifiedBy,
                    LastModified = DateTime.Now
                };
                db.ClientERPSettings.Add(clientSettings);
            }
            else
            {
                clientSettings.ERPTypeId = erpTypeId;
                clientSettings.ModifiedBy = modifiedBy;
                clientSettings.LastModified = DateTime.Now;
            }

            // If switching to a non-custom type, clear custom visibility settings
            var erpType = await db.ERPTypes.FindAsync(erpTypeId);
            if (erpType != null && !erpType.IsCustom)
            {
                var customSettings = await db.MenuVisibilities.ToListAsync();
                db.MenuVisibilities.RemoveRange(customSettings);
            }

            await db.SaveChangesAsync();

            return new OutputResponse(0, "ERP Type updated successfully");
        }
        catch (Exception ex)
        {
            return new OutputResponse(-1, ex.Message);
        }
    }

    public async Task<OutputResponse> SaveCustomMenuVisibilityAsync(List<MenuVisibilityItem> items)
    {
        try
        {
            await using var db = contextProvider.CreateDbContext();
            // Verify client is using Custom type
            var clientSettings = await db.ClientERPSettings.FirstOrDefaultAsync();
            if (clientSettings != null)
            {
                var erpType = await db.ERPTypes.FindAsync(clientSettings.ERPTypeId);
                if (erpType == null || !erpType.IsCustom)
                {
                    return new OutputResponse(-1, "Custom menu visibility can only be saved when ERP Type is set to Custom");
                }
            }

            // Clear existing custom settings
            var existing = await db.MenuVisibilities.ToListAsync();
            db.MenuVisibilities.RemoveRange(existing);

            // Add new settings
            foreach (var item in items)
            {
                db.MenuVisibilities.Add(new ZakiSAASERP.Models.Settings.MenuVisibility
                {
                    MenuDetailId = item.MenuId,
                    IsVisible = item.IsVisible,
                    LastModified = DateTime.Now
                });
            }

            await db.SaveChangesAsync();

            return new OutputResponse(0, "Menu settings saved successfully");
        }
        catch (Exception ex)
        {
            return new OutputResponse(-1, ex.Message);
        }
    }

    public async Task<List<MenuDetails>> GetVisibleMenusAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        var config = await GetMenuConfigurationAsync();
        var visibleIds = config.MenuItems.Where(m => m.IsVisible).Select(m => m.MenuId).ToList();

        return await db.MenuDetails
            .Where(m => m.Active && visibleIds.Contains(m.Id))
            .OrderBy(m => m.Position)
            .ToListAsync();
    }

    public async Task<List<MenuDetails>> GetAllMenuDetailsAsync()
    {
        await using var db = contextProvider.CreateDbContext();
        return await db.MenuDetails
            .Where(m => m.Active)
            .OrderBy(m => m.Position)
            .ToListAsync();
    }
}



