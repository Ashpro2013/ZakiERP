using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Shared.Services;

namespace AshproSAASERP.Services;

public class FormFactor : IFormFactor
{
    public void DirectPrinting(PrintParameterDTO dto)
    {
#if ANDROID
        var engine = new SkiaPrintEngine();
        var skBitmap = engine.RenderInvoice(dto);

        var printMgr = Android.App.Application.Context
            .GetSystemService(Android.Content.Context.PrintService) as Android.Print.PrintManager;

        printMgr.Print("Invoice", new BitmapPrintAdapter(skBitmap), null);
#endif
    }

    public string GetConFromRegistryForWin()
    {
        throw new NotImplementedException();
    }

    public string GetFormFactor()
    {
        return DeviceInfo.Idiom.ToString();
    }

    public string GetPlatform()
    {
        return DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
    }

    public void SetConInRegistryForWin(string value)
    {
        throw new NotImplementedException();
    }

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
            return "MAUI Device";
        }
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
