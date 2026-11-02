namespace AshproSAASERP.Models.DTO
{
    public class SupplierPendingInvoiceDTO
    {
        public int? SL { get; set; }
        public DateTime? Date { get; set; }
        public string? VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public int? VoucherId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public string Narration { get; set; }

        public string PaymentMode { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public decimal Discount { get; set; }
    }
}
