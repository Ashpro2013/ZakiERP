namespace ZakiSAASERP.Models.ViewModel;

public class ExInvoiceICV
{
    public int InvoiceICVId { get; set; }
    public int InvICVNo { get; set; }
    public int VoucherId { get; set; }
    public string? VoucherNo { get; set; }
    public string? VoucherType { get; set; }
    public DateTime? InvICVDate { get; set; }
    public string? DetailedStatus { get; set; }
    public string? Message { get; set; }
}
