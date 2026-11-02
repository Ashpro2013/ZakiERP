using ZakiSAASERP.Models.DTO;

namespace ZakiSAASERP.Interface.Settings;

public interface IProductExportRepo
{
    Task<List<ProductExportRowDTO>> GetProductsForExportAsync();
    byte[] GenerateCsv(List<ProductExportRowDTO> products);
    Task<byte[]> ExportProductsToCsvAsync();
}
