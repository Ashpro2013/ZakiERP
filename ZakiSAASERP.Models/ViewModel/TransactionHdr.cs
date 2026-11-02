namespace ZakiSAASERP.Models.ViewModel;

public class TransactionHdr
{
    public int HdrId { get; set; }
    public string HdrNo { get; set; } = default!;
    public DateTime HdrDate { get; set; }
    public string? RefNo { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal LineDsicount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? AditionalAmount { get; set; }
    public decimal? Freight { get; set; }
    public decimal? AdditionalDiscPertge { get; set; }
    public decimal? AdditionalDiscAmnt { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TotalQuantity { get; set; }
    public string? Remarks { get; set; }
    public string? InvoiceDate { get; set; }
    public string? QRCode { get; set; }
    public byte[]? Image { get; set; }
    public string? VoucherType { get; set; }
    public string? FromBranch { get; set; }
    public string? ToBranch { get; set; }
    public string? EnteredBy { get; set; }
    public int PartyId { get; set; }
    public int FK_StoreId { get; set; }
    public int FK_BranchId { get; set; }
    public int FK_UserId { get; set; }
    public string? HijriDate { get; set; }
    public int? DeliveryNoteId { get; set; }
    public int? SalesInvoiceId { get; set; }
    public string? InvoiceQRCode { get; set; }

    // Approval/Rejection specific fields for printing
    public string? ApprovedBy { get; set; }
    public string? ApprovedDate { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectedDate { get; set; }
    public string? RejectionReason { get; set; }
    public string? CustomerName { get; set; }
    public string? SalesMan { get; set; }
    public string? Extra1 { get; set; }
    public string? Extra2 { get; set; }
    public string? Extra3 { get; set; }
    public string? Extra4 { get; set; }
    public string? Extra5 { get; set; }
    public string? Extra6 { get; set; }
    public string? Extra7 { get; set; }
    public string? Extra8 { get; set; }
    public string? Extra9 { get; set; }
    public string? Extra10 { get; set; }
    public decimal? Tendered { get; set; }
    public decimal? Changes { get; set; }
}
