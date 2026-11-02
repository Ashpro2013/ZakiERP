using AshproSAASERP.Models.Masters;
namespace AshproSAASERP.Models.DTO;

public record PreLoginDatas(List<BranchMaster> branches, List<FinancialYear> fYears, DateTime? SubEndDate, bool IsExpired = false, bool IsTrial = false);
public record ActivationResponse(
    string? licenseKey,
    string? DeviceId,
    string? licenseType,
    string? startDate,
    string? expiryDate,
    int ForDays
);
public record ActivationResult(
    bool Success,
    string? Message,
    ActivationResponse? Data = null
);
public record ActivationRequest(
    string? Email,
    string? LicenseKey,
    string? DeviceId,
    string? DeviceIdName,
    string? ProductCode,
    bool IsReactivationEnabled = false
);
public record ActivationRecheckRequest(
    string? Email,
    string? LicenseKey,
    string? DeviceId,
    string? ProductCode
);