using System.Threading.Tasks;
using ZakiSAASERP.Models.DTO;
using ZakiSAASERP.Models.Settings;

namespace ZakiSAASERP.Interface.Settings;

public interface IShiftClosingRepo
{
    Task<OutputResponse> SaveShiftClosingAsync(ShiftClosingRecord record);
    Task<ShiftClosingReportResponse> GetShiftClosingReportAsync(ShiftClosingReportRequest request);
    Task<ShiftClosingRecord?> GetShiftClosingByIdAsync(int id);
}

