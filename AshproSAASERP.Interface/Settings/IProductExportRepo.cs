using AshproSAASERP.Models.DTO;

namespace AshproSAASERP.Interface.Settings;

public interface IProductExportRepo
{
    Task<List<ProductExportRowDTO>> GetProductsForExportAsync();
    byte[] GenerateCsv(List<ProductExportRowDTO> products);
    Task<byte[]> ExportProductsToCsvAsync();
}
