using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.Enums;
using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.Others;
using AshproSAASERP.Models.Services;
using AshproSAASERP.Models.ViewModel;
using AshproSAASERP.Shared.Services;
using AshproStringExtension;
using QRCoder;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using ZXing;
using ZXing.Common;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;
using PointF = System.Drawing.PointF;
using SizeF = System.Drawing.SizeF;
using RectangleF = System.Drawing.RectangleF;

namespace AshproSAASERP.Desk.Services;

/// <summary>
/// Direct A4 invoice printing engine for desktop using System.Drawing.Printing.
/// Replicates the layout, multi-page support, and coordinates of SyncfusionPrintHelper.InvoiceMethod.
/// </summary>
public class A4PrintHelper
{
    #region Fields
    private PrintDesignHdr entDMaster = new();
    private List<PrintDesignDtls> entDSettings = new();
    private CompanyDetails entCompany = new();
    private TransactionHdr entInvoice = new();
    private Party? entParty;
    private List<TransactionDtls> entDetails = new();
    private List<FooterDetails> entAdjustments = new();

    private int linesPrinted;
    private int currentPage;
    private int totalPages;
    private float iTopMarginPt;

    private PrintDocument? printDoc;
    #endregion

    #region Helpers
    /// <summary>
    /// Converts 96 DPI designer pixels to 72 DPI points (matches SyncfusionConverter.ConvertFromPixels).
    /// </summary>
    private float Pt(float px) => px * 72f / 96f;
    #endregion

    #region Public API
    /// <summary>
    /// Prints an invoice on A4 paper directly to the configured or default printer.
    /// </summary>
    public void PrintInvoice(PrintParameterDTO dto, int copies = 1)
    {
        entDMaster = dto.hdr ?? new();
        entDSettings = dto.dtls ?? new();
        entCompany = dto.Company ?? new();
        entInvoice = dto.tranHdr ?? new();
        entParty = dto.party;
        entDetails = dto.tranDtls ?? new();
        entAdjustments = dto.fDtls ?? new();

        // Normalize location strings (e.g. replace spaces with underscores to match Enum string values)
        foreach (var d in entDSettings)
        {
            if (!string.IsNullOrEmpty(d.Location))
            {
                d.Location = d.Location.Replace(' ', '_');
            }
        }

        // Initialize line item sequence numbers
        int isl = 1;
        entDetails.ForEach(x => { x.SL = isl++; });

        linesPrinted = 0;
        currentPage = 1;

        int linesPerPage = entDMaster.LinesPerPage > 0 ? entDMaster.LinesPerPage : 20;
        totalPages = entDetails.Count <= 0 ? 1 : (int)Math.Ceiling((double)entDetails.Count / Math.Max(1, linesPerPage));

        printDoc = new PrintDocument
        {
            PrintController = new StandardPrintController(),
            OriginAtMargins = false
        };

        // Configure A4 page settings (827 x 1169 in 100ths of an inch)
        PaperSize? a4Paper = null;
        try
        {
            foreach (PaperSize ps in printDoc.PrinterSettings.PaperSizes)
            {
                if (ps.Kind == PaperKind.A4 || ps.PaperName.Equals("A4", StringComparison.OrdinalIgnoreCase))
                {
                    a4Paper = ps;
                    break;
                }
            }
        }
        catch
        {
            // Printer queries might fail on disconnected drivers, fall back to explicit A4
        }

        printDoc.DefaultPageSettings.PaperSize = a4Paper ?? new PaperSize("A4", 827, 1169) { RawKind = (int)PaperKind.A4 };
        printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

        // Target printer
        string? invoicePrinter = !string.IsNullOrWhiteSpace(entDMaster.PrinterName)
            ? entDMaster.PrinterName
            : PermanentSettingsData.InvoicePrinterName;

        if (!string.IsNullOrWhiteSpace(invoicePrinter))
        {
            string targetPrinter = invoicePrinter;
            try
            {
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    if (printer.Equals(targetPrinter, StringComparison.OrdinalIgnoreCase))
                    {
                        targetPrinter = printer;
                        break;
                    }
                }
            }
            catch { }
            printDoc.PrinterSettings.PrinterName = targetPrinter;
        }

        printDoc.PrinterSettings.Copies = (short)Math.Max(1, copies);

        printDoc.BeginPrint += (s, e) =>
        {
            linesPrinted = 0;
            currentPage = 1;
        };

        printDoc.PrintPage += PrintDoc_PrintPage;

        try
        {
            printDoc.Print();
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // User cancelled print dialog
        }
    }
    #endregion

    #region PrintPage Event Handler
    private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
    {
        Graphics g = e.Graphics!;

        // Use GraphicsUnit.Point (72 DPI) so all coordinates match Syncfusion / standard PDF coordinates exactly
        g.PageUnit = GraphicsUnit.Point;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // Classify design items into sections
        var headerSettings = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Company_Section.ToString() ||
                        x.Location == InvoiceLabelPosition.Invoice_Section.ToString() ||
                        x.Location == InvoiceLabelPosition.Party_Section.ToString() ||
                        x.Location == InvoiceLabelPosition.Invoice_Details_Header.ToString() ||
                        (string.IsNullOrEmpty(x.Location) && x.ControlType.Contains("Line")))
            .OrderBy(x => x.PositionY).ThenBy(x => x.PositionX)
            .ToList();

        var detailSettings = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Details_Area.ToString())
            .OrderBy(x => x.PositionX)
            .ToList();

        var footerSettings = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Footer_Section.ToString() ||
                        x.Location == InvoiceLabelPosition.Footer_Bottom.ToString() ||
                        x.Location == InvoiceLabelPosition.Footer_Details.ToString())
            .OrderBy(x => x.PositionY).ThenBy(x => x.PositionX)
            .ToList();

        // 1. Draw Header and All Border/Grid Lines on Every Page
        foreach (var item in headerSettings)
        {
            object? src = item.Location switch
            {
                var l when l == InvoiceLabelPosition.Company_Section.ToString() => entCompany,
                var l when l == InvoiceLabelPosition.Invoice_Section.ToString() => entInvoice,
                var l when l == InvoiceLabelPosition.Party_Section.ToString() => entParty,
                _ => null
            };
            DrawItem(g, item, src, isFixedY: true);
        }

        // 2. Determine Details Starting Y in Points
        if (detailSettings.Any())
        {
            iTopMarginPt = Pt(detailSettings.Min(x => x.PositionY));
        }
        else if (headerSettings.Any())
        {
            iTopMarginPt = Pt(headerSettings.Max(x => x.PositionY + x.Height) + 15);
        }
        else
        {
            iTopMarginPt = Pt(280);
        }

        int linesPerPage = entDMaster.LinesPerPage > 0 ? entDMaster.LinesPerPage : 20;
        float defaultRowHeightPt = Pt(detailSettings.Any(x => x.Height > 0)
            ? detailSettings.Where(x => x.Height > 0).Min(x => (float)x.Height)
            : 20f);

        // Find table bottom line to avoid table rows running over into bottom QR / Total area
        float maxTableYPt = Pt(920f);
        var tableBottomLine = entDSettings.FirstOrDefault(x => x.ControlType == "HorizontalLine" && x.PositionY > 500 && x.PositionY < 1000);
        if (tableBottomLine != null)
        {
            maxTableYPt = Pt(tableBottomLine.PositionY) - 5f;
        }
        else if (footerSettings.Any())
        {
            maxTableYPt = Pt(footerSettings.Min(f => f.PositionY)) - 30f;
        }

        int linesOnThisPage = 0;

        // 3. Print Details Line by Line
        while (linesPrinted < entDetails.Count)
        {
            var currentRow = entDetails[linesPrinted];
            bool isLastLine = (linesPrinted == entDetails.Count - 1);

            // Dynamically measure row height based on all columns (for multi-line item names)
            float maxMeasuredHeightPt = 0f;
            foreach (var col in detailSettings)
            {
                if (col.ControlType == "ValueControl" || col.ControlType == "ValueControle")
                {
                    string? val = GetValueFromEntity(currentRow, col)?.ToString();
                    if (!string.IsNullOrEmpty(val))
                    {
                        using Font f = BuildFont(col);
                        using StringFormat sf = BuildFormat(col);
                        float colWidthPt = Pt(col.Width);
                        if (colWidthPt > 0)
                        {
                            SizeF measuredPt = g.MeasureString(val, f, (int)Math.Ceiling(colWidthPt), sf);
                            if (measuredPt.Height > maxMeasuredHeightPt)
                            {
                                maxMeasuredHeightPt = measuredPt.Height;
                            }
                        }
                    }
                }
            }

            float rowHeightPt = Math.Max(defaultRowHeightPt, maxMeasuredHeightPt + 2f);

            // Check if row exceeds page space
            if (linesOnThisPage >= linesPerPage || (iTopMarginPt + rowHeightPt >= maxTableYPt && !isLastLine))
            {
                // Draw footer page number and advance to next page
                DrawPageNumber(g, currentPage, totalPages);
                currentPage++;
                e.HasMorePages = true;
                return;
            }

            // Draw current detail row
            foreach (var col in detailSettings)
            {
                if (!string.IsNullOrEmpty(col.Spec) && col.Spec.Equals("M", StringComparison.OrdinalIgnoreCase))
                {
                    string? value = GetValueFromEntity(currentRow, col)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        iTopMarginPt += rowHeightPt;
                        DrawItem(g, col, currentRow, isFixedY: false, customHeightPt: rowHeightPt);
                    }
                }
                else
                {
                    DrawItem(g, col, currentRow, isFixedY: false, customHeightPt: rowHeightPt);
                }
            }

            iTopMarginPt += rowHeightPt;
            linesPrinted++;
            linesOnThisPage++;
        }

        // 4. Print Footer Section on Final Page
        foreach (var item in footerSettings)
        {
            DrawItem(g, item, entInvoice, isFixedY: true);
        }

        // 5. Draw Page Number
        DrawPageNumber(g, currentPage, Math.Max(totalPages, currentPage));
        e.HasMorePages = false;
    }
    #endregion

    #region Rendering Engine
    private void DrawItem(Graphics g, PrintDesignDtls item, object? data, bool isFixedY = true, float? customHeightPt = null)
    {
        float x = Pt(item.PositionX);
        float y = isFixedY ? Pt(item.PositionY) : iTopMarginPt;
        float w = Pt(item.Width);
        float h = customHeightPt ?? Pt(item.Height);

        using Font font = BuildFont(item);
        using StringFormat sf = BuildFormat(item);

        if (item.ControlType == "LabelControl")
        {
            g.DrawString(item.FieldText, font, Brushes.Black, new RectangleF(x, y, w, h), sf);
        }
        else if (item.ControlType == "ValueControl" || item.ControlType == "ValueControle")
        {
            if (item.FieldText == "Logo" || item.FieldText == "HeaderImage" || item.FieldText == "FooterImage" || item.FieldText == "Image")
            {
                byte[]? imgBytes = GetImageBytesFromEntity(data, item.FieldText);
                DrawImage(g, imgBytes, x, y, w, h);
            }
            else if (item.FieldText == "InvoiceQRCode" || item.FieldText == "QRCode" || item.FieldText == "Location")
            {
                string? qrText = GetValueFromEntity(data, item)?.ToString();
                float size = Math.Min(w, h);
                DrawQr(g, qrText, x, y, size > 0 ? size : Pt(120));
            }
            else if (item.FieldText == "InvoiceBarcode" || item.FieldText == "Barcode")
            {
                string? barcodeText = GetValueFromEntity(data, item)?.ToString();
                DrawBarcode(g, barcodeText, x, y, w, h > 0 ? h : Pt(40));
            }
            else
            {
                string? valStr = GetValueFromEntity(data, item)?.ToString();
                if (!string.IsNullOrEmpty(valStr))
                {
                    g.DrawString(valStr, font, Brushes.Black, new RectangleF(x, y, w, h), sf);
                }
            }
        }
        else if (item.ControlType == "HorizontalDotedLine")
        {
            using Pen p = new Pen(Color.Black, 1f) { DashPattern = new float[] { 5, 3 } };
            g.DrawLine(p, x, y, x + w, y);
        }
        else if (item.ControlType == "HorizontalLine")
        {
            using Pen p = new Pen(Color.Black, 1f);
            g.DrawLine(p, x, y, x + w, y);
        }
        else if (item.ControlType == "VerticalLine")
        {
            using Pen p = new Pen(Color.Black, 1f);
            g.DrawLine(p, x, Pt(item.PositionY), x, Pt(item.PositionY + item.Height));
        }
    }

    private void DrawPageNumber(Graphics g, int page, int total)
    {
        using Font font = new Font("Arial", 8f, FontStyle.Regular, GraphicsUnit.Point);
        float y = Pt(1095) + 3f;
        if (y > 825f) y = 825f;
        g.DrawString($"Page {page}/{total}", font, Brushes.Black, new PointF(Pt(35), y));
    }

    private void DrawImage(Graphics g, byte[]? bytes, float x, float y, float w, float h)
    {
        if (bytes == null || bytes.Length == 0) return;
        try
        {
            using MemoryStream ms = new MemoryStream(bytes);
            using Image img = Image.FromStream(ms);
            g.DrawImage(img, new RectangleF(x, y, w, h));
        }
        catch { }
    }

    private void DrawQr(Graphics g, string? text, float x, float y, float size)
    {
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            using QRCodeGenerator gen = new();
            using QRCodeData data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using QRCode qr = new(data);
            using Bitmap bmp = qr.GetGraphic(20);
            g.DrawImage(bmp, new RectangleF(x, y, size, size));
        }
        catch { }
    }

    private void DrawBarcode(Graphics g, string? text, float x, float y, float w, float h)
    {
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = (int)Math.Ceiling(w * 96f / 72f),
                    Height = (int)Math.Ceiling(h * 96f / 72f),
                    Margin = 0
                }
            };

            using Bitmap bmp = writer.Write(text);
            g.DrawImage(bmp, new RectangleF(x, y, w, h));
        }
        catch { }
    }
    #endregion

    #region Helpers
    private Font BuildFont(PrintDesignDtls item)
    {
        FontStyle style = FontStyle.Regular;
        if (item.IsBold || string.Equals(item.FStyle, "Bold", StringComparison.OrdinalIgnoreCase))
            style |= FontStyle.Bold;
        if (string.Equals(item.FStyle, "Italic", StringComparison.OrdinalIgnoreCase))
            style |= FontStyle.Italic;
        if (string.Equals(item.FStyle, "Underline", StringComparison.OrdinalIgnoreCase))
            style |= FontStyle.Underline;
        if (string.Equals(item.FStyle, "Strikeout", StringComparison.OrdinalIgnoreCase))
            style |= FontStyle.Strikeout;

        float size = item.FontSize > 0 ? item.FontSize : 9f;
        string fontName = item.Font?.Trim() ?? "";

        string fontFamily = fontName switch
        {
            "Helvetica" => "Arial",
            "ZapfDingbats" => "Wingdings",
            "Courier" => "Courier New",
            "Symbol" => "Symbol",
            _ => "Times New Roman" // Matches Syncfusion's default TimesRoman
        };

        try
        {
            return new Font(fontFamily, size, style, GraphicsUnit.Point);
        }
        catch
        {
            return new Font(FontFamily.GenericSansSerif, size, style, GraphicsUnit.Point);
        }
    }

    private StringFormat BuildFormat(PrintDesignDtls item)
    {
        StringFormat sf = new StringFormat
        {
            Trimming = StringTrimming.Word,
            FormatFlags = StringFormatFlags.LineLimit
        };

        sf.Alignment = (item.Alignment ?? "").ToLowerInvariant() switch
        {
            "right" => StringAlignment.Far,
            "center" => StringAlignment.Center,
            _ => StringAlignment.Near
        };

        sf.LineAlignment = StringAlignment.Near;
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

            // Fallback for company header/logo
            if (data is CompanyDetails comp)
            {
                if (fieldName.Equals("Logo", StringComparison.OrdinalIgnoreCase) || fieldName.Equals("HeaderImage", StringComparison.OrdinalIgnoreCase))
                {
                    return comp.HeaderImage;
                }
                if (fieldName.Equals("FooterImage", StringComparison.OrdinalIgnoreCase))
                {
                    return comp.FooterImage;
                }
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

            // QR Code property resolution (handle both QRCode and InvoiceQRCode)
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
