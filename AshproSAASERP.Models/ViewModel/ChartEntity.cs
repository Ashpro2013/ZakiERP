namespace AshproSAASERP.Models.ViewModel
{
    public class ChartEntity
    {
        public string CashInHand { get; set; } = "0.00";
        public string CashAtBank { get; set; } = "0.00";
        public string Payable { get; set; } = "0.00";
        public string Receivable { get; set; } = "0.00";
        public List<ChartValue> GroupwiseSalesList { get; set; } = new();
        public List<ChartValue> GroupwiseStockList { get; set; } = new();
        public List<ChartListStatement> SalesList { get; set; } = new();
        public List<ChartListStatement> PurchaseList { get; set; } = new();
        public List<ChartValue> DailySalesList { get; set; } = new();
        public List<ChartValue> DailyPurchaseList { get; set; } = new();

    }
}
