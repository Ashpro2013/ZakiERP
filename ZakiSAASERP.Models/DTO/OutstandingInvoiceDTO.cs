namespace ZakiSAASERP.Models.DTO;

public class OutstandingInvoiceDTO
{
    public int InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime? InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ReturnAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string? CustomerName { get; set; }

    // UI properties
    public bool IsSelected { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal Discount { get; set; }
    public string? Narration { get; set; }
    public decimal RemainingAmount => OutstandingAmount - PaymentAmount - Discount;
    public bool IsOpeningEntry { get; set; }
}

