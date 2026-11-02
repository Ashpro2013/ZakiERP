using ZakiSAASERP.Models.Masters;

namespace ZakiSAASERP.POS.Services;

public static class AppData
{
    public static int? UserId { get; set; } = 1;
    public static string? UserName { get; set; } = "Admin";
    public static bool IsLogin { get; set; } = false;
    public static int BranchId { get; set; } = 1;
    public static int? FYearId { get; set; } = 1;
    public static bool IsMaui { get; set; } = false;
    public static bool IsWindows { get; set; } = true;
    public static string DBProvider { get; set; } = "MSSQL";
    public static string? ConnectionString { get; set; }
    public static CompanyDetails? CompanyDetails { get; set; }
    private static string? _terminal;
    public static string Terminal
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_terminal)) return _terminal;
            try
            {
                var machine = Environment.MachineName;
                if (!string.IsNullOrWhiteSpace(machine)) return machine;
            }
            catch { }
            return "Terminal 1";
        }
        set => _terminal = value;
    }
}

