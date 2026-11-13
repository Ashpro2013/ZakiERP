using ZakiSAASERP.ZatcaEngine.Models;

public class Invoice
{
    public string Uuid { get; set; } = Guid.NewGuid().ToString();
    public string InvoiceNumber { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public int InvoiceTypeCode { get; set; } = 388;
    public string? InvoiceSubType { get; set; }

    public Party Seller { get; set; } = new();
    public Party Buyer { get; set; } = new();

    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }

    public List<InvoiceLine> Lines { get; set; } = new();

    public string PaymentMeansCode { get; set; } = "10";
    public string PaymentInstruction { get; set; } = "Payment Notes";

    public List<Allowance> Allowances { get; set; } = new();

    public string PreviousInvoiceHash { get; set; } = "";
    public int Icv { get; set; }
    public string? BillingReferenceId { get; set; }
}
