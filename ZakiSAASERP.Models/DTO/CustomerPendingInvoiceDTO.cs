namespace ZakiSAASERP.Models.DTO
{
    public class CustomerPendingInvoiceDTO
    {
        public int? SL { get; set; }
        public DateTime? Date { get; set; }
        public string? VoucherNo { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public int? VoucherId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;

        public string PaymentMode { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public decimal Discount { get; set; }
    }
}
