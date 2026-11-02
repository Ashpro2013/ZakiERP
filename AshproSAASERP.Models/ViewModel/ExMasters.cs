namespace AshproSAASERP.Models.ViewModel;

public class ExMasters : Masters
{
    public string Code { get; set; }
    public decimal Amount { get; set; }
    public string ExtraField { get; set; }
    public string ReferenceNo { get; set; }
    public int? CategoryId { get; set; }
    public byte[]? ItemImage { get; set; }
}
