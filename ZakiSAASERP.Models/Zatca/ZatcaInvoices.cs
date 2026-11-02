namespace ZakiSAASERP.Models.Zatca;

public class ZatcaInvoices
{
    public int Id { get; set; }
    public int FK_BranchId { get; set; }
    public string invoiceHash { get; set; }
    public string uuid { get; set; }
    public string EncodedInvoice { get; set; }
    public string InvoiceType { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string InvNo { get; set; }
    public int InvoiceId { get; set; }
    public string Code { get; set; }
    public string ZatcaCertificate { get; set; }
    public string ZatcaSecret { get; set; }
    public string SMode { get; set; }
    public int InvoiceIcv { get; set; }
}

