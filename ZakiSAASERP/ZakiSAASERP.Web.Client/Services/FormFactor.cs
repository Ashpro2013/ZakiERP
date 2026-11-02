using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.ViewModel;
using ZakiSAASERP.Shared.Services;

namespace ZakiSAASERP.Web.Client.Services;

public class FormFactor : IFormFactor
{
    public void DirectPrinting(PrintParameterDTO dto)
    {
        throw new NotImplementedException();
    }

    public string GetConFromRegistryForWin()
    {
        throw new NotImplementedException();
    }

    public string GetFormFactor()
    {
        return "WebAssembly";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }

    public void SetConInRegistryForWin(string value)
    {
        throw new NotImplementedException();
    }

    public string GetDeviceIdName()
    {
        return "Web Browser";
    }

    public string GetProductCode() => "16";

    public Task PrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> data)
    {
        throw new NotImplementedException();
    }

    public Task SyncfusionPrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> data)
    {
        throw new NotImplementedException();
    }

    public string GetMydocumentsPath()
    {
        return string.Empty;
    }
}
