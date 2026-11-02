using AshproSAASERP.ZatcaEngine.Hashing;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace AshproSAASERP.ZatcaEngine.Crypto;

public class XadesSigner
{
    static XadesSigner()
    {
        Ecdsa256SignatureDescription.Register();
    }

    public string SignXml(
        string xml,
        string privateKeyPem,
        string certificateBase64,
        string? invoiceHash = null,
        DateTime? issueDate = null)
    {
        Ecdsa256SignatureDescription.Register();

        var (certBytes, cleanCert) = ZatcaCertificateHelper.NormalizeCertificate(certificateBase64);
        if (certBytes == null || string.IsNullOrEmpty(cleanCert))
        {
            throw new ArgumentException("A valid X.509 certificate must be provided for signing.", nameof(certificateBase64));
        }

        var cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadCertificate(certBytes);

        if (string.IsNullOrEmpty(invoiceHash))
        {
            var canonicalizer = new XmlCanonicalizer();
            var cleanDoc = canonicalizer.LoadXml(xml);
            cleanDoc = canonicalizer.RemoveSignatureAndExtensions(cleanDoc);
            var canonBytes = canonicalizer.Canonicalize(cleanDoc);
            invoiceHash = Convert.ToBase64String(SHA256.HashData(canonBytes));
        }

        string digitalSignature = GenerateDigitalSignature(invoiceHash, privateKeyPem);

        string issuerName = cert.IssuerName.Name;
        string serialNumber = BigInteger.Parse("00" + cert.SerialNumber, System.Globalization.NumberStyles.HexNumber).ToString();

        // Calculate certificate digest: Base64(Hex(SHA256(cleanCert)))
        byte[] certHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(cleanCert));
        string certHashHex = Convert.ToHexStringLower(certHashBytes);
        string certDigest = Convert.ToBase64String(Encoding.UTF8.GetBytes(certHashHex));

        var timestampDate = issueDate ?? (cert.NotBefore.Year >= 2000 ? cert.NotBefore : DateTime.Now);
        if (timestampDate.Year < 2000) timestampDate = DateTime.Now;
        string signingTime = timestampDate.ToString("yyyy-MM-ddTHH:mm:ss");

        // Build UBLExtensions containing SignedProperties exactly matching ZATCA specification
        string ublExtensionsXml = $@"<ext:UBLExtensions xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
    <ext:UBLExtension>
        <ext:ExtensionURI>urn:oasis:names:specification:ubl:dsig:enveloped:xades</ext:ExtensionURI>
        <ext:ExtensionContent>
            <sig:UBLDocumentSignatures xmlns:sac=""urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2"" xmlns:sbc=""urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2"" xmlns:sig=""urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2"">
                <sac:SignatureInformation>
                    <cbc:ID xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">urn:oasis:names:specification:ubl:signature:1</cbc:ID>
                    <sbc:ReferencedSignatureID>urn:oasis:names:specification:ubl:signature:Invoice</sbc:ReferencedSignatureID>
                    <ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" Id=""signature"">
                        <ds:SignedInfo>
                            <ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2006/12/xml-c14n11""/>
                            <ds:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256""/>
                            <ds:Reference Id=""invoiceSignedData"" URI="""">
                                <ds:Transforms>
                                    <ds:Transform Algorithm=""http://www.w3.org/TR/1999/REC-xpath-19991116"">
                                        <ds:XPath>not(//ancestor-or-self::ext:UBLExtensions)</ds:XPath>
                                    </ds:Transform>
                                    <ds:Transform Algorithm=""http://www.w3.org/TR/1999/REC-xpath-19991116"">
                                        <ds:XPath>not(//ancestor-or-self::cac:Signature)</ds:XPath>
                                    </ds:Transform>
                                    <ds:Transform Algorithm=""http://www.w3.org/TR/1999/REC-xpath-19991116"">
                                        <ds:XPath>not(//ancestor-or-self::cac:AdditionalDocumentReference[cbc:ID='QR'])</ds:XPath>
                                    </ds:Transform>
                                    <ds:Transform Algorithm=""http://www.w3.org/2006/12/xml-c14n11""/>
                                </ds:Transforms>
                                <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/>
                                <ds:DigestValue>{invoiceHash}</ds:DigestValue>
                            </ds:Reference>
                            <ds:Reference Type=""http://www.w3.org/2000/09/xmldsig#SignatureProperties"" URI=""#xadesSignedProperties"">
                                <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/>
                                <ds:DigestValue></ds:DigestValue>
                            </ds:Reference>
                        </ds:SignedInfo>
                        <ds:SignatureValue>{digitalSignature}</ds:SignatureValue>
                        <ds:KeyInfo>
                            <ds:X509Data>
                                <ds:X509Certificate>{cleanCert}</ds:X509Certificate>
                            </ds:X509Data>
                        </ds:KeyInfo>
                        <ds:Object>
                            <xades:QualifyingProperties xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"" Target=""signature"">
                                <xades:SignedProperties Id=""xadesSignedProperties"">
                                    <xades:SignedSignatureProperties>
                                        <xades:SigningTime>{signingTime}</xades:SigningTime>
                                        <xades:SigningCertificate>
                                            <xades:Cert>
                                                <xades:CertDigest>
                                                    <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/>
                                                    <ds:DigestValue>{certDigest}</ds:DigestValue>
                                                </xades:CertDigest>
                                                <xades:IssuerSerial>
                                                    <ds:X509IssuerName>{issuerName}</ds:X509IssuerName>
                                                    <ds:X509SerialNumber>{serialNumber}</ds:X509SerialNumber>
                                                </xades:IssuerSerial>
                                            </xades:Cert>
                                        </xades:SigningCertificate>
                                    </xades:SignedSignatureProperties>
                                </xades:SignedProperties>
                            </xades:QualifyingProperties>
                        </ds:Object>
                    </ds:Signature>
                </sac:SignatureInformation>
            </sig:UBLDocumentSignatures>
        </ext:ExtensionContent>
    </ext:UBLExtension>
</ext:UBLExtensions>";

        var extDoc = new XmlDocument();
        extDoc.PreserveWhitespace = true;
        extDoc.LoadXml(ublExtensionsXml);

        var nsmgrExt = new XmlNamespaceManager(extDoc.NameTable);
        nsmgrExt.AddNamespace("xades", "http://uri.etsi.org/01903/v1.3.2#");
        nsmgrExt.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

        var spNode = extDoc.SelectSingleNode("//xades:SignedProperties", nsmgrExt);
        if (spNode != null)
        {
            using var msIn = new MemoryStream(Encoding.UTF8.GetBytes(spNode.OuterXml));
            var spTransform = new XmlDsigC14NTransform(false);
            spTransform.LoadInput(msIn);
            using var msOut = (MemoryStream)spTransform.GetOutput();
            string canonSpXml = Encoding.UTF8.GetString(msOut.ToArray());
            canonSpXml = canonSpXml.Replace("></ds:DigestMethod>", "/>").Replace(" />", "/>").Replace("\r", "");

            byte[] spHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonSpXml));
            string spHashHex = Convert.ToHexStringLower(spHashBytes);
            string signedPropertiesDigest = Convert.ToBase64String(Encoding.UTF8.GetBytes(spHashHex));

            var spDigestNode = extDoc.SelectSingleNode("//ds:Reference[@URI='#xadesSignedProperties']/ds:DigestValue", nsmgrExt);
            if (spDigestNode != null)
            {
                spDigestNode.InnerText = signedPropertiesDigest;
            }
        }

        var xmlDoc = new XmlDocument();
        xmlDoc.PreserveWhitespace = true;
        xmlDoc.LoadXml(xml);

        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

        var existingExt = xmlDoc.SelectSingleNode("//ext:UBLExtensions", nsmgr);
        var importedExt = xmlDoc.ImportNode(extDoc.DocumentElement, true);

        if (existingExt != null)
        {
            existingExt.ParentNode?.ReplaceChild(importedExt, existingExt);
        }
        else
        {
            xmlDoc.DocumentElement?.PrependChild(importedExt);
        }

        return xmlDoc.OuterXml;
    }

    public string GenerateDigitalSignature(string invoiceHashBase64, string privateKeyPem)
    {
        byte[] hashBytes = Convert.FromBase64String(invoiceHashBase64);
        var key = LoadPrivateKey(privateKeyPem);

        ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
        signer.Init(true, key);
        signer.BlockUpdate(hashBytes, 0, hashBytes.Length);
        byte[] signatureBytes = signer.GenerateSignature();
        return Convert.ToBase64String(signatureBytes);
    }

    public string EmbedQrCode(string xml, string qrBase64)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(xml);

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

        var existingQrNode = doc.SelectSingleNode("//cac:AdditionalDocumentReference[cbc:ID='QR']", nsmgr);
        existingQrNode?.ParentNode?.RemoveChild(existingQrNode);

        var qrElem = doc.CreateElement("cac", "AdditionalDocumentReference", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        var idElem = doc.CreateElement("cbc", "ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        idElem.InnerText = "QR";
        qrElem.AppendChild(idElem);

        var attachmentElem = doc.CreateElement("cac", "Attachment", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        var binaryElem = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        binaryElem.SetAttribute("mimeCode", "text/plain");
        binaryElem.InnerText = qrBase64;
        attachmentElem.AppendChild(binaryElem);
        qrElem.AppendChild(attachmentElem);

        var pihNode = doc.SelectSingleNode("//cac:AdditionalDocumentReference[cbc:ID='PIH']", nsmgr);
        if (pihNode != null)
        {
            pihNode.ParentNode?.InsertAfter(qrElem, pihNode);
        }
        else
        {
            var sigNode = doc.SelectSingleNode("//cac:Signature", nsmgr);
            if (sigNode != null)
            {
                sigNode.ParentNode?.InsertBefore(qrElem, sigNode);
            }
            else
            {
                var supplierNode = doc.SelectSingleNode("//cac:AccountingSupplierParty", nsmgr);
                if (supplierNode != null)
                {
                    supplierNode.ParentNode?.InsertBefore(qrElem, supplierNode);
                }
                else
                {
                    doc.DocumentElement?.AppendChild(qrElem);
                }
            }
        }

        return doc.OuterXml;
    }

    private AsymmetricKeyParameter LoadPrivateKey(string privateKeyPem)
    {
        var cleanPem = (privateKeyPem ?? "").Trim();
        if (!cleanPem.Contains("-----BEGIN EC PRIVATE KEY-----") &&
            !cleanPem.Contains("-----END EC PRIVATE KEY-----") &&
            !cleanPem.Contains("-----BEGIN PRIVATE KEY-----") &&
            !cleanPem.Contains("-----END PRIVATE KEY-----"))
        {
            cleanPem = "-----BEGIN EC PRIVATE KEY-----\n" + cleanPem + "\n-----END EC PRIVATE KEY-----";
        }

        using var reader = new StringReader(cleanPem);
        var pemReader = new PemReader(reader);
        var keyObject = pemReader.ReadObject();

        if (keyObject is AsymmetricCipherKeyPair keyPair)
        {
            return keyPair.Private;
        }
        else if (keyObject is AsymmetricKeyParameter keyParam)
        {
            return keyParam;
        }
        else
        {
            throw new InvalidOperationException("Unsupported or invalid ECDSA private key PEM structure.");
        }
    }
}
