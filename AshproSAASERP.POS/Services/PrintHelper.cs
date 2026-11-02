using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.Enums;
using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.Others;
using AshproSAASERP.Models.Services;
using AshproSAASERP.Models.ViewModel;
using AshproStringExtension;
using QRCoder;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace AshproSAASERP.POS.Services;

public class PrintHelper
{
    #region Fields
    private List<PrintDesignDtls> entDSettings = new();
    private List<TransactionDtls> entDetails = new();
    private PrintDesignHdr entDMaster = new();
    private CompanyDetails entCompany = new();
    private TransactionHdr entInvoice = new();
    private Party? entParty;
    private int iTopMargin;
    private int linesPrinted;
    private int iIndex;
    private bool isPrinted;
    private int totalPages;
    private int itemHeight;
    private readonly PrintDocument printDoc;
    #endregion

    #region Constructor
    public PrintHelper()
    {
        PrintController printController = new StandardPrintController();
        printDoc = new PrintDocument();
        printDoc.PrintController = printController;
        printDoc.PrintPage += PrintDoc_PrintPage;
    }
    #endregion

    #region Public API
    public void PrintInvoice(PrintParameterDTO dto)
    {
        entDMaster = dto.hdr ?? new PrintDesignHdr { LinesPerPage = 100, SpaceBtwnLines = 2 };
        entDSettings = dto.dtls ?? new List<PrintDesignDtls>();
        entCompany = dto.Company ?? new CompanyDetails();
        entInvoice = dto.tranHdr ?? new TransactionHdr();
        entParty = dto.party;
        entDetails = dto.tranDtls ?? new List<TransactionDtls>();
        linesPrinted = 0;
        iIndex = 0;
        isPrinted = false;
        iTopMargin = 0;

        int linesPerPage = entDMaster.LinesPerPage > 0 ? entDMaster.LinesPerPage : 100;
        totalPages = (entDetails.Count / linesPerPage) + 1;

        printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        string? invoicePrinter = !string.IsNullOrWhiteSpace(entDMaster.PrinterName)
            ? entDMaster.PrinterName
            : PermanentSettingsData.InvoicePrinterName;

        if (!string.IsNullOrWhiteSpace(invoicePrinter))
        {
            string targetPrinter = invoicePrinter;
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                if (printer.Equals(targetPrinter, StringComparison.OrdinalIgnoreCase))
                {
                    targetPrinter = printer;
                    break;
                }
            }
            printDoc.PrinterSettings.PrinterName = targetPrinter;
        }

        try
        {
            printDoc.Print();
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // User cancelled the print dialog
        }
    }

    public void PrintText(string text, string? printerName = null)
    {
        try
        {
            var doc = new PrintDocument();
            doc.PrintController = new StandardPrintController();
            string? target = !string.IsNullOrWhiteSpace(printerName) ? printerName : PermanentSettingsData.InvoicePrinterName;
            if (!string.IsNullOrWhiteSpace(target))
            {
                doc.PrinterSettings.PrinterName = target;
            }

            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineIdx = 0;

            doc.PrintPage += (sender, e) =>
            {
                using var font = new Font("Courier New", 9f);
                float y = 10;
                float lineHeight = font.GetHeight(e.Graphics!);

                while (lineIdx < lines.Length)
                {
                    if (y + lineHeight > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }
                    e.Graphics!.DrawString(lines[lineIdx], font, System.Drawing.Brushes.Black, 10, y);
                    y += lineHeight;
                    lineIdx++;
                }
                e.HasMorePages = false;
            };

            doc.Print();
        }
        catch { }
    }

    public async Task OpenCashDrawer(string? printerName = null)
    {
        await Task.Run(() =>
        {
            string? targetPrinter = !string.IsNullOrWhiteSpace(printerName)
                ? printerName
                : PermanentSettingsData.InvoicePrinterName;

            if (string.IsNullOrWhiteSpace(targetPrinter))
            {
                try
                {
                    targetPrinter = new PrinterSettings().PrinterName;
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(targetPrinter))
            {
                throw new InvalidOperationException("No invoice or receipt printer configured to open cash drawer.");
            }

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

            // Standard ESC/POS pulse sequences (Pin 2 and Pin 5 kick)
            byte[] drawerCodes = [
                27, 112, 0, 25, 250,
                27, 112, 1, 25, 250,
                16, 20, 1, 0, 5
            ];

            bool success = RawPrinterHelper.SendBytesToPrinter(targetPrinter, drawerCodes);
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                throw new Exception($"Failed to send open drawer signal to printer '{targetPrinter}'. Error code: {error}");
            }
        });
    }
    #endregion

    #region Core Print Engine
    private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
    {
        Graphics g = e.Graphics!;

        if (!isPrinted)
        {
            PrintHeader(g);
            PrintDetailsHeader(g);
            isPrinted = true;
        }

        var detailsStart = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Details_Area.ToString() && x.FieldText != "ItemArabicName")
            .OrderBy(x => x.PositionY)
            .FirstOrDefault();

        if (detailsStart != null)
        {
            iTopMargin = detailsStart.PositionY;
        }
        else
        {
            var anyDetail = entDSettings.FirstOrDefault(x => x.Location == InvoiceLabelPosition.Details_Area.ToString());
            if (anyDetail != null) iTopMargin = anyDetail.PositionY;
        }

        PrintDetails(g);
        PrintFooterSection(g);
    }

    private void PrintHeader(Graphics g)
    {
        var headers = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Company_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Invoice_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Party_Section.ToString())
            .OrderBy(x => x.PositionY);

        foreach (var item in headers)
        {
            object? src =
                item.Location == InvoiceLabelPosition.Company_Section.ToString() ? entCompany :
                item.Location == InvoiceLabelPosition.Invoice_Section.ToString() ? entInvoice :
                entParty;

            DrawItem(g, item, src);
        }

        if (headers.Any())
        {
            iTopMargin = headers.Max(x => x.PositionY + x.Height) + 10;
        }
    }

    private void PrintDetailsHeader(Graphics g)
    {
        var hdr = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Invoice_Details_Header.ToString())
            .OrderBy(x => x.PositionY);

        foreach (var item in hdr)
        {
            DrawItem(g, item, null);
        }
    }

    private void PrintDetails(Graphics g)
    {
        int isl = 1;
        entDetails.ForEach(x => x.SL = isl++);
        var ds = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Details_Area.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        var firstLineItems = ds.Where(x => x.FieldText != "ItemArabicName").ToList();
        var arabicItem = ds.FirstOrDefault(x => x.FieldText == "ItemArabicName");
        int linesPerPage = entDMaster.LinesPerPage > 0 ? entDMaster.LinesPerPage : 100;

        while (linesPrinted < entDetails.Count && iIndex < linesPerPage)
        {
            var currentRow = entDetails[linesPrinted];
            itemHeight = 18;

            foreach (var item in firstLineItems)
            {
                DrawItem(g, item, currentRow, false);
            }

            int line1Height = Math.Max(itemHeight, 18);
            int arabicHeight = 0;

            if (arabicItem != null)
            {
                string? arabicVal = GetValueFromEntity(currentRow, arabicItem)?.ToString();
                if (!string.IsNullOrWhiteSpace(arabicVal))
                {
                    Font arabicFont = BuildFont(arabicItem);
                    StringFormat arabicSf = BuildFormat(arabicItem);

                    int arabicX = arabicItem.PositionX;
                    int arabicY = iTopMargin + line1Height;
                    int arabicWidth = arabicItem.Width > 0 ? arabicItem.Width : 140;

                    var arabicSize = g.MeasureString(arabicVal, arabicFont, arabicWidth, arabicSf);
                    arabicHeight = Math.Max((int)Math.Ceiling(arabicSize.Height), 16);

                    g.DrawString(arabicVal, arabicFont, Brushes.Black,
                        new RectangleF(arabicX, arabicY, arabicWidth, arabicHeight), arabicSf);
                }
            }

            int rowTotalHeight = line1Height + arabicHeight;
            iTopMargin += rowTotalHeight + entDMaster.SpaceBtwnLines;
            linesPrinted++;
            iIndex++;
        }

        if (iIndex >= linesPerPage)
        {
            iIndex = 0;
            isPrinted = false;
        }
    }

    private void DrawItem(Graphics g, PrintDesignDtls item, object? data, bool IsFixed = true)
    {
        Font font = BuildFont(item);
        StringFormat sf = BuildFormat(item);

        int x = item.PositionX;
        int y = iTopMargin;

        if (item.ControlType == "LabelControl")
        {
            g.DrawString(item.FieldText, font, Brushes.Black,
                new RectangleF(x, IsFixed ? item.PositionY : y, item.Width, item.Height), sf);
        }
        else if (item.ControlType == "ValueControl")
        {
            if (!IsFixed && item.FieldText == "ItemArabicName")
            {
                return;
            }

            object? val = GetValueFromEntity(data, item);

            if (item.FieldText == "InvoiceQRCode")
            {
                DrawQrZxing(g, val?.ToString(), x, IsFixed ? item.PositionY : y, item.Width, item.Height);
            }
            else if (item.FieldText == "InvoiceBarcode")
            {
                DrawBarcode(g, val?.ToString(), x, IsFixed ? item.PositionY : y, item.Width, item.Height);
            }
            else
            {
                if (item.FieldText == "ItemName")
                {
                    var size = g.MeasureString(val?.ToString() ?? "", font, item.Width, sf);
                    int measuredH = Math.Max((int)Math.Ceiling(size.Height), 18);
                    g.DrawString(val?.ToString() ?? "", font, Brushes.Black,
                        new RectangleF(x, IsFixed ? item.PositionY : y, item.Width, measuredH), sf);

                    itemHeight = measuredH;
                }
                else
                {
                    g.DrawString(val?.ToString() ?? "", font, Brushes.Black,
                        new RectangleF(x, IsFixed ? item.PositionY : y, item.Width, item.Height), sf);
                }
            }
        }
        else if (item.ControlType == "HorizontalDotedLine")
        {
            using Pen p = new Pen(Color.Black, 1) { DashPattern = [5, 3] };
            g.DrawLine(p, x, IsFixed ? item.PositionY : y, x + item.Width, IsFixed ? item.PositionY : y);
            if (!IsFixed)
            {
                iTopMargin = iTopMargin + item.Height + entDMaster.SpaceBtwnLines;
            }
        }
        else if (item.ControlType == "HorizontalLine")
        {
            using Pen p = new Pen(Color.Black, 1);
            g.DrawLine(p, x, item.PositionY, x + item.Width, item.PositionY);
        }
        else if (item.ControlType == "VerticalLine")
        {
            using Pen p = new Pen(Color.Black, 1);
            g.DrawLine(p, x, item.PositionY, x, item.PositionY + item.Height);
        }
    }

    private void DrawQrZxing(Graphics g, string? text, int x, int y, int width, int height)
    {
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = width > 0 ? width : 100,
                    Height = height > 0 ? height : 100,
                    Margin = 1
                }
            };

            var skBitmap = writer.Write(text);
            if (skBitmap != null)
            {
                using var bitmap = ToSystemDrawingBitmap(skBitmap);
                if (bitmap != null)
                {
                    g.DrawImage(bitmap, x, y, width, height);
                }
            }
        }
        catch { }
    }

    public static Bitmap? ToSystemDrawingBitmap(SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(data.ToArray());
        using var temp = new Bitmap(stream);
        return new Bitmap(temp);
    }

    private void DrawBarcode(Graphics g, string? text, int x, int y, int w, int h)
    {
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = w > 0 ? w : 120,
                    Height = h > 0 ? h : 40,
                    Margin = 0
                }
            };
            var skBitmap = writer.Write(text);
            if (skBitmap != null)
            {
                using var bmp = ToSystemDrawingBitmap(skBitmap);
                if (bmp != null)
                {
                    g.DrawImage(bmp, x, y, w, h);
                }
            }
        }
        catch { }
    }

    private void PrintFooterSection(Graphics g)
    {
        var headers = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Footer_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Footer_Bottom.ToString())
            .OrderBy(x => x.PositionY).ThenBy(x => x.PositionX);

        foreach (var item in headers)
        {
            DrawItem(g, item, entInvoice, false);
            if (item.FieldText.IsArabic() || item.Spec == "N")
            {
                iTopMargin = iTopMargin + item.Height + entDMaster.SpaceBtwnLines;
            }
        }
    }

    private Font BuildFont(PrintDesignDtls item)
    {
        FontStyle style = FontStyle.Regular;
        if (item.FStyle == "Bold") style = FontStyle.Bold;
        if (item.FStyle == "Italic") style = FontStyle.Italic;
        int size = item.FontSize > 0 ? item.FontSize : 10;
        return new Font(item.Font ?? "Arial", size, style);
    }

    private StringFormat BuildFormat(PrintDesignDtls item)
    {
        return item.Alignment switch
        {
            "Right" => new StringFormat { Alignment = StringAlignment.Far },
            "Center" => new StringFormat { Alignment = StringAlignment.Center },
            _ => new StringFormat { Alignment = StringAlignment.Near }
        };
    }

    private object? GetValueFromEntity(object? data, PrintDesignDtls dSettings)
    {
        if (data == null || dSettings == null) return null;
        try
        {
            string? strProperty = dSettings.FieldText;
            if (string.IsNullOrEmpty(strProperty)) return null;
            var item = data.GetType().GetProperties().FirstOrDefault(x => x.Name == strProperty);
            if (item == null) return null;

            var underlyingType = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType;
            if (underlyingType == typeof(decimal))
            {
                if (item.Name != "Quantity")
                {
                    return item.GetValue(data, null).toDecimal().ToString("N2");
                }
                else
                {
                    return item.GetValue(data, null).toDecimal().ToString("G29");
                }
            }

            switch (item.PropertyType.Name)
            {
                case "DateTime":
                    DateTime dt = item.GetValue(data, null)?.ToString()?.toDateTime() ?? DateTime.Now;
                    string? format = dSettings.Spec;
                    return string.IsNullOrEmpty(format) ? dt.ToString("dd-MM-yyyy") : dt.ToString(format);
                case "Byte[]":
                    return (byte[]?)item.GetValue(data, null);
                default:
                    return item.GetValue(data, null);
            }
        }
        catch
        {
            return null;
        }
    }
    #endregion
}

public static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDocName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDataType;
    }

    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    public static bool SendBytesToPrinter(string szPrinterName, byte[] bytes)
    {
        IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
        Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);

        bool success = false;
        if (OpenPrinter(szPrinterName, out IntPtr hPrinter, IntPtr.Zero))
        {
            DOCINFOA di = new DOCINFOA
            {
                pDocName = "OpenCashDrawer",
                pDataType = "RAW"
            };

            if (StartDocPrinter(hPrinter, 1, di))
            {
                if (StartPagePrinter(hPrinter))
                {
                    success = WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out _);
                    EndPagePrinter(hPrinter);
                }
                EndDocPrinter(hPrinter);
            }
            ClosePrinter(hPrinter);
        }
        Marshal.FreeCoTaskMem(pUnmanagedBytes);
        return success;
    }
}

