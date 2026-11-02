using AshproSAASERP.Models.DTO;
using AshproSAASERP.Models.Enums;
using AshproSAASERP.Models.Masters;
using AshproSAASERP.Models.Others;
using AshproSAASERP.Models.ViewModel;
using AshproStringExtension;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System.Data;
using Color = Syncfusion.Drawing.Color;

namespace AshproSAASERP.Web.Services;

public class SyncfusionPrintHelper
{
    #region Varibales
    List<PrintDesignDtls> entDSettings = new();
    List<FooterDetails> entAdjustments = new();
    PrintDesignHdr entDMaster = new();
    CompanyDetails entCompany = new();
    TransactionHdr entInvoice = new();
    TransactionHdr tMaster = new();
    TransactionHdr entAdjustment = new();
    List<BarcodeData> entBarcodeData = new();
    BarcodeDesignHdr barcodeDesignHdr = new();
    List<BarcodeDesignDtls> barcodeDesignDtls = new();
    List<TransactionDtls> entDetails = new();
    Party? entParty;
    int iTopMargin, linesPrinted, iIndex;
    bool isPrinted;
    bool isLast = true;
    int Ypos;
    PdfFont? font;
    PdfStringFormat? stringFormat;
    PdfPen? dottedPen;
    Dictionary<string, object>? ReportHdr;
    Dictionary<string, object>? ReportDtls;
    int ItemHeigt = 0;
    #endregion

    #region Method
    void DrawFooter(PdfPage currentPage, PdfGraphics Graphics, int page, int total)
    {
        var font = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
        float y = currentPage.GetClientSize().Height - 20;
        Graphics.DrawString($"Page {page}/{total}", font,
            PdfBrushes.Black, new RectangleF(25, y, 200, 20));
    }
    private void ItemFontAndStringFormatFindMethod(PrintDesignDtls item, bool isBold = false)
    {
        try
        {
            var fontfamily = new PdfFontFamily();
            switch (item.Font)
            {
                case "Helvetica":
                    fontfamily = PdfFontFamily.Helvetica; break;
                case "ZapfDingbats":
                    fontfamily = PdfFontFamily.ZapfDingbats; break;
                case "Courier":
                    fontfamily = PdfFontFamily.Courier; break;
                case "Symbol":
                    fontfamily = PdfFontFamily.Symbol; break;
                default:
                    fontfamily = PdfFontFamily.TimesRoman; break;
            }
            PdfFontStyle pdfFontStyle = item.FStyle switch
            {
                "Bold" => PdfFontStyle.Bold,
                "Italic" => PdfFontStyle.Italic,
                "Underline" => PdfFontStyle.Underline,
                "Strikeout" => PdfFontStyle.Strikeout,
                _ => PdfFontStyle.Regular
            };
            if (isBold)
            {
                pdfFontStyle = PdfFontStyle.Bold;
            }
            font = new PdfStandardFont(fontfamily, (int)item.FontSize, pdfFontStyle);

            stringFormat = new PdfStringFormat();
            stringFormat.Alignment = (item.Alignment ?? "") switch
            {
                "Right" => PdfTextAlignment.Right,
                "Center" => PdfTextAlignment.Center,
                _ => PdfTextAlignment.Left
            };
            stringFormat.WordWrap = PdfWordWrapType.Word;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private void BarcodeItemFntNStringFormatFindMethod(BarcodeDesignDtls item)
    {
        try
        {
            PdfFontFamily fontfamily = item.fontFamily switch
            {
                "Helvetica" => PdfFontFamily.Helvetica,
                "ZapfDingbats" => PdfFontFamily.ZapfDingbats,
                "Courier" => PdfFontFamily.Courier,
                "Symbol" => PdfFontFamily.Symbol,
                _ => PdfFontFamily.TimesRoman
            };
            PdfFontStyle pdfFontStyle = item.fStyle switch
            {
                "Bold" => PdfFontStyle.Bold,
                "Italic" => PdfFontStyle.Italic,
                "Underline" => PdfFontStyle.Underline,
                "Strikeout" => PdfFontStyle.Strikeout,
                _ => PdfFontStyle.Regular
            };
            font = new PdfStandardFont(fontfamily, (int)item.fontSize, pdfFontStyle);
            stringFormat = new PdfStringFormat();
            stringFormat.Alignment = (item.Alignment ?? "") switch
            {
                "Right" => PdfTextAlignment.Right,
                "Center" => PdfTextAlignment.Center,
                _ => PdfTextAlignment.Left
            };
        }
        catch (Exception)
        {
            throw;
        }
    }
    private object? GetValueFromBarcodeEntity(BarcodeData data, BarcodeDesignDtls dSettings)
    {
        try
        {
            if (data == null) { return null; }
            string strProperty = dSettings.Name!;
            object? obj = null;
            var item = data.GetType().GetProperties().FirstOrDefault(x => x.Name == strProperty);
            if (item == null) { return null; }
            switch (item.PropertyType.Name)
            {
                case "Decimal":
                    if (item.Name != "Quantity")
                    {
                        obj = item.GetValue(data, null).toDecimal().ToString("N2");
                    }
                    else
                    {
                        obj = item.GetValue(data, null).toDecimal().ToString("G29");
                    }
                    if (obj == null) { obj = 0; }
                    break;
                case "Byte[]":
                    byte[] logo = (byte[])item.GetValue(data, null)!;
                    obj = logo;
                    break;
                default:
                    obj = item.GetValue(data, null);
                    if (obj == null) { obj = string.Empty; }
                    break;
            }
            return obj;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            return null;
        }
    }
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

    private object? getValueFromEntity(object? data, PrintDesignDtls dSettings)
    {
        try
        {
            if (data == null) { return null; }
            string strProperty = dSettings.FieldText;
            object? obj = null;
            var item = data.GetType().GetProperties().FirstOrDefault(x => x.Name == strProperty);
            if (item == null) { return null; }
            switch (item.PropertyType.Name)
            {
                case "Decimal":
                    if (item.Name != "Quantity")
                    {
                        obj = item.GetValue(data, null).toDecimal().ToString("N2");
                    }
                    else
                    {
                        obj = item.GetValue(data, null).toDecimal().ToString("G29");
                    }
                    if (obj == null) { obj = 0; }
                    break;
                case "DateTime":
                    DateTime dt = item.GetValue(data, null)!.ToString().toDateTime();
                    string format = dSettings.Spec!;
                    if (format == string.Empty)
                    {
                        obj = GetNormalDate(dt);
                        obj = obj.ToString()!.Replace("T", " ");
                    }
                    else
                    {
                        obj = dt.ToString(format);
                    }
                    break;
                case "Byte[]":
                    byte[] logo = (byte[])item.GetValue(data, null)!;
                    obj = logo;
                    break;
                default:
                    obj = item.GetValue(data, null);
                    if (obj == null) { obj = string.Empty; }
                    break;
            }
            return obj;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
            return null;
        }
    }
    private object? GetValueFromReportEntity(PrintDesignDtls dSettings, object obj)
    {
        try
        {
            if (obj == null)
                return "";

            string propertyName = dSettings.FieldText;

            var property = obj.GetType().GetProperty(propertyName);
            if (property == null)
                return null;

            var value = property.GetValue(obj);

            if (value == null)
                return null;

            switch (value)
            {
                case decimal dec:
                    return dec.ToString("N2");

                case DateTime dt:
                    return string.IsNullOrEmpty(dSettings.Spec)
                        ? GetNormalDate(dt)?.ToString()?.Replace("T", " ")
                        : dt.ToString(dSettings.Spec);

                case byte[] bytes:
                    return bytes; // logo or image

                default:
                    return value;
            }
        }
        catch (Exception)
        {

            return "";
        }
    }
    private PdfImage? getImageValueFromEntity(object? data, string strProperty)
    {
        PdfImage? img = null;
        foreach (var item in data!.GetType().GetProperties())
        {
            if (item.Name == strProperty && item.GetValue(data, null) != null)
            {
                byte[] logo = (byte[])item.GetValue(data, null)!;
                MemoryStream stream = new MemoryStream(logo);
                img = new PdfBitmap(stream);
                break;
            }
        }
        return img;
    }
    #endregion

    #region Core Method
    private void NormalWritingMethod(PrintDesignDtls item, object? obj, PdfGraphics Graphics, PdfPage page)
    {
        try
        {
            if (item.ControlType == "ValueControle" || item.ControlType == "ValueControl")
            {
                ItemFontAndStringFormatFindMethod(item);
                if (item.Location != InvoiceLabelPosition.Details_Area.ToString() && item.Location != InvoiceLabelPosition.Footer_Section.ToString() && item.Location != InvoiceLabelPosition.Footer_Details.ToString() && item.Location != InvoiceLabelPosition.Footer_Bottom.ToString())
                {
                    if (item.FieldText == "Logo")
                    {
                        PdfImage? img = getImageValueFromEntity(obj, item.FieldText);
                        if (img != null)
                        {
                            Graphics.DrawImage(img, new RectangleF(item.PositionX, Ypos, item.Width, item.Height));
                            Ypos = Ypos + item.Height;
                        }
                    }
                    else if (item.FieldText == "InvoiceQRCode")
                    {
                        int size = Math.Min(item.Width, item.Height);
                        string? strVal = getValueFromEntity(obj, item)!.ToString();
                        if (!string.IsNullOrEmpty(strVal))
                        {
                            PdfQRBarcode barcode = new PdfQRBarcode();
                            barcode.ErrorCorrectionLevel = PdfErrorCorrectionLevel.High;
                            barcode.XDimension = 3;
                            barcode.Text = strVal;
                            barcode.Draw(page, new PointF(item.PositionX, Ypos), new SizeF(size, size));
                            Ypos = Ypos + item.Height;
                        }
                    }
                    else if (item.FieldText == "Location")
                    {
                        int size = Math.Min(item.Width, item.Height);
                        string? strVal = getValueFromEntity(obj, item)!.ToString();
                        if (!string.IsNullOrEmpty(strVal))
                        {
                            PdfQRBarcode barcode = new PdfQRBarcode();
                            barcode.ErrorCorrectionLevel = PdfErrorCorrectionLevel.High;
                            barcode.XDimension = 3;
                            barcode.Text = strVal;
                            barcode.Draw(page, new PointF(item.PositionX, Ypos), new SizeF(size, size));
                            Ypos = Ypos + item.Height;
                        }
                    }
                    else if (item.FieldText == "InvoiceBarcode")
                    {
                        string? sValue = getValueFromEntity(obj, item)!.ToString();
                        if (!string.IsNullOrEmpty(sValue))
                        {
                            PdfCode128Barcode barcode = new PdfCode128Barcode();
                            barcode.BarHeight = item.Height;
                            barcode.Text = sValue;
                            barcode.Draw(page, new PointF(item.PositionX, Ypos));
                            Ypos = Ypos + item.Height;
                        }
                    }
                    else
                    {
                        object? sValue = getValueFromEntity(obj, item);
                        if (sValue != null)
                        {
                            Graphics.DrawString(sValue.ToString(), font, new PdfSolidBrush(Color.Black), new RectangleF(item.PositionX, Ypos, item.Width, item.Height), stringFormat);
                            PrintDesignDtls? dSettings = entDSettings!.FirstOrDefault(x => x.PositionY == item.PositionY && x.Spec == "M");
                            if (dSettings == null)
                            {
                                Ypos = Ypos + item.Height;
                            }
                        }
                    }
                }
                else
                {
                    if (item.FieldText == "InvoiceBarcode")
                    {
                        string? sValue = getValueFromEntity(obj, item)!.ToString();
                        if (!string.IsNullOrEmpty(sValue))
                        {
                            PdfCode128Barcode barcode = new PdfCode128Barcode();
                            barcode.BarHeight = item.Height;
                            barcode.Text = sValue;
                            barcode.Draw(page, new PointF(item.PositionX, iTopMargin));
                            iTopMargin = iTopMargin + (int)entDMaster!.SpaceBtwnLines + item.Height;
                        }
                    }
                    else if (item.FieldText == "InvoiceQRCode")
                    {
                        int size = Math.Min(item.Width, item.Height);
                        string? strVal = getValueFromEntity(obj, item)!.ToString();

                        if (!string.IsNullOrEmpty(strVal))
                        {
                            PdfQRBarcode barcode = new PdfQRBarcode();
                            barcode.ErrorCorrectionLevel = PdfErrorCorrectionLevel.High;
                            barcode.XDimension = 3;
                            barcode.Text = strVal;
                            barcode.Draw(page, new PointF(item.PositionX, iTopMargin), new SizeF(size, size));
                            iTopMargin = iTopMargin + (int)entDMaster!.SpaceBtwnLines + item.Height;
                        }
                    }
                    else
                    {
                        if (item.FieldText == "ItemQRCode")
                        {
                            int size = Math.Min(item.Width + 40, item.Height + 40);
                            string? strVal = getValueFromEntity(obj, item)!.ToString();
                            if (!string.IsNullOrEmpty(strVal))
                            {
                                PdfQRBarcode barcode = new PdfQRBarcode();
                                barcode.ErrorCorrectionLevel = PdfErrorCorrectionLevel.High;
                                barcode.XDimension = 3;
                                barcode.Text = strVal;
                                barcode.Draw(page, new PointF(item.PositionX, iTopMargin), new SizeF(size, size));
                            }
                        }
                        else
                        {
                            Graphics.DrawString(getValueFromEntity(obj, item)!.ToString(), font, new PdfSolidBrush(Color.Black), new RectangleF(item.PositionX, iTopMargin, item.Width, item.Height), stringFormat);
                        }
                    }
                    if (item.Location == InvoiceLabelPosition.Footer_Section.ToString() || item.Location == InvoiceLabelPosition.Footer_Bottom.ToString())
                    {
                        iTopMargin = iTopMargin + ((int)entDMaster!.SpaceBtwnLines);
                    }
                }
            }
            else if (item.ControlType == "LabelControl")
            {
                ItemFontAndStringFormatFindMethod(item);
                if (item.Location != InvoiceLabelPosition.Details_Area.ToString() && item.Location != InvoiceLabelPosition.Footer_Section.ToString() && item.Location != InvoiceLabelPosition.Footer_Details.ToString() && item.Location != InvoiceLabelPosition.Footer_Bottom.ToString())
                {
                    Graphics.DrawString(item.FieldText, font, new PdfSolidBrush(Color.Black), new RectangleF(item.PositionX, Ypos, item.Width, item.Height), stringFormat);
                    if (!string.IsNullOrEmpty(item.Spec) && item.Spec.ToUpper() == "M")
                    {
                        Ypos = Ypos + item.Height;
                    }
                }
                else
                {
                    if (item.Location == InvoiceLabelPosition.Footer_Section.ToString() || item.Location == InvoiceLabelPosition.Footer_Bottom.ToString())
                    {
                        if (!string.IsNullOrEmpty(item.Spec) && item.Spec == "ar")
                        {
                            iTopMargin = iTopMargin - ((int)entDMaster!.SpaceBtwnLines);
                        }
                    }
                    Graphics.DrawString(item.FieldText, font, new PdfSolidBrush(Color.Black), new RectangleF(item.PositionX, iTopMargin, item.Width, item.Height), stringFormat);
                }
            }
            else if (item.ControlType == "HorizontalDotedLine")
            {
                ItemFontAndStringFormatFindMethod(item);
                if (item.Location != InvoiceLabelPosition.Details_Area.ToString() && item.Location != InvoiceLabelPosition.Footer_Section.ToString() && item.Location != InvoiceLabelPosition.Footer_Details.ToString() && item.Location != InvoiceLabelPosition.Footer_Bottom.ToString())
                {
                    Graphics.DrawLine(dottedPen, new Point(item.PositionX, Ypos), new Point(item.PositionX + item.Width, Ypos));
                    Ypos = Ypos + item.Height;
                }
                else
                {
                    if (item.Location == InvoiceLabelPosition.Footer_Section.ToString())
                    {
                        iTopMargin = iTopMargin + entDMaster!.SpaceBtwnLines;
                        Graphics.DrawLine(dottedPen, new Point(item.PositionX, iTopMargin), new Point(item.PositionX + item.Width, iTopMargin));
                    }
                    else
                    {
                        Graphics.DrawLine(dottedPen, new Point(item.PositionX, iTopMargin), new Point(item.PositionX + item.Width, iTopMargin));
                    }
                    iTopMargin = iTopMargin + entDMaster!.SpaceBtwnLines;
                }
            }
            else
            {
                ItemFontAndStringFormatFindMethod(item);
                if (item.Location != InvoiceLabelPosition.Details_Area.ToString() && item.Location != InvoiceLabelPosition.Footer_Section.ToString() && item.Location != InvoiceLabelPosition.Footer_Details.ToString() && item.Location != InvoiceLabelPosition.Footer_Bottom.ToString())
                {
                    Graphics.DrawLine(new PdfPen(Color.Black, item.FontSize), new Point(item.PositionX, Ypos), new Point(item.PositionX + item.Width, Ypos));
                    Ypos = Ypos + item.Height;
                }
                else
                {
                    if (item.Location == InvoiceLabelPosition.Footer_Section.ToString())
                    {
                        iTopMargin = iTopMargin + entDMaster!.SpaceBtwnLines;
                        Graphics.DrawLine(new PdfPen(Color.Black, item.FontSize), new Point(item.PositionX, iTopMargin), new Point(item.PositionX + item.Width, iTopMargin));
                    }
                    else
                    {
                        Graphics.DrawLine(new PdfPen(Color.Black, item.FontSize), new Point(item.PositionX, iTopMargin), new Point(item.PositionX + item.Width, iTopMargin));
                    }
                    iTopMargin = iTopMargin + entDMaster!.SpaceBtwnLines;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    private void WritingMethod(PrintDesignDtls item, object? obj, PdfGraphics Graphics, PdfPage page, byte[]? qrcode = null, float? customHeight = null)
    {
        try
        {
            PdfUnitConverter converter = new PdfUnitConverter();
            float px(float v) => converter.ConvertFromPixels(v, PdfGraphicsUnit.Point);
            if (item.ControlType == "ValueControl" || item.ControlType == "ValueControle")
            {
                ItemFontAndStringFormatFindMethod(item);
                if (item.Location != InvoiceLabelPosition.Details_Area.ToString() && item.Location != InvoiceLabelPosition.Footer_Section.ToString() && item.Location != InvoiceLabelPosition.Footer_Details.ToString() && item.Location != InvoiceLabelPosition.Footer_Bottom.ToString())
                {
                    if (item.FieldText == "Image" || item.FieldText == "HeaderImage" || item.FieldText == "FooterImage")
                    {
                        PdfImage? img = getImageValueFromEntity(obj, item.FieldText);
                        if (img != null)
                        {
                            Graphics.DrawImage(img, new RectangleF(px(item.PositionX), px(item.PositionY), px(item.Width), px(item.Height)));
                        }
                    }
                    else if (item.FieldText == "Barcode")
                    {
                        string? sValue = getValueFromEntity(obj, item)!.ToString();
                        if (!string.IsNullOrEmpty(sValue))
                        {
                            PdfCode128Barcode barcode = new PdfCode128Barcode();
                            barcode.BarHeight = px(item.Height);
                            barcode.Text = sValue;
                            barcode.Draw(page, new PointF(px(item.PositionX), px(item.PositionY)));
                        }
                    }
                    else if (item.FieldText == "InvoiceQRCode")
                    {
                        int size = Math.Min((int)px(item.Width), (int)px(item.Height));
                        string? strVal = getValueFromEntity(obj, item)!.ToString();
                        if (!string.IsNullOrEmpty(strVal))
                        {
                            PdfQRBarcode barcode = new PdfQRBarcode();
                            barcode.ErrorCorrectionLevel = PdfErrorCorrectionLevel.High;
                            barcode.XDimension = 3;
                            barcode.Text = strVal;
                            barcode.Draw(page, new PointF(px(item.PositionX), px(item.PositionY)), new SizeF(size, size));
                        }
                    }
                    else
                    {
                        object? sValue = getValueFromEntity(obj, item);
                        if (sValue != null)
                        {
                            Graphics.DrawString(sValue.ToString(), font, new PdfSolidBrush(Color.Black), new RectangleF(px(item.PositionX), px(item.PositionY), px(item.Width), px(item.Height)), stringFormat);
                        }
                    }
                }
                else
                {
                    if (item.Location == InvoiceLabelPosition.Footer_Section.ToString() || item.Location == InvoiceLabelPosition.Footer_Bottom.ToString())
                    {
                        if (item.FieldText == "Barcode")
                        {
                            string? sValue = getValueFromEntity(obj, item)!.ToString();
                            if (!string.IsNullOrEmpty(sValue))
                            {
                                PdfCode128Barcode barcode = new PdfCode128Barcode();
                                barcode.BarHeight = px(item.Height);
                                barcode.Text = sValue;
                                barcode.Draw(page, new PointF(px(item.PositionX), px(item.PositionY)));
                            }
                        }
                        else if (item.FieldText == "InvoiceQRCode")
                        {
                            int size = Math.Min((int)px(item.Width), (int)px(item.Height));
                            string? strVal = getValueFromEntity(obj, item)!.ToString();
                            if (!string.IsNullOrEmpty(strVal))
                            {
                                PdfQRBarcode barcode = new PdfQRBarcode();
                                barcode.ErrorCorrectionLevel = PdfErrorCorrectionLevel.High;
                                barcode.XDimension = 3;
                                barcode.Text = strVal;
                                barcode.Draw(page, new PointF(px(item.PositionX), px(item.PositionY)), new SizeF(size, size));
                            }
                        }
                        else
                        {
                            Graphics.DrawString(getValueFromEntity(obj, item)!.ToString(), font, new PdfSolidBrush(Color.Black), new RectangleF(px(item.PositionX), px(item.PositionY), px(item.Width), px(item.Height)), stringFormat);
                        }
                    }
                    else
                    {
                        if (item.FieldText == "Barcode")
                        {
                            string? sValue = getValueFromEntity(obj, item)!.ToString();
                            if (!string.IsNullOrEmpty(sValue))
                            {
                                PdfCode128Barcode barcode = new PdfCode128Barcode();
                                barcode.BarHeight = px(item.Height);
                                barcode.Text = sValue;
                                barcode.Draw(page, new PointF(px(item.PositionX), px(iTopMargin)));
                                iTopMargin = iTopMargin + (int)entDMaster!.SpaceBtwnLines + item.Height;
                            }
                        }
                        else
                        {
                            if (item.FieldText == "QRCode")
                            {
                                int size = Math.Min((int)px(item.Width) + 40, (int)px(item.Height) + 40);
                                string? strVal = getValueFromEntity(obj, item)!.ToString();
                                if (!string.IsNullOrEmpty(strVal))
                                {
                                    PdfQRBarcode barcode = new PdfQRBarcode();
                                    barcode.ErrorCorrectionLevel = PdfErrorCorrectionLevel.High;
                                    barcode.XDimension = 3;
                                    barcode.Text = strVal;
                                    barcode.Draw(page, new PointF(px(item.PositionX), px(iTopMargin)), new SizeF(size, size));
                                }
                            }
                            else
                            {
                                object? rawVal = getValueFromEntity(obj, item);
                                string? sText = rawVal?.ToString();
                                if (!string.IsNullOrEmpty(sText))
                                {
                                    float cellH = (customHeight.HasValue && customHeight.Value > item.Height) ? customHeight.Value : item.Height;
                                    Graphics.DrawString(sText, font, new PdfSolidBrush(Color.Black), new RectangleF(px(item.PositionX), px(iTopMargin), px(item.Width), px(cellH)), stringFormat);
                                }
                            }
                            if (item.Location == InvoiceLabelPosition.Footer_Section.ToString() || item.Location == InvoiceLabelPosition.Footer_Bottom.ToString())
                            {
                                iTopMargin = iTopMargin + ((int)entDMaster!.SpaceBtwnLines);
                            }
                        }
                    }
                }
            }
            else if (item.ControlType == "LabelControl")
            {
                ItemFontAndStringFormatFindMethod(item);
                Graphics.DrawString(item.FieldText, font, new PdfSolidBrush(Color.Black), new RectangleF(px(item.PositionX), px(item.PositionY), px(item.Width), px(item.Height)), stringFormat);
            }
            else if (item.ControlType == "HorizontalDotedLine")
            {
                ItemFontAndStringFormatFindMethod(item);
                if (item.Location != InvoiceLabelPosition.Details_Area.ToString() && item.Location != InvoiceLabelPosition.Footer_Section.ToString() && item.Location != InvoiceLabelPosition.Footer_Details.ToString() && item.Location != InvoiceLabelPosition.Footer_Bottom.ToString())
                {
                    Graphics.DrawLine(dottedPen, new Point(item.PositionX, Ypos), new Point(item.PositionX + item.Width, Ypos));
                    Ypos = Ypos + item.Height;
                }
                else
                {
                    if (item.Location == InvoiceLabelPosition.Footer_Section.ToString())
                    {
                        iTopMargin = iTopMargin + entDMaster!.SpaceBtwnLines;
                        Graphics.DrawLine(dottedPen, new Point(item.PositionX, iTopMargin), new Point(item.PositionX + item.Width, iTopMargin));
                    }
                    else
                    {
                        Graphics.DrawLine(dottedPen, new Point(item.PositionX, iTopMargin), new Point(item.PositionX + item.Width, iTopMargin));
                    }
                    iTopMargin = iTopMargin + entDMaster!.SpaceBtwnLines;
                }
            }
            else if (item.ControlType == "HorizontalLine")
            {
                Graphics.DrawLine(new PdfPen(Color.Black, 1), new Point((int)px(item.PositionX), (int)px(item.PositionY)), new Point((int)px(item.PositionX) + (int)px(item.Width), (int)px(item.PositionY)));
            }
            else
            {
                Graphics.DrawLine(new PdfPen(Color.Black, 1), new Point((int)px(item.PositionX), (int)px(item.PositionY)), new Point((int)px(item.PositionX), (int)px(item.PositionY) + (int)px(item.Height)));
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    #endregion

    #region Evets
    public async Task<MemoryStream?> PrintReportMethod(PrintDesignHdr _entDMaster, List<PrintDesignDtls> _entDSettings, Dictionary<string, object> _ReportHdr, Dictionary<string, object> _ReportDtls, string? sFileName = null, bool isPreview = false)
    {
        try
        {
            entDMaster = _entDMaster;
            entDSettings = _entDSettings;
            entDSettings.ForEach(x =>
            {
                x.Location = x.Location.Replace(' ', '_');
            });
            ReportHdr = _ReportHdr;
            ReportDtls = _ReportDtls;
            return ReportPrintMethod();
        }
        catch (Exception) { throw; }
    }
    public async Task<MemoryStream?> PrintReportMethodNew(PrintDesignDTO design, Dictionary<string, object> _ReportHdr, Dictionary<string, object> _ReportDtls, bool LastRowBold = true)
    {
        try
        {
            entDMaster = design.hdr;
            entDSettings = design.dtls;
            entDSettings.ForEach(x =>
            {
                x.Location = x.Location.Replace(' ', '_');
            });
            ReportHdr = _ReportHdr;
            ReportDtls = _ReportDtls;
            return ReportPrintFastMethod(LastRowBold);
        }
        catch (Exception) { throw; }
    }
    public async Task<MemoryStream?> PrintMethod(PrintParameterDTO dto, int iCopyToPrint = 1, string? sFileName = null, bool isPreview = false)
    {
        try
        {
            linesPrinted = 0;
            isPrinted = false;
            iTopMargin = 0;
            iIndex = 0;
            Ypos = 0;
            isLast = true;

            entDMaster = dto.hdr;
            entDSettings = dto.dtls;
            entDSettings.ForEach(x =>
            {
                x.Location = x.Location.Replace(' ', '_');
            });
            entCompany = dto.Company;
            entInvoice = dto.tranHdr;
            entParty = dto.party;
            entDetails = dto.tranDtls;
            entAdjustment = dto.tranHdr;
            entAdjustments = dto.fDtls;
            tMaster = dto.tranHdr;
            int isl = 1;
            entDetails.ForEach(x => { x.SL = isl; isl++; });
            dottedPen = new PdfPen(Color.Black, 1);
            float[] dashValues = { 5.0f, 3.0f };
            dottedPen.DashStyle = PdfDashStyle.Custom;
            dottedPen.DashPattern = dashValues;
            if (entDMaster.PrintType == "A4")
            {
                return InvoiceMethod();

            }
            else
            {
                return NormalPrintingMethod();
            }
        }
        catch (Exception) { throw; }
    }
    public async Task<MemoryStream?> BarcodeGenerteMethod(BarcodeDesignDTO dto, List<BarcodeData> data)
    {
        entBarcodeData = data;
        barcodeDesignHdr = dto.hdr;
        barcodeDesignDtls = dto.dtls;
        return BarcodeMethod();
    }
    public MemoryStream BarcodeMethod()
    {
        PdfUnitConverter converter = new PdfUnitConverter();
        float px(float v) => converter.ConvertFromPixels(v, PdfGraphicsUnit.Point);
        PdfDocument document = new PdfDocument();
        document.PageSettings.Margins.All = 0;
        PdfSection section = document.Sections.Add();
        section.PageSettings.Rotate = PdfPageRotateAngle.RotateAngle0;
        var iHeight = entBarcodeData!.Sum(x => x.Quantity) * (barcodeDesignHdr!.Height ?? 100);
        section.PageSettings.Size = new SizeF(px(barcodeDesignHdr.Width ?? 300), px(iHeight));
        PdfPage currentPage = section.Pages.Add();
        SizeF clientSize = currentPage.GetClientSize();
        PdfGraphics Graphics = currentPage.Graphics;

        try
        {
            int ypos = 0;
            foreach (var barcodeData in entBarcodeData!)
            {
                for (int i = 0; i < barcodeData.Quantity; i++)
                {
                    foreach (var item in barcodeDesignDtls!)
                    {
                        BarcodeItemFntNStringFormatFindMethod(item);
                        if (item.Name != "Barcode")
                        {
                            string? sValue = GetValueFromBarcodeEntity(barcodeData, item)!.ToString();
                            if (!string.IsNullOrEmpty(sValue))
                                Graphics.DrawString($"{item.Prefix}{sValue}", font, new PdfSolidBrush(Color.Black), new RectangleF(px(item.Position_x), px(item.Position_y + ypos), px(item.Width), px(item.Height)), stringFormat);
                            else
                            {
                                if (!string.IsNullOrEmpty(item.Prefix))
                                    Graphics.DrawString(item.Prefix, font, new PdfSolidBrush(Color.Black), new RectangleF(px(item.Position_x), px(item.Position_y + ypos), px(item.Width), px(item.Height)), stringFormat);
                            }
                        }
                        else
                        {
                            string? sValue = GetValueFromBarcodeEntity(barcodeData, item)!.ToString();
                            if (!string.IsNullOrEmpty(sValue))
                            {
                                PdfCode128Barcode barcode = new PdfCode128Barcode();
                                barcode.BarHeight = px(item.Height);
                                barcode.Text = sValue;
                                barcode.Draw(currentPage, new PointF(px(item.Position_x), px(item.Position_y + ypos)));
                            }
                        }
                    }
                    ypos = ypos + barcodeDesignHdr.Height ?? 100;
                }

            }
        }
        catch (Exception)
        {
            throw;
        }
        MemoryStream stream = new MemoryStream();
        document.Save(stream);
        document.Close(true);
        return stream;
    }
    public MemoryStream InvoiceMethod()
    {
        using PdfDocument document = new PdfDocument();

        document.PageSettings.Margins.All = 0;

        PdfSection section = document.Sections.Add();
        section.PageSettings.Rotate = PdfPageRotateAngle.RotateAngle0;
        section.PageSettings.Size = PdfPageSize.A4;

        PdfPage currentPage = section.Pages.Add();
        PdfGraphics graphics = currentPage.Graphics;

        PdfUnitConverter converter = new PdfUnitConverter();
        float px(float v) => converter.ConvertFromPixels(v, PdfGraphicsUnit.Point);
        float ptToPx(float v) => converter.ConvertToPixels(v, PdfGraphicsUnit.Point);

        int linesPerPage = (int)entDMaster!.LinesPerPage;
        int spaceBetween = (int)entDMaster.SpaceBtwnLines;

        int totalLines = entDetails!.Count;
        int totalPages = totalLines <= 0 ? 1 : (int)Math.Ceiling((double)totalLines / Math.Max(1, linesPerPage));

        int pageNumber = 1;

        // -----------------------------
        // Preload Settings (IMPORTANT)
        // -----------------------------

        var settings = entDSettings!;

        var headerSettings = settings
            .Where(x =>
                x.Location == InvoiceLabelPosition.Company_Section.ToString() ||
                x.Location == InvoiceLabelPosition.Invoice_Section.ToString() ||
                x.Location == InvoiceLabelPosition.Party_Section.ToString() ||
                x.Location == InvoiceLabelPosition.Invoice_Details_Header.ToString())
            .ToList();

        var detailSettings = settings
            .Where(x => x.Location == InvoiceLabelPosition.Details_Area.ToString())
            .ToList();

        var footerSettings = settings
            .Where(x => x.Location == InvoiceLabelPosition.Footer_Section.ToString())
            .ToList();

        bool hasHeader = headerSettings.Any();
        bool hasDetails = detailSettings.Any();
        bool hasFooter = footerSettings.Any();

        float defaultRowHeight = 20f;
        if (detailSettings.Any(x => x.Height > 0))
        {
            defaultRowHeight = detailSettings.Where(x => x.Height > 0).Min(x => (float)x.Height);
        }

        // -----------------------------
        // Main Loop
        // -----------------------------

        while (linesPrinted < totalLines)
        {
            // ===========================
            // HEADER
            // ===========================

            if (!isPrinted)
            {
                if (hasHeader)
                {
                    foreach (var item in headerSettings)
                    {
                        switch (item.Location)
                        {
                            case var l when l == InvoiceLabelPosition.Company_Section.ToString():
                                WritingMethod(item, entCompany, graphics, currentPage);
                                break;

                            case var l when l == InvoiceLabelPosition.Invoice_Section.ToString():
                                WritingMethod(item, entInvoice, graphics, currentPage);
                                break;

                            case var l when l == InvoiceLabelPosition.Party_Section.ToString():
                                WritingMethod(item, entParty, graphics, currentPage);
                                break;

                            case var l when l == InvoiceLabelPosition.Invoice_Details_Header.ToString():
                                WritingMethod(item, null, graphics, currentPage);
                                iTopMargin = Ypos + spaceBetween + 15;
                                break;
                        }
                    }
                }

                if (hasDetails)
                {
                    iTopMargin = detailSettings[0].PositionY;
                }

                isPrinted = true;
            }

            // ===========================
            // DETAILS
            // ===========================

            float rowHeight = defaultRowHeight;

            if (hasDetails)
            {
                var currentRow = entDetails[linesPrinted];

                // Calculate dynamic row height based on text measurement across all columns
                float maxMeasuredHeightPx = 0f;

                foreach (var item in detailSettings)
                {
                    if (item.ControlType == "ValueControl" || item.ControlType == "ValueControle")
                    {
                        string? val = getValueFromEntity(currentRow, item)?.ToString();
                        if (!string.IsNullOrEmpty(val))
                        {
                            ItemFontAndStringFormatFindMethod(item);
                            float colWidthPt = px(item.Width);
                            if (colWidthPt > 0 && font != null)
                            {
                                PdfStringFormat measureFormat = new PdfStringFormat
                                {
                                    Alignment = stringFormat?.Alignment ?? PdfTextAlignment.Left,
                                    WordWrap = PdfWordWrapType.Word
                                };
                                SizeF measuredPt = font.MeasureString(val, colWidthPt, measureFormat);
                                float measuredPx = ptToPx(measuredPt.Height);
                                if (measuredPx > maxMeasuredHeightPx)
                                {
                                    maxMeasuredHeightPx = measuredPx;
                                }
                            }
                        }
                    }
                }

                rowHeight = Math.Max(defaultRowHeight, maxMeasuredHeightPx + 4f);

                foreach (var item in detailSettings)
                {
                    if (!string.IsNullOrEmpty(item.Spec) &&
                        item.Spec.Equals("M", StringComparison.OrdinalIgnoreCase))
                    {
                        string? value = getValueFromEntity(currentRow, item)?.ToString();

                        if (!string.IsNullOrEmpty(value))
                        {
                            iTopMargin += (int)Math.Ceiling(rowHeight);
                            WritingMethod(item, currentRow, graphics, currentPage, customHeight: rowHeight);
                        }
                    }
                    else
                    {
                        WritingMethod(item, currentRow, graphics, currentPage, customHeight: rowHeight);
                    }
                }
            }

            // ===========================
            // LINE ADVANCE
            // ===========================

            iTopMargin += (int)Math.Ceiling(rowHeight);

            iIndex++;
            linesPrinted++;

            bool isLastLine = linesPrinted == totalLines;

            // ===========================
            // PAGE BREAK
            // ===========================

            float maxTableY = footerSettings.FirstOrDefault() != null
                ? footerSettings.First().PositionY - 20
                : (entDMaster.PageHeight - 120);

            if (iIndex >= linesPerPage || (iTopMargin + defaultRowHeight >= maxTableY && !isLastLine))
            {
                iIndex = 0;
                isPrinted = false;

                DrawFooter(currentPage, graphics, pageNumber, totalPages);

                pageNumber++;

                if (!isLastLine)
                {
                    currentPage = document.Pages.Add();
                    graphics = currentPage.Graphics;
                }
            }
        }

        // ===========================
        // FINAL FOOTER
        // ===========================

        if (hasFooter)
        {
            foreach (var item in footerSettings)
            {
                WritingMethod(item, entAdjustment, graphics, currentPage);
            }
        }

        DrawFooter(currentPage, graphics, pageNumber, Math.Max(totalPages, pageNumber));

        // ===========================
        // SAVE
        // ===========================

        MemoryStream stream = new MemoryStream();

        document.Save(stream);

        stream.Position = 0;

        return stream;
    }
    public MemoryStream NormalPrintingMethod()
    {
        void InMethodnextPage(PdfDocument document, ref PdfSection section, ref PdfPage currentPage, ref SizeF clientSize, ref PdfGraphics graphics)
        {
            float maxHeight = (entDMaster.PrintType == "Thermal" && entDMaster.PageHeight > 2000) ? 842f : entDMaster.PageHeight;
            if (iTopMargin + 4 >= maxHeight)
            {
                iIndex = 0;
                iTopMargin = 0;
                section = document.Sections.Add();
                //Create page settings to the section
                section.PageSettings.Rotate = PdfPageRotateAngle.RotateAngle0;
                if (entDMaster.PrintType == "Thermal")
                {
                    float pWidth = entDMaster.PageWidth > 0 ? entDMaster.PageWidth : 290f;
                    section.PageSettings.Size = new SizeF(pWidth, maxHeight);
                }
                else
                {
                    section.PageSettings.Size = PdfPageSize.A4;
                }
                //Add page to the section and initialize graphics for the page
                currentPage = section.Pages.Add();
                clientSize = currentPage.GetClientSize();
                graphics = currentPage.Graphics;
            }
        }
        PdfDocument document = new PdfDocument();
        document.PageSettings.Margins.All = 0;
        PdfSection section = document.Sections.Add();
        //Create page settings to the section
        section.PageSettings.Rotate = PdfPageRotateAngle.RotateAngle0;
        if (entDMaster.PrintType == "Thermal")
        {
            float pWidth = entDMaster.PageWidth > 0 ? entDMaster.PageWidth : 290f;
            float pHeight = (entDMaster.PageHeight > 0 && entDMaster.PageHeight <= 2000) ? entDMaster.PageHeight : 842f;
            section.PageSettings.Size = new SizeF(pWidth, pHeight);
        }
        else
        {
            section.PageSettings.Size = PdfPageSize.A4;
        }
        PdfPage currentPage = section.Pages.Add();
        SizeF clientSize = currentPage.GetClientSize();
        PdfGraphics graphics = currentPage.Graphics;
        try
        {
            if (entDSettings!.Any())
            {
                List<PrintDesignDtls> ds = new List<PrintDesignDtls>();
                ds = entDSettings!.Where(x => x.Location == InvoiceLabelPosition.Details_Area.ToString()).ToList();
                Ypos = 0;
                while (linesPrinted < entDetails!.Count)
                {
                    if (!isPrinted)
                    {
                        if (!ds.Any()) { linesPrinted = entDetails.Count; }
                        #region Invoice Top Section
                        List<PrintDesignDtls> dSettings = entDSettings!.Where(x => x.Location == InvoiceLabelPosition.Invoice_Section.ToString() || x.Location == InvoiceLabelPosition.Company_Section.ToString()
                        || x.Location == InvoiceLabelPosition.Party_Section.ToString()).OrderBy(x => x.PositionY).ToList();
                        if (dSettings.Count > 0)
                        {
                            foreach (var item in dSettings)
                            {
                                string? sVal = string.Empty;
                                if (item.ControlType == "LabelControl")
                                {
                                    PrintDesignDtls? dItem = dSettings.FirstOrDefault(x => (x.ControlType == "ValueControle" || x.ControlType == "ValueControl") && x.PositionY == item.PositionY);
                                    if (dItem != null)
                                    {
                                        if (item.Location == InvoiceLabelPosition.Invoice_Section.ToString())
                                        {
                                            sVal = getValueFromEntity(entInvoice, dItem)!.ToString();
                                        }
                                        else if (item.Location == InvoiceLabelPosition.Company_Section.ToString())
                                        {
                                            sVal = getValueFromEntity(entCompany, dItem)!.ToString();
                                        }
                                        else if (item.Location == InvoiceLabelPosition.Party_Section.ToString())
                                        {
                                            var val = getValueFromEntity(entParty, dItem);
                                            if (val != null)
                                                sVal = val.ToString();
                                        }
                                        if (sVal != null && sVal != string.Empty)
                                        {
                                            NormalWritingMethod(item, null, graphics, currentPage);
                                        }
                                    }
                                    else
                                    {
                                        NormalWritingMethod(item, null, graphics, currentPage);
                                    }
                                }
                                else if (item.ControlType == "ValueControle" || item.ControlType == "ValueControl")
                                {
                                    try
                                    {
                                        object? obj = entInvoice;
                                        if (item.Location == InvoiceLabelPosition.Invoice_Section.ToString())
                                        {
                                            sVal = getValueFromEntity(entInvoice, item)!.ToString();
                                        }
                                        else if (item.Location == InvoiceLabelPosition.Company_Section.ToString())
                                        {
                                            sVal = getValueFromEntity(entCompany, item)!.ToString();
                                            obj = entCompany;
                                        }
                                        else if (item.Location == InvoiceLabelPosition.Party_Section.ToString())
                                        {
                                            var val = getValueFromEntity(entParty, item);
                                            if (val != null)
                                                sVal = val.ToString();
                                            obj = entParty;
                                        }
                                        if (sVal != null && sVal != string.Empty)
                                        {
                                            NormalWritingMethod(item, obj, graphics, currentPage);
                                        }
                                    }
                                    catch (Exception)
                                    {

                                        continue;
                                    }
                                }
                                else { NormalWritingMethod(item, null, graphics, currentPage); }
                            }
                        }
                        else { continue; }
                        #endregion

                        #region Invoice Details Header
                        dSettings = entDSettings!.Where(x => x.Location == InvoiceLabelPosition.Invoice_Details_Header.ToString()).OrderBy(x => x.PositionY).ToList();
                        if (dSettings.Count > 0)
                        {
                            foreach (var item in dSettings)
                            {
                                NormalWritingMethod(item, null, graphics, currentPage);
                            }
                        }
                        else { continue; }
                        #endregion
                        iTopMargin = Ypos + (int)entDMaster!.SpaceBtwnLines;
                    }
                    isPrinted = true;

                    #region Details Method
                    if (ds.Any())
                    {
                        foreach (var item in ds)
                        {
                            if (item.Spec == null || item.Spec != "M")
                            {
                                NormalWritingMethod(item, entDetails[linesPrinted], graphics, currentPage);
                            }
                            else
                            {
                                string? sValue = getValueFromEntity(entDetails[linesPrinted], item)!.ToString();
                                if (sValue != null && sValue != string.Empty)
                                {
                                    NormalWritingMethod(item, entDetails[linesPrinted], graphics, currentPage);
                                }
                                iTopMargin = iTopMargin + item.FontSize + (int)entDMaster!.SpaceBtwnLines;

                                //
                                iIndex++;
                                linesPrinted++;
                                if (linesPrinted == entDetails.Count)
                                {
                                    isLast = true;
                                }
                                InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                                //
                            }

                        }

                    }
                    #endregion
                }
                if (isLast)
                {
                    InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                    iTopMargin = iTopMargin - (int)entDMaster!.SpaceBtwnLines;
                    if (entDSettings!.Count(x => x.Location == InvoiceLabelPosition.Footer_Section.ToString()) > 0)
                    {
                        List<PrintDesignDtls> dSettings = new List<PrintDesignDtls>();
                        dSettings = entDSettings!.Where(x => x.Location == InvoiceLabelPosition.Footer_Section.ToString()).OrderBy(x => x.PositionY).ToList();
                        foreach (var item in dSettings)
                        {
                            if (item.ControlType == "LabelControl")
                            {
                                PrintDesignDtls? dItem = dSettings.FirstOrDefault(x => (x.ControlType == "ValueControle" || x.ControlType == "ValueControl") && x.PositionY == item.PositionY);
                                if (dItem != null)
                                {
                                    if (getValueFromEntity(entAdjustment, dItem).toDecimal() != 0)
                                    {
                                        InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                                        NormalWritingMethod(item, entAdjustment, graphics, currentPage);

                                    }
                                }
                                else
                                {
                                    InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                                    NormalWritingMethod(item, entAdjustment, graphics, currentPage);
                                }
                            }
                            else if (item.ControlType == "ValueControle" || item.ControlType == "ValueControl")
                            {
                                string? sVal = getValueFromEntity(entAdjustment, item)!.ToString();
                                decimal dVal = 0M;
                                bool isNumeric = decimal.TryParse(sVal, out dVal);
                                if (isNumeric)
                                {
                                    if (getValueFromEntity(entAdjustment, item).toDecimal() != 0)
                                    {
                                        InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                                        NormalWritingMethod(item, entAdjustment, graphics, currentPage);
                                    }
                                }
                                else
                                {
                                    InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                                    NormalWritingMethod(item, entAdjustment, graphics, currentPage);
                                }
                            }
                            else
                            {
                                InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                                NormalWritingMethod(item, entAdjustment, graphics, currentPage);
                            }
                        }
                    }
                    if (entDSettings!.Count(x => x.Location == InvoiceLabelPosition.Footer_Details.ToString()) > 0)
                    {
                        foreach (var adj in entAdjustments!)
                        {
                            List<PrintDesignDtls> dSettings = new List<PrintDesignDtls>();
                            dSettings = entDSettings!.Where(x => x.Location == InvoiceLabelPosition.Footer_Details.ToString()).OrderBy(x => x.PositionY).ToList();
                            foreach (var item in dSettings)
                            {
                                InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                                NormalWritingMethod(item, adj, graphics, currentPage);
                            }
                            iTopMargin = iTopMargin + (int)entDMaster.SpaceBtwnLines;
                        }
                    }
                    iTopMargin = iTopMargin + (int)entDMaster.SpaceBtwnLines;
                    if (entDSettings!.Count(x => x.Location == InvoiceLabelPosition.Footer_Bottom.ToString()) > 0)
                    {
                        List<PrintDesignDtls> dSettings = new List<PrintDesignDtls>();
                        dSettings = entDSettings!.Where(x => x.Location == InvoiceLabelPosition.Footer_Bottom.ToString()).OrderBy(x => x.PositionY).ToList();
                        foreach (var item in dSettings)
                        {
                            InMethodnextPage(document, ref section, ref currentPage, ref clientSize, ref graphics);
                            NormalWritingMethod(item, tMaster, graphics, currentPage);
                        }
                    }
                }
            }
        }
        catch (Exception) { }
        MemoryStream stream = new MemoryStream();
        document.Save(stream);
        document.Close(true);
        return stream;
    }
    public MemoryStream ReportPrintMethod()
    {
        var detailsEnum = ReportDtls["Details"] as System.Collections.IEnumerable;
        if (detailsEnum == null) return null;

        var details = detailsEnum.Cast<object>().ToList(); // Avoid IEnumerator overhead

        // -----------------------------
        // PDF Init
        // -----------------------------
        PdfDocument document = new PdfDocument();

        document.EnableMemoryOptimization = true;
        document.Compression = PdfCompressionLevel.Best;

        document.PageSettings.Margins.All = 0;
        document.PageSettings.Size = PdfPageSize.A4;

        PdfPage currentPage = document.Pages.Add();
        PdfGraphics graphics = currentPage.Graphics;

        PdfUnitConverter converter = new PdfUnitConverter();
        float px(float v) => converter.ConvertFromPixels(v, PdfGraphicsUnit.Point);

        // -----------------------------
        // Reusable Objects
        // -----------------------------
        PdfSolidBrush blackBrush = new PdfSolidBrush(Color.Black);
        PdfPen blackPen = new PdfPen(Color.Black, 1);

        PdfStringFormat wrapFormat = new PdfStringFormat
        {
            WordWrap = PdfWordWrapType.Word,
            Alignment = PdfTextAlignment.Left
        };

        // -----------------------------
        // Cache Settings (NO LINQ IN LOOP)
        // -----------------------------
        var headerSettings = entDSettings
            .Where(x => x.Location == ReportLabelPosition.Report_Header.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        var detailHeaderSettings = entDSettings
            .Where(x => x.Location == ReportLabelPosition.Report_Details_Header.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        var detailSettings = entDSettings
            .Where(x => x.Location == ReportLabelPosition.Report_Details_Area.ToString())
            .OrderBy(x => x.PositionY)
            .ThenBy(x => x.PositionX)
            .ToList();

        var footerSettings = entDSettings
            .Where(x => x.Location == ReportLabelPosition.Report_Footer_Section.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        int pageHeight = entDSettings
            .Where(x => x.ControlType == "HorizontalLine")
            .Max(x => x.PositionY);

        int space = (int)entDMaster.SpaceBtwnLines;

        int pageNo = 1;
        int totalPages = (details.Count / (int)entDMaster.LinesPerPage) + 1;

        // -----------------------------
        // Reflection Cache (BIG SPEEDUP)
        // -----------------------------
        var getterCache = new Dictionary<string, Func<object, object>>();

        Type detailType = null;

        // -----------------------------
        // Draw Header
        // -----------------------------
        void DrawHeader()
        {
            foreach (var item in headerSettings)
            {
                ItemFontAndStringFormatFindMethod(item);

                // Image
                if (item.FieldText == "Image" &&
                    ReportHdr.ContainsKey("Image") &&
                    ReportHdr["Image"] is byte[] logo)
                {
                    using var ms = new MemoryStream(logo);
                    PdfImage img = new PdfBitmap(ms);

                    graphics.DrawImage(img,
                        new RectangleF(px(item.PositionX), px(item.PositionY),
                                       px(item.Width), px(item.Height)));
                }

                // Labels / Values
                if (item.ControlType == "LabelControl" ||
                    item.ControlType == "ValueControl")
                {
                    string text = item.ControlType == "ValueControl"
                        ? ReportHdr.GetValueOrDefault(item.FieldText)?.ToString()
                        : item.FieldText;

                    if (!string.IsNullOrEmpty(text))
                    {
                        graphics.DrawString(
                            text, font, blackBrush,
                            new RectangleF(px(item.PositionX), px(item.PositionY),
                                           px(item.Width), px(item.Height)),
                            stringFormat);
                    }
                }

                // Lines
                if (item.ControlType == "HorizontalLine")
                {
                    graphics.DrawLine(blackPen,
                        px(item.PositionX), px(item.PositionY),
                        px(item.PositionX + item.Width), px(item.PositionY));
                }

                if (item.ControlType == "VerticalLine")
                {
                    graphics.DrawLine(blackPen,
                        px(item.PositionX), px(item.PositionY),
                        px(item.PositionX), px(item.PositionY + item.Height));
                }
            }
        }

        // -----------------------------
        // Draw Detail Header
        // -----------------------------
        void DrawDetailHeader()
        {
            foreach (var item in detailHeaderSettings)
            {
                ItemFontAndStringFormatFindMethod(item);

                if (item.ControlType == "LabelControl")
                {
                    graphics.DrawString(
                        item.FieldText, font, blackBrush,
                        new RectangleF(px(item.PositionX), px(item.PositionY),
                                       px(item.Width), px(item.Height)),
                        stringFormat);
                }
            }
        }

        // -----------------------------
        // First Page
        // -----------------------------
        DrawHeader();
        DrawDetailHeader();

        int startY = detailSettings.FirstOrDefault()?.PositionY ?? 0;
        int topMargin = startY;

        int defaultRowHeight = detailSettings.FirstOrDefault()?.Height ?? 20;

        // -----------------------------
        // Main Loop
        // -----------------------------
        foreach (var detail in details)
        {
            if (detail == null) continue;

            // Build reflection cache once
            if (detailType == null)
            {
                detailType = detail.GetType();

                foreach (var col in detailSettings)
                {
                    var prop = detailType.GetProperty(col.FieldText);

                    if (prop != null)
                        getterCache[col.FieldText] = x => prop.GetValue(x);
                }
            }

            bool isBold = false;
            int rowHeight = defaultRowHeight;

            foreach (var item in detailSettings)
            {
                if (!getterCache.TryGetValue(item.FieldText, out var getter))
                    continue;

                var val = getter(detail);

                if (val == null) continue;

                string text = val.ToString();

                if (text.Contains("#"))
                {
                    isBold = true;
                    text = text.Replace("#", "");
                }

                ItemFontAndStringFormatFindMethod(item, isBold);

                RectangleF rect = new RectangleF(
                    px(item.PositionX),
                    px(topMargin),
                    px(item.Width),
                    px(item.Height));

                // Only measure if multiline
                if (item.Height > 20)
                {
                    SizeF size = font.MeasureString(text, item.Width, wrapFormat);
                    rowHeight = Math.Max(rowHeight, (int)size.Height);

                    graphics.DrawString(text, font, blackBrush, rect, wrapFormat);
                }
                else
                {
                    graphics.DrawString(text, font, blackBrush, rect, stringFormat);
                }
            }

            topMargin += rowHeight + space;

            // -----------------------------
            // Page Break
            // -----------------------------
            if (topMargin + rowHeight >= pageHeight)
            {
                DrawFooter(currentPage, graphics, pageNo, totalPages);

                pageNo++;

                currentPage = document.Pages.Add();
                graphics = currentPage.Graphics;

                DrawHeader();
                DrawDetailHeader();

                topMargin = startY;
            }
        }

        // -----------------------------
        // Final Footer
        // -----------------------------
        foreach (var item in footerSettings)
        {
            ItemFontAndStringFormatFindMethod(item);

            string text = item.ControlType == "ValueControl"
                ? ReportHdr.GetValueOrDefault(item.FieldText)?.ToString()
                : item.FieldText;

            if (!string.IsNullOrEmpty(text))
            {
                graphics.DrawString(
                    text, font, blackBrush,
                    new RectangleF(px(item.PositionX), px(item.PositionY),
                                   px(item.Width), px(item.Height)),
                    stringFormat);
            }
        }

        DrawFooter(currentPage, graphics, pageNo, totalPages);

        // -----------------------------
        // Save
        // -----------------------------
        MemoryStream stream = new MemoryStream();

        document.Save(stream);
        document.Close(true);

        stream.Position = 0;

        return stream;
    }
    public MemoryStream ReportPrintFastMethod(bool LastRowBold)
    {
        var details = ReportDtls["Details"] as System.Collections.IEnumerable;
        if (details == null) return null;

        // -----------------------------
        // PDF Init
        // -----------------------------
        PdfDocument document = new PdfDocument();

        document.EnableMemoryOptimization = true;
        document.Compression = PdfCompressionLevel.Best;

        document.PageSettings.Margins.All = 20;
        document.PageSettings.Size = PdfPageSize.A4;

        PdfPage page = document.Pages.Add();
        PdfGraphics graphics = page.Graphics;

        PdfUnitConverter converter = new PdfUnitConverter();
        float px(float v) => converter.ConvertFromPixels(v, PdfGraphicsUnit.Point);

        // -----------------------------
        // Cache Settings
        // -----------------------------
        var headerSettings = entDSettings
            .Where(x => x.Location == ReportLabelPosition.Report_Header.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        var detailHeaderSettings = entDSettings
            .Where(x => x.Location == ReportLabelPosition.Report_Details_Header.ToString() && x.ControlType == "LabelControl")
            .OrderBy(x => x.PositionX)
            .ToList();

        var detailSettings = entDSettings
            .Where(x => x.Location == ReportLabelPosition.Report_Details_Area.ToString())
            .OrderBy(x => x.PositionX)
            .ToList();

        var footerSettings = entDSettings
            .Where(x => x.Location == ReportLabelPosition.Report_Footer_Section.ToString())
            .OrderBy(x => x.PositionY)
            .ToList();

        // -----------------------------
        // Reusable Objects
        // -----------------------------
        PdfSolidBrush blackBrush = new PdfSolidBrush(Color.Black);
        PdfPen blackPen = new PdfPen(Color.Black, 1);

        // -----------------------------
        // Draw Header
        // -----------------------------
        float DrawHeader(PdfPage p, PdfGraphics g)
        {
            float maxY = 0;

            // Logo
            var imageHdr = headerSettings.FirstOrDefault(x => x.FieldText == "Image");

            if (imageHdr != null &&
                ReportHdr.ContainsKey(imageHdr.FieldText) &&
                ReportHdr[imageHdr.FieldText] is byte[] logo)
            {
                using var imgStream = new MemoryStream(logo);
                PdfImage img = new PdfBitmap(imgStream);

                g.DrawImage(img,
                    new RectangleF(
                        px(imageHdr.PositionX),
                        px(imageHdr.PositionY),
                        px(imageHdr.Width),
                        px(imageHdr.Height)));
            }

            // Labels & Values
            foreach (var item in headerSettings.Where(x =>
                         x.ControlType == "LabelControl" || x.ControlType == "ValueControl"))
            {
                ItemFontAndStringFormatFindMethod(item);

                string text = item.ControlType == "ValueControl"
                    ? ReportHdr[item.FieldText]?.ToString()
                    : item.FieldText;

                if (string.IsNullOrEmpty(text)) continue;

                g.DrawString(
                    text,
                    font,
                    blackBrush,
                    new RectangleF(px(item.PositionX), px(item.PositionY),
                                   px(item.Width), px(item.Height)));

                maxY = Math.Max(maxY, item.PositionY + item.Height);
            }

            // Lines

            return maxY + 10; // Next section start
        }

        // -----------------------------
        // Draw Footer
        // -----------------------------

        // -----------------------------
        // Build DataTable
        // -----------------------------
        DataTable table = new DataTable();

        foreach (var col in detailSettings)
        {
            table.Columns.Add(col.FieldText);
        }

        // Reflection Cache
        Dictionary<string, Func<object, object>> getterCache =
            new Dictionary<string, Func<object, object>>();

        Type detailType = null;

        foreach (var detail in details)
        {
            if (detail == null) continue;

            if (detailType == null)
            {
                detailType = detail.GetType();

                foreach (var col in detailSettings)
                {
                    var prop = detailType.GetProperty(col.FieldText);

                    if (prop != null)
                    {
                        getterCache[col.FieldText] =
                            x => prop.GetValue(x);
                    }
                }
            }

            DataRow row = table.NewRow();

            foreach (var col in detailSettings)
            {
                if (getterCache.TryGetValue(col.FieldText, out var getter))
                {
                    var val = getter(detail);

                    if (val is decimal d)
                        row[col.FieldText] = Math.Round(d, 2).ToString("N2");
                    else
                        row[col.FieldText] = val ?? "";
                }
                else
                {
                    row[col.FieldText] = "";
                }
            }

            table.Rows.Add(row);
        }

        // -----------------------------
        // Create PdfGrid
        // -----------------------------
        PdfGrid grid = new PdfGrid();

        grid.DataSource = table;
        for (int i = 0; i < detailSettings.Count; i++)
        {
            grid.Columns[i].Width = px(detailSettings[i].Width);
        }
        // Custom column headers
        if (grid.Headers.Count > 0)
        {
            PdfGridRow hRow = grid.Headers[0];

            for (int i = 0; i < detailHeaderSettings.Count; i++)
            {
                hRow.Cells[i].Value =
                    detailHeaderSettings[i].FieldText;
            }
        }

        grid.RepeatHeader = true;
        grid.AllowRowBreakAcrossPages = true;

        // Header Style
        PdfGridRow headerRow = grid.Headers[0];

        headerRow.Style.BackgroundBrush =
            new PdfSolidBrush(new PdfColor(230, 230, 230));

        headerRow.Style.Font =
            new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold);

        headerRow.Style.TextBrush = blackBrush;

        // Cell Style
        grid.Style.Font = font;
        grid.Style.TextBrush = blackBrush;

        grid.Style.CellPadding = new PdfPaddings(3, 3, 3, 3);

        //grid.ApplyBuiltinStyle(PdfGridBuiltinStyle.GridTable4Accent1);
        if (LastRowBold)
        {
            var lastRow = grid.Rows[grid.Rows.Count - 1];
            lastRow.Style.Font =
                new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
        }
        // -----------------------------
        // Layout Format (Paging)
        // -----------------------------
        PdfLayoutFormat layout = new PdfLayoutFormat
        {
            Layout = PdfLayoutType.Paginate,
            Break = PdfLayoutBreakType.FitPage
        };

        // -----------------------------
        // Draw Everything
        // -----------------------------
        int pageNo = 1;

        PdfPage currentPage = page;

        float top = DrawHeader(currentPage, graphics);

        PdfLayoutResult result =
            grid.Draw(currentPage, new PointF(0, top), layout);

        // -----------------------------
        // Save
        // -----------------------------
        MemoryStream stream = new MemoryStream();

        document.Save(stream);
        document.Close(true);

        stream.Position = 0;

        return stream;
    }
    #endregion
}

