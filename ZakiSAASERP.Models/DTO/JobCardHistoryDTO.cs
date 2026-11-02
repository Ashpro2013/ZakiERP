
namespace ZakiSAASERP.Models.DTO;

public class JobCardHistoryDTO
{
    public string JobCardNo { get; set; } = string.Empty;
    public DateTime? JobCardDate { get; set; }
    public DateTime? PromisedDate { get; set; }
    public string PlateNo { get; set; } = string.Empty;
}
