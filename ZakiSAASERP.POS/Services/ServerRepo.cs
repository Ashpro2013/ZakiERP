using ZakiSAASERP.Interface.Others;
using ZakiSAASERP.Models.DTO;
using System.IO;
using System.Text.Json;

namespace ZakiSAASERP.POS.Services;

public class ServerRepo : IServerRepo
{
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
                var json = await File.ReadAllTextAsync(file);

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
                continue;
            }
            catch (IOException)
            {
                continue;
            }
        }

        return list;
    }

    public async Task<PrintDesignDTO?> GetPDesignFromFileAsync(string VoucherType)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "PrintDesign", $"{VoucherType}.json");
        if (!File.Exists(path)) return null;
        var json = await File.ReadAllTextAsync(path);
        var result = JsonSerializer.Deserialize<PrintDesignDTO>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (result == null || result.hdr == null || result.dtls == null || !result.dtls.Any()) return null;
        return result;
    }

    public async Task SavePDesignToFileAsync(PrintDesignDTO dto)
    {
        if (dto is null || dto.hdr is null || string.IsNullOrWhiteSpace(dto.hdr.VoucherType)) return;
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "PrintDesign");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var path = Path.Combine(dir, $"{dto.hdr.VoucherType}.json");
        if (File.Exists(path)) return;
        var json = JsonSerializer.Serialize(dto);
        await File.WriteAllTextAsync(path, json);
    }
}
