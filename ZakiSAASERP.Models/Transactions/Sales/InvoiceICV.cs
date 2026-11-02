namespace ZakiSAASERP.Models.Transactions.Sales;

[Table("InvoiceICV")]
public class InvoiceICV
{
    [Key]
    public int InvoiceICVId { get; set; }
    public int InvICVNo { get; set; }
    public string VoucherType { get; set; } = default!;
    public int VoucherId { get; set; }
    public string? VoucherNo { get; set; }
    public DateTime? InvICVDate { get; set; }
    public bool ICVStatus { get; set; }
    public string? DetailedStatus { get; set; }
    public string? ICVMessage { get; set; }
    public string? ICVInfoCode { get; set; }
    public string? ICVInfoMessages { get; set; }
    public string? ICVInfoStatus { get; set; }
    public bool ReUploaded { get; set; } = false;
    public string? InvoiceHash { get; set; }
    public string? InvoicePreviousHash { get; set; }
    public int FK_BranchId { get; set; }
}
