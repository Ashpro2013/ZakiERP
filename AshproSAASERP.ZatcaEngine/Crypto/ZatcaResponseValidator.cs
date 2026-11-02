using System.Security.Cryptography.Xml;
using System.Xml;

namespace AshproSAASERP.ZatcaEngine.Crypto;

public class ZatcaResponseValidator
{
    static ZatcaResponseValidator()
    {
        Ecdsa256SignatureDescription.Register();
    }

    public bool ValidateResponseSignature(
        string responseXml,
        string certificateBase64)
    {
        Ecdsa256SignatureDescription.Register();

        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.LoadXml(responseXml);

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("ds",
            SignedXml.XmlDsigNamespaceUrl);

        var signatureNode =
            doc.SelectSingleNode("//ds:Signature", nsmgr) as XmlElement;

        if (signatureNode == null)
            return false;

        var signedXml = new SignedXml(doc);
        signedXml.LoadXml(signatureNode);

        var certBytes = Convert.FromBase64String(certificateBase64);
        var cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadCertificate(certBytes);

        return signedXml.CheckSignature(cert, true);
    }
}
