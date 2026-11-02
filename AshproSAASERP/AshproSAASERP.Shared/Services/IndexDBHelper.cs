using AshproSAASERP.Models.User;
using System.Text.Json;

namespace AshproSAASERP.Shared.Services;

public class IndexDBHelper(IndexedDbFactory IndexedDbFactory, IViewService api)
{
    public async Task<List<CommonModel>> LoadComboFromIndexedDB(string key)
    {
        var indrepo = IndexedDbFactory.Create<SingleRecordEntity>("erpdetails");
        SingleRecordEntity? entity = null;
        try
        {
            entity = await indrepo.GetAsync(key);
        }
        catch (Exception) { }

        if (entity == null)
        {
            var list = await api.GetCombofill(key, AppData.BranchId);

            await indrepo.SaveAsync(new SingleRecordEntity
            {
                id = key,
                data = new CommonModelDTO(list)
            });
            return list;
        }
        else
        {
            var jsonElement = (JsonElement)entity.data;
            var entityData = JsonSerializer.Deserialize<CommonModelDTO>(
    jsonElement.GetRawText(),
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

            return entityData!.commonModels;
        }
    }
    public async Task<PrintDesignDTO> LoadPrintDesignFromIndexedDB(string key,bool IsThermal = false)
    {
        PrintDesignDTO pdto;
        var indrepo = IndexedDbFactory.Create<SingleRecordEntity>("erpdetails");
        SingleRecordEntity? entity = null;
        try
        {
            entity = await indrepo.GetAsync(key);
        }
        catch (Exception) { }

        if (entity == null)
        {
            pdto = await api.GetPrintDesignAsync(new PrintDesignReq(0, key, IsThermal));
            if (pdto == null || pdto.hdr == null || pdto.dtls.Any() == false) return null;
            await indrepo.SaveAsync(new SingleRecordEntity
            {
                id = key,
                data = pdto,
                CreatedDate = DateTime.Now
            });
        }
        else
        {
            var jsonElement = (JsonElement)entity.data;
            var entityData = JsonSerializer.Deserialize<PrintDesignDTO>(
    jsonElement.GetRawText(),
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

            pdto = entityData!;
        }
        return pdto;
    }
    public async Task<ChartEntity?> LoadChartEntity(bool IsReload = false)
    {
        ChartEntity? chart = null;
        var indrepo = IndexedDbFactory.Create<SingleRecordEntity>("erpdetails");
        SingleRecordEntity? entity = null;
        try
        {
            if (!IsReload)
                entity = await indrepo.GetAsync("chartdatas");
        }
        catch (Exception) { }

        if (IsReload)
        {
            // Guard against invalid BranchId (session not recovered yet)
            if (AppData.BranchId == 0) return null;

            chart = await api.GetChartValues(AppData.BranchId, "N2");

            if (chart is not null)
            {
                await indrepo.SaveAsync(new SingleRecordEntity
                {
                    id = "chartdatas",
                    data = chart,
                    CreatedDate = DateTime.Now
                });
            }
        }
        else
        {
            if (entity == null) return null;
            var jsonElement = (JsonElement)entity.data;
            var entityData = JsonSerializer.Deserialize<ChartEntity>(
    jsonElement.GetRawText(),
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

            chart = entityData!;
        }
        return chart;
    }
    public async Task<List<MenuDetails>> LoadMenuDetails()
    {
        Console.WriteLine("Loading Menu Details from IndexedDB...");
        var indrepo = IndexedDbFactory.Create<MenuDetails>("menus");
        List<MenuDetails>? entity = null;
        try
        {
            entity = await indrepo.GetAllAsync();
        }
        catch (Exception) { }

        if (entity == null || !entity.Any() || !entity.Any(m => m.Name == "ProductImport") || !entity.Any(m => m.Name == "ProductExport"))
        {
            Console.WriteLine("Loading Menu Details from API...");
            var menuConfig = await api.GetMenuConfiguration();

            // 3. Transform DTO to MenuDetails & Apply Visibility
            var mappedMenus = menuConfig.MenuItems.Select(m => new MenuDetails
            {
                Id = m.MenuId,
                Name = m.Name,
                MenuCaption = m.MenuCaption,
                ArabicCaption = m.ArabicCaption,
                ParentId = m.ParentId,
                Position = m.Position,
                AssemblyName = m.AssemblyName,
                ClassName = m.ClassName,
                Active = m.IsVisible
            }).ToList();

            try { await indrepo.ClearAsync(); } catch { }
            await indrepo.SaveAsync(mappedMenus);
            return mappedMenus;
        }
        else
        {
            return entity;
        }
    }
    public async Task ClearIndexDBStore()
    {
        var indrepo = IndexedDbFactory.Create<SingleRecordEntity>("erpdetails");
        await indrepo.ClearAsync();
        var menus = IndexedDbFactory.Create<MenuDetails>("menus");
        await menus.ClearAsync();
    }
}
