using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Enums;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.Services;
using ZakiSAASERP.Models.ViewModel;
using AshproStringExtension;
using FileDownloader;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Graphics;
using Color = Syncfusion.Drawing.Color;

namespace ZakiSAASERP.Shared.Services;

/// <summary>
/// Syncfusion PDF thermal invoice generator for 80mm / thermal receipts.
/// Replicates the structure and flow of ZakiSAASERP.Desk.Services.PrintHelper using the Syncfusion PDF library.
/// </summary>
public class SyncfusionPrintHelperThermal
{
    #region Fields
    private PrintDesignHdr entDMaster = new();
    private List<PrintDesignDtls> entDSettings = new();
    private CompanyDetails entCompany = new();
    private TransactionHdr entInvoice = new();
    private Party? entParty;
    private List<TransactionDtls> entDetails = new();
    private List<FooterDetails> entAdjustments = new();

    private float iTopMargin;
    private int linesPrinted;
    private int iIndex;
    private int itemHeight = 20;
    private int currentPageNumber = 1;

    private PdfDocument? document;
    private PdfSection? section;
    private PdfPage? currentPage;
    private PdfGraphics? graphics;
    private float pageWidth = 290f;
    private float pageHeight = 842f;

    private static readonly object s_FontLock = new();
    private static byte[]? s_ArabicFontBytes;

    /// <summary>
    /// Optional custom TrueType font bytes (e.g. Arial, Tahoma, Amiri) provided by the application.
    /// </summary>
    public static byte[]? CustomArabicFontBytes { get; set; }

    /// <summary>
    /// Cache of created PdfFont instances for the current render session to optimize performance and memory.
    /// </summary>
    private readonly Dictionary<(float size, PdfFontStyle style, bool isTrueType), PdfFont> fontCache = new();
    #endregion

    #region Public API
    /// <summary>
    /// Generates thermal PDF invoice and optionally downloads or returns bytes.
    /// Matches SyncfusionPrintHelper signature.
    /// </summary>
    public async Task<byte[]?> PrintMethod(Downloader? downloader, PrintParameterDTO dto, int iCopyToPrint = 1, string? sFileName = null, bool isPreview = false)
    {
        try
        {
            byte[] bytes = GenerateThermalPdfBytes(dto);

            if (isPreview || downloader == null)
            {
                return bytes;
            }

            string fileName = string.IsNullOrWhiteSpace(sFileName) ? "ThermalInvoice.pdf" : sFileName;
            for (int i = 0; i < iCopyToPrint; i++)
            {
                await downloader.DownloadFromStream(bytes, fileName);
            }

            return bytes;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Alias for PrintMethod specifically for thermal printing.
    /// </summary>
    public async Task<byte[]?> PrintThermalMethod(Downloader? downloader, PrintParameterDTO dto, int iCopyToPrint = 1, string? sFileName = null, bool isPreview = false)
    {
        return await PrintMethod(downloader, dto, iCopyToPrint, sFileName, isPreview);
    }

    /// <summary>
    /// Generates a PDF byte array for thermal invoice.
    /// </summary>
    public byte[] GenerateThermalPdfBytes(PrintParameterDTO dto)
    {
        using MemoryStream ms = GenerateThermalPdfStream(dto);
        return ms.ToArray();
    }

    /// <summary>
    /// Generates a PDF MemoryStream for thermal invoice.
    /// </summary>
    public MemoryStream GenerateThermalPdfStream(PrintParameterDTO dto)
    {
        Initialize(dto);

        document = new PdfDocument();
        document.PageSettings.Margins.All = 0;

        section = document.Sections.Add();
        section.PageSettings.Rotate = PdfPageRotateAngle.RotateAngle0;
        section.PageSettings.Size = new SizeF(pageWidth, pageHeight);

        currentPage = section.Pages.Add();
        graphics = currentPage.Graphics;

        try
        {
            // 1. Header (Company, Invoice, Party)
            PrintHeader();

            // 2. Details Header (Column Titles)
            PrintDetailsHeader();

            // 3. Details Area (Line Items)
            PrintDetails();

            // 4. Footer Section (Totals, Adjustments, QR Code, Notes)
            PrintFooterSection();
        }
        catch (Exception)
        {
            throw;
        }

        MemoryStream stream = new MemoryStream();
        document.Save(stream);
        document.Close(true);
        stream.Position = 0;
        return stream;
    }
    #endregion

    #region Core Engine
    private void Initialize(PrintParameterDTO dto)
    {
        fontCache.Clear();
        entDMaster = dto.hdr ?? new();
        entDSettings = dto.dtls ?? new();
        entCompany = dto.Company ?? new();
        entInvoice = dto.tranHdr ?? new();
        entParty = dto.party;
        entDetails = dto.tranDtls ?? new();
        entAdjustments = dto.fDtls ?? new();

        foreach (var d in entDSettings)
        {
            if (!string.IsNullOrEmpty(d.Location))
            {
                d.Location = d.Location.Replace(' ', '_');
            }
        }

        int isl = 1;
        entDetails.ForEach(x => x.SL = isl++);

        linesPrinted = 0;
        iIndex = 0;
        iTopMargin = 0;
        currentPageNumber = 1;
        itemHeight = 20;

        pageWidth = entDMaster.PageWidth > 0 ? entDMaster.PageWidth : 290f;
        pageHeight = CalculateTotalHeight();
    }

    private void PrintHeader()
    {
        var headers = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Company_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Invoice_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Party_Section.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        foreach (var item in headers)
        {
            object? src = item.Location switch
            {
                var l when l == InvoiceLabelPosition.Company_Section.ToString() => entCompany,
                var l when l == InvoiceLabelPosition.Invoice_Section.ToString() => entInvoice,
                _ => entParty
            };

            DrawItem(item, src, isFixed: true);
        }

        iTopMargin = headers.Any() ? headers.Max(x => x.PositionY + x.Height) + 10 : 0;
    }

    private void PrintDetailsHeader()
    {
        var hdr = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Invoice_Details_Header.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        foreach (var item in hdr)
        {
            DrawItem(item, null, isFixed: true);
        }
    }

    private void PrintDetails()
    {
        var detailsStart = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Details_Area.ToString() && x.FieldText != "ItemArabicName")
            .OrderBy(x => x.PositionY)
            .FirstOrDefault();
        if (detailsStart != null && detailsStart.PositionY > 0)
        {
            iTopMargin = detailsStart.PositionY;
        }

        var ds = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Details_Area.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        var firstLineItems = ds.Where(x => x.FieldText != "ItemArabicName").ToList();
        var arabicItem = ds.FirstOrDefault(x => x.FieldText == "ItemArabicName");

        int linesPerPage = entDMaster.LinesPerPage > 0 ? entDMaster.LinesPerPage : 100;
        int spaceBetween = entDMaster.SpaceBtwnLines > 0 ? entDMaster.SpaceBtwnLines : 5;

        while (linesPrinted < entDetails.Count && iIndex < linesPerPage)
        {
            var currentRow = entDetails[linesPrinted];
            itemHeight = 20;

            EnsurePageSpace(itemHeight + spaceBetween);

            foreach (var item in firstLineItems)
            {
                DrawItem(item, currentRow, isFixed: false);
            }

            float line1Height = Math.Max(itemHeight, 20);
            float arabicHeight = 0;

            if (arabicItem != null)
            {
                string? arabicVal = GetValueFromEntity(currentRow, arabicItem)?.ToString();
                if (!string.IsNullOrWhiteSpace(arabicVal))
                {
                    float arabicX = arabicItem.PositionX;
                    float arabicY = iTopMargin + line1Height;
                    float arabicWidth = arabicItem.Width > 0 ? arabicItem.Width : 140;

                    PdfFont font = BuildFont(arabicItem, arabicVal);
                    PdfStringFormat sf = BuildFormat(arabicItem, arabicVal);

                    SizeF size = font.MeasureString(arabicVal, arabicWidth, sf);
                    arabicHeight = Math.Max(arabicItem.Height > 0 ? arabicItem.Height : 18, size.Height);

                    EnsurePageSpace(line1Height + arabicHeight + spaceBetween);
                    arabicY = iTopMargin + line1Height;

                    graphics?.DrawString(arabicVal, font, PdfBrushes.Black,
                        new RectangleF(arabicX, arabicY, arabicWidth, arabicHeight), sf);
                }
            }

            iTopMargin += line1Height + arabicHeight + spaceBetween;
            linesPrinted++;
            iIndex++;
        }
    }

    private void PrintFooterSection()
    {
        var footers = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Footer_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Footer_Bottom.ToString()
                     || x.Location == InvoiceLabelPosition.Footer_Details.ToString())
            .OrderBy(x => x.PositionY).ThenBy(x => x.PositionX)
            .ToList();

        int spaceBetween = entDMaster.SpaceBtwnLines > 0 ? entDMaster.SpaceBtwnLines : 5;

        // Group footer items by PositionY so items on the same horizontal row (e.g. Label and Value) share the same Y
        var groups = footers.GroupBy(x => x.PositionY).OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            float maxRowH = 0f;
            foreach (var item in group)
            {
                object? src = item.Location == InvoiceLabelPosition.Footer_Details.ToString() && entAdjustments.Any()
                    ? entAdjustments.First()
                    : entInvoice;

                EnsurePageSpace(item.Height > 0 ? item.Height : 20);
                DrawItem(item, src, isFixed: false);

                if (item.Height > maxRowH) maxRowH = item.Height;
            }

            float rowH = maxRowH > 0 ? maxRowH : 18f;
            iTopMargin += rowH + spaceBetween;
        }
    }

    private void EnsurePageSpace(float neededHeight)
    {
        if (document == null || section == null) return;
        if (iTopMargin + neededHeight > pageHeight)
        {
            currentPage = section.Pages.Add();
            graphics = currentPage.Graphics;
            iTopMargin = 10;
            currentPageNumber++;
        }
    }

    private float CalculateTotalHeight()
    {
        var headers = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Company_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Invoice_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Party_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Invoice_Details_Header.ToString())
            .ToList();

        float headerEnd = headers.Any() ? headers.Max(x => x.PositionY + x.Height) + 15 : 200f;
        float totalH = headerEnd;

        int space = entDMaster.SpaceBtwnLines > 0 ? entDMaster.SpaceBtwnLines : 5;

        // Details
        foreach (var d in entDetails)
        {
            float rowH = (!string.IsNullOrEmpty(d.ItemName) && d.ItemName.Length > 25) ? 35f : 22f;
            if (!string.IsNullOrEmpty(d.ItemArabicName))
            {
                rowH += 20f;
            }
            totalH += rowH + space;
        }

        // Footers
        var footers = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Footer_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Footer_Bottom.ToString()
                     || x.Location == InvoiceLabelPosition.Footer_Details.ToString())
            .ToList();

        var footerGroups = footers.GroupBy(x => x.PositionY);
        foreach (var g in footerGroups)
        {
            float rowH = g.Max(x => x.Height > 0 ? x.Height : 18f);
            totalH += rowH + space;
        }

        totalH += 35f; // Bottom safety padding

        if (entDMaster.PageHeight > 0 && entDMaster.PageHeight <= 2000)
        {
            return Math.Max(entDMaster.PageHeight, totalH);
        }

        return Math.Max(totalH, 400f);
    }
    #endregion

    #region Rendering Engine
    private void DrawItem(PrintDesignDtls item, object? data, bool isFixed = true)
    {
        if (graphics == null || currentPage == null) return;

        float x = item.PositionX;
        float y = isFixed ? item.PositionY : iTopMargin;
        float w = item.Width;
        float h = item.Height;

        if (item.ControlType == "LabelControl")
        {
            string labelText = item.FieldText ?? string.Empty;
            PdfFont font = BuildFont(item, labelText);
            PdfStringFormat sf = BuildFormat(item, labelText);
            graphics.DrawString(labelText, font, PdfBrushes.Black, new RectangleF(x, y, w, h), sf);
        }
        else if (item.ControlType == "ValueControl" || item.ControlType == "ValueControle")
        {
            if (!isFixed && item.FieldText == "ItemArabicName")
            {
                return;
            }

            object? val = GetValueFromEntity(data, item);

            if (item.FieldText == "InvoiceQRCode" || item.FieldText == "QRCode" || item.FieldText == "Location")
            {
                string? qrText = val?.ToString();
                if (!string.IsNullOrEmpty(qrText))
                {
                    float size = Math.Min(w, h > 0 ? h : w);
                    if (size <= 0) size = 120;
                    DrawQr(currentPage, qrText, x, y, size);
                }
            }
            else if (item.FieldText == "InvoiceBarcode" || item.FieldText == "Barcode")
            {
                string? barcodeText = val?.ToString();
                if (!string.IsNullOrEmpty(barcodeText))
                {
                    DrawBarcode(currentPage, barcodeText, x, y, w, h > 0 ? h : 40);
                }
            }
            else if (item.FieldText == "Logo" || item.FieldText == "HeaderImage" || item.FieldText == "FooterImage" || item.FieldText == "Image")
            {
                byte[]? imgBytes = GetImageBytesFromEntity(data, item.FieldText);
                DrawImage(graphics, imgBytes, x, y, w, h);
            }
            else
            {
                string? valStr = val?.ToString();
                if (!string.IsNullOrEmpty(valStr))
                {
                    PdfFont font = BuildFont(item, valStr);
                    PdfStringFormat sf = BuildFormat(item, valStr);

                    if (item.FieldText == "ItemName")
                    {
                        SizeF size = font.MeasureString(valStr, w, sf);
                        float measuredH = Math.Max(h, size.Height);
                        graphics.DrawString(valStr, font, PdfBrushes.Black, new RectangleF(x, y, w, measuredH), sf);
                        itemHeight = (int)Math.Ceiling(measuredH);
                    }
                    else
                    {
                        graphics.DrawString(valStr, font, PdfBrushes.Black, new RectangleF(x, y, w, h), sf);
                    }
                }
            }
        }
        else if (item.ControlType == "LabelValue" || item.ControlType == "LabelValueControl")
        {
            object? val = GetValueFromEntity(data, item);
            string valStr = val?.ToString() ?? string.Empty;
            string combinedText = string.IsNullOrEmpty(valStr) ? (item.FieldText ?? string.Empty) : $"{item.FieldText}: {valStr}";
            if (!string.IsNullOrEmpty(combinedText))
            {
                PdfFont font = BuildFont(item, combinedText);
                PdfStringFormat sf = BuildFormat(item, combinedText);
                graphics.DrawString(combinedText, font, PdfBrushes.Black, new RectangleF(x, y, w, h), sf);
            }
        }
        else if (item.ControlType == "HorizontalDotedLine")
        {
            PdfPen dottedPen = new PdfPen(Color.Black, 1);
            dottedPen.DashStyle = PdfDashStyle.Custom;
            dottedPen.DashPattern = new float[] { 5.0f, 3.0f };
            graphics.DrawLine(dottedPen, new PointF(x, y), new PointF(x + w, y));
            if (!isFixed)
            {
                iTopMargin += (h > 0 ? h : 2) + entDMaster.SpaceBtwnLines;
            }
        }
        else if (item.ControlType == "HorizontalLine")
        {
            PdfPen pen = new PdfPen(Color.Black, 1);
            graphics.DrawLine(pen, new PointF(x, y), new PointF(x + w, y));
            if (!isFixed)
            {
                iTopMargin += (h > 0 ? h : 2) + entDMaster.SpaceBtwnLines;
            }
        }
        else if (item.ControlType == "VerticalLine")
        {
            PdfPen pen = new PdfPen(Color.Black, 1);
            graphics.DrawLine(pen, new PointF(x, item.PositionY), new PointF(x, item.PositionY + h));
        }
    }

    private void DrawQr(PdfPage page, string text, float x, float y, float size)
    {
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            PdfQRBarcode barcode = new PdfQRBarcode
            {
                ErrorCorrectionLevel = PdfErrorCorrectionLevel.High,
                XDimension = 3,
                Text = text
            };
            barcode.Draw(page, new PointF(x, y), new SizeF(size, size));
        }
        catch { }
    }

    private void DrawBarcode(PdfPage page, string text, float x, float y, float w, float h)
    {
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            PdfCode128Barcode barcode = new PdfCode128Barcode
            {
                BarHeight = h,
                Text = text
            };
            barcode.Draw(page, new PointF(x, y));
        }
        catch { }
    }

    private void DrawImage(PdfGraphics g, byte[]? bytes, float x, float y, float w, float h)
    {
        if (bytes == null || bytes.Length == 0) return;
        try
        {
            using MemoryStream ms = new MemoryStream(bytes);
            PdfBitmap bmp = new PdfBitmap(ms);
            g.DrawImage(bmp, new RectangleF(x, y, w, h));
        }
        catch { }
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Checks if a string contains any Arabic or complex script characters.
    /// </summary>
    public static bool ContainsArabic(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (char c in text)
        {
            if ((c >= 0x0600 && c <= 0x06FF) || // Arabic
                (c >= 0x0750 && c <= 0x077F) || // Arabic Supplement
                (c >= 0x08A0 && c <= 0x08FF) || // Arabic Extended-A
                (c >= 0xFB50 && c <= 0xFDFF) || // Arabic Presentation Forms-A
                (c >= 0xFE70 && c <= 0xFEFF))   // Arabic Presentation Forms-B
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Loads TrueType font bytes with Arabic glyph support from embedded resource, application path, or system fonts.
    /// </summary>
    public static byte[]? GetArabicFontBytes()
    {
        if (s_ArabicFontBytes != null && s_ArabicFontBytes.Length > 0)
            return s_ArabicFontBytes;

        lock (s_FontLock)
        {
            if (s_ArabicFontBytes != null && s_ArabicFontBytes.Length > 0)
                return s_ArabicFontBytes;

            if (CustomArabicFontBytes != null && CustomArabicFontBytes.Length > 0)
            {
                s_ArabicFontBytes = CustomArabicFontBytes;
                return s_ArabicFontBytes;
            }

            // 1. Check embedded resource in ZakiSAASERP.Shared assembly
            try
            {
                var asm = typeof(SyncfusionPrintHelperThermal).Assembly;
                var resName = asm.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("arial.ttf", StringComparison.OrdinalIgnoreCase)
                                      || n.EndsWith("tahoma.ttf", StringComparison.OrdinalIgnoreCase)
                                      || n.EndsWith("NotoSansArabic-Regular.ttf", StringComparison.OrdinalIgnoreCase)
                                      || n.EndsWith("Amiri-Regular.ttf", StringComparison.OrdinalIgnoreCase));
                if (resName != null)
                {
                    using var stream = asm.GetManifestResourceStream(resName);
                    if (stream != null)
                    {
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        s_ArabicFontBytes = ms.ToArray();
                        return s_ArabicFontBytes;
                    }
                }
            }
            catch { }

            // 2. Check candidate font files on disk
            try
            {
                string[] candidatePaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts", "arial.ttf"),
                    Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts", "tahoma.ttf"),
                    Path.Combine(AppContext.BaseDirectory, "wwwroot", "fonts", "arial.ttf"),
                    Path.Combine(AppContext.BaseDirectory, "fonts", "arial.ttf"),
                    Path.Combine(AppContext.BaseDirectory, "arial.ttf"),

                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "tahoma.ttf"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf"),
                    @"C:\Windows\Fonts\arial.ttf",
                    @"C:\Windows\Fonts\tahoma.ttf",

                    "/system/fonts/NotoNaskhArabic-Regular.ttf",
                    "/system/fonts/NotoSansArabic-Regular.ttf",
                    "/system/fonts/DroidSansArabic.ttf",
                    "/system/fonts/Roboto-Regular.ttf",

                    "/usr/share/fonts/truetype/noto/NotoSansArabic-Regular.ttf",
                    "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                    "/usr/share/fonts/truetype/msttcorefonts/arial.ttf"
                };

                foreach (var path in candidatePaths)
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        s_ArabicFontBytes = File.ReadAllBytes(path);
                        return s_ArabicFontBytes;
                    }
                }
            }
            catch { }
        }

        return null;
    }

    private PdfFont BuildFont(PrintDesignDtls item, string? textToRender = null)
    {
        PdfFontStyle style = PdfFontStyle.Regular;
        if (item.IsBold || string.Equals(item.FStyle, "Bold", StringComparison.OrdinalIgnoreCase))
            style |= PdfFontStyle.Bold;
        if (string.Equals(item.FStyle, "Italic", StringComparison.OrdinalIgnoreCase))
            style |= PdfFontStyle.Italic;
        if (string.Equals(item.FStyle, "Underline", StringComparison.OrdinalIgnoreCase))
            style |= PdfFontStyle.Underline;
        if (string.Equals(item.FStyle, "Strikeout", StringComparison.OrdinalIgnoreCase))
            style |= PdfFontStyle.Strikeout;

        float size = item.FontSize > 0 ? item.FontSize : 9f;

        bool hasArabic = ContainsArabic(textToRender) || ContainsArabic(item.FieldText);
        string fontName = (item.Font ?? "").ToLowerInvariant();
        bool isSpecialSymbol = fontName == "symbol" || fontName == "zapfdingbats";

        byte[]? ttfBytes = GetArabicFontBytes();
        if (ttfBytes != null && ttfBytes.Length > 0 && (hasArabic || !isSpecialSymbol))
        {
            var cacheKey = (size, style, true);
            if (fontCache.TryGetValue(cacheKey, out var cachedTtf))
            {
                return cachedTtf;
            }

            try
            {
                PdfTrueTypeFont ttf = new PdfTrueTypeFont(new MemoryStream(ttfBytes), size, style);
                fontCache[cacheKey] = ttf;
                return ttf;
            }
            catch
            {
                // Fall back to standard font if TrueType instantiation fails
            }
        }

        var stdKey = (size, style, false);
        if (fontCache.TryGetValue(stdKey, out var cachedStd))
        {
            return cachedStd;
        }

        PdfFontFamily fontfamily = fontName switch
        {
            "timesroman" or "times new roman" or "times" => PdfFontFamily.TimesRoman,
            "courier" or "courier new" => PdfFontFamily.Courier,
            "symbol" => PdfFontFamily.Symbol,
            "zapfdingbats" => PdfFontFamily.ZapfDingbats,
            _ => PdfFontFamily.Helvetica
        };

        PdfFont stdFont = new PdfStandardFont(fontfamily, size, style);
        fontCache[stdKey] = stdFont;
        return stdFont;
    }

    private PdfStringFormat BuildFormat(PrintDesignDtls item, string? textToRender = null)
    {
        PdfTextAlignment alignment = (item.Alignment ?? "").ToLowerInvariant() switch
        {
            "right" => PdfTextAlignment.Right,
            "center" => PdfTextAlignment.Center,
            _ => PdfTextAlignment.Left
        };

        PdfStringFormat sf = new PdfStringFormat
        {
            Alignment = alignment,
            LineAlignment = PdfVerticalAlignment.Top,
            WordWrap = PdfWordWrapType.Word,
            ComplexScript = true
        };

        return sf;
    }

    private byte[]? GetImageBytesFromEntity(object? data, string fieldName)
    {
        if (data == null) return null;
        try
        {
            var prop = data.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
            if (prop != null && prop.PropertyType == typeof(byte[]))
            {
                return (byte[]?)prop.GetValue(data);
            }

            if (data is CompanyDetails comp)
            {
                if (fieldName.Equals("Logo", StringComparison.OrdinalIgnoreCase) || fieldName.Equals("HeaderImage", StringComparison.OrdinalIgnoreCase))
                    return comp.HeaderImage;
                if (fieldName.Equals("FooterImage", StringComparison.OrdinalIgnoreCase))
                    return comp.FooterImage;
            }
        }
        catch { }
        return null;
    }

    private object? GetValueFromEntity(object? data, PrintDesignDtls dSettings)
    {
        try
        {
            if (data == null || dSettings == null) return null;

            string? strProperty = dSettings.FieldText;
            if (string.IsNullOrEmpty(strProperty)) return null;

            // QR Code resolution (handle both QRCode and InvoiceQRCode)
            if (strProperty.Equals("InvoiceQRCode", StringComparison.OrdinalIgnoreCase) || strProperty.Equals("QRCode", StringComparison.OrdinalIgnoreCase))
            {
                var qrProp = data.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals("InvoiceQRCode", StringComparison.OrdinalIgnoreCase) || x.Name.Equals("QRCode", StringComparison.OrdinalIgnoreCase));
                if (qrProp != null)
                {
                    var qrVal = qrProp.GetValue(data, null);
                    if (qrVal != null && !string.IsNullOrWhiteSpace(qrVal.ToString()))
                        return qrVal.ToString();
                }
            }

            var prop = data.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(strProperty, StringComparison.OrdinalIgnoreCase));
            if (prop == null) return null;

            var val = prop.GetValue(data, null);
            if (val == null) return string.Empty;

            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (underlyingType == typeof(decimal))
            {
                decimal dec = Convert.ToDecimal(val);
                if (prop.Name.Contains("Quantity", StringComparison.OrdinalIgnoreCase) || prop.Name.Equals("Qty", StringComparison.OrdinalIgnoreCase))
                {
                    return dec.ToString(PermanentSettingsData.QuantityFormat);
                }
                return dec.ToString(PermanentSettingsData.CurrencyFormat);
            }

            if (underlyingType == typeof(DateTime))
            {
                DateTime dt = Convert.ToDateTime(val);
                if (!string.IsNullOrEmpty(dSettings.Spec))
                {
                    return dt.ToString(dSettings.Spec);
                }
                return CrudHelper.GetNormalDate(dt)?.ToString()?.Replace("T", " ") ?? dt.ToString("dd-MM-yyyy");
            }

            if (underlyingType == typeof(byte[]))
            {
                return (byte[])val;
            }

            return val.ToString();
        }
        catch
        {
            return null;
        }
    }
    #endregion
}
