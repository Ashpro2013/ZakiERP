using ZakiSAASERP.Models.User;

namespace ZakiSAASERP.Shared.Services;

public class AppData
{
    public static int? UserId { get; set; }
    public static string? UserName { get; set; }
    public static int BranchId { get; set; }
    public static bool IsArabic { get; set; } = false;
    public static string? sTheme { get; set; }
    public static string? printer { get; set; }
    public static string? FullScreen { get; set; }
    public static bool IsLogin { get; set; } = false;
    public static bool IsMaui { get; set; } = false;
    public static bool IsWindows { get; set; } = false;

    public static string OEMAgency { get; set; } = "ZAKI ERP";
    public static int viewWidth { get; set; }
    public static int? FYearId { get; set; }
    public static string? tocken { get; set; }
    public static bool IsMauiApi { get; set; } = true;
    public static CommonModel? PassingValue { get; set; }
    public static CompanyDetails? CompanyDetails { get; set; }
    public static List<WebUserPrivilege>? privileges { get; set; }
    public static List<MenuDetails>? menuList { get; set; }
    public static DateTime? SubEndDate { get; set; }
    public static bool IsExpaired { get; set; } = false;
    public static bool IsTrial { get; set; } = false;
    public static string DBProvider { get; set; } = string.Empty;
    public static string? ConnectionString { get; set; }
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
