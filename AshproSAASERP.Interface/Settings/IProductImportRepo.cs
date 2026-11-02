using AshproSAASERP.Models.DTO;

namespace AshproSAASERP.Interface.Settings;

public interface IProductImportRepo
{
    Task<ProductImportResultDTO> ImportProductsAsync(List<ProductImportRowDTO> rows);
    byte[] GenerateSampleCsv();
    List<ProductImportRowDTO> ParseCsv(string csvContent, out List<string> parseErrors);
}
