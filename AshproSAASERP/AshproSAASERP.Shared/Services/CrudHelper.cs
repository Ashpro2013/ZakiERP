using FileDownloader;
namespace AshproSAASERP.Shared.Services;

public sealed class CrudHelper
{
    #region Inclusive Tax
    public static Func<decimal, decimal, decimal> GetInclusiveAmount = (Taxable, Vat) =>
     Taxable - (Taxable * 100) / (100 + Vat);
    public static Func<decimal, decimal, decimal> GetInclusiveBaseAmount = (Taxable, Vat) =>
    (Taxable * 100) / (100 + Vat);
    public static decimal GetExAmountFromInc(decimal IncAmount, decimal vat) => (IncAmount * 100) / (100 + vat);
    public static decimal GetIncAmountFromExAmount(decimal ExAmount, decimal vat) => ExAmount * (1 + (vat / 100));
    public static string GetNormalDate(DateTime dateTime)
    {
        try
        {
            System.Globalization.CultureInfo enCul = new System.Globalization.CultureInfo("en-US");
            string sVal = dateTime.ToString("dd-MM-yyyyTHH:mm:ss", enCul);
            return sVal;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public static async Task PrintInvoiceMethodLocal(Downloader downloader, PrintParameterDTO dto, ModelService? model = null, bool isPreview = false)
    {
        byte[]? iByte;
        if (dto.hdr?.PrintType == "Thermal")
        {
            SyncfusionPrintHelperThermal thermalHelper = new SyncfusionPrintHelperThermal();
            iByte = await thermalHelper.PrintMethod(downloader, dto, 1, null, isPreview);
        }
        else
        {
            SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
            iByte = await pHelper.PrintMethod(downloader, dto, 1, null, isPreview);
        }
        if (isPreview && iByte is not null && model is not null)
            await model!.InvoicePreviewDialog(iByte);
    }
    public static async Task PrintInvoiceMethod(PrintParameterDTO dto, IJSRuntime js)
    {
        byte[]? iByte;
        if (dto.hdr?.PrintType == "Thermal")
        {
            SyncfusionPrintHelperThermal thermalHelper = new SyncfusionPrintHelperThermal();
            iByte = await thermalHelper.PrintMethod(null, dto, 1, null, true);
        }
        else
        {
            SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
            iByte = await pHelper.PrintMethod(null, dto, 1, null, true);
        }
        if (iByte != null)
            await js.InvokeVoidAsync("openPdfFromBytes", Convert.ToBase64String(iByte));
    }
    public static async Task<byte[]> PrintBarcodeMethodLocal(BarcodeDesignDTO dto, List<BarcodeData> barcodeData, Downloader? downloader = null, ModelService? model = null, bool isPreview = false)
    {
        SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
        var iByte = await pHelper.BarcodeGenerteMethod(dto, barcodeData, model, downloader, isPreview);
        if (isPreview && iByte is not null && model is not null)
            await model!.InvoicePreviewDialog(iByte);
        return iByte!;
    }
    public static async Task PrintBarcodeMethod(BarcodeDesignDTO dto, List<BarcodeData> barcodeData, IJSRuntime js, ModelService? model)
    {
        SyncfusionPrintHelper pHelper = new SyncfusionPrintHelper();
        var iByte = await pHelper.BarcodeGenerteMethod(dto, barcodeData, model, null, true);
        await js.InvokeVoidAsync("openPdfFromBytes", Convert.ToBase64String(iByte));
    }
    /// <summary>
    /// Converts Gregorian date to Hijri date
    /// </summary>
    public static string ConvertToHijri(DateTime? gregorianDate)
    {
        if (!gregorianDate.HasValue) return "";
        try
        {
            var hijri = new System.Globalization.HijriCalendar();
            int day = hijri.GetDayOfMonth(gregorianDate.Value);
            int month = hijri.GetMonth(gregorianDate.Value);
            int year = hijri.GetYear(gregorianDate.Value);
            return $"{day:00}/{month:00}/{year}";
        }
        catch
        {
            return "";
        }
    }
    public static async Task Navigation(TabService tService, int id, string view, LedgerReportRequest? req = null)
    {
        var menu = AppData.menuList.FirstOrDefault(m => m.Name == view);
        var parameters = req == null ? new Dictionary<string, object> { ["Id"] = id }
                                   : new Dictionary<string, object> { ["req"] = req };
        var tabrequest = new TabRequest
        {
            Id = menu.Id,
            Name = AppData.IsArabic ? menu.ArabicCaption : menu.MenuCaption,
            View = menu.AssemblyName,
            Parameters = parameters
        };
        await tService.OpenTabAsync(tabrequest);
    }
    public static async Task ShowImagePopup(ModelService Dialog, IDialogService DialogService, string img)
    {
        try
        {
            var parameters = new DialogParameters
            {
                ["ImageUrl"] = img
            };
            await DialogService.ShowAsync<ImagePreviewDialog>("Preview", parameters);
        }
        catch (Exception ex)
        {
            await Dialog.DialogMethod(ex.Message);
        }
    }
    #endregion
}
