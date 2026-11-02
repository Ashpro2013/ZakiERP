namespace AshproSAASERP.Models.DTO
{
    public class SupplierPaymentReportDTO
    {
        public int? SL { get; set; }
        public DateTime? Date { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public string PaymentMode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public string ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string Bank { get; set; }
    }
}
