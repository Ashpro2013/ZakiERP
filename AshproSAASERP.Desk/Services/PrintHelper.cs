using AshproSAASERP.Models.Services;
using AshproSAASERP.Shared.Services;
using AshproStringExtension;
using global::AshproSAASERP.Models.DTO;
using global::AshproSAASERP.Models.Enums;
using global::AshproSAASERP.Models.Masters;
using global::AshproSAASERP.Models.Others;
using global::AshproSAASERP.Models.ViewModel;
using MudBlazor;
using QRCoder;
using SkiaSharp;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Graphics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using static MudBlazor.CategoryTypes;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace AshproSAASERP.Desk.Services;

public class PrintHelper
{
    #region Fields
    List<PrintDesignDtls> entDSettings = new();
    List<TransactionDtls> entDetails = new();
    PrintDesignHdr entDMaster = new();
    CompanyDetails entCompany = new();
    TransactionHdr entInvoice = new();
    Party? entParty;
    int iTopMargin;
    int linesPrinted;
    int iIndex;
    bool isPrinted;
    bool isLast;

    int currentPage = 1;
    int totalPages;
    int itemHeight;

    PrintDocument printDoc;

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
        entDMaster = dto.hdr;
        entDSettings = dto.dtls;
        entCompany = dto.Company;
        entInvoice = dto.tranHdr;
        entParty = dto.party;
        entDetails = dto.tranDtls;
        linesPrinted = 0;
        iIndex = 0;
        isPrinted = false;
        isLast = false;
        iTopMargin = 0;
        currentPage = 1;

        totalPages = (entDetails.Count / entDMaster.LinesPerPage) + 1;

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
            // User cancelled the file save / print dialog
        }
    }
    private string GetValueFromBarcodeEntity(BarcodeData data, BarcodeDesignDtls dSettings)
    {
        try
        {
            if (dSettings.Name == "Label")
                return "";
            if (data == null) { return null; }
            string strProperty = dSettings.Name!;
            object? obj = null;
            var item = data.GetType().GetProperties().FirstOrDefault(x => x.Name == strProperty);
            if (item == null) { return null; }
            var underlyingType = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType;
            if (underlyingType == typeof(decimal))
            {
                if (item.Name != "Quantity")
                {
                    obj = item.GetValue(data, null).toDecimal().ToString(PermanentSettingsData.CurrencyFormat);
                }
                if (obj == null) { obj = 0; }
            }
            else if (underlyingType == typeof(byte[]))
            {
                byte[] logo = (byte[])item.GetValue(data, null)!;
                obj = logo;
            }
            else
            {
                obj = item.GetValue(data, null);
                if (obj == null) { obj = string.Empty; }
            }
            return obj==null ? "" : obj.ToString();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            return "";
        }
    }
    private Font BuildFontBarcode(BarcodeDesignDtls item)
    {
        FontStyle style = FontStyle.Regular;
        if (item.fStyle == "Bold") style = FontStyle.Bold;
        if (item.fStyle == "Italic") style = FontStyle.Italic;
        if (item.fontSize == 0) { item.fontSize = 10; }
        return new Font(item.fontFamily ?? "Arial", item.fontSize, style);
    }

    private StringFormat BuildFormatBarcode(BarcodeDesignDtls item)
    {
        return item.Alignment switch
        {
            "Right" => new StringFormat { Alignment = StringAlignment.Far },
            "Center" => new StringFormat { Alignment = StringAlignment.Center },
            _ => new StringFormat { Alignment = StringAlignment.Near }
        };
    }

    /// <summary>
    /// Prints barcode labels one per page based on BarcodeDesignDTO and item quantities.
    /// </summary>
    public async Task PrintBarcodeOld(BarcodeDesignDTO dto, List<BarcodeData> entBarcodeData)
    {
        try
        {
            var hdr = dto.hdr;
            var dtls = dto.dtls;
            using PrintDocument barcodePrintDoc = new PrintDocument();
            barcodePrintDoc.PrintController = new StandardPrintController();
            barcodePrintDoc.OriginAtMargins = false;
            barcodePrintDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            // Configure target printer from BarcodeDesignHdr or permanent settings
            barcodePrintDoc.PrinterSettings.PrinterName = !string.IsNullOrWhiteSpace(hdr?.PrinterName)
                ? hdr.PrinterName
                : PermanentSettingsData.BarcodePrinterName;

            if (!string.IsNullOrWhiteSpace(hdr?.PaperName))
            {
                try
                {
                    foreach (PaperSize ps in barcodePrintDoc.PrinterSettings.PaperSizes)
                    {
                        if (ps.PaperName.Equals(hdr.PaperName, StringComparison.OrdinalIgnoreCase))
                        {
                            barcodePrintDoc.DefaultPageSettings.PaperSize = ps;
                            break;
                        }
                    }
                }
                catch { }
            }
            else if (hdr?.Width > 0 && hdr?.Height > 0)
            {
                try
                {
                    int w = (int)Math.Round((hdr.Width.Value / 96.0) * 100);
                    int h = (int)Math.Round((hdr.Height.Value / 96.0) * 100);
                    barcodePrintDoc.DefaultPageSettings.PaperSize = new PaperSize("Custom", w, h);
                }
                catch { }
            }

            // Flatten barcode data by Quantity so each unit gets its own page
            List<BarcodeData> barcodeList = new();
            if (entBarcodeData != null)
            {
                foreach (var barcodeData in entBarcodeData)
                {
                    int qty = barcodeData.Quantity > 0 ? barcodeData.Quantity : 1;
                    for (int i = 0; i < qty; i++)
                    {
                        barcodeList.Add(barcodeData);
                    }
                }
            }

            if (barcodeList.Count == 0)
            {
                return;
            }

            int currentBarcodeIndex = 0;

            barcodePrintDoc.BeginPrint += (s, e) =>
            {
                currentBarcodeIndex = 0;
            };

            barcodePrintDoc.PrintPage += (s, e) =>
            {
                try
                {
                    if (currentBarcodeIndex < barcodeList.Count)
                    {
                        var barcodeData = barcodeList[currentBarcodeIndex];
                        if (dtls != null && e.Graphics != null)
                        {
                            if (hdr?.IsBorderIncluded == true && hdr.Width > 0 && hdr.Height > 0)
                            {
                                e.Graphics.DrawRectangle(Pens.Black, 0, 0, hdr.Width.Value, hdr.Height.Value);
                            }

                            foreach (var item in dtls)
                            {
                                using System.Drawing.Font font = BuildFontBarcode(item);
                                using StringFormat sf = BuildFormatBarcode(item);
                                if (item.Name != "Barcode")
                                {
                                    string? val = GetValueFromBarcodeEntity(barcodeData, item);
                                    e.Graphics.DrawString($"{item.Prefix}{val}", font, Brushes.Black, new RectangleF(item.Position_x, item.Position_y, item.Width, item.Height), sf);
                                }
                                else
                                {
                                    string? sValue = GetValueFromBarcodeEntity(barcodeData, item);
                                    if (!string.IsNullOrEmpty(sValue))
                                    {
                                        CreateQRCode(e.Graphics, sValue, item.Width, item.Height, item.Position_x, item.Position_y, isBarcode: hdr?.IsBarcode ?? true, includeLabel: hdr?.GenerateLabel ?? true);
                                    }
                                }
                            }
                        }

                        currentBarcodeIndex++;
                    }

                    // Print next barcode on the next page if remaining
                    e.HasMorePages = currentBarcodeIndex < barcodeList.Count;
                }
                catch (Exception ex)
                {
                    e.HasMorePages = false;
                    Console.WriteLine($"Error printing barcode page: {ex.Message}");
                }
            };

            await Task.Run(() => barcodePrintDoc.Print());
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // User cancelled print dialog
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error printing barcode: {ex.Message}");
            throw;
        }
    }
    public async Task PrintBarcode(
        BarcodeDesignDTO dto,
        List<BarcodeData> entBarcodeData)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (entBarcodeData == null || entBarcodeData.Count == 0)
            return;

        try
        {
            using PrintDocument barcodePrintDoc = SetupBarcodePrintDocument(dto, entBarcodeData, out var list);
            if (list.Count == 0)
                return;

            barcodePrintDoc.PrintController = new StandardPrintController();

            await Task.Run(() =>
            {
                barcodePrintDoc.Print();
            });
        }
        catch (System.ComponentModel.Win32Exception ex)
            when (ex.NativeErrorCode == 1223)
        {
            // User cancelled printing.
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error printing barcode: {ex}");
            throw;
        }
    }

   

    private PrintDocument SetupBarcodePrintDocument(
        BarcodeDesignDTO dto,
        List<BarcodeData> entBarcodeData,
        out List<BarcodeData> barcodeList)
    {
        var hdr = dto.hdr;
        var dtls = dto.dtls;

        if (hdr == null)
            throw new InvalidOperationException("Barcode design header is missing.");

        if (dtls == null || !dtls.Any())
            throw new InvalidOperationException("Barcode design details are missing.");

        PrintDocument barcodePrintDoc = new PrintDocument();
        barcodePrintDoc.OriginAtMargins = false;
        barcodePrintDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        barcodePrintDoc.DefaultPageSettings.Landscape = false;

        string printerName =
            !string.IsNullOrWhiteSpace(hdr.PrinterName)
                ? hdr.PrinterName
                : PermanentSettingsData.BarcodePrinterName;

        if (!string.IsNullOrWhiteSpace(printerName))
        {
            barcodePrintDoc.PrinterSettings.PrinterName = printerName;
        }

        bool hasCustomSize =
            hdr.Width.HasValue &&
            hdr.Height.HasValue &&
            hdr.Width.Value > 0 &&
            hdr.Height.Value > 0;

        if (hasCustomSize)
        {
            int paperWidth = (int)Math.Round(hdr.Width.Value * 100.0 / 96.0);
            int paperHeight = (int)Math.Round(hdr.Height.Value * 100.0 / 96.0);
            barcodePrintDoc.DefaultPageSettings.PaperSize = new PaperSize("BarcodeCustom", paperWidth, paperHeight);
        }
        else if (!string.IsNullOrWhiteSpace(hdr.PaperName) && barcodePrintDoc.PrinterSettings.IsValid)
        {
            foreach (PaperSize ps in barcodePrintDoc.PrinterSettings.PaperSizes)
            {
                if (string.Equals(ps.PaperName, hdr.PaperName, StringComparison.OrdinalIgnoreCase))
                {
                    barcodePrintDoc.DefaultPageSettings.PaperSize = ps;
                    break;
                }
            }
        }

        barcodeList = new List<BarcodeData>();
        foreach (var barcodeData in entBarcodeData)
        {
            if (barcodeData == null) continue;
            int quantity = barcodeData.Quantity > 0 ? barcodeData.Quantity : 1;
            for (int i = 0; i < quantity; i++)
            {
                barcodeList.Add(barcodeData);
            }
        }

        var itemsToPrint = barcodeList;
        int currentBarcodeIndex = 0;

        barcodePrintDoc.BeginPrint += (sender, e) =>
        {
            currentBarcodeIndex = 0;
        };

        barcodePrintDoc.PrintPage += (sender, e) =>
        {
            Graphics? graphics = e.Graphics;
            if (graphics == null)
            {
                e.HasMorePages = false;
                return;
            }

            try
            {
                if (currentBarcodeIndex >= itemsToPrint.Count)
                {
                    e.HasMorePages = false;
                    return;
                }

                BarcodeData barcodeData = itemsToPrint[currentBarcodeIndex];

                const float designDpi = 96f;
                const float printerDpi = 100f;
                float scale = printerDpi / designDpi;

                GraphicsState state = graphics.Save();
                try
                {
                    graphics.ScaleTransform(scale, scale);
                    RenderBarcodeLabelToGraphics(
                        graphics,
                        hdr,
                        dtls,
                        barcodeData,
                        hasCustomSize ? (float)hdr.Width.Value : 0,
                        hasCustomSize ? (float)hdr.Height.Value : 0);
                }
                finally
                {
                    graphics.Restore(state);
                }

                currentBarcodeIndex++;
                e.HasMorePages = currentBarcodeIndex < itemsToPrint.Count;
            }
            catch (Exception ex)
            {
                e.HasMorePages = false;
                Console.WriteLine($"Error printing barcode page: {ex}");
            }
        };

        return barcodePrintDoc;
    }

    /// <summary>
    /// Renders a single barcode label to a Graphics context using the design configuration.
    /// </summary>
    public void RenderBarcodeLabelToGraphics(
        Graphics graphics,
        BarcodeDesignHdr? hdr,
        List<BarcodeDesignDtls>? dtls,
        BarcodeData barcodeData,
        float width,
        float height)
    {
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        if (hdr?.IsBorderIncluded == true && width > 0 && height > 0)
        {
            using Pen borderPen = new Pen(Color.Black, 1);
            graphics.DrawRectangle(borderPen, 0, 0, width - 1, height - 1);
        }

        if (dtls != null)
        {
            foreach (var item in dtls)
            {
                if (item == null)
                    continue;

                using Font font = BuildFontBarcode(item);
                using StringFormat sf = BuildFormatBarcode(item);

                if (!string.Equals(item.Name, "Barcode", StringComparison.OrdinalIgnoreCase))
                {
                    string? val = GetValueFromBarcodeEntity(barcodeData, item);
                    if (string.IsNullOrEmpty(val) && !string.IsNullOrEmpty(item.Text) && item.Name == "Label")
                    {
                        val = item.Text;
                    }

                    string text = $"{item.Prefix}{val}";
                    RectangleF rect = new RectangleF(item.Position_x, item.Position_y, item.Width, item.Height);
                    graphics.DrawString(text, font, Brushes.Black, rect, sf);
                }
                else
                {
                    string? sValue = GetValueFromBarcodeEntity(barcodeData, item);
                    if (!string.IsNullOrEmpty(sValue))
                    {
                        CreateQRCode(
                            graphics,
                            sValue,
                            item.Width,
                            item.Height,
                            item.Position_x,
                            item.Position_y,
                            isBarcode: hdr?.IsBarcode ?? true,
                            includeLabel: hdr?.GenerateLabel ?? true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates a rendered PNG byte array for a single barcode label using PrintHelper.
    /// </summary>
    public byte[] GenerateBarcodeLabelImage(BarcodeDesignDTO dto, BarcodeData barcodeData)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (barcodeData == null) throw new ArgumentNullException(nameof(barcodeData));

        var hdr = dto.hdr;
        var dtls = dto.dtls;

        int width = (hdr?.Width.HasValue == true && hdr.Width.Value > 0) ? hdr.Width.Value : 300;
        int height = (hdr?.Height.HasValue == true && hdr.Height.Value > 0) ? hdr.Height.Value : 156;

        using Bitmap bitmap = new Bitmap(width, height);
        bitmap.SetResolution(96f, 96f);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);

        RenderBarcodeLabelToGraphics(graphics, hdr, dtls, barcodeData, width, height);

        using MemoryStream ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }

    /// <summary>
    /// Generates a rendered PNG base64 data URL string for a single barcode label using PrintHelper.
    /// </summary>
    public string GenerateBarcodeLabelImageBase64(BarcodeDesignDTO dto, BarcodeData barcodeData)
    {
        byte[] bytes = GenerateBarcodeLabelImage(dto, barcodeData);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }

    /// <summary>
    /// Prints barcode labels using SyncfusionPrintHelper.BarcodeGenerteMethod and outputs to configured printer.
    /// </summary>
    public async Task SyncfusionPrintBarcode(BarcodeDesignDTO dto, List<BarcodeData> entBarcodeData, ModelService? dialog = null)
    {
        try
        {
            if (entBarcodeData == null || entBarcodeData.Count == 0) return;

            SyncfusionPrintHelper syncfusionHelper = new SyncfusionPrintHelper();
            byte[]? pdfBytes = await syncfusionHelper.BarcodeGenerteMethod(dto, entBarcodeData, dialog, null, false);
            if (pdfBytes != null && pdfBytes.Length > 0)
            {
                int totalLabels = entBarcodeData.Sum(x => x.Quantity > 0 ? x.Quantity : 1);
                await PrintBarcodePdf(pdfBytes, dto.hdr, totalLabels);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SyncfusionPrintBarcode: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Prints the generated barcode PDF byte array to the configured printer.
    /// If the PDF contains all labels on a single page, it automatically slices them into separate pages for 1-barcode-per-page printing.
    /// </summary>
    public async Task PrintBarcodePdf(byte[] barcodeBytes, BarcodeDesignHdr? hdr = null, int totalLabels = 1)
    {
        if (barcodeBytes == null || barcodeBytes.Length == 0) return;

        try
        {
            using var randomAccessStream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(barcodeBytes);
                await writer.StoreAsync();
                await writer.FlushAsync();
            }
            randomAccessStream.Seek(0);

            var pdfDoc = await Windows.Data.Pdf.PdfDocument.LoadFromStreamAsync(randomAccessStream);
            if (pdfDoc.PageCount == 0) return;

            List<Bitmap> pageImages = new();
            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                using var page = pdfDoc.GetPage(i);
                using var pageStream = new InMemoryRandomAccessStream();
                var renderOptions = new PdfPageRenderOptions();
                if (page.Size.Width > 0 && page.Size.Height > 0)
                {
                    // Render at 300 DPI for sharp, scan-ready barcodes (PDF points are 72 DPI)
                    renderOptions.DestinationWidth = (uint)Math.Round(page.Size.Width * 300f / 72f);
                    renderOptions.DestinationHeight = (uint)Math.Round(page.Size.Height * 300f / 72f);
                }
                await page.RenderToStreamAsync(pageStream, renderOptions);
                using var netStream = pageStream.AsStreamForRead();
                using var tempBmp = new Bitmap(netStream);
                pageImages.Add(new Bitmap(tempBmp));
            }

            if (pageImages.Count == 0) return;

            // If the PDF contains multiple labels combined in 1 page, slice it into individual label images
            if (totalLabels > 1 && pageImages.Count == 1)
            {
                var fullBmp = pageImages[0];
                List<Bitmap> slicedImages = new();
                for (int i = 0; i < totalLabels; i++)
                {
                    int y = (int)Math.Round(i * (double)fullBmp.Height / totalLabels);
                    int nextY = (int)Math.Round((i + 1) * (double)fullBmp.Height / totalLabels);
                    int h = nextY - y;
                    if (h > 0)
                    {
                        Bitmap slice = new Bitmap(fullBmp.Width, h);
                        using (Graphics g = Graphics.FromImage(slice))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.DrawImage(fullBmp, new Rectangle(0, 0, fullBmp.Width, h), new Rectangle(0, y, fullBmp.Width, h), GraphicsUnit.Pixel);
                        }
                        slicedImages.Add(slice);
                    }
                }
                fullBmp.Dispose();
                pageImages = slicedImages;
            }

            using PrintDocument barcodePrintDoc = new PrintDocument();
            barcodePrintDoc.PrintController = new StandardPrintController();
            barcodePrintDoc.OriginAtMargins = false;
            barcodePrintDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            // Configure target printer from BarcodeDesignHdr or permanent settings
            barcodePrintDoc.PrinterSettings.PrinterName = !string.IsNullOrWhiteSpace(hdr?.PrinterName)
                ? hdr.PrinterName
                : PermanentSettingsData.BarcodePrinterName;
            // Paper Size configuration (converting 72 DPI PDF points to 100 DPI hundredths of an inch)
            using var firstPage = pdfDoc.GetPage(0);
            int effectiveLabels = totalLabels > 0 ? totalLabels : 1;
            int paperWidthInHundredths = (int)Math.Round(firstPage.Size.Width * 100f / 72f);
            int paperHeightInHundredths = (int)Math.Round((firstPage.Size.Height / effectiveLabels) * 100f / 72f);

            if (!string.IsNullOrWhiteSpace(hdr?.PaperName))
            {
                try
                {
                    foreach (PaperSize ps in barcodePrintDoc.PrinterSettings.PaperSizes)
                    {
                        if (ps.PaperName.Equals(hdr.PaperName, StringComparison.OrdinalIgnoreCase))
                        {
                            barcodePrintDoc.DefaultPageSettings.PaperSize = ps;
                            break;
                        }
                    }
                }
                catch { }
            }
            else if (paperWidthInHundredths > 0 && paperHeightInHundredths > 0)
            {
                try
                {
                    barcodePrintDoc.DefaultPageSettings.PaperSize = new PaperSize(hdr?.DesignName ?? "Custom", paperWidthInHundredths, paperHeightInHundredths);
                }
                catch { }
            }

            int printPageIndex = 0;
            barcodePrintDoc.PrintPage += (s, e) =>
            {
                if (printPageIndex < pageImages.Count)
                {
                    var bmp = pageImages[printPageIndex];
                    if (e.Graphics != null)
                    {
                        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        e.Graphics.DrawImage(bmp,new Rectangle(0,0, Math.Min(bmp.Width, e.PageBounds.Width),Math.Min(bmp.Height, e.PageBounds.Height)));
                    }

                    printPageIndex++;
                    e.HasMorePages = printPageIndex < pageImages.Count;
                }
            };

            await Task.Run(() => barcodePrintDoc.Print());

            foreach (var img in pageImages)
            {
                img.Dispose();
            }
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // User cancelled print dialog
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error printing barcode PDF: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Directly prints the generated barcode PDF byte array with specified printer name.
    /// </summary>
    public async Task PrintBarcodePdf(byte[] barcodeBytes, string? printerName)
    {
        var hdr = string.IsNullOrWhiteSpace(printerName) ? null : new BarcodeDesignHdr { PrinterName = printerName };
        await PrintBarcodePdf(barcodeBytes, hdr);
    }

    /// <summary>
    /// Sends ESC/POS kick pulse to open cash drawer connected to invoice printer or default printer.
    /// </summary>
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

            // Standard ESC/POS pulse sequences:
            // ESC p 0 25 250 (Pin 2)
            // ESC p 1 25 250 (Pin 5)
            // DLE DC4 1 0 5 (Real-time kick)
            byte[] drawerCodes = new byte[]
            {
                27, 112, 0, 25, 250,
                27, 112, 1, 25, 250,
                16, 20, 1, 0, 5
            };

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

    #endregion

    #region Header

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

        iTopMargin = headers.Max(x => x.PositionY + x.Height) + 10;
    }

    #endregion

    #region Details Header

    private void PrintDetailsHeader(Graphics g)
    {
        var hdr = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Invoice_Details_Header.ToString())
            .OrderBy(x => x.PositionY);

        foreach (var item in hdr)
            DrawItem(g, item, null);

    }

    #endregion

    #region Details

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

        while (linesPrinted < entDetails.Count && iIndex < entDMaster.LinesPerPage)
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

        if (iIndex >= entDMaster.LinesPerPage)
        {
            iIndex = 0;
            isPrinted = false;
        }
    }

    #endregion

    #region Draw Item (heart of your designer)

    private void DrawItem(Graphics g, PrintDesignDtls item, object? data, bool IsFixed = true)
    {
        System.Drawing.Font font = BuildFont(item);
        StringFormat sf = BuildFormat(item);

        int x = item.PositionX;
        int y = iTopMargin;

        if (item.ControlType == "LabelControl")
        {
            g.DrawString(item.FieldText, font, Brushes.Black,
                new RectangleF(x, IsFixed ? item.PositionY : y, item.Width, item.Height), sf);
            return;
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
                //DrawQr(g, val?.ToString(), x, IsFixed ? item.PositionY : y, item.Width);
                DrawQrZxing(g, val?.ToString(), x, IsFixed ? item.PositionY : y, item.Width, item.Height);
                return;
            }
            else if (item.FieldText == "InvoiceBarcode")
            {
                DrawBarcode(g, val?.ToString(), x, IsFixed ? item.PositionY : y, item.Width, item.Height);
                return;
            }
            else
            {
                if (item.FieldText == "ItemName")
                {
                    var size = g.MeasureString(val?.ToString(), font, item.Width, sf);
                    int measuredH = Math.Max((int)Math.Ceiling(size.Height), 18);
                    g.DrawString(val?.ToString(), font, Brushes.Black,
                        new RectangleF(x, IsFixed ? item.PositionY : y, item.Width, measuredH), sf);

                    itemHeight = measuredH;
                }
                else
                {
                    g.DrawString(val?.ToString(), font, Brushes.Black,
       new RectangleF(x, IsFixed ? item.PositionY : y, item.Width, item.Height), sf);
                }

            }
        }
        else if (item.ControlType == "HorizontalDotedLine")
        {
            using Pen p = new Pen(Color.Black, 1) { DashPattern = new float[] { 5, 3 } };
            g.DrawLine(p, x,IsFixed ? item.PositionY:y, x + item.Width, IsFixed ? item.PositionY: y);
            if(!IsFixed)
            {
                iTopMargin = iTopMargin + item.Height + entDMaster.SpaceBtwnLines;
            }
        }
        else if (item.ControlType == "HorizontalLine")
        {
            using Pen p = new Pen(Color.Black, 1);
            g.DrawLine(p, x, item.PositionY, x + item.Width, item.PositionY);
            return;
        }

        // SOLID VERTICAL LINE
        else if (item.ControlType == "VerticalLine")
        {
            using Pen p = new Pen(Color.Black, 1);
            g.DrawLine(p, x, item.PositionY, x, item.PositionY + item.Height);
            return;
        }
    }

    #endregion

    #region Barcode + QR

    private void DrawQr(Graphics g, string? text, int x, int y, int size)
    {
        if (string.IsNullOrEmpty(text)) return;

        using QRCodeGenerator gen = new();
        using QRCodeData data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using QRCode qr = new(data);
        using Bitmap bmp = qr.GetGraphic(4);

        g.DrawImage(bmp, x, y, size, size);
    }
    private void DrawQrZxing(Graphics g, string? text, int x, int y, int width, int height)
    {
        try
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,

                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
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
        catch (Exception) { }
    }
    public static bool CheckQRCodeContent(string content)
    {
        return !string.IsNullOrEmpty(content) && Encoding.UTF8.GetByteCount(content) <= 2952;
    }
    public static void CreateQRCode(Graphics g,
    string text,
    int width,
    int height,
    int x,
    int y,
    bool isBarcode = false,
    bool includeLabel = true,
    bool applyMargin = true)
    {
        if (string.IsNullOrWhiteSpace(text) ||
            !CheckQRCodeContent(text))
        {
            return;
        }

        try
        {
            var writer = new BarcodeWriter
            {
                Format = isBarcode
                    ? BarcodeFormat.CODE_128
                    : BarcodeFormat.QR_CODE,

                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = applyMargin ? 1 : 0,
                    PureBarcode = !includeLabel
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
        catch (Exception) { }
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

        var writer = new BarcodeWriter<Bitmap>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = w,
                Height = h,
                Margin = 0
            }
        };

        using Bitmap bmp = writer.Write(text);
        g.DrawImage(bmp, x, y);

    }

    #endregion

    private void PrintFooterSection(Graphics g)
    {
        var headers = entDSettings
            .Where(x => x.Location == InvoiceLabelPosition.Footer_Section.ToString()
                     || x.Location == InvoiceLabelPosition.Footer_Bottom.ToString())
            .OrderBy(x => x.PositionY).ThenBy(x => x.PositionX);

        foreach (var item in headers)
        {
            object? src = entInvoice;

            DrawItem(g, item, src,false);
            if (item.FieldText.IsArabic() == true || item.Spec == "N")
            {
                iTopMargin = iTopMargin +item.Height + entDMaster.SpaceBtwnLines;
            }
        }
    }

    #region Footer

    private void DrawFooter(Graphics g)
    {
        using Font f = new Font("Arial", 8);
        g.DrawString($"Page {currentPage}/{totalPages}", f, Brushes.Black, 25, 1100);
    }

    #endregion

    #region Helpers

    private Font BuildFont(PrintDesignDtls item)
    {
        FontStyle style = FontStyle.Regular;
        if (item.FStyle == "Bold") style = FontStyle.Bold;
        if (item.FStyle == "Italic") style = FontStyle.Italic;
        if (item.FontSize == 0) { item.FontSize = 10; }
        return new Font(item.Font ?? "Arial", item.FontSize, style);
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
    private object? GetValueFromEntity(object data, PrintDesignDtls dSettings)
    {
        try
        {
            if (data == null || dSettings == null) { return null; }
            string strProperty = dSettings.FieldText;
            var item = data.GetType().GetProperties().FirstOrDefault(x => x.Name == strProperty);
            if (item == null) { return null; }
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
                    DateTime dt = item.GetValue(data, null).ToString().toDateTime();
                    string format = dSettings.Spec;
                    if (format == string.Empty)
                    {
                        return dt.ToString("dd-MM-yyyy");
                    }
                    else
                    {
                        return dt.ToString(format);
                    }
                case "Byte[]":
                    return (byte[])item.GetValue(data, null);
                default:
                    return item.GetValue(data, null);
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
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
    public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

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

