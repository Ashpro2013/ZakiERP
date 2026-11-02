using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Shared.Services;
using System.Runtime.InteropServices;

namespace AshproSAASERP.Mob.Services;

public class FormFactor : IFormFactor
{
    public void DirectPrinting(PrintParameterDTO dto)
    {
        PrintHelper printHelper = new PrintHelper();
        printHelper.PrintInvoice(dto);
    }
    public string GetConFromRegistryForWin() => AppData.DBProvider == "SQLite" ? "Data Source=Ashpro_db.db" : RegistryHelper.GetData("ConString");

    public string GetDeviceIdName()
    {
        try
        {
            var manufacturer = DeviceInfo.Manufacturer;
            var model = DeviceInfo.Model;
            var name = DeviceInfo.Name;
            return $"{manufacturer} {model} ({name})".Trim();
        }
        catch
        {
            return "MAUI Mobile Device";
        }
    }

    public string GetFormFactor() => "Mobile App";

    public string GetMydocumentsPath()
    {
        return string.Empty;
    }

    public string GetPlatform() => RuntimeInformation.OSDescription;

    public string GetProductCode() => "16";

    public Task PrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> data)
    {
        throw new NotImplementedException();
    }

    public void SetConInRegistryForWin(string value) => RegistryHelper.SetData("ConString", value);

    public Task SyncfusionPrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> data)
    {
        throw new NotImplementedException();
    }
}
