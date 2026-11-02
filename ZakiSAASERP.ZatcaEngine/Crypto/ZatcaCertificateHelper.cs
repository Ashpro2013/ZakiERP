using System.Formats.Asn1;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace ZakiSAASERP.ZatcaEngine.Crypto;

public static class ZatcaCertificateHelper
{
    /// <summary>
    /// Normalizes any certificate input (raw PEM, Base64 DER, double-Base64 PEM, double-Base64 DER, or quoted string)
    /// into pure DER bytes (ASN.1 SEQUENCE starting with 0x30) and a clean Base64 string of the DER bytes (starts with MII...).
    /// </summary>
    public static (byte[]? DerBytes, string? CleanDerBase64) NormalizeCertificate(string? rawCert)
    {
        if (string.IsNullOrWhiteSpace(rawCert))
            return (null, null);

        string text = rawCert.Trim().Trim('\"', '\'').Trim();

        // Unwrap up to 3 layers of encoding (e.g. double-Base64, PEM-in-Base64, etc.)
        for (int i = 0; i < 3; i++)
        {
            // If it's a PEM string, strip headers and footers
            if (text.Contains("-----BEGIN CERTIFICATE-----", StringComparison.OrdinalIgnoreCase))
            {
                var startIdx = text.IndexOf("-----BEGIN CERTIFICATE-----", StringComparison.OrdinalIgnoreCase) + "-----BEGIN CERTIFICATE-----".Length;
                var endIdx = text.IndexOf("-----END CERTIFICATE-----", StringComparison.OrdinalIgnoreCase);
                if (endIdx > startIdx)
                {
                    text = text.Substring(startIdx, endIdx - startIdx);
                }
                else
                {
                    text = text.Replace("-----BEGIN CERTIFICATE-----", "", StringComparison.OrdinalIgnoreCase)
                               .Replace("-----END CERTIFICATE-----", "", StringComparison.OrdinalIgnoreCase);
                }
            }

            text = text.Replace("\r", "").Replace("\n", "").Replace(" ", "").Trim();

            if (string.IsNullOrWhiteSpace(text))
                return (null, null);

            byte[] decoded;
            try
            {
                decoded = Convert.FromBase64String(text);
            }
            catch
            {
                // Not valid Base64
                return (null, null);
            }

            // Case 1: decoded bytes are ASN.1 DER SEQUENCE (starts with 0x30)
            if (decoded.Length > 4 && decoded[0] == 0x30)
            {
                return (decoded, text);
            }

            // Case 2: decoded bytes are an ASCII / UTF-8 string (e.g. another layer of Base64 or PEM)
            try
            {
                string innerStr = Encoding.UTF8.GetString(decoded).Trim();
                if (!string.IsNullOrWhiteSpace(innerStr))
                {
                    text = innerStr;
                    continue;
                }
            }
            catch
            {
                // Not UTF-8 string
            }

            // Fallback if decoded has length > 0
            return (decoded, text);
        }

        return (null, null);
    }

    /// <summary>
    /// Safely extracts SubjectPublicKeyInfo bytes (Tag 8) and Certificate Signature bytes (Tag 9)
    /// without throwing NullReferenceException.
    /// </summary>
    public static (byte[]? PublicKeyBytes, byte[]? CertSignatureBytes) ExtractPublicKeyAndSignature(byte[] derBytes)
    {
        if (derBytes == null || derBytes.Length == 0)
            return (null, null);

        byte[]? publicKeyBytes = null;
        byte[]? certSignatureBytes = null;

        // Method 1: Bouncy Castle parser (primary)
        try
        {
            var parser = new X509CertificateParser();
            var bcCert = parser.ReadCertificate(derBytes);
            if (bcCert != null)
            {
                certSignatureBytes = bcCert.GetSignature();
                var spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(bcCert.GetPublicKey());
                publicKeyBytes = spki.GetEncoded();
            }
        }
        catch { }

        // Method 2: .NET X509CertificateLoader / DotNetUtilities fallback
        if (publicKeyBytes == null || certSignatureBytes == null)
        {
            try
            {
                var dotNetCert = X509CertificateLoader.LoadCertificate(derBytes);
                if (dotNetCert != null)
                {
                    if (publicKeyBytes == null)
                    {
                        publicKeyBytes = dotNetCert.PublicKey.ExportSubjectPublicKeyInfo();
                    }

                    if (certSignatureBytes == null)
                    {
                        try
                        {
                            var bcCert = DotNetUtilities.FromX509Certificate(dotNetCert);
                            if (bcCert != null)
                            {
                                certSignatureBytes = bcCert.GetSignature();
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        // Method 3: Direct ITU-T X.509 RFC 5280 ASN.1 sequence extraction for Signature bytes (Tag 9)
        if (certSignatureBytes == null)
        {
            try
            {
                var reader = new AsnReader(derBytes, AsnEncodingRules.DER);
                var certSeq = reader.ReadSequence();
                certSeq.ReadSequence(); // Skip tbsCertificate
                certSeq.ReadSequence(); // Skip signatureAlgorithm
                certSignatureBytes = certSeq.ReadBitString(out int _);
            }
            catch { }
        }

        return (publicKeyBytes, certSignatureBytes);
    }
}
