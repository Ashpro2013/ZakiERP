namespace AshproSAASERP.Desk.zatca
{
    public class InvoiceItems : ZatcaZeroTaxReason
    {
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal TotalPrice => ProductPrice * ProductQuantity;
        public decimal DiscountValue { get; set; }
        public decimal TotalPriceAfterDiscount => TotalPrice - DiscountValue;
        public decimal VatPercentage { get; set; }
        public decimal VatValue { get; set; }
        public decimal TotalWithVat => TotalPriceAfterDiscount + VatValue;
        public bool IsInclusive { get; set; }
    }
}

