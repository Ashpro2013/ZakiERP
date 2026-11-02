using AshproSAASERP.ZatcaEngine.Models;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace AshproSAASERP.ZatcaEngine.Onboarding;

public class CsrGenerator
{
    public Task<CsrResult> GenerateAsync(CsrRequest request)
    {
        try
        {
            // Auto-format serial number if needed
            if (!string.IsNullOrWhiteSpace(request.SerialNumber) && !request.SerialNumber.Contains("|"))
            {
                request.SerialNumber = $"1-ASHPRO|2-ERP|3-{request.SerialNumber.Trim()}";
            }
            else if (string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                request.SerialNumber = $"1-ASHPRO|2-ERP|3-{Guid.NewGuid():D}";
            }

            if (string.IsNullOrWhiteSpace(request.CountryCode))
            {
                request.CountryCode = "SA";
            }

            // Input validation matching ZATCA rules ported from ZatcaIntegrationSDK
            var (isValid, validationErrors) = ValidateCsrInputs(request);
            if (!isValid)
            {
                return Task.FromResult(new CsrResult
                {
                    IsSuccess = false,
                    ErrorMessage = string.Join("\n", validationErrors)
                });
            }

            // Generate secp256k1 key pair
            // Generate secp256k1 key pair using named curve OID (secp256k1 / 1.3.132.0.10)
            // CRITICAL: Must use SecObjectIdentifiers.SecP256k1 so public key uses namedCurve OID instead of explicit parameters
            var keyGen = new ECKeyPairGenerator();
            keyGen.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP256k1, new SecureRandom()));
            AsymmetricCipherKeyPair keyPair = keyGen.GenerateKeyPair();

            // Subject DN (CN, O, OU, C reversed to C, OU, O, CN according to ZATCA specifications)
            var subjectAttributes = new Dictionary<DerObjectIdentifier, string>
            {
                { X509Name.CN, request.CommonName.Trim() },
                { X509Name.O, request.OrganizationName.Trim() },
                { X509Name.OU, (!string.IsNullOrWhiteSpace(request.OrganizationUnitName) ? request.OrganizationUnitName : request.OrganizationName).Trim() },
                { X509Name.C, (!string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode : "SA").Trim() }
            };

            var subjectKeys = subjectAttributes.Keys.Reverse().ToList();
            var subject = new X509Name(subjectKeys, subjectAttributes);

            // Extensions Generator for ZATCA Phase 2
            var extGenerator = new X509ExtensionsGenerator();

            // 1. Certificate Template Name extension (OID 1.3.6.1.4.1.311.20.2)
            string env = request.Environment?.Trim().ToLowerInvariant() ?? "";
            string certificateTemplateName = env switch
            {
                "production" => "ZATCA-Code-Signing",
                "simulation" => "PREZATCA-Code-Signing",
                "developer" => "TSTZATCA-Code-Signing",
                "sandbox" => "TSTZATCA-Code-Signing",
                "test" => "TSTZATCA-Code-Signing",
                _ => "TSTZATCA-Code-Signing"
            };

            // Using DisplayText(2, templateName) which encodes as DerUtf8String matching ZatcaIntegrationSDK
            extGenerator.AddExtension(
                new DerObjectIdentifier("1.3.6.1.4.1.311.20.2"),
                false,
                new DisplayText(2, certificateTemplateName));

            // 2. Subject Alternative Name (SAN) extension (dirName)
            // ZATCA Phase 2 expects SAN with directoryName containing:
            // 2.5.4.4 (SN / Surname): EGS Serial Number
            // 0.9.2342.19200300.100.1.1 (UID): 15-digit VAT Number
            // 2.5.4.12 (T / Title): 4-digit Invoice Type
            // 2.5.4.26 (Registered Address): Location
            // 2.5.4.15 (Business Category): Industry
            string serialNumber = request.SerialNumber.Trim();
            string vatNumber = request.OrganizationIdentifier.Trim();
            string invoiceType = !string.IsNullOrWhiteSpace(request.InvoiceType) ? request.InvoiceType.Trim() : "1100";
            string location = !string.IsNullOrWhiteSpace(request.Location) ? request.Location.Trim() : "Riyadh";
            string businessCategory = !string.IsNullOrWhiteSpace(request.BusinessCategory) ? request.BusinessCategory.Trim() : "IT";

            var sanDict = new Dictionary<DerObjectIdentifier, string>
            {
                { X509Name.Surname, serialNumber },
                { X509Name.UID, vatNumber },
                { X509Name.T, invoiceType },
                { new DerObjectIdentifier("2.5.4.26"), location },
                { X509Name.BusinessCategory, businessCategory }
            };

            var sanKeys = sanDict.Keys.ToList();
            var sanSubj = new X509Name(sanKeys, sanDict);
            var generalNames = new GeneralNames(new GeneralName(sanSubj));

            extGenerator.AddExtension(
                X509Extensions.SubjectAlternativeName,
                false,
                generalNames);

            X509Extensions extensions = extGenerator.Generate();

            // Wrap extensions into PKCS#9 extensionRequest attribute
            var extRequestAttr = new AttributePkcs(
                PkcsObjectIdentifiers.Pkcs9AtExtensionRequest,
                new DerSet(extensions));

            var attributes = new DerSet(extRequestAttr);

            // Create CSR with extensions using SHA256withECDSA
            var csr = new Pkcs10CertificationRequest(
                "SHA256withECDSA",
                subject,
                keyPair.Public,
                attributes,
                keyPair.Private);

            // Export CSR PEM (standard Unix newlines \n)
            string csrPem;
            using (var sw = new StringWriter())
            {
                sw.NewLine = "\n";
                var pemWriter = new PemWriter(sw);
                pemWriter.WriteObject(csr);
                pemWriter.Writer.Flush();
                csrPem = sw.ToString().Replace("\r\n", "\n").Trim() + "\n";
            }

            // Export Private Key PEM
            string privateKeyPem;
            using (var sw = new StringWriter())
            {
                sw.NewLine = "\n";
                var pemWriter = new PemWriter(sw);
                pemWriter.WriteObject(keyPair.Private);
                pemWriter.Writer.Flush();
                privateKeyPem = sw.ToString().Replace("\r\n", "\n").Trim() + "\n";
            }

            // ZATCA expects the base64-encoded string of the CSR PEM content
            string csrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(csrPem));

            return Task.FromResult(new CsrResult
            {
                IsSuccess = true,
                CsrPem = csrPem,
                CsrBase64 = csrBase64,
                PrivateKeyPem = privateKeyPem
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new CsrResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            });
        }
    }

    public static (bool IsValid, List<string> Errors) ValidateCsrInputs(CsrRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.CommonName))
        {
            errors.Add("CommonName is a mandatory field.");
        }
        else if (Regex.IsMatch(request.CommonName, "[!@#$%&*_]"))
        {
            errors.Add("Invalid CommonName: The CommonName should only contain alphanumeric characters, and special characters ('! @ # $ % & * _') are not allowed.");
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode))
        {
            errors.Add("Country name is a mandatory field.");
        }
        else if (request.CountryCode.Trim().Length < 2 || request.CountryCode.Trim().Length > 3)
        {
            errors.Add("Invalid country code name, please provide a valid 2-letter country code (e.g. SA).");
        }

        if (string.IsNullOrWhiteSpace(request.OrganizationName))
        {
            errors.Add("Organization name is a mandatory field.");
        }
        else if (Regex.IsMatch(request.OrganizationName, "[!@#$%&*_]"))
        {
            errors.Add("Invalid OrganizationName: The OrganizationName should only contain alphanumeric characters, and special characters ('! @ # $ % & * _') are not allowed.");
        }

        if (string.IsNullOrWhiteSpace(request.OrganizationUnitName))
        {
            errors.Add("Organization unit name is a mandatory field.");
        }
        else if (Regex.IsMatch(request.OrganizationUnitName, "[!@#$%&*_]"))
        {
            errors.Add("Invalid OrganizationUnitName: The OrganizationUnitName should only contain alphanumeric characters, and special characters ('! @ # $ % & * _') are not allowed.");
        }

        if (string.IsNullOrWhiteSpace(request.OrganizationIdentifier))
        {
            errors.Add("Organization identifier (VAT Number) is a mandatory field.");
        }
        else
        {
            string vat = request.OrganizationIdentifier.Trim();
            if (vat.Length != 15)
            {
                errors.Add("Invalid organization identifier, please provide a valid 15-digit VAT number.");
            }
            if (!vat.StartsWith("3"))
            {
                errors.Add("Invalid organization identifier, organization identifier should start with digit 3.");
            }
            if (!vat.EndsWith("3"))
            {
                errors.Add("Invalid organization identifier, organization identifier should end with digit 3.");
            }
            if (vat.Length == 15 && vat.Substring(10, 1) == "1" && (request.OrganizationUnitName?.Trim().Length ?? 0) != 10)
            {
                errors.Add("Invalid organization unit name, please provide a valid 10-digit group VAT number for organization unit name.");
            }
        }

        if (string.IsNullOrWhiteSpace(request.InvoiceType))
        {
            errors.Add("Invoice type is a mandatory field.");
        }
        else
        {
            string invType = request.InvoiceType.Trim();
            if (invType.Length != 4 || !Regex.IsMatch(invType, "^[0-1]{4}$", RegexOptions.IgnoreCase))
            {
                errors.Add("Invalid invoice type, please provide a valid 4-digit invoice type flag (e.g. 1100).");
            }
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            errors.Add("Location is a mandatory field.");
        }
        else if (Regex.IsMatch(request.Location, "[!@#$%&*_]"))
        {
            errors.Add("Invalid Location: The Location should only contain alphanumeric characters, and special characters ('! @ # $ % & * _') are not allowed.");
        }

        if (string.IsNullOrWhiteSpace(request.BusinessCategory))
        {
            errors.Add("BusinessCategory / Industry is a mandatory field.");
        }
        else if (Regex.IsMatch(request.BusinessCategory, "[!@#$%&*_]"))
        {
            errors.Add("Invalid Industry: The Industry should only contain alphanumeric characters, and special characters ('! @ # $ % & * _') are not allowed.");
        }

        if (string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            errors.Add("Serial number is a mandatory field.");
        }
        else
        {
            string sn = request.SerialNumber.Trim();
            if (!Regex.IsMatch(sn, @"1-(.+)\|2-(.+)\|3-(.+)", RegexOptions.IgnoreCase))
            {
                errors.Add("Invalid serial number. Serial number should be in the format: 1-...|2-...|3-....");
            }
            if (sn.Contains('='))
            {
                errors.Add("Invalid SerialNumber: The SerialNumber should only contain alphanumeric characters, and '=' is not allowed.");
            }
        }

        return (errors.Count == 0, errors);
    }
}
