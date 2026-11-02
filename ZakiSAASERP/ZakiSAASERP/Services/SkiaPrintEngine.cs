using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Enums;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.ViewModel;
using MudBlazor;
using QRCoder;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.SkiaSharp;

namespace ZakiSAASERP.Shared.Services;

public class SkiaPrintEngine
{
    PrintDesignHdr entDMaster = new();
    List<PrintDesignDtls> entDSettings = new();
    List<TransactionDtls> entDetails = new();
    CompanyDetails entCompany = new();
    TransactionHdr entInvoice = new();
    Party? entParty;

    int linesPrinted;
    int iTopMargin;
    public SKBitmap RenderInvoice(PrintParameterDTO dto)
    {
        entDMaster = dto.hdr;
        entDSettings = dto.dtls;
        entDetails = dto.tranDtls;
        entCompany = dto.Company;
        entInvoice = dto.tranHdr;
        entParty = dto.party;

        var info = new SKImageInfo(
            entDMaster.PageWidth,
            entDMaster.PageHeight,
            SKColorType.Rgba8888,
            SKAlphaType.Premul);

        var bitmap = new SKBitmap(info);
        var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        DrawHeader(canvas);
        DrawDetailsHeader(canvas);
        DrawDetails(canvas);

        return bitmap;
    }

    public byte[] GenerateInvoicePdf(PrintParameterDTO dto)
    {
        entDMaster = dto.hdr;
        entDSettings = dto.dtls;
        entDetails = dto.tranDtls;
        entCompany = dto.Company;
        entInvoice = dto.tranHdr;
        entParty = dto.party;

        using var stream = new MemoryStream();
        using var document = SKDocument.CreatePdf(stream);

        var pageSize = new SKSize(entDMaster.PageWidth, entDMaster.PageHeight);
        var canvas = document.BeginPage(pageSize.Width, pageSize.Height);

        iTopMargin = 0;
        linesPrinted = 0;

        DrawHeader(canvas);
        DrawDetailsHeader(canvas);
        DrawDetails(canvas);

        document.EndPage();
        document.Close();

        return stream.ToArray();
    }

    #region Header

    void DrawHeader(SKCanvas canvas)
    {
        var headers = entDSettings.Where(x =>
            x.Location == InvoiceLabelPosition.Company_Section.ToString()
         || x.Location == InvoiceLabelPosition.Invoice_Section.ToString()
         || x.Location == InvoiceLabelPosition.Party_Section.ToString());

        foreach (var item in headers)
            DrawItem(canvas, item, ResolveData(item));
    }

    #endregion

    #region Details

    void DrawDetailsHeader(SKCanvas canvas)
    {
        var hdr = entDSettings.Where(x => x.Location == InvoiceLabelPosition.Invoice_Details_Header.ToString());
        foreach (var item in hdr)
            DrawItem(canvas, item, null);

        iTopMargin += entDMaster.SpaceBtwnLines;
    }

    void DrawDetails(SKCanvas canvas)
    {
        var details = entDSettings.Where(x => x.Location == InvoiceLabelPosition.Details_Area.ToString());

        foreach (var row in entDetails)
        {
            foreach (var item in details)
                DrawItem(canvas, item, row);

            iTopMargin += entDMaster.SpaceBtwnLines;
        }
    }

    #endregion

    #region Drawing Engine

    void DrawItem(SKCanvas canvas, PrintDesignDtls item, object? data)
    {
        var paint = CreatePaint(item);
        float x = item.PositionX;
        float y = iTopMargin + item.PositionY;

        if (item.ControlType == "LabelControl")
        {
            canvas.DrawText(item.FieldText, x, y, paint);
            return;
        }

        if (item.ControlType == "ValueControle")
        {
            var value = GetValueFromEntity(data, item)?.ToString();

            if (item.FieldText == "InvoiceQRCode")
                DrawQr(canvas, value, x, y, item.Width);

            else if (item.FieldText == "InvoiceBarcode")
                DrawBarcode(canvas, value, x, y, item.Width, item.Height);

            else
                canvas.DrawText(value ?? "", x, y, paint);
        }
    }

    #endregion

    #region QR & Barcode
    static SKBitmap ConvertToSkiaBitmap(Bitmap bitmap)
    {
        var skBitmap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

        var data = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        skBitmap.InstallPixels(
            new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul),
            data.Scan0,
            data.Stride);

        bitmap.UnlockBits(data);

        return skBitmap;
    }
    void DrawQr(SKCanvas canvas, string? text, float x, float y, float size)
    {
        if (string.IsNullOrEmpty(text)) return;

        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qr = new QRCode(data);

        using System.Drawing.Bitmap gdiBmp = qr.GetGraphic(4);

        using SKBitmap skBmp = ConvertToSkiaBitmap(gdiBmp);

        canvas.DrawBitmap(skBmp, new SKRect(x, y, x + size, y + size));
    }


    void DrawBarcode(SKCanvas canvas, string? text, float x, float y, float w, float h)
    {
        if (string.IsNullOrEmpty(text)) return;

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Width = (int)w,
                Height = (int)h,
                Margin = 0
            }
        };

        var img = writer.Write(text);
        canvas.DrawBitmap(img, x, y);
    }

    #endregion

    #region Helpers

    SKPaint CreatePaint(PrintDesignDtls item)
    {
        var paint = new SKPaint
        {
            TextSize = item.FontSize,
            Color = SKColors.Black,
            IsAntialias = true
        };

        paint.Typeface = SKTypeface.FromFamilyName(item.Font ?? "Arial",
            item.FStyle == "Bold" ? SKFontStyle.Bold :
            item.FStyle == "Italic" ? SKFontStyle.Italic :
            SKFontStyle.Normal);

        return paint;
    }

    object? ResolveData(PrintDesignDtls item)
    {
        return item.Location switch
        {
            "Company_Section" => entCompany,
            "Invoice_Section" => entInvoice,
            "Party_Section" => entParty,
            _ => null
        };
    }

    object? GetValueFromEntity(object? data, PrintDesignDtls item)
    {
        if (data == null) return null;
        var prop = data.GetType().GetProperty(item.FieldText);
        return prop?.GetValue(data);
    }

    #endregion
}
