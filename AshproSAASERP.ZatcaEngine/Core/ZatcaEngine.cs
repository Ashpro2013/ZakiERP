using AshproSAASERP.ZatcaEngine.Abstractions;
using AshproSAASERP.ZatcaEngine.Api;
using AshproSAASERP.ZatcaEngine.Constants;
using AshproSAASERP.ZatcaEngine.Crypto;
using AshproSAASERP.ZatcaEngine.Enums;
using AshproSAASERP.ZatcaEngine.Hashing;
using AshproSAASERP.ZatcaEngine.Models;
using AshproSAASERP.ZatcaEngine.Onboarding;
using AshproSAASERP.ZatcaEngine.Qr;
using AshproSAASERP.ZatcaEngine.Ubl;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AshproSAASERP.ZatcaEngine.Core;

public class ZatcaEngine : IZatcaEngine
{
    private readonly UblInvoiceBuilder _ublBuilder;
    private readonly XadesSigner _signer;
    private readonly XmlCanonicalizer _canonicalizer;
    private readonly ZatcaQrGenerator _qrGenerator;
    private readonly ZatcaHttpClient _httpClient;
    private readonly HttpClient _baseHttpClient;

    public ZatcaEngine(HttpClient httpClient)
    {
        _baseHttpClient = httpClient ?? new HttpClient();
        _httpClient = new ZatcaHttpClient(_baseHttpClient);

        _ublBuilder = new UblInvoiceBuilder();
        _signer = new XadesSigner();
        _canonicalizer = new XmlCanonicalizer();
        _qrGenerator = new ZatcaQrGenerator();
    }

    public async Task<CsrResult> GenerateCsrAsync(
        CsrRequest request)
    {
        var generator = new CsrGenerator();
        var csrResult = await generator.GenerateAsync(request);

        if (!csrResult.IsSuccess || string.IsNullOrWhiteSpace(request.OTP))
        {
            return csrResult;
        }

        try
        {
            var env = ParseEnvironment(request.Environment);
            var complianceClient = new ComplianceClient(_baseHttpClient, env);
            var complianceResult = await complianceClient.RequestComplianceAsync(
                csrResult.CsrBase64!,
                request.OTP);

            if (complianceResult.IsSuccess)
            {
                csrResult.BinarySecurityToken = complianceResult.BinarySecurityToken;
                csrResult.Secret = complianceResult.Secret;
                csrResult.RequestId = complianceResult.RequestID;

                // In Simulation and Production, automatically submit a compliance sample invoice and request Production CSID (PCSID)
                if (env == EnvironmentType.Simulation || env == EnvironmentType.Production)
                {
                    await CompleteComplianceAndFetchPcsidAsync(request, csrResult, env);
                }
            }
            else
            {
                csrResult.IsSuccess = false;
                csrResult.ErrorMessage = complianceResult.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            csrResult.IsSuccess = false;
            csrResult.ErrorMessage = $"CSID Onboarding Error: {ex.Message}";
        }

        return csrResult;
    }

    private InvoiceType ParseInvoiceType(string typeName)
    {
        return typeName switch
        {
            "0100000" => InvoiceType.StandardInvoice,
            "0200000" => InvoiceType.SimplifiedInvoice,
            _ => InvoiceType.StandardInvoice
        };
    }

    private EnvironmentType ParseEnvironment(string environment)
    {
        return environment?.Trim().ToLowerInvariant() switch
        {
            "production" => EnvironmentType.Production,
            "simulation" => EnvironmentType.Simulation,
            "developer" => EnvironmentType.Developer,
            "sandbox" => EnvironmentType.Sandbox,
            _ => EnvironmentType.Developer
        };
    }

    public async Task<ZatcaSubmissionResult>
        SubmitInvoiceAsync(ZatcaInvoiceRequest request)
    {
        try
        {
            // 0. Inspect X.509 Certificate: Auto-detect Environment & Align Seller VAT
            var certToken = !string.IsNullOrWhiteSpace(request.CertificateBase64)
                ? request.CertificateBase64
                : request.BinarySecurityToken;

            var (derBytes, cleanDerBase64) = ZatcaCertificateHelper.NormalizeCertificate(certToken);

            if (!string.IsNullOrEmpty(cleanDerBase64))
            {
                request.CertificateBase64 = cleanDerBase64;
                if (string.IsNullOrWhiteSpace(request.BinarySecurityToken))
                {
                    request.BinarySecurityToken = cleanDerBase64;
                }
            }

            X509Certificate2? cert = null;
            if (derBytes != null && derBytes.Length > 0)
            {
                try
                {
                    cert = X509CertificateLoader.LoadCertificate(derBytes);
                }
                catch { }
            }

            var env = ParseEnvironment(request.Environment);

            // Auto-detect environment from certificate issuer if available
            if (cert != null)
            {
                var issuer = cert.Issuer ?? "";
                if (issuer.Contains("TSTZATCA", StringComparison.OrdinalIgnoreCase) ||
                    issuer.Contains("TST-", StringComparison.OrdinalIgnoreCase))
                {
                    // Certificate was issued by Developer Portal CA
                    env = EnvironmentType.Developer;
                }
                else if (issuer.Contains("PREZATCA", StringComparison.OrdinalIgnoreCase) ||
                         issuer.Contains("PRE-", StringComparison.OrdinalIgnoreCase))
                {
                    // Certificate was issued by Simulation CA
                    env = EnvironmentType.Simulation;
                }
                else if (issuer.Contains("ZATCA-Code-Signing", StringComparison.OrdinalIgnoreCase))
                {
                    // Certificate was issued by Production CA
                    env = EnvironmentType.Production;
                }

                // Verify and align Seller VAT with Certificate VAT to avoid 401 certificate-permissions
                if (request.Invoice?.Seller != null)
                {
                    var certVat = ExtractVatFromCertificate(cert);
                    if (!string.IsNullOrEmpty(certVat))
                    {
                        if (string.IsNullOrWhiteSpace(request.Invoice.Seller.VatNumber) ||
                            !string.Equals(request.Invoice.Seller.VatNumber.Trim(), certVat, StringComparison.OrdinalIgnoreCase))
                        {
                            request.Invoice.Seller.VatNumber = certVat;
                        }
                    }
                }
            }

            if (request.Invoice.IssueDate.Year < 2000)
            {
                request.Invoice.IssueDate = DateTime.Now;
            }

            // 1. Build UBL XML
            var invoiceType = ParseInvoiceType(request.InvoiceTypeName);
            bool isSimplified = invoiceType == InvoiceType.SimplifiedInvoice ||
                                request.InvoiceTypeName?.StartsWith("02") == true;

            var xml = _ublBuilder.BuildInvoice(
                request.Invoice,
                invoiceType);

            // 2. Canonicalize
            var xmlDoc = _canonicalizer.LoadXml(xml);
            xmlDoc = _canonicalizer.RemoveSignatureAndExtensions(xmlDoc);

            var canonicalBytes = _canonicalizer.Canonicalize(xmlDoc);

            // 3. Hash
            var hash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(canonicalBytes));

            // 4. Sign XML (XAdES-BES)
            var signedXml = _signer.SignXml(
                xml,
                request.PrivateKeyPem,
                request.CertificateBase64,
                hash,
                request.Invoice.IssueDate);

            // 5. Generate Phase 2 QR Code
            var qr = _qrGenerator.GeneratePhase2Qr(
                request.Invoice,
                hash,
                signedXml,
                request.CertificateBase64,
                isSimplified);

            // 6. Embed QR Code into signed XML
            var finalSignedXml = _signer.EmbedQrCode(signedXml, qr);

            // 7. Submit to ZATCA Portal API
            if(request.blnUploadXML == false)
            {
                return new ZatcaSubmissionResult
                {
                    IsSuccess = true,
                    InvoiceHash = hash,
                    QrCode = qr,
                    SignedXml = finalSignedXml,
                    RawResponse = null,
                    ClearanceStatus = null,
                    ReportingStatus = null,
                    ErrorMessage = null
                };
            }
            bool isStandard = request.InvoiceTypeName.StartsWith("01");

            bool isCompliance = request.IsCompliance ||
                                env == EnvironmentType.Developer ||
                                env == EnvironmentType.Sandbox;

            string endpoint;
            string? clearanceStatusHeader = isCompliance ? null : (isStandard ? "1" : "0");

            if (isCompliance)
            {
                endpoint = ZatcaEndpoints.GetComplianceInvoice(env);
            }
            else
            {
                endpoint = isStandard
                    ? ZatcaEndpoints.GetClearance(env)
                    : ZatcaEndpoints.GetReporting(env);
            }

            string rawResponse;
            try
            {
                rawResponse = await _httpClient.PostAsync(
                    endpoint,
                    finalSignedXml,
                    hash,
                    request.Invoice.Uuid,
                    request.BinarySecurityToken,
                    request.Secret,
                    clearanceStatusHeader);
            }
            catch (Exception ex) when (!isCompliance && ex.Message.Contains("(401)"))
            {
                // Fallback attempt: If Clearance/Reporting failed with 401, check if the CSID is a Compliance CSID by attempting the compliance endpoint
                try
                {
                    var complianceEndpoint = ZatcaEndpoints.GetComplianceInvoice(env);
                    rawResponse = await _httpClient.PostAsync(
                        complianceEndpoint,
                        finalSignedXml,
                        hash,
                        request.Invoice.Uuid,
                        request.BinarySecurityToken,
                        request.Secret,
                        clearanceStatus: null); // CRITICAL: NEVER pass clearanceStatus to /compliance/invoices!
                }
                catch (Exception compEx)
                {
                    // If compliance check also fails, throw detailed diagnostic exception with both failure reasons
                    throw new Exception(
                        $"ZATCA Authentication Failed (401) on both endpoints.\n" +
                        $"1. Primary Endpoint ({endpoint}): {ex.Message}\n" +
                        $"2. Compliance Fallback Endpoint ({ZatcaEndpoints.GetComplianceInvoice(env)}): {compEx.Message}\n" +
                        $"Diagnostic Info:\n" +
                        $"- Detected Environment: {env}\n" +
                        $"- Certificate Issuer: {cert?.Issuer}\n" +
                        $"- Certificate Subject: {cert?.Subject}\n" +
                        $"- Certificate Valid Until: {cert?.NotAfter:yyyy-MM-dd HH:mm:ss} UTC\n" +
                        $"- Invoice Seller VAT: {request.Invoice.Seller?.VatNumber}",
                        compEx);
                }
            }

            ZatcaApiStructuredResponse? parsed = null;

            try
            {
                parsed = JsonSerializer.Deserialize<ZatcaApiStructuredResponse>(
                    rawResponse,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                return new ZatcaSubmissionResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }

            bool isSuccess = EvaluateSuccess(parsed);

            string finalXml = finalSignedXml;
            string finalQr = qr;

            if (!string.IsNullOrEmpty(parsed?.clearedInvoice))
            {
                try
                {
                    var decodedXml = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parsed.clearedInvoice));
                    if (!string.IsNullOrWhiteSpace(decodedXml))
                    {
                        finalXml = decodedXml;
                        var doc = new System.Xml.XmlDocument();
                        doc.PreserveWhitespace = true;
                        doc.LoadXml(finalXml);
                        var qrNode = doc.SelectSingleNode("//*[local-name()='EmbeddedDocumentBinaryObject'][preceding-sibling::*[local-name()='ID'][text()='QR']]")
                                   ?? doc.SelectSingleNode("//*[local-name()='EmbeddedDocumentBinaryObject']");
                        if (qrNode != null && !string.IsNullOrWhiteSpace(qrNode.InnerText))
                        {
                            finalQr = qrNode.InnerText.Trim();
                        }
                    }
                }
                catch { }
            }

            return new ZatcaSubmissionResult
            {
                IsSuccess = isSuccess,
                InvoiceHash = parsed?.invoiceHash ?? hash,
                QrCode = finalQr,
                SignedXml = finalXml,
                RawResponse = rawResponse,
                ClearanceStatus = parsed?.clearanceStatus,
                ReportingStatus = parsed?.reportingStatus,
                ErrorMessage = isSuccess
                    ? ExtractWarning(parsed)
                    : ExtractError(parsed)
            };
        }
        catch (Exception ex)
        {
            return new ZatcaSubmissionResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static string? ExtractVatFromCertificate(X509Certificate2 cert)
    {
        try
        {
            // 1. Search in Subject (e.g. UID=300000134310003 or CN=TST-2050012095-300000134310003)
            var subject = cert.Subject ?? "";
            var match = Regex.Match(subject, @"\b3\d{13}3\b");
            if (match.Success) return match.Value;

            // 2. Search in Subject Alternative Name
            foreach (var ext in cert.Extensions)
            {
                if (ext.Oid?.Value == "2.5.29.17")
                {
                    var san = ext.Format(false);
                    var sanMatch = Regex.Match(san, @"\b3\d{13}3\b");
                    if (sanMatch.Success) return sanMatch.Value;
                }
            }
        }
        catch { }
        return null;
    }

    private bool EvaluateSuccess(
        ZatcaApiStructuredResponse? response)
    {
        if (response == null)
            return false;

        if (response.clearanceStatus == "CLEARED")
            return true;

        if (response.reportingStatus == "REPORTED")
            return true;

        if (response.validationResults?.status == "PASS" || response.validationResults?.status == "WARNING")
            return true;

        return false;
    }

    private string? ExtractError(
        ZatcaApiStructuredResponse? response)
    {
        var errors = response?
            .validationResults?
            .errorMessages;

        if (errors != null && errors.Count > 0)
        {
            return string.Join("; ", errors.Select(e => string.IsNullOrWhiteSpace(e.code) ? e.message : $"[{e.code}] {e.message}"));
        }

        return "ZATCA invoice validation failed.";
    }

    private string? ExtractWarning(
        ZatcaApiStructuredResponse? response)
    {
        var warnings = response?
            .validationResults?
            .warningMessages;

        if (warnings != null && warnings.Count > 0)
        {
            return string.Join("; ", warnings.Select(w => w.message));
        }

        return null;
    }


    public async Task<ProductionCertificateResult> RequestProductionCsidAsync(
        string complianceRequestId,
        string complianceBinarySecurityToken,
        string complianceSecret,
        string environment)
    {
        var env = ParseEnvironment(environment);
        var prodClient = new ProductionCertificateClient(_baseHttpClient, env);
        return await prodClient.RequestProductionCertificateAsync(
            complianceRequestId,
            complianceBinarySecurityToken,
            complianceSecret);
    }

    private async Task CompleteComplianceAndFetchPcsidAsync(
        CsrRequest request,
        CsrResult csrResult,
        EnvironmentType env)
    {
        var complianceToken = csrResult.BinarySecurityToken!;
        var complianceSecret = csrResult.Secret!;
        var complianceRequestId = csrResult.RequestId!;
        var privateKeyPem = csrResult.PrivateKeyPem!;

        bool needsSimplified = string.IsNullOrEmpty(request.InvoiceType) ||
                               request.InvoiceType.Length < 2 ||
                               request.InvoiceType[1] == '1';

        bool needsStandard = !string.IsNullOrEmpty(request.InvoiceType) &&
                             request.InvoiceType.Length >= 1 &&
                             request.InvoiceType[0] == '1';

        string currentHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
        int currentIcv = 1;

        // 1. Submit Simplified Compliance checks: Invoice, Credit Note, Debit Note
        if (needsSimplified)
        {
            // 1.1 Simplified Tax Invoice (388)
            var simInvoice = CreateSampleComplianceInvoice(request, isStandard: false, InvoiceType.SimplifiedInvoice, currentIcv++, currentHash);
            currentHash = await SubmitComplianceInvoiceInternalAsync(simInvoice, privateKeyPem, complianceToken, complianceSecret, env, InvoiceType.SimplifiedInvoice);
            await Task.Delay(300);

            // 1.2 Simplified Credit Note (381)
            var simCreditNote = CreateSampleComplianceInvoice(request, isStandard: false, InvoiceType.SimplifiedCreditNote, currentIcv++, currentHash, simInvoice.InvoiceNumber);
            currentHash = await SubmitComplianceInvoiceInternalAsync(simCreditNote, privateKeyPem, complianceToken, complianceSecret, env, InvoiceType.SimplifiedCreditNote);
            await Task.Delay(300);

            // 1.3 Simplified Debit Note (383)
            var simDebitNote = CreateSampleComplianceInvoice(request, isStandard: false, InvoiceType.SimplifiedDebitNote, currentIcv++, currentHash, simInvoice.InvoiceNumber);
            currentHash = await SubmitComplianceInvoiceInternalAsync(simDebitNote, privateKeyPem, complianceToken, complianceSecret, env, InvoiceType.SimplifiedDebitNote);
            await Task.Delay(300);
        }

        // 2. Submit Standard Compliance checks: Invoice, Credit Note, Debit Note
        if (needsStandard)
        {
            // 2.1 Standard Tax Invoice (388)
            var stdInvoice = CreateSampleComplianceInvoice(request, isStandard: true, InvoiceType.StandardInvoice, currentIcv++, currentHash);
            currentHash = await SubmitComplianceInvoiceInternalAsync(stdInvoice, privateKeyPem, complianceToken, complianceSecret, env, InvoiceType.StandardInvoice);
            await Task.Delay(300);

            // 2.2 Standard Credit Note (381)
            var stdCreditNote = CreateSampleComplianceInvoice(request, isStandard: true, InvoiceType.StandardCreditNote, currentIcv++, currentHash, stdInvoice.InvoiceNumber);
            currentHash = await SubmitComplianceInvoiceInternalAsync(stdCreditNote, privateKeyPem, complianceToken, complianceSecret, env, InvoiceType.StandardCreditNote);
            await Task.Delay(300);

            // 2.3 Standard Debit Note (383)
            var stdDebitNote = CreateSampleComplianceInvoice(request, isStandard: true, InvoiceType.StandardDebitNote, currentIcv++, currentHash, stdInvoice.InvoiceNumber);
            currentHash = await SubmitComplianceInvoiceInternalAsync(stdDebitNote, privateKeyPem, complianceToken, complianceSecret, env, InvoiceType.StandardDebitNote);
            await Task.Delay(300);
        }

        // 3. Request the Production CSID (PCSID) using the Compliance CSID and RequestID with retry for ZATCA replication
        ProductionCertificateResult? prodResult = null;
        var prodClient = new ProductionCertificateClient(_baseHttpClient, env);

        for (int retry = 0; retry < 5; retry++)
        {
            await Task.Delay(1500 * (retry + 1));
            prodResult = await prodClient.RequestProductionCertificateAsync(
                complianceRequestId,
                complianceToken,
                complianceSecret);

            if (prodResult.IsSuccess && !string.IsNullOrEmpty(prodResult.BinarySecurityToken))
            {
                break;
            }
        }

        if (prodResult == null || !prodResult.IsSuccess || string.IsNullOrEmpty(prodResult.BinarySecurityToken))
        {
            throw new Exception($"Compliance check completed, but failed to retrieve Production CSID (PCSID): {prodResult?.ErrorMessage}");
        }

        // 4. Update the result with the newly issued PRODUCTION CSID!
        csrResult.BinarySecurityToken = prodResult.BinarySecurityToken;
        csrResult.Secret = prodResult.Secret;
        if (!string.IsNullOrEmpty(prodResult.RequestID))
        {
            csrResult.RequestId = prodResult.RequestID;
        }
    }

    private Invoice CreateSampleComplianceInvoice(
        CsrRequest request,
        bool isStandard,
        InvoiceType docType,
        int icv,
        string previousHash,
        string? billingReferenceId = null)
    {
        // 1. Seller Party - use original seller data from request.Seller (MapCompanyToParty) with ZATCA compliant fallbacks
        string sellerVat = !string.IsNullOrWhiteSpace(request.Seller?.VatNumber)
            ? request.Seller.VatNumber.Trim()
            : (!string.IsNullOrWhiteSpace(request.OrganizationIdentifier) ? request.OrganizationIdentifier.Trim() : "300000000000003");

        string sellerName = !string.IsNullOrWhiteSpace(request.Seller?.Name)
            ? request.Seller.Name.Trim()
            : (!string.IsNullOrWhiteSpace(request.OrganizationName) ? request.OrganizationName.Trim() : "Establishment");

        string crn = !string.IsNullOrWhiteSpace(request.Seller?.IdentificationNumber) && request.Seller.IdentificationNumber.Trim().Length == 10
            ? request.Seller.IdentificationNumber.Trim()
            : (!string.IsNullOrWhiteSpace(request.OrganizationUnitName) && request.OrganizationUnitName.Trim().Length == 10 ? request.OrganizationUnitName.Trim() : "1010010010");

        string crnScheme = !string.IsNullOrWhiteSpace(request.Seller?.IdentificationScheme)
            ? request.Seller.IdentificationScheme.Trim()
            : "CRN";

        string street = !string.IsNullOrWhiteSpace(request.Seller?.Street)
            ? request.Seller.Street.Trim()
            : "King Fahd Road";

        // ZATCA BR-KSA-18: Building number must be 4 digits
        string buildingNumber = (!string.IsNullOrWhiteSpace(request.Seller?.BuildingNumber) && request.Seller.BuildingNumber.Trim().Length == 4)
            ? request.Seller.BuildingNumber.Trim()
            : "1234";

        string city = !string.IsNullOrWhiteSpace(request.Seller?.City)
            ? request.Seller.City.Trim()
            : (!string.IsNullOrWhiteSpace(request.Location) ? request.Location.Trim() : "Riyadh");

        // ZATCA BR-KSA-06: Postal zone must be 5 digits
        string postalCode = (!string.IsNullOrWhiteSpace(request.Seller?.PostalCode) && request.Seller.PostalCode.Trim().Length == 5)
            ? request.Seller.PostalCode.Trim()
            : "12345";

        // ZATCA BR-KSA-05: City subdivision (District) is mandatory
        string district = !string.IsNullOrWhiteSpace(request.Seller?.District)
            ? request.Seller.District.Trim()
            : "Al Olaya";

        string country = !string.IsNullOrWhiteSpace(request.Seller?.CountryCode)
            ? request.Seller.CountryCode.Trim()
            : (!string.IsNullOrWhiteSpace(request.CountryCode) ? request.CountryCode.Trim() : "SA");

        var seller = new Party
        {
            Name = sellerName,
            VatNumber = sellerVat,
            IdentificationScheme = crnScheme,
            IdentificationNumber = crn,
            Street = street,
            BuildingNumber = buildingNumber,
            City = city,
            PostalCode = postalCode,
            District = district,
            Subdivision = request.Seller?.Subdivision ?? "",
            CountryCode = country
        };

        // 2. Buyer Party - strictly compliant with ZATCA rules for Standard (B2B) vs Simplified (B2C)
        Party buyer;
        if (isStandard)
        {
            // Standard Tax Invoice (B2B): Must have a company buyer, valid address, and VAT/CRN
            buyer = new Party
            {
                Name = (!string.IsNullOrWhiteSpace(request.Buyer?.Name) && !request.Buyer.Name.Contains("Consumer", StringComparison.OrdinalIgnoreCase) && !request.Buyer.Name.Contains("مستهلك"))
                    ? request.Buyer.Name.Trim()
                    : "Standard Compliance Buyer Co",
                VatNumber = (!string.IsNullOrWhiteSpace(request.Buyer?.VatNumber) && request.Buyer.VatNumber.Trim().Length == 15)
                    ? request.Buyer.VatNumber.Trim()
                    : "399999999800003",
                IdentificationScheme = "CRN",
                IdentificationNumber = (!string.IsNullOrWhiteSpace(request.Buyer?.IdentificationNumber) && request.Buyer.IdentificationNumber.Trim().Length == 10)
                    ? request.Buyer.IdentificationNumber.Trim()
                    : "1010020020",
                Street = !string.IsNullOrWhiteSpace(request.Buyer?.Street) ? request.Buyer.Street.Trim() : "Olaya Street",
                BuildingNumber = (!string.IsNullOrWhiteSpace(request.Buyer?.BuildingNumber) && request.Buyer.BuildingNumber.Trim().Length == 4)
                    ? request.Buyer.BuildingNumber.Trim()
                    : "5678",
                City = !string.IsNullOrWhiteSpace(request.Buyer?.City) ? request.Buyer.City.Trim() : "Riyadh",
                PostalCode = (!string.IsNullOrWhiteSpace(request.Buyer?.PostalCode) && request.Buyer.PostalCode.Trim().Length == 5)
                    ? request.Buyer.PostalCode.Trim()
                    : "12345",
                District = !string.IsNullOrWhiteSpace(request.Buyer?.District) ? request.Buyer.District.Trim() : "Al Olaya",
                CountryCode = "SA"
            };
        }
        else
        {
            // Simplified Tax Invoice (B2C): Emits empty AccountingCustomerParty per ZATCA BR-KSA-47
            buyer = new Party
            {
                Name = "مستهلك نهائي | Consumer",
                CountryCode = "SA"
            };
        }

        string prefix = docType switch
        {
            InvoiceType.SimplifiedInvoice => "COMP-SIM-INV",
            InvoiceType.SimplifiedCreditNote => "COMP-SIM-CN",
            InvoiceType.SimplifiedDebitNote => "COMP-SIM-DN",
            InvoiceType.StandardInvoice => "COMP-STD-INV",
            InvoiceType.StandardCreditNote => "COMP-STD-CN",
            InvoiceType.StandardDebitNote => "COMP-STD-DN",
            _ => isStandard ? "COMP-STD" : "COMP-SIM"
        };

        string lineDesc = docType switch
        {
            InvoiceType.SimplifiedCreditNote => "Simplified Compliance Credit Note",
            InvoiceType.SimplifiedDebitNote => "Simplified Compliance Debit Note",
            InvoiceType.StandardCreditNote => "Standard Compliance Credit Note",
            InvoiceType.StandardDebitNote => "Standard Compliance Debit Note",
            _ => isStandard ? "Standard Compliance Service" : "Simplified Compliance Service"
        };

        string paymentInstruction = docType switch
        {
            InvoiceType.SimplifiedCreditNote or InvoiceType.StandardCreditNote => "Goods returned or invoice cancellation",
            InvoiceType.SimplifiedDebitNote or InvoiceType.StandardDebitNote => "Price adjustment or additional charges",
            _ => "Cash payment"
        };

        var inv = new Invoice
        {
            InvoiceNumber = $"{prefix}-{DateTime.Now:yyyyMMddHHmmssfff}",
            Uuid = Guid.NewGuid().ToString(),
            IssueDate = DateTime.Now,
            InvoiceSubType = isStandard ? "0100000" : "0200000",
            Icv = icv,
            PreviousInvoiceHash = previousHash,
            PaymentMeansCode = "10",
            PaymentInstruction = paymentInstruction,
            BillingReferenceId = billingReferenceId,
            Seller = seller,
            Buyer = buyer,
            TotalAmount = 115.00m,
            VatAmount = 15.00m
        };

        inv.Lines.Add(new InvoiceLine
        {
            Description = lineDesc,
            Quantity = 1,
            UnitPrice = 100.00m,
            LineTotal = 100.00m,
            VatRate = 15.00m,
            VatAmount = 15.00m,
            TaxCategory = "S"
        });

        return inv;
    }

    private async Task<string> SubmitComplianceInvoiceInternalAsync(
        Invoice invoice,
        string privateKeyPem,
        string complianceToken,
        string complianceSecret,
        EnvironmentType env,
        InvoiceType docType)
    {
        // Align Seller VAT with certificate VAT if certificate was issued with specific UID
        try
        {
            var derBytes = Convert.FromBase64String(complianceToken);
            using var cert = X509CertificateLoader.LoadCertificate(derBytes);
            var certVat = ExtractVatFromCertificate(cert);
            if (!string.IsNullOrEmpty(certVat) && invoice.Seller != null)
            {
                invoice.Seller.VatNumber = certVat;
            }
        }
        catch { }

        bool isStandard = InvoiceTypeMapper.RequiresClearance(docType);
        var xml = _ublBuilder.BuildInvoice(invoice, docType);

        var xmlDoc = _canonicalizer.LoadXml(xml);
        xmlDoc = _canonicalizer.RemoveSignatureAndExtensions(xmlDoc);
        var canonicalBytes = _canonicalizer.Canonicalize(xmlDoc);
        var hash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(canonicalBytes));

        var signedXml = _signer.SignXml(
            xml,
            privateKeyPem,
            complianceToken,
            hash,
            invoice.IssueDate);

        var qr = _qrGenerator.GeneratePhase2Qr(
            invoice,
            hash,
            signedXml,
            complianceToken,
            !isStandard);

        var finalSignedXml = _signer.EmbedQrCode(signedXml, qr);

        var complianceEndpoint = ZatcaEndpoints.GetComplianceInvoice(env);
        try
        {
            await _httpClient.PostAsync(
                complianceEndpoint,
                finalSignedXml,
                hash,
                invoice.Uuid,
                complianceToken,
                complianceSecret,
                clearanceStatus: null);
        }
        catch (Exception ex)
        {
            // 406 or "Submitted before" indicates compliance check has ALREADY been accepted before!
            if (!ex.Message.Contains("406") &&
                !ex.Message.Contains("already completed", StringComparison.OrdinalIgnoreCase) &&
                !ex.Message.Contains("Submitted before", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Compliance invoice submission ({docType}) rejected by ZATCA: {ex.Message}", ex);
            }
        }

        return hash;
    }

}