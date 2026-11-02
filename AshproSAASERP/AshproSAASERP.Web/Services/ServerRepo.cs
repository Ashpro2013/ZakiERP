using AshproSAASERP.Models.DTO;
using System.Text.Json;
namespace AshproSAASERP.Web.Services;

public class ServerRepo : IServerRepo
{
    public async Task<PrintDesignDTO> GetPDesignFromFileAsync(string VoucherType)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "PrintDesign", $"{VoucherType}.json");
        if (!System.IO.File.Exists(path)) return null;
        var json = await System.IO.File.ReadAllTextAsync(path);

        var result = JsonSerializer.Deserialize<PrintDesignDTO>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (result == null || result.hdr == null || result.dtls == null || !result.dtls.Any()) return null;
        return result;

    }
    public async Task<List<PrintDesignDTO>> GetFullPDesignFromFileAsync()
    {
        var list = new List<PrintDesignDTO>();

        var path = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "wwwroot",
            "PrintDesign"
        );

        if (!Directory.Exists(path))
            return list;

        var files = Directory.GetFiles(
            path,
            "*.json",
            SearchOption.TopDirectoryOnly
        );

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        foreach (var file in files)
        {
            try
            {
                var json = await System.IO.File.ReadAllTextAsync(file);

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                var result = JsonSerializer.Deserialize<PrintDesignDTO>(
                    json,
                    options
                );

                if (result?.hdr == null ||
                    result.dtls == null ||
                    !result.dtls.Any())
                {
                    continue;
                }

                list.Add(result);
            }
            catch (JsonException)
            {
                // Skip invalid JSON files
                continue;
            }
            catch (IOException)
            {
                // Skip files that cannot be read
                continue;
            }
        }

        return list;
    }

    public async Task SavePDesignToFileAsync(PrintDesignDTO dto)
    {
        if (dto is null || dto.hdr is null || string.IsNullOrWhiteSpace(dto.hdr.VoucherType)) return;
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "PrintDesign", $"{dto.hdr.VoucherType}.json");
        if (System.IO.File.Exists(path)) return;
        var json = System.Text.Json.JsonSerializer.Serialize(dto);
        await System.IO.File.WriteAllTextAsync(path, json);
    }

}
