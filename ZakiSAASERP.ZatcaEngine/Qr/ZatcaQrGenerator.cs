using ZakiSAASERP.ZatcaEngine.Crypto;
using ZakiSAASERP.ZatcaEngine.Models;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace ZakiSAASERP.ZatcaEngine.Qr;

public class ZatcaQrGenerator
{
    public string GeneratePhase2Qr(
        Invoice invoice,
        string invoiceHashBase64,
        string signedXml,
        string? certificateBase64 = null,
        bool isSimplified = true)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        if (!string.IsNullOrWhiteSpace(signedXml))
        {
            try { doc.LoadXml(signedXml); } catch { }
        }

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        nsmgr.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

        // 1. Seller Name (Tag 1)
        var sellerName = doc.SelectSingleNode("//cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName", nsmgr)?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(sellerName))
            sellerName = invoice.Seller?.Name?.Trim() ?? string.Empty;

        // 2. Seller VAT Number (Tag 2)
        var vatNumber = doc.SelectSingleNode("//cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID", nsmgr)?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(vatNumber))
            vatNumber = invoice.Seller?.VatNumber?.Trim() ?? string.Empty;

        // 3. Issue Date & Time (Tag 3: YYYY-MM-DDTHH:MM:SS)
        var issueDateXml = doc.SelectSingleNode("//cbc:IssueDate", nsmgr)?.InnerText?.Trim();
        var issueTimeXml = doc.SelectSingleNode("//cbc:IssueTime", nsmgr)?.InnerText?.Trim();
        string timestamp;
        if (!string.IsNullOrWhiteSpace(issueDateXml) && !string.IsNullOrWhiteSpace(issueTimeXml))
        {
            if (issueTimeXml.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
                issueTimeXml = issueTimeXml.TrimEnd('Z', 'z');

            timestamp = $"{issueDateXml}T{issueTimeXml}";
        }
        else
        {
            var issueDate = invoice.IssueDate.Year >= 2000 ? invoice.IssueDate : DateTime.Now;
            timestamp = issueDate.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        // 4. Invoice Total (Tag 4: Tax inclusive amount)
        var totalAmountXml = doc.SelectSingleNode("//cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount", nsmgr)?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(totalAmountXml))
            totalAmountXml = doc.SelectSingleNode("//cac:LegalMonetaryTotal/cbc:PayableAmount", nsmgr)?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(totalAmountXml))
            totalAmountXml = invoice.TotalAmount.ToString("0.00");

        // 5. VAT Total (Tag 5)
        var vatAmountXml = doc.SelectSingleNode("//cac:TaxTotal/cbc:TaxAmount", nsmgr)?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(vatAmountXml))
            vatAmountXml = invoice.VatAmount.ToString("0.00");

        // 6. Invoice Hash (Tag 6)
        var hashXml = doc.SelectSingleNode("//ds:Reference[@Id='invoiceSignedData']/ds:DigestValue", nsmgr)?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(hashXml))
            hashXml = invoiceHashBase64 ?? string.Empty;

        // 7. Signature Value (Tag 7)
        var signatureValue = doc.SelectSingleNode("//ds:SignatureValue", nsmgr)?.InnerText?.Trim();
        if (string.IsNullOrWhiteSpace(signatureValue))
            signatureValue = ExtractSignatureValue(signedXml);

        // Certificate: extract for Tag 8 and Tag 9
        var certRaw = !string.IsNullOrWhiteSpace(certificateBase64)
            ? certificateBase64
            : (doc.SelectSingleNode("//ds:X509Certificate", nsmgr)?.InnerText?.Trim() ?? ExtractCertificate(signedXml));

        byte[]? publicKeyBytes = null;
        byte[]? certSignatureBytes = null;

        if (!string.IsNullOrWhiteSpace(certRaw))
        {
            var (derBytes, _) = ZatcaCertificateHelper.NormalizeCertificate(certRaw);
            if (derBytes != null && derBytes.Length > 0)
            {
                var (pub, sig) = ZatcaCertificateHelper.ExtractPublicKeyAndSignature(derBytes);
                publicKeyBytes = pub;
                certSignatureBytes = sig;
            }
        }

        if (publicKeyBytes == null || publicKeyBytes.Length == 0)
        {
            throw new InvalidOperationException("Failed to extract SubjectPublicKeyInfo (Tag 8) from certificate for ZATCA QR code generation.");
        }

        if (isSimplified && (certSignatureBytes == null || certSignatureBytes.Length == 0))
        {
            throw new InvalidOperationException("Failed to extract Certificate Signature (Tag 9) from certificate for ZATCA simplified invoice QR code generation.");
        }

        var tlv = new TlvBuilder();
        tlv.AddTag(1, sellerName);
        tlv.AddTag(2, vatNumber);
        tlv.AddTag(3, timestamp);
        tlv.AddTag(4, totalAmountXml);
        tlv.AddTag(5, vatAmountXml);
        tlv.AddTag(6, hashXml);
        tlv.AddTag(7, signatureValue ?? string.Empty);
        tlv.AddTag(8, publicKeyBytes);

        if (isSimplified && certSignatureBytes != null && certSignatureBytes.Length > 0)
        {
            tlv.AddTag(9, certSignatureBytes);
        }

        return tlv.BuildBase64();
    }

    public string GeneratePhase2Qr(
        Invoice invoice,
        string invoiceHashBase64,
        string signedXml)
    {
        return GeneratePhase2Qr(invoice, invoiceHashBase64, signedXml, null, true);
    }

    private string ExtractSignatureValue(string signedXml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(signedXml))
                return string.Empty;

            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(signedXml);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            var node = doc.SelectSingleNode("//ds:SignatureValue", nsmgr);
            if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                return node.InnerText.Trim();
            }

            var fallbackNode = doc.GetElementsByTagName("SignatureValue");
            if (fallbackNode.Count > 0 && !string.IsNullOrWhiteSpace(fallbackNode[0]?.InnerText))
            {
                return fallbackNode[0]!.InnerText.Trim();
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractCertificate(string signedXml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(signedXml))
                return string.Empty;

            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(signedXml);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            var node = doc.SelectSingleNode("//ds:X509Certificate", nsmgr);
            if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                return node.InnerText.Trim();
            }

            var fallbackNode = doc.GetElementsByTagName("X509Certificate");
            if (fallbackNode.Count > 0 && !string.IsNullOrWhiteSpace(fallbackNode[0]?.InnerText))
            {
                return fallbackNode[0]!.InnerText.Trim();
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
