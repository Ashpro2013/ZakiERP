using AshproSAASERP.ZatcaEngine.Constants;
using AshproSAASERP.ZatcaEngine.Enums;
using AshproSAASERP.ZatcaEngine.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AshproSAASERP.ZatcaEngine.Onboarding;

public enum ZatcaMode
{
    Developer,
    Simulation,
    Production
}

public class CertificateRequest
{
    public string CommonName { get; set; } = "";
    public string OrganizationName { get; set; } = "";
    public string OrganizationUnitName { get; set; } = "";
    public string CountryName { get; set; } = "SA";
    public string SerialNumber { get; set; } = "";
    public string OrganizationIdentifier { get; set; } = "";
    public string InvoiceType { get; set; } = "1100";
    public string Location { get; set; } = "Riyadh";
    public string BusinessCategory { get; set; } = "";
    public string OTP { get; set; } = "";
    public ZatcaMode Mode { get; set; } = ZatcaMode.Simulation;
}

public class CertificateResponse
{
    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; }
    public string? CSR { get; set; }
    public string? PrivateKey { get; set; }
    public string? CSID { get; set; }
    public string? SecretKey { get; set; }
    public string? RequestId { get; set; }
}

public class CSIDInfo
{
    public string? CertPem { get; set; }
    public string? CSR { get; set; }
    public string? PrivateKey { get; set; }
    public string? Secret { get; set; }
}

public class CSIDGenerator
{
    private readonly ZatcaMode _mode;
    private readonly HttpClient _httpClient;
    private readonly CsrGenerator _csrGenerator;

    public CSIDGenerator(ZatcaMode mode = ZatcaMode.Simulation, HttpClient? httpClient = null)
    {
        _mode = mode;
        _httpClient = httpClient ?? new HttpClient();
        _csrGenerator = new CsrGenerator();
    }

    public CertificateResponse GenerateCSR(CertificateRequest certRequest)
    {
        return GenerateCSRAsync(certRequest).GetAwaiter().GetResult();
    }

    public async Task<CertificateResponse> GenerateCSRAsync(CertificateRequest certRequest)
    {
        var response = new CertificateResponse();

        var csrRequest = new CsrRequest
        {
            CommonName = certRequest.CommonName,
            OrganizationName = certRequest.OrganizationName,
            OrganizationUnitName = certRequest.OrganizationUnitName,
            CountryCode = certRequest.CountryName,
            SerialNumber = certRequest.SerialNumber,
            OrganizationIdentifier = certRequest.OrganizationIdentifier,
            InvoiceType = certRequest.InvoiceType,
            Location = certRequest.Location,
            BusinessCategory = certRequest.BusinessCategory,
            OTP = certRequest.OTP,
            Environment = _mode.ToString()
        };

        var result = await _csrGenerator.GenerateAsync(csrRequest);
        if (result.IsSuccess)
        {
            response.IsSuccess = true;
            response.CSR = result.CsrPem;
            response.PrivateKey = result.PrivateKeyPem;
        }
        else
        {
            response.IsSuccess = false;
            response.ErrorMessage = result.ErrorMessage;
        }

        return response;
    }

    public CertificateResponse GenerateCSID(CertificateRequest certRequest)
    {
        return GenerateCSIDAsync(certRequest).GetAwaiter().GetResult();
    }

    public async Task<CertificateResponse> GenerateCSIDAsync(CertificateRequest certRequest)
    {
        var response = new CertificateResponse();

        if (string.IsNullOrWhiteSpace(certRequest.OTP))
        {
            response.IsSuccess = false;
            response.ErrorMessage = "You must enter OTP!";
            return response;
        }

        // 1. Generate CSR first
        var csrResponse = await GenerateCSRAsync(certRequest);
        if (!csrResponse.IsSuccess || string.IsNullOrEmpty(csrResponse.CSR))
        {
            response.IsSuccess = false;
            response.ErrorMessage = csrResponse.ErrorMessage ?? "CSR generation failed.";
            return response;
        }

        response.CSR = csrResponse.CSR;
        response.PrivateKey = csrResponse.PrivateKey;

        // 2. Request Compliance CSID from ZATCA
        var env = _mode switch
        {
            ZatcaMode.Production => EnvironmentType.Production,
            ZatcaMode.Simulation => EnvironmentType.Simulation,
            ZatcaMode.Developer => EnvironmentType.Developer,
            _ => EnvironmentType.Developer
        };

        var complianceClient = new ComplianceClient(_httpClient, env);
        string csrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(csrResponse.CSR));

        var complianceResult = await complianceClient.RequestComplianceAsync(csrBase64, certRequest.OTP.Trim());
        if (!complianceResult.IsSuccess)
        {
            response.IsSuccess = false;
            response.ErrorMessage = complianceResult.ErrorMessage;
            return response;
        }

        response.IsSuccess = true;
        response.CSID = complianceResult.BinarySecurityToken;
        response.SecretKey = complianceResult.Secret;
        response.RequestId = complianceResult.RequestID;

        // If the mode is Developer or Simulation compliance, return the compliance certificate
        return response;
    }

    public async Task<CertificateResponse> GetProductionCSIDAsync(
        string complianceRequestId,
        string complianceBinarySecurityToken,
        string secret)
    {
        var response = new CertificateResponse();

        var env = _mode switch
        {
            ZatcaMode.Production => EnvironmentType.Production,
            ZatcaMode.Simulation => EnvironmentType.Simulation,
            ZatcaMode.Developer => EnvironmentType.Developer,
            _ => EnvironmentType.Developer
        };

        var prodClient = new ProductionCertificateClient(_httpClient, env);
        var result = await prodClient.RequestProductionCertificateAsync(
            complianceRequestId,
            complianceBinarySecurityToken,
            secret);

        if (!result.IsSuccess)
        {
            response.IsSuccess = false;
            response.ErrorMessage = result.ErrorMessage;
            return response;
        }

        response.IsSuccess = true;
        response.CSID = result.BinarySecurityToken;
        response.SecretKey = result.Secret;
        response.RequestId = result.RequestID;
        return response;
    }
}
