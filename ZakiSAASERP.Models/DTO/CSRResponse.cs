namespace ZakiSAASERP.Models.DTO;

public class CSRResponse
{
    public string? ZatcaCSR { get; set; } = null;
    public string? CSRSecret { get; set; } = null;
    public string? CSIDToken { get; set; } = null;
    public string? CSIDSecret { get; set; } = null;
    public int? BranchId { get; set; } = null;
    public string? PreviousHash { get; set; } = null;
    public string? OTP { get; set; } = null;
}

