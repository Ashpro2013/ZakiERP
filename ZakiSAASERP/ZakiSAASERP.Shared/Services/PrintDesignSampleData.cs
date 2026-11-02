using System.IO;
using System.Text.Json;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Enums;
using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.ViewModel;

namespace ZakiSAASERP.Shared.Services;

public static class PrintDesignSampleData
{
    public static PrintDesignDTO GetFallbackDesign(string voucherType, bool isThermal)
    {
        try
        {
            string fileName = isThermal ? "Sales_Invoice_Thermal.json" : "Sales_Invoice.json";
            string[] possiblePaths =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "PrintDesign", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PrintDesign", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "ZakiSAASERP.Web", "wwwroot", "PrintDesign", fileName),
                Path.Combine("d:\\AshproSAAS\\ZakiSAASERP\\ZakiSAASERP.Web\\wwwroot\\PrintDesign", fileName)
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var dto = JsonSerializer.Deserialize<PrintDesignDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto?.hdr != null && dto.dtls != null && dto.dtls.Any())
                        return dto;
                }
            }
        }
        catch
        {
            // Ignore file errors and fallback to programmatic template
        }

        return CreateFallbackDesignProgrammatic(voucherType, isThermal);
    }

    private static PrintDesignDTO CreateFallbackDesignProgrammatic(string voucherType, bool isThermal)
    {
        if (isThermal)
        {
            var thHdr = new PrintDesignHdr
            {
                PrintDesignHdrId = 0,
                DesignName = "POS Thermal Receipt",
                VoucherType = "Sales_Invoice_Thermal",
                FormType = "Invoice",
                PrintType = "Thermal",
                IsDefault = true,
                PageWidth = 290,
                PageHeight = 30000,
                LinesPerPage = 100,
                SpaceBtwnLines = 5
            };

            var thControls = new List<PrintDesignDtls>
            {
                new() { ControlName = "LabelControl_1", ControlType = "LabelControl", FieldText = "Simplified Tax Invoice / فاتورة ضريبية مبسطة", PositionX = 0, PositionY = 10, Width = 290, Height = 20, FontSize = 11, IsBold = true, Alignment = "Center", Location = "Company_Section" },
                new() { ControlName = "ValueControl_1", ControlType = "ValueControl", FieldText = "Name", PositionX = 0, PositionY = 35, Width = 290, Height = 20, FontSize = 13, IsBold = true, Alignment = "Center", Location = "Company_Section" },
                new() { ControlName = "ValueControl_2", ControlType = "ValueControl", FieldText = "NameArabic", PositionX = 0, PositionY = 55, Width = 290, Height = 20, FontSize = 12, IsBold = true, Alignment = "Center", Location = "Company_Section" },
                new() { ControlName = "ValueControl_3", ControlType = "ValueControl", FieldText = "StreetName", PositionX = 0, PositionY = 75, Width = 290, Height = 18, FontSize = 9, IsBold = false, Alignment = "Center", Location = "Company_Section" },
                new() { ControlName = "LabelControl_2", ControlType = "LabelControl", FieldText = "VAT No / الرقم الضريبي:", PositionX = 0, PositionY = 95, Width = 130, Height = 18, FontSize = 9, IsBold = true, Alignment = "Left", Location = "Company_Section" },
                new() { ControlName = "ValueControl_4", ControlType = "ValueControl", FieldText = "VatNo", PositionX = 130, PositionY = 95, Width = 160, Height = 18, FontSize = 9, IsBold = true, Alignment = "Right", Location = "Company_Section" },
                new() { ControlName = "HorizontalLine_1", ControlType = "HorizontalLine", FieldText = "", PositionX = 0, PositionY = 115, Width = 290, Height = 2, FontSize = 1, Location = "Company_Section" },
                new() { ControlName = "LabelControl_3", ControlType = "LabelControl", FieldText = "Invoice No:", PositionX = 0, PositionY = 125, Width = 80, Height = 18, FontSize = 9, IsBold = true, Alignment = "Left", Location = "Invoice_Section" },
                new() { ControlName = "ValueControl_5", ControlType = "ValueControl", FieldText = "HdrNo", PositionX = 80, PositionY = 125, Width = 210, Height = 18, FontSize = 9, IsBold = true, Alignment = "Left", Location = "Invoice_Section" },
                new() { ControlName = "LabelControl_4", ControlType = "LabelControl", FieldText = "Date:", PositionX = 0, PositionY = 145, Width = 60, Height = 18, FontSize = 9, IsBold = true, Alignment = "Left", Location = "Invoice_Section" },
                new() { ControlName = "ValueControl_6", ControlType = "ValueControl", FieldText = "InvoiceDate", PositionX = 60, PositionY = 145, Width = 230, Height = 18, FontSize = 9, Alignment = "Left", Location = "Invoice_Section" },
                new() { ControlName = "HorizontalDotedLine_1", ControlType = "HorizontalDotedLine", FieldText = "", PositionX = 0, PositionY = 168, Width = 290, Height = 2, FontSize = 1, Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_5", ControlType = "LabelControl", FieldText = "Item / الصنف", PositionX = 0, PositionY = 175, Width = 120, Height = 18, FontSize = 9, IsBold = true, Alignment = "Left", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_6", ControlType = "LabelControl", FieldText = "Qty", PositionX = 120, PositionY = 175, Width = 35, Height = 18, FontSize = 9, IsBold = true, Alignment = "Center", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_7", ControlType = "LabelControl", FieldText = "Rate", PositionX = 155, PositionY = 175, Width = 55, Height = 18, FontSize = 9, IsBold = true, Alignment = "Right", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_8", ControlType = "LabelControl", FieldText = "Total", PositionX = 210, PositionY = 175, Width = 80, Height = 18, FontSize = 9, IsBold = true, Alignment = "Right", Location = "Invoice_Details_Header" },
                new() { ControlName = "HorizontalLine_2", ControlType = "HorizontalLine", FieldText = "", PositionX = 0, PositionY = 195, Width = 290, Height = 2, FontSize = 1, Location = "Invoice_Details_Header" },
                new() { ControlName = "ValueControl_7", ControlType = "ValueControl", FieldText = "ItemName", PositionX = 0, PositionY = 202, Width = 120, Height = 20, FontSize = 8, Alignment = "Left", Location = "Details_Area" },
                new() { ControlName = "ValueControl_8", ControlType = "ValueControl", FieldText = "Qty", PositionX = 120, PositionY = 202, Width = 35, Height = 20, FontSize = 9, Alignment = "Center", Location = "Details_Area" },
                new() { ControlName = "ValueControl_9", ControlType = "ValueControl", FieldText = "Rate", PositionX = 155, PositionY = 202, Width = 55, Height = 20, FontSize = 9, Alignment = "Right", Location = "Details_Area" },
                new() { ControlName = "ValueControl_10", ControlType = "ValueControl", FieldText = "TotalAmnt", PositionX = 210, PositionY = 202, Width = 80, Height = 20, FontSize = 9, Alignment = "Right", Location = "Details_Area" },
                new() { ControlName = "HorizontalDotedLine_2", ControlType = "HorizontalDotedLine", FieldText = "", PositionX = 0, PositionY = 228, Width = 290, Height = 2, FontSize = 1, Location = "Footer_Section" },
                new() { ControlName = "LabelControl_9", ControlType = "LabelControl", FieldText = "Gross / الإجمالي:", PositionX = 0, PositionY = 236, Width = 140, Height = 18, FontSize = 9, Alignment = "Left", Location = "Footer_Section" },
                new() { ControlName = "ValueControl_11", ControlType = "ValueControl", FieldText = "Gross", PositionX = 140, PositionY = 236, Width = 150, Height = 18, FontSize = 9, Alignment = "Right", Location = "Footer_Section" },
                new() { ControlName = "LabelControl_10", ControlType = "LabelControl", FieldText = "VAT (15%) / ضريبة:", PositionX = 0, PositionY = 256, Width = 140, Height = 18, FontSize = 9, Alignment = "Left", Location = "Footer_Section" },
                new() { ControlName = "ValueControl_12", ControlType = "ValueControl", FieldText = "TaxAmnt", PositionX = 140, PositionY = 256, Width = 150, Height = 18, FontSize = 9, Alignment = "Right", Location = "Footer_Section" },
                new() { ControlName = "LabelControl_11", ControlType = "LabelControl", FieldText = "Net Total / الصافي:", PositionX = 0, PositionY = 276, Width = 140, Height = 20, FontSize = 10, IsBold = true, Alignment = "Left", Location = "Footer_Section" },
                new() { ControlName = "ValueControl_13", ControlType = "ValueControl", FieldText = "TotalAmount", PositionX = 140, PositionY = 276, Width = 150, Height = 20, FontSize = 11, IsBold = true, Alignment = "Right", Location = "Footer_Section" },
                new() { ControlName = "HorizontalLine_3", ControlType = "HorizontalLine", FieldText = "", PositionX = 0, PositionY = 300, Width = 290, Height = 2, FontSize = 1, Location = "Footer_Bottom" },
                new() { ControlName = "QRCode_1", ControlType = "QRCode", FieldText = "InvoiceQRCode", PositionX = 75, PositionY = 310, Width = 140, Height = 140, Location = "Footer_Bottom" },
                new() { ControlName = "LabelControl_12", ControlType = "LabelControl", FieldText = "Thank you for your visit! / شكراً لزيارتكم", PositionX = 0, PositionY = 460, Width = 290, Height = 18, FontSize = 9, Alignment = "Center", Location = "Footer_Bottom" }
            };

            for (int i = 0; i < thControls.Count; i++) thControls[i].PIndex = i;
            return new PrintDesignDTO(thHdr, thControls);
        }
        else
        {
            var a4Hdr = new PrintDesignHdr
            {
                PrintDesignHdrId = 0,
                DesignName = "Standard Sales Invoice",
                VoucherType = "Sales_Invoice",
                FormType = "Invoice",
                PrintType = "A4",
                IsDefault = true,
                PageWidth = 700,
                PageHeight = 1000,
                LinesPerPage = 40,
                SpaceBtwnLines = 10
            };

            var a4Controls = new List<PrintDesignDtls>
            {
                new() { ControlName = "LabelControl_1", ControlType = "LabelControl", FieldText = "TAX INVOICE / فاتورة ضريبية", PositionX = 50, PositionY = 30, Width = 700, Height = 30, FontSize = 18, IsBold = true, Alignment = "Center", Location = "Company_Section" },
                new() { ControlName = "HorizontalLine_1", ControlType = "HorizontalLine", FieldText = "", PositionX = 50, PositionY = 65, Width = 700, Height = 2, FontSize = 1, Location = "Company_Section" },
                new() { ControlName = "LabelControl_2", ControlType = "LabelControl", FieldText = "Customer:", PositionX = 50, PositionY = 75, Width = 100, Height = 20, FontSize = 11, IsBold = true, Alignment = "Left", Location = "Party_Section" },
                new() { ControlName = "ValueControl_1", ControlType = "ValueControl", FieldText = "Name", PositionX = 150, PositionY = 75, Width = 240, Height = 20, FontSize = 11, IsBold = true, Alignment = "Left", Location = "Party_Section" },
                new() { ControlName = "LabelControl_3", ControlType = "LabelControl", FieldText = "VAT No:", PositionX = 50, PositionY = 100, Width = 100, Height = 20, FontSize = 10, Alignment = "Left", Location = "Party_Section" },
                new() { ControlName = "ValueControl_2", ControlType = "ValueControl", FieldText = "VatNo", PositionX = 150, PositionY = 100, Width = 240, Height = 20, FontSize = 10, Alignment = "Left", Location = "Party_Section" },
                new() { ControlName = "LabelControl_4", ControlType = "LabelControl", FieldText = "Invoice No:", PositionX = 420, PositionY = 75, Width = 100, Height = 20, FontSize = 11, IsBold = true, Alignment = "Left", Location = "Invoice_Section" },
                new() { ControlName = "ValueControl_3", ControlType = "ValueControl", FieldText = "HdrNo", PositionX = 520, PositionY = 75, Width = 230, Height = 20, FontSize = 11, IsBold = true, Alignment = "Left", Location = "Invoice_Section" },
                new() { ControlName = "LabelControl_5", ControlType = "LabelControl", FieldText = "Date:", PositionX = 420, PositionY = 100, Width = 100, Height = 20, FontSize = 10, Alignment = "Left", Location = "Invoice_Section" },
                new() { ControlName = "ValueControl_4", ControlType = "ValueControl", FieldText = "InvoiceDate", PositionX = 520, PositionY = 100, Width = 230, Height = 20, FontSize = 10, Alignment = "Left", Location = "Invoice_Section" },
                new() { ControlName = "HorizontalLine_2", ControlType = "HorizontalLine", FieldText = "", PositionX = 50, PositionY = 130, Width = 700, Height = 2, FontSize = 1, Location = "Company_Section" },
                new() { ControlName = "LabelControl_6", ControlType = "LabelControl", FieldText = "#", PositionX = 50, PositionY = 140, Width = 40, Height = 22, FontSize = 10, IsBold = true, Alignment = "Center", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_7", ControlType = "LabelControl", FieldText = "Item Code", PositionX = 90, PositionY = 140, Width = 110, Height = 22, FontSize = 10, IsBold = true, Alignment = "Left", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_8", ControlType = "LabelControl", FieldText = "Description", PositionX = 200, PositionY = 140, Width = 240, Height = 22, FontSize = 10, IsBold = true, Alignment = "Left", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_9", ControlType = "LabelControl", FieldText = "Qty", PositionX = 440, PositionY = 140, Width = 60, Height = 22, FontSize = 10, IsBold = true, Alignment = "Right", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_10", ControlType = "LabelControl", FieldText = "Rate", PositionX = 500, PositionY = 140, Width = 70, Height = 22, FontSize = 10, IsBold = true, Alignment = "Right", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_11", ControlType = "LabelControl", FieldText = "VAT (15%)", PositionX = 570, PositionY = 140, Width = 80, Height = 22, FontSize = 10, IsBold = true, Alignment = "Right", Location = "Invoice_Details_Header" },
                new() { ControlName = "LabelControl_12", ControlType = "LabelControl", FieldText = "Total", PositionX = 650, PositionY = 140, Width = 100, Height = 22, FontSize = 10, IsBold = true, Alignment = "Right", Location = "Invoice_Details_Header" },
                new() { ControlName = "HorizontalLine_3", ControlType = "HorizontalLine", FieldText = "", PositionX = 50, PositionY = 165, Width = 700, Height = 1, FontSize = 1, Location = "Invoice_Details_Header" },
                new() { ControlName = "ValueControl_5", ControlType = "ValueControl", FieldText = "SL", PositionX = 50, PositionY = 175, Width = 40, Height = 22, FontSize = 9, Alignment = "Center", Location = "Details_Area" },
                new() { ControlName = "ValueControl_6", ControlType = "ValueControl", FieldText = "ItemCode", PositionX = 90, PositionY = 175, Width = 110, Height = 22, FontSize = 9, Alignment = "Left", Location = "Details_Area" },
                new() { ControlName = "ValueControl_7", ControlType = "ValueControl", FieldText = "ItemName", PositionX = 200, PositionY = 175, Width = 240, Height = 22, FontSize = 9, Alignment = "Left", Location = "Details_Area" },
                new() { ControlName = "ValueControl_8", ControlType = "ValueControl", FieldText = "Qty", PositionX = 440, PositionY = 175, Width = 60, Height = 22, FontSize = 9, Alignment = "Right", Location = "Details_Area" },
                new() { ControlName = "ValueControl_9", ControlType = "ValueControl", FieldText = "Rate", PositionX = 500, PositionY = 175, Width = 70, Height = 22, FontSize = 9, Alignment = "Right", Location = "Details_Area" },
                new() { ControlName = "ValueControl_10", ControlType = "ValueControl", FieldText = "TaxAmnt", PositionX = 570, PositionY = 175, Width = 80, Height = 22, FontSize = 9, Alignment = "Right", Location = "Details_Area" },
                new() { ControlName = "ValueControl_11", ControlType = "ValueControl", FieldText = "TotalAmnt", PositionX = 650, PositionY = 175, Width = 100, Height = 22, FontSize = 9, Alignment = "Right", Location = "Details_Area" },
                new() { ControlName = "HorizontalLine_4", ControlType = "HorizontalLine", FieldText = "", PositionX = 50, PositionY = 480, Width = 700, Height = 2, FontSize = 1, Location = "Footer_Section" },
                new() { ControlName = "LabelControl_13", ControlType = "LabelControl", FieldText = "Sub Total:", PositionX = 480, PositionY = 490, Width = 120, Height = 20, FontSize = 10, Alignment = "Left", Location = "Footer_Section" },
                new() { ControlName = "ValueControl_12", ControlType = "ValueControl", FieldText = "Gross", PositionX = 600, PositionY = 490, Width = 150, Height = 20, FontSize = 10, Alignment = "Right", Location = "Footer_Section" },
                new() { ControlName = "LabelControl_14", ControlType = "LabelControl", FieldText = "VAT Total (15%):", PositionX = 480, PositionY = 515, Width = 120, Height = 20, FontSize = 10, Alignment = "Left", Location = "Footer_Section" },
                new() { ControlName = "ValueControl_13", ControlType = "ValueControl", FieldText = "TaxAmnt", PositionX = 600, PositionY = 515, Width = 150, Height = 20, FontSize = 10, Alignment = "Right", Location = "Footer_Section" },
                new() { ControlName = "LabelControl_15", ControlType = "LabelControl", FieldText = "Grand Total:", PositionX = 480, PositionY = 540, Width = 120, Height = 24, FontSize = 12, IsBold = true, Alignment = "Left", Location = "Footer_Section" },
                new() { ControlName = "ValueControl_14", ControlType = "ValueControl", FieldText = "TotalAmount", PositionX = 600, PositionY = 540, Width = 150, Height = 24, FontSize = 12, IsBold = true, Alignment = "Right", Location = "Footer_Section" },
                new() { ControlName = "QRCode_1", ControlType = "QRCode", FieldText = "InvoiceQRCode", PositionX = 50, PositionY = 490, Width = 120, Height = 120, Location = "Footer_Section" }
            };

            for (int i = 0; i < a4Controls.Count; i++) a4Controls[i].PIndex = i;
            return new PrintDesignDTO(a4Hdr, a4Controls);
        }
    }
    public static PrintParameterDTO CreateSampleInvoiceData(PrintDesignHdr hdr, List<PrintDesignDtls> controls, bool isThermal)
    {
        // 1. Company Information
        var company = new CompanyDetails
        {
            Name = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.Name) ? AppData.CompanyDetails.Name : "Ashpro Enterprise Solutions Ltd.",
            NameArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.NameArabic) ? AppData.CompanyDetails.NameArabic : "شركة أشبور للحلول البرمجية المتكاملة",
            VatNo = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.VatNo) ? AppData.CompanyDetails.VatNo : "300012345600003",
            VatNoArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.VatNoArabic) ? AppData.CompanyDetails.VatNoArabic : "٣٠٠٠١٢٣٤٥٦٠٠٠٠٣",
            BuildingNo = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.BuildingNo) ? AppData.CompanyDetails.BuildingNo : "4125",
            BuildingNoArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.BuildingNoArabic) ? AppData.CompanyDetails.BuildingNoArabic : "٤١٢٥",
            StreetName = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.StreetName) ? AppData.CompanyDetails.StreetName : "King Fahd Road",
            StreetNameArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.StreetNameArabic) ? AppData.CompanyDetails.StreetNameArabic : "طريق الملك فهد",
            District = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.District) ? AppData.CompanyDetails.District : "Al Olaya District",
            DistrictArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.DistrictArabic) ? AppData.CompanyDetails.DistrictArabic : "حي العليا",
            City = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.City) ? AppData.CompanyDetails.City : "Riyadh",
            CityArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.CityArabic) ? AppData.CompanyDetails.CityArabic : "الرياض",
            PostalCode = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.PostalCode) ? AppData.CompanyDetails.PostalCode : "12214",
            PostalCodeArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.PostalCodeArabic) ? AppData.CompanyDetails.PostalCodeArabic : "١٢٢١٤",
            AdditionalNo = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.AdditionalNo) ? AppData.CompanyDetails.AdditionalNo : "8842",
            AdditionalNoArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.AdditionalNoArabic) ? AppData.CompanyDetails.AdditionalNoArabic : "٨٨٤٢",
            Telephone = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.Telephone) ? AppData.CompanyDetails.Telephone : "+966 11 234 5678",
            TelephoneArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.TelephoneArabic) ? AppData.CompanyDetails.TelephoneArabic : "+٩٦٦ ١١ ٢٣٤ ٥٦٧٨",
            Mobile = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.Mobile) ? AppData.CompanyDetails.Mobile : "+966 50 123 4567",
            MobileArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.MobileArabic) ? AppData.CompanyDetails.MobileArabic : "+٩٦٦ ٥٠ ١٢٣ ٤٥٦٧",
            Email = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.Email) ? AppData.CompanyDetails.Email : "info@ashproerp.com",
            Contact = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.Contact) ? AppData.CompanyDetails.Contact : "Customer Service",
            ContactArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.ContactArabic) ? AppData.CompanyDetails.ContactArabic : "خدمة العملاء",
            IdentificationNo = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.IdentificationNo) ? AppData.CompanyDetails.IdentificationNo : "1010123456",
            IdentificationNoArabic = !string.IsNullOrWhiteSpace(AppData.CompanyDetails?.IdentificationNoArabic) ? AppData.CompanyDetails.IdentificationNoArabic : "١٠١٠١٢٣٤٥٦",
            OtherSellerId = "1010123456",
            OtherSellerIdArabic = "١٠١٠١٢٣٤٥٦",
            CountrySubentity = "Riyadh Region",
            CountrySubentityArabic = "منطقة الرياض",
            HeaderImage = AppData.CompanyDetails?.HeaderImage,
            FooterImage = AppData.CompanyDetails?.FooterImage
        };

        // 2. Party (Customer)
        var party = new Party
        {
            CustomerId = 101,
            Code = "CUST-00101",
            Name = isThermal ? "Cash Customer" : "Modern Horizons Trading Co.",
            NameArabic = isThermal ? "عميل نقدي" : "شركة آفاق الحديثة للتجارة",
            Address = isThermal ? "Riyadh, KSA" : "Al Ma'ather St, Riyadh 12713, Saudi Arabia",
            AddressArabic = isThermal ? "الرياض، المملكة العربية السعودية" : "شارع المعذر، الرياض ١٢٧١٣، المملكة العربية السعودية",
            VatNo = isThermal ? "310000000000003" : "310987654300003",
            Telephone = "+966 11 987 6543",
            Mobile = "+966 55 987 6543",
            Email = "accounts@modernhorizons.com",
            Contact = "Fahad Al-Harbi",
            BuildingNo = "3421",
            StreetName = "Al Ma'ather St",
            District = "Al Wisham",
            City = "Riyadh",
            PostalCode = "12713",
            AdditionalNo = "7654"
        };

        // 3. Line Items
        List<TransactionDtls> details;
        if (isThermal)
        {
            details = new List<TransactionDtls>
            {
                new TransactionDtls
                {
                    SL = 1,
                    DtlsId = 1,
                    ItemCode = "POS-001",
                    ItemName = "Premium Arabica Coffee 500g",
                    ItemArabicName = "حبوب بن أرابيكا فاخر 500 جم",
                    Barcode = "62810010001",
                    UnitName = "PCS",
                    Quantity = 2.00m,
                    ExRate = 45.00m,
                    InRate = 51.75m,
                    GrossAmount = 90.00m,
                    DiscountPerc = 0m,
                    DiscountAmnt = 0m,
                    TaxableAmnt = 90.00m,
                    TaxPerc = 15.00m,
                    TaxAmount = 13.50m,
                    LineTotal = 103.50m,
                    Description = "Medium Roast"
                },
                new TransactionDtls
                {
                    SL = 2,
                    DtlsId = 2,
                    ItemCode = "POS-002",
                    ItemName = "Butter Croissant Fresh",
                    ItemArabicName = "كرواسون زبدة طازج",
                    Barcode = "62810010002",
                    UnitName = "PCS",
                    Quantity = 3.00m,
                    ExRate = 12.00m,
                    InRate = 13.80m,
                    GrossAmount = 36.00m,
                    DiscountPerc = 0m,
                    DiscountAmnt = 0m,
                    TaxableAmnt = 36.00m,
                    TaxPerc = 15.00m,
                    TaxAmount = 5.40m,
                    LineTotal = 41.40m,
                    Description = "Fresh Bakery"
                },
                new TransactionDtls
                {
                    SL = 3,
                    DtlsId = 3,
                    ItemCode = "POS-003",
                    ItemName = "Mineral Water 500ml",
                    ItemArabicName = "مياه معدنية 500 مل",
                    Barcode = "62810010003",
                    UnitName = "BTL",
                    Quantity = 2.00m,
                    ExRate = 3.00m,
                    InRate = 3.45m,
                    GrossAmount = 6.00m,
                    DiscountPerc = 0m,
                    DiscountAmnt = 0m,
                    TaxableAmnt = 6.00m,
                    TaxPerc = 15.00m,
                    TaxAmount = 0.90m,
                    LineTotal = 6.90m,
                    Description = "Natural Spring Water"
                }
            };
        }
        else
        {
            details = new List<TransactionDtls>
            {
                new TransactionDtls
                {
                    SL = 1,
                    DtlsId = 1,
                    ItemCode = "HW-101",
                    ItemName = "Dell Latitude 5540 Core i7 16GB / 512GB SSD",
                    ItemArabicName = "كمبيوتر محمول ديل كور آي 7",
                    Barcode = "884116382101",
                    UnitName = "UNIT",
                    Quantity = 2.00m,
                    ExRate = 4200.00m,
                    InRate = 4830.00m,
                    GrossAmount = 8400.00m,
                    DiscountPerc = 5.00m,
                    DiscountAmnt = 420.00m,
                    TaxableAmnt = 7980.00m,
                    TaxPerc = 15.00m,
                    TaxAmount = 1197.00m,
                    LineTotal = 9177.00m,
                    Description = "15.6 inch FHD, Windows 11 Pro, 3 Yr Onsite Warranty"
                },
                new TransactionDtls
                {
                    SL = 2,
                    DtlsId = 2,
                    ItemCode = "ACC-205",
                    ItemName = "Dell Premier Multi-Device Wireless Keyboard & Mouse",
                    ItemArabicName = "لوحة مفاتيح وفأرة لاسلكية ديل بريمير",
                    Barcode = "884116382205",
                    UnitName = "SET",
                    Quantity = 2.00m,
                    ExRate = 320.00m,
                    InRate = 368.00m,
                    GrossAmount = 640.00m,
                    DiscountPerc = 0m,
                    DiscountAmnt = 0m,
                    TaxableAmnt = 640.00m,
                    TaxPerc = 15.00m,
                    TaxAmount = 96.00m,
                    LineTotal = 736.00m,
                    Description = "Bluetooth 5.0 and 2.4GHz RF Connection"
                },
                new TransactionDtls
                {
                    SL = 3,
                    DtlsId = 3,
                    ItemCode = "MON-310",
                    ItemName = "Dell UltraSharp 27-inch 4K USB-C Hub Monitor (U2723QE)",
                    ItemArabicName = "شاشة ديل ألترا شارب 27 بوصة 4K",
                    Barcode = "884116382310",
                    UnitName = "UNIT",
                    Quantity = 2.00m,
                    ExRate = 2150.00m,
                    InRate = 2472.50m,
                    GrossAmount = 4300.00m,
                    DiscountPerc = 2.50m,
                    DiscountAmnt = 107.50m,
                    TaxableAmnt = 4192.50m,
                    TaxPerc = 15.00m,
                    TaxAmount = 628.88m,
                    LineTotal = 4821.38m,
                    Description = "IPS Black, 90W USB-C Power Delivery, RJ45"
                }
            };
        }

        // 4. Totals calculations
        decimal gross = details.Sum(x => x.GrossAmount);
        decimal discount = details.Sum(x => x.DiscountAmnt);
        decimal taxable = details.Sum(x => x.TaxableAmnt);
        decimal tax = details.Sum(x => x.TaxAmount);
        decimal net = taxable + tax;
        decimal totalQty = details.Sum(x => x.Quantity);
        decimal tendered = isThermal ? Math.Ceiling(net / 10) * 10 : net;
        decimal change = isThermal ? (tendered - net) : 0m;

        // Sample ZATCA QR Code string
        string sampleQr = "AQdBc2hwcm8CDzMwMDAxMjM0NTYwMDAwMxMUMjAyNi0xMC0yMVQxNDozMDowMFoFBDUxLjc1BgUxMy41MA==";

        // 5. Transaction Header
        var tranHdr = new TransactionHdr
        {
            HdrId = 1,
            HdrNo = isThermal ? "POS-2026-00432" : "INV-2026-00185",
            InvoiceDate = DateTime.Now.ToString("dd-MM-yyyy"),
            HdrDate = DateTime.Now,
            RefNo = isThermal ? "POS-432" : "PO-99812",
            GrossAmount = gross,
            LineDsicount = discount,
            TaxableAmount = taxable,
            TaxTotal = tax,
            NetAmount = net,
            GrandTotal = net,
            TotalQuantity = totalQty,
            Remarks = isThermal ? "Thank you for shopping with us! Please come again." : "Standard payment terms: 30 days. Goods received in good condition.",
            EnteredBy = isThermal ? "Cashier-01" : "Admin",
            SalesMan = isThermal ? "Cashier-01" : "Ahmed Al-Mansoor",
            CustomerName = party.Name,
            FromBranch = "Main Showroom",
            ToBranch = "Main Showroom",
            HijriDate = CrudHelper.ConvertToHijri(DateTime.Now),
            InvoiceQRCode = sampleQr,
            QRCode = isThermal ? "POS-2026-00432" : "INV-2026-00185",
            Tendered = tendered,
            Changes = change,
            VoucherType = isThermal ? "Sales_Invoice_Thermal" : "Sales_Invoice"
        };

        // 6. Footer Details
        var footerDetails = isThermal
            ? new List<FooterDetails>
            {
                new FooterDetails { Name = "Cash Tendered", ArabicName = "المبلغ المستلم", Value = tendered },
                new FooterDetails { Name = "Change Due", ArabicName = "المتبقي للعميل", Value = change }
            }
            : new List<FooterDetails>
            {
                new FooterDetails { Name = "Bank / Mada Card", ArabicName = "مدى / بطاقة مصرفية", Value = net }
            };

        // 7. Ensure Print Header properties
        var printHdr = new PrintDesignHdr
        {
            PrintDesignHdrId = hdr.PrintDesignHdrId,
            DesignName = string.IsNullOrEmpty(hdr.DesignName) ? (isThermal ? "POS Thermal Receipt" : "Standard Sales Invoice") : hdr.DesignName,
            VoucherType = isThermal ? "Sales_Invoice_Thermal" : "Sales_Invoice",
            FormType = "Invoice",
            PrintType = isThermal ? "Thermal" : "A4",
            IsDefault = hdr.IsDefault,
            PageWidth = hdr.PageWidth > 0 ? hdr.PageWidth : (isThermal ? 290 : 700),
            PageHeight = hdr.PageHeight > 0 ? hdr.PageHeight : (isThermal ? 1000 : 1000),
            LinesPerPage = hdr.LinesPerPage > 0 ? hdr.LinesPerPage : (isThermal ? 100 : 40),
            SpaceBtwnLines = hdr.SpaceBtwnLines > 0 ? hdr.SpaceBtwnLines : (isThermal ? 5 : 10),
            BagroundImagePath = hdr.BagroundImagePath,
            PrinterName = hdr.PrinterName,
            FK_BranchId = hdr.FK_BranchId
        };

        return new PrintParameterDTO(printHdr, controls, tranHdr, details, party, company, footerDetails);
    }
}
