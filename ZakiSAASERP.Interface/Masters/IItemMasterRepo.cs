using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Models.Zatca;

namespace ZakiSAASERP.Interface.Masters;

public interface IItemMasterRepo : ICommonInterface<ItemMaster>
{
    Task<ItemMultiUnit> GetItemMultiUnit(int ItemId, int UnitId, string Barcode);
    Task<ItemMultiUnitPDTO> GetItemMultiUnitPDTO(PaginationModel pagination);
    Task<string> GetNextItemCodeAsync(int? categoryId, int? subCategoryId, string? catCode, string? subCatCode);
    Task<(List<ItemDialogDTO>, int)> GetItemsForDialogWithStock(PaginationModel pagination);
    Task<(List<ItemMultiUnitDialogDTO>, int)> GetMultiUnitItemsForDialogWithStock(PaginationModel pagination);
    Task<List<ItemMultiUnit>> GetItemMultiUnits(int itemId);
    Task<ZeroTaxReason> GetZeroTaxReason(int id);
    Task<ItemStockInfoDTO> GetItemStockInfoAsync(int itemId);
    Task<List<BarcodeData>> GetBarcodeItemsAsync(int? categoryId = null);
}
