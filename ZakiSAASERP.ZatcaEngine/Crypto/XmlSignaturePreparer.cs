using System.Xml;

namespace ZakiSAASERP.ZatcaEngine.Crypto;

public class XmlSignaturePreparer
{
    public XmlDocument PrepareForSigning(string xml)
    {
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(xml);

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("ext",
            "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
        nsmgr.AddNamespace("ds",
            "http://www.w3.org/2000/09/xmldsig#");

        // Remove existing Signature if any
        var sigNode = doc.SelectSingleNode("//ds:Signature", nsmgr);
        sigNode?.ParentNode?.RemoveChild(sigNode);

        return doc;
    }
}
