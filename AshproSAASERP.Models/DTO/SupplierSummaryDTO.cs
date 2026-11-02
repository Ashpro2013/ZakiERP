namespace AshproSAASERP.Models.DTO
{
    public class SupplierSummaryDTO
    {
        public int? SL { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}
