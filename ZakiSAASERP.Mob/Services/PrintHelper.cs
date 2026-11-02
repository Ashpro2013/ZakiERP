using System.Drawing;
using AshproStringExtension;
using QRCoder;
using ZXing;
using ZXing.Common;

using global::ZakiSAASERP.Models.DTO;
using global::ZakiSAASERP.Models.Enums;
using global::ZakiSAASERP.Models.Masters;
using global::ZakiSAASERP.Models.Others;
using global::ZakiSAASERP.Models.ViewModel;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace ZakiSAASERP.Mob.Services;

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

    #endregion

    #region Constructor
    public PrintHelper()
    {
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

        totalPages = (entDetails.Count / (entDMaster?.LinesPerPage > 0 ? entDMaster.LinesPerPage : 1)) + 1;

        if (OperatingSystem.IsWindows())
        {
            PrintWindowsInvoice();
        }
    }

    private void PrintWindowsInvoice()
    {
#if WINDOWS
        PrintController printController = new StandardPrintController();
        PrintDocument printDoc = new PrintDocument();
        printDoc.PrintController = printController;
        printDoc.PrintPage += PrintDoc_PrintPage;
        printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        printDoc.Print();
#endif
    }

    #endregion

    #region Core Print Engine

#if WINDOWS
    private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
    {
        Graphics g = e.Graphics;

        if (!isPrinted)
        {
            PrintHeader(g);
            PrintDetailsHeader(g);
            isPrinted = true;
        }
        iTopMargin = entDSettings.FirstOrDefault(x => x.Location == InvoiceLabelPosition.Details_Area.ToString())?.PositionY ?? 0;
        PrintDetails(g);

        DrawFooter(g);

        if (linesPrinted < entDetails.Count)
        {
            currentPage++;
            e.HasMorePages = true;
        }
        else
        {
            PrintFooterSection(g);
            e.HasMorePages = false;
        }
    }
#endif

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

        iTopMargin = (headers.Any() ? headers.Max(x => x.PositionY + x.Height) : 0) + 10;
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
            .OrderBy(x => x.PositionY);

        while (linesPrinted < entDetails.Count && iIndex < entDMaster.LinesPerPage)
        {
            foreach (var item in ds)
            {
                DrawItem(g, item, entDetails[linesPrinted], true);
            }

            iTopMargin += (entDMaster.SpaceBtwnLines) + 15;
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

    #region Draw Item

    private void DrawItem(Graphics g, PrintDesignDtls item, object? data, bool IsDetails = false)
    {
        System.Drawing.Font font = BuildFont(item);
        StringFormat sf = BuildFormat(item);

        int x = item.PositionX;
        int y = iTopMargin;

        if (item.ControlType == "LabelControl")
        {
            g.DrawString(item.FieldText, font, Brushes.Black,
                new RectangleF(x, item.PositionY, item.Width, item.Height), sf);
            return;
        }
        else if (item.ControlType == "ValueControl")
        {
            object? val = GetValueFromEntity(data, item);

            if (item.FieldText == "InvoiceQRCode")
            {
                DrawQr(g, val?.ToString(), x, item.PositionY, item.Width);
                return;
            }
            else if (item.FieldText == "InvoiceBarcode")
            {
                DrawBarcode(g, val?.ToString(), x, y, item.Width, item.Height);
                return;
            }
            else
            {
                g.DrawString(val?.ToString(), font, Brushes.Black,
                       new RectangleF(x, IsDetails ? y : item.PositionY, item.Width, item.Height), sf);
            }
        }
        else if (item.ControlType == "HorizontalDotedLine")
        {
            using Pen p = new Pen(Color.Black, 1) { DashPattern = new float[] { 5, 3 } };
            g.DrawLine(p, x, y, x + item.Width, y);
        }
        else if (item.ControlType == "HorizontalLine")
        {
            using Pen p = new Pen(Color.Black, 1);
            g.DrawLine(p, x, item.PositionY, x + item.Width, item.PositionY);
            return;
        }

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
            .OrderBy(x => x.PositionY);

        foreach (var item in headers)
        {
            object? src = entInvoice;

            DrawItem(g, item, src);
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
    private object? GetValueFromEntity(object? data, PrintDesignDtls dSettings)
    {
        try
        {
            if (data == null || dSettings == null) { return null; }
            string strProperty = dSettings.FieldText;
            var item = data.GetType().GetProperties().FirstOrDefault(x => x.Name == strProperty);
            if (item == null) { return null; }
            switch (item.PropertyType.Name)
            {
                case "Decimal":
                    if (item.Name != "Quantity")
                    {
                        return item.GetValue(data, null).toDecimal().ToString("N2");
                    }
                    else
                    {
                        return item.GetValue(data, null).toDecimal().ToString("G29");
                    }
                case "DateTime":
                    DateTime dt = item.GetValue(data, null)?.ToString()?.toDateTime() ?? DateTime.MinValue;
                    string format = dSettings.Spec;
                    if (string.IsNullOrEmpty(format))
                    {
                        return dt.ToString("dd-MM-yyyy");
                    }
                    else
                    {
                        return dt.ToString(format);
                    }
                case "Byte[]":
                    return (byte[]?)item.GetValue(data, null);
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
