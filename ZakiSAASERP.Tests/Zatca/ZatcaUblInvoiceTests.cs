using System.Text;
using System.Xml.Linq;
using ZakiSAASERP.ZatcaEngine.Enums;
using ZakiSAASERP.ZatcaEngine.Hashing;
using ZakiSAASERP.ZatcaEngine.Models;
using ZakiSAASERP.ZatcaEngine.Qr;
using ZakiSAASERP.ZatcaEngine.Ubl;

namespace ZakiSAASERP.Tests.Zatca;

public class ZatcaUblInvoiceTests
{
    private Invoice CreateSampleInvoice(string invoiceNumber = "INV-2026-001")
    {
        return new Invoice
        {
            InvoiceNumber = invoiceNumber,
            Uuid = "c2a96752-6a75-4b55-a01b-c7fb8a452ef3",
            IssueDate = new DateTime(2026, 4, 15, 14, 30, 0),
            Seller = new Party
            {
                Name = "Zaki Technology Ltd",
                VatNumber = "310123456700003",
                IdentificationNumber = "1010123456",
                IdentificationScheme = "CRN",
                Street = "King Fahd Road",
                BuildingNumber = "1234",
                City = "Riyadh",
                PostalCode = "12345",
                District = "Al Olaya",
                CountryCode = "SA"
            },
            Buyer = new Party
            {
                Name = "Al Amal Trading Co",
                VatNumber = "310987654300003",
                Street = "Tahlia Street",
                BuildingNumber = "5678",
                City = "Jeddah",
                PostalCode = "21452",
                District = "Al Rawdah",
                CountryCode = "SA"
            },
            Lines = new List<InvoiceLine>
            {
                new InvoiceLine
                {
                    Description = "ERP Cloud Subscription",
                    Quantity = 1,
                    UnitPrice = 1000m,
                    VatRate = 15m,
                    TaxCategory = "S",
                    LineTotal = 1000m,
                    VatAmount = 150m
                },
                new InvoiceLine
                {
                    Description = "Implementation Service",
                    Quantity = 2,
                    UnitPrice = 500m,
                    VatRate = 15m,
                    TaxCategory = "S",
                    LineTotal = 1000m,
                    VatAmount = 150m
                }
            },
            VatAmount = 300m,
            TotalAmount = 2300m
        };
    }

    [Fact]
    public void BuildInvoice_StandardTaxInvoice_GeneratesValidUblXml()
    {
        // Arrange
        var builder = new UblInvoiceBuilder();
        var invoice = CreateSampleInvoice();

        // Act
        string xml = builder.BuildInvoice(invoice, InvoiceType.StandardInvoice);

        // Assert
        Assert.NotNull(xml);
        var doc = XDocument.Parse(xml);
        var root = doc.Root;
        Assert.NotNull(root);
        Assert.Equal("Invoice", root.Name.LocalName);

        // Check Namespaces
        var cbc = UblNamespaces.cbc;
        var cac = UblNamespaces.cac;

        // ProfileID & ID
        Assert.Equal("reporting:1.0", root.Element(cbc + "ProfileID")?.Value);
        Assert.Equal("INV-2026-001", root.Element(cbc + "ID")?.Value);
        Assert.Equal("c2a96752-6a75-4b55-a01b-c7fb8a452ef3", root.Element(cbc + "UUID")?.Value);

        // Standard Invoice Type Code (388, name 0100000)
        var typeCodeElem = root.Element(cbc + "InvoiceTypeCode");
        Assert.NotNull(typeCodeElem);
        Assert.Equal("388", typeCodeElem.Value);
        Assert.Equal("0100000", typeCodeElem.Attribute("name")?.Value);

        // Currency
        Assert.Equal("SAR", root.Element(cbc + "DocumentCurrencyCode")?.Value);
        Assert.Equal("SAR", root.Element(cbc + "TaxCurrencyCode")?.Value);

        // Supplier Party
        var supplier = root.Element(cac + "AccountingSupplierParty");
        Assert.NotNull(supplier);
        var sellerVat = supplier.Element(cac + "Party")?.Element(cac + "PartyTaxScheme")?.Element(cbc + "CompanyID")?.Value;
        Assert.Equal("310123456700003", sellerVat);

        // Monetary Totals
        var legalTotal = root.Element(cac + "LegalMonetaryTotal");
        Assert.NotNull(legalTotal);
        Assert.Equal("2000.00", legalTotal.Element(cbc + "LineExtensionAmount")?.Value);
        Assert.Equal("2000.00", legalTotal.Element(cbc + "TaxExclusiveAmount")?.Value);
        Assert.Equal("2300.00", legalTotal.Element(cbc + "TaxInclusiveAmount")?.Value);
        Assert.Equal("2300.00", legalTotal.Element(cbc + "PayableAmount")?.Value);

        // Tax Total
        var taxTotal = root.Element(cac + "TaxTotal");
        Assert.NotNull(taxTotal);
        Assert.Equal("300.00", taxTotal.Element(cbc + "TaxAmount")?.Value);
    }

    [Fact]
    public void BuildInvoice_SimplifiedInvoice_UsesSimplifiedInvoiceTypeCode()
    {
        // Arrange
        var builder = new UblInvoiceBuilder();
        var invoice = CreateSampleInvoice();

        // Act
        string xml = builder.BuildInvoice(invoice, InvoiceType.SimplifiedInvoice);

        // Assert
        var doc = XDocument.Parse(xml);
        var typeCodeElem = doc.Root?.Element(UblNamespaces.cbc + "InvoiceTypeCode");
        Assert.NotNull(typeCodeElem);
        Assert.Equal("388", typeCodeElem.Value);
        Assert.Equal("0200000", typeCodeElem.Attribute("name")?.Value);
    }

    [Fact]
    public void BuildInvoice_CreditNote_IncludesBillingReferenceAndCode381()
    {
        // Arrange (Credit Note referencing original invoice INV-2026-001)
        var builder = new UblInvoiceBuilder();
        var invoice = CreateSampleInvoice("CRN-2026-001");
        invoice.BillingReferenceId = "INV-2026-001";

        // Act
        string xml = builder.BuildInvoice(invoice, InvoiceType.StandardCreditNote);

        // Assert
        var doc = XDocument.Parse(xml);
        var root = doc.Root!;
        var cbc = UblNamespaces.cbc;
        var cac = UblNamespaces.cac;

        // Code 381 for Credit Note
        var typeCodeElem = root.Element(cbc + "InvoiceTypeCode");
        Assert.Equal("381", typeCodeElem?.Value);

        // Mandatory BR-KSA-56 BillingReference
        var billingRef = root.Element(cac + "BillingReference");
        Assert.NotNull(billingRef);
        var refId = billingRef.Element(cac + "InvoiceDocumentReference")?.Element(cbc + "ID")?.Value;
        Assert.Equal("INV-2026-001", refId);
    }

    [Fact]
    public void BuildInvoice_DebitNote_IncludesBillingReferenceAndCode383()
    {
        // Arrange (Debit Note referencing original invoice INV-2026-001)
        var builder = new UblInvoiceBuilder();
        var invoice = CreateSampleInvoice("DBN-2026-001");
        invoice.BillingReferenceId = "INV-2026-001";

        // Act
        string xml = builder.BuildInvoice(invoice, InvoiceType.StandardDebitNote);

        // Assert
        var doc = XDocument.Parse(xml);
        var root = doc.Root!;
        var cbc = UblNamespaces.cbc;
        var cac = UblNamespaces.cac;

        // Code 383 for Debit Note
        var typeCodeElem = root.Element(cbc + "InvoiceTypeCode");
        Assert.Equal("383", typeCodeElem?.Value);

        // Mandatory BillingReference
        var billingRef = root.Element(cac + "BillingReference");
        Assert.NotNull(billingRef);
        var refId = billingRef.Element(cac + "InvoiceDocumentReference")?.Element(cbc + "ID")?.Value;
        Assert.Equal("INV-2026-001", refId);
    }

    [Fact]
    public void BuildInvoice_WithZeroRatedAndExemptItems_GeneratesTaxSubtotalsProperly()
    {
        // Arrange: Mixed tax items
        var invoice = CreateSampleInvoice();
        invoice.Lines = new List<InvoiceLine>
        {
            new InvoiceLine
            {
                Description = "Standard Taxed Item",
                Quantity = 1, UnitPrice = 100m, LineTotal = 100m,
                VatRate = 15m, TaxCategory = "S", VatAmount = 15m
            },
            new InvoiceLine
            {
                Description = "Zero Rated Medicine",
                Quantity = 1, UnitPrice = 50m, LineTotal = 50m,
                VatRate = 0m, TaxCategory = "Z", VatAmount = 0m,
                TaxExemptionReasonCode = "VATEX-SA-29"
            },
            new InvoiceLine
            {
                Description = "Exempt Financial Service",
                Quantity = 1, UnitPrice = 200m, LineTotal = 200m,
                VatRate = 0m, TaxCategory = "E", VatAmount = 0m,
                TaxExemptionReasonCode = "VATEX-SA-30"
            }
        };
        invoice.VatAmount = 15m;
        invoice.TotalAmount = 365m;

        var builder = new UblInvoiceBuilder();

        // Act
        string xml = builder.BuildInvoice(invoice, InvoiceType.StandardInvoice);

        // Assert
        var doc = XDocument.Parse(xml);
        var taxSubtotals = doc.Root!.Elements(UblNamespaces.cac + "TaxTotal")
            .SelectMany(t => t.Elements(UblNamespaces.cac + "TaxSubtotal")).ToList();

        Assert.Equal(3, taxSubtotals.Count);
        var categories = taxSubtotals
            .Select(s => s.Element(UblNamespaces.cac + "TaxCategory")?.Element(UblNamespaces.cbc + "ID")?.Value)
            .OrderBy(x => x).ToList();

        Assert.Contains("S", categories);
        Assert.Contains("Z", categories);
        Assert.Contains("E", categories);
    }

    [Fact]
    public void TlvBuilder_EncodesAndDecodes_Tags1To5Correctly()
    {
        // Arrange
        string seller = "مؤسسة زكي للحلول البرمجية";
        string vat = "310123456700003";
        string timestamp = "2026-04-15T14:30:00";
        string total = "115.00";
        string vatAmount = "15.00";

        var builder = new TlvBuilder();
        builder.AddTag(1, seller);
        builder.AddTag(2, vat);
        builder.AddTag(3, timestamp);
        builder.AddTag(4, total);
        builder.AddTag(5, vatAmount);

        // Act
        byte[] tlvBytes = builder.Build();
        string base64 = builder.BuildBase64();

        // Assert: Parse raw TLV bytes back to verify compliance
        Assert.NotEmpty(tlvBytes);
        Assert.NotEmpty(base64);

        int index = 0;
        var decodedTags = new Dictionary<int, string>();
        while (index < tlvBytes.Length)
        {
            int tag = tlvBytes[index++];
            int length = tlvBytes[index++];
            string val = Encoding.UTF8.GetString(tlvBytes, index, length);
            index += length;
            decodedTags[tag] = val;
        }

        Assert.Equal(seller, decodedTags[1]);
        Assert.Equal(vat, decodedTags[2]);
        Assert.Equal(timestamp, decodedTags[3]);
        Assert.Equal(total, decodedTags[4]);
        Assert.Equal(vatAmount, decodedTags[5]);
    }

    [Fact]
    public void TlvBuilder_MultiByteLength_EncodesPayLoadsGreaterThan127Bytes()
    {
        // Arrange: A long string payload > 127 bytes
        string longPayload = new string('A', 150);
        var builder = new TlvBuilder();
        builder.AddTag(1, longPayload);

        // Act
        byte[] tlvBytes = builder.Build();

        // Assert: Tag is 1, Length byte 1 has high bit set (0x81 for 1 length byte), then length 150
        Assert.Equal(1, tlvBytes[0]);
        Assert.Equal(0x81, tlvBytes[1]);
        Assert.Equal(150, tlvBytes[2]);
        Assert.Equal(153, tlvBytes.Length); // 1 tag + 2 length + 150 value
    }

    [Fact]
    public void InvoiceHashGenerator_ComputesSha256DigestDeterministically()
    {
        // Arrange
        var builder = new UblInvoiceBuilder();
        var invoice = CreateSampleInvoice();
        string xml = builder.BuildInvoice(invoice, InvoiceType.StandardInvoice);

        var canonicalizer = new XmlCanonicalizer();
        var doc = canonicalizer.LoadXml(xml);
        var cleanDoc = canonicalizer.RemoveSignatureAndExtensions(doc);
        byte[] c14nBytes = canonicalizer.Canonicalize(cleanDoc);

        var hashGen = new InvoiceHashGenerator();

        // Act
        string hash1 = hashGen.GenerateHashBase64(c14nBytes);
        string hash2 = hashGen.GenerateHashBase64(c14nBytes);

        // Assert
        Assert.NotNull(hash1);
        Assert.Equal(hash1, hash2);
        // Base64 of a 32-byte SHA256 digest has exactly 44 characters
        Assert.Equal(44, hash1.Length);
        Assert.EndsWith("=", hash1);
    }
}
