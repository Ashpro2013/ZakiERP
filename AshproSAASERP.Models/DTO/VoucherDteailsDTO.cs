namespace AshproSAASERP.Models.DTO;

public record CustomerReceiptVchrHdr(string Date, string CustomerName, decimal TotalAmount, string? ReceiptNo = null, string? RVNo = null, string? ChequeNo = null, string? BankName = null, string? ChequeDate = null, string? TotalInWords = null);
public record CustomerReceiptVchrDtls(string InvoiceNo, string InvoiceDate, decimal Amount, string? Remarks = null);

public record SupplierPaymentVchrHdr(string Date, string SupplierName, decimal TotalAmount, string? PaymentNo = null, string? RVNo = null, string? ChequeNo = null, string? BankName = null, string? ChequeDate = null, string? TotalInWords = null);
public record SupplierPaymentVchrDtls(string PurchaseNo, string PurchaseDate, decimal Amount, string? Remarks = null);

