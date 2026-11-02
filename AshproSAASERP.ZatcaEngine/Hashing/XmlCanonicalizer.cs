using System.Security.Cryptography.Xml;
using System.Xml;

namespace AshproSAASERP.ZatcaEngine.Hashing;

public class XmlCanonicalizer
{
    public XmlDocument LoadXml(string xml)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(xml);

        if (!doc.PreserveWhitespace)
            throw new Exception("Whitespace must be preserved.");

        return doc;
    }

    public XmlDocument RemoveSignatureAndExtensions(XmlDocument doc)
    {
        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
        nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        nsmgr.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

        // 1. Remove UBLExtensions
        var extNodes = doc.SelectNodes("//*[local-name()='UBLExtensions']");
        if (extNodes != null)
        {
            foreach (XmlNode node in extNodes)
                node.ParentNode?.RemoveChild(node);
        }

        // 2. Remove cac:AdditionalDocumentReference where cbc:ID = 'QR'
        var qrNodes = doc.SelectNodes("//*[local-name()='AdditionalDocumentReference'][*[local-name()='ID' and normalize-space(text())='QR']]");
        if (qrNodes != null)
        {
            foreach (XmlNode node in qrNodes)
                node.ParentNode?.RemoveChild(node);
        }

        // 3. Remove cac:Signature and ds:Signature
        var sigNodes = doc.SelectNodes("//*[local-name()='Signature']");
        if (sigNodes != null)
        {
            foreach (XmlNode node in sigNodes)
                node.ParentNode?.RemoveChild(node);
        }

        return doc;
    }

    public byte[] Canonicalize(XmlDocument doc)
    {
        var transform = new XmlDsigC14NTransform(false);
        transform.LoadInput(doc);

        using var stream = (Stream)transform.GetOutput(typeof(Stream));
        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        return ms.ToArray();
    }
}
