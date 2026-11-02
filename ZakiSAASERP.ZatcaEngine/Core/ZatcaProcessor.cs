using ZakiSAASERP.ZatcaEngine.Api;
using ZakiSAASERP.ZatcaEngine.Crypto;
using ZakiSAASERP.ZatcaEngine.Enums;
using ZakiSAASERP.ZatcaEngine.Hashing;
using ZakiSAASERP.ZatcaEngine.Models;
using ZakiSAASERP.ZatcaEngine.Qr;
using ZakiSAASERP.ZatcaEngine.Ubl;

namespace ZakiSAASERP.ZatcaEngine.Core;

public class ZatcaProcessor
{
    private readonly ZatcaOptions _options;
    private readonly HttpClient _httpClient;

    public ZatcaProcessor(
        ZatcaOptions options,
        HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public async Task<ZatcaResult> ProcessAsync(
        Invoice invoice,
        InvoiceType invoiceType)
    {
        // ===============================
        // 1️⃣ Build UBL XML
        // ===============================
        var ublBuilder = new UblInvoiceBuilder();
        string xml = ublBuilder.BuildInvoice(invoice, invoiceType);

        // ===============================
        // 2️⃣ Canonicalize + Hash
        // ===============================
        var canonicalizer = new XmlCanonicalizer();
        var xmlDoc = canonicalizer.LoadXml(xml);
        xmlDoc = canonicalizer.RemoveSignatureAndExtensions(xmlDoc);

        var canonicalBytes = canonicalizer.Canonicalize(xmlDoc);

        var hashGen = new InvoiceHashGenerator();
        string invoiceHash = hashGen.GenerateHashBase64(canonicalBytes);

        // ===============================
        // 3️⃣ Sign XML (XAdES)
        // ===============================
        var signer = new XadesSigner();
        string signedXml = signer.SignXml(
            xml,
            _options.PrivateKeyPem,
            _options.CertificatePem);

        // ===============================
        // 4️⃣ Generate QR (Phase 2)
        // ===============================
        var qrGenerator = new ZatcaQrGenerator();
        string qrBase64 = qrGenerator.GeneratePhase2Qr(
            invoice,
            invoiceHash,
            signedXml);

        // ===============================
        // 5️⃣ Embed QR & Submit to ZATCA
        // ===============================
        string finalSignedXml = signer.EmbedQrCode(signedXml, qrBase64);

        var zatcaHttp = new ZatcaHttpClient(_httpClient);

        string response;

        if (InvoiceTypeMapper.RequiresClearance(invoiceType))
        {
            var clearance = new ClearanceClient(
                zatcaHttp,
                _options.Environment);

            response = await clearance.SubmitAsync(
                finalSignedXml,
                invoiceHash,
                invoice.Uuid,
                _options.BinarySecurityToken,
                _options.Secret);
        }
        else
        {
            var reporting = new ReportingClient(
                zatcaHttp,
                _options.Environment);

            response = await reporting.SubmitAsync(
                finalSignedXml,
                invoiceHash,
                invoice.Uuid,
                _options.BinarySecurityToken,
                _options.Secret);
        }

        // ===============================
        // 6️⃣ Return Final Result
        // ===============================
        return new ZatcaResult
        {
            Status = "Submitted",
            InvoiceHash = invoiceHash,
            SignedXml = signedXml,
            QrBase64 = qrBase64,
            RawResponse = response
        };
    }
}
