namespace AshproSAASERP.Models.Settings;

public class ShiftDetails
{
    public decimal TotalSale { get; set; }
    public decimal TotalSalesReturn { get; set; }
    public decimal TotalPurchase { get; set; }
    public decimal TotalCashSales { get; set; }
    public decimal TotalCardSales { get; set; }
    public decimal TotalCreditSales { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal TotalCashReceipt { get; set; }
    public decimal TotalCardReceipt { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public string Terminal { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

