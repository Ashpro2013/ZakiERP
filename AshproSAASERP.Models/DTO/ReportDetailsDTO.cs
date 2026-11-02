namespace AshproSAASERP.Models.DTO;

public record StockReportHdr(string Date, string Time, string TotalStockValue);
public record StockReportDtls(int? SL, string ItemName, decimal StockQty, string Unit, decimal? Rate, decimal StockValue);
public record StockReportResponse(List<StockReportDtls> reports, decimal TotalStockValue);
public record SalesReportHdr(string DateRange, string PrintDate, string TotalGross, string TotalDiscount, string TotalAmount, string TotalVat, string TotalNet);

public record SalesMonitorDtls(int? SL, int Id, string InvoiceNo, string Date, string CustomerName, string SalesmanName, string BillType, decimal Amount, decimal Received, decimal Discount, decimal Return, decimal Balance, string Status);
public record SalesMonitorResponse(List<SalesMonitorDtls> Reports, decimal? TotalAmount, decimal? TotalReceived, decimal? TotalDiscount, decimal? TotalReturn, decimal? TotalBalance, int TotalCount);
public record SalesMonitorRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? CustomerId, int? SalesmanId, string InvoiceType, string BillType, int PageNumber = 0, int PageSize = 0, string Status = "All", string SearchTerm = "");
public record SalesMonitorReceiptItem(string ReceiptNo, int ReceiptId, string Date, decimal Amount, decimal Discount);

public record PurchaseMonitorDtls(int? SL, int Id, string InvoiceNo, string Date, string SupplierName, string BillType, decimal Amount, decimal Paid, decimal Discount, decimal Return, decimal Balance, string Status);
public record PurchaseMonitorResponse(List<PurchaseMonitorDtls> Reports, decimal? TotalAmount, decimal? TotalPaid, decimal? TotalDiscount, decimal? TotalReturn, decimal? TotalBalance, int TotalCount);
public record PurchaseMonitorRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? SupplierId, string BillType, int PageNumber = 0, int PageSize = 0, string Status = "All", string SearchTerm = "");
public record PurchaseMonitorPaymentItem(string PaymentNo, int PaymentId, string Date, decimal Amount, decimal Discount);

public record TransactionMonitorDtls(int? SL, int Id, string InvoiceNo, string Date, string PartyName, decimal Amount, string QuotNo, string RefNo, string SalesNo, string DeliveryNo, string ProformaNo);
public record TransactionMonitorResponse(List<TransactionMonitorDtls> Reports, decimal? TotalAmount, int TotalCount);
public record TransactionMonitorRequest(int BranchId, DateTime FromDate, DateTime ToDate, string TransactionType, int? PartyId, string SearchTerm = "");

public record SalesReportDtls(int? SL, int? Id, string? InvoiceNo, string? Date, string? CustomerName, string? SalesmanName, string? BillType, string? InvType, decimal? GrossAmount, decimal? DiscountAmount, decimal? Amount, decimal? VatAmount, decimal? NetAmount);
public record SalesReportResponse(List<SalesReportDtls> Reports, decimal? TotalGross, decimal? TotalDiscount, decimal? TotalAmount, decimal? TotalVat, decimal? TotalNet);

public record SalesReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? CustomerId, int? SalesmanId, string InvoiceType, string BillType);
public record SalesReturnReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? CustomerId, int? SalesmanId, string InvoiceType, string BillType);
public record SalesReturnReportHdr(string DateRange, string PrintDate, string TotalGross, string TotalDiscount, string TotalAmount, string TotalVat, string TotalNet);
public record SalesReturnReportDtls(int? SL, int? Id, string? InvoiceNo, string? Date, string? CustomerName, string? SalesmanName, string? BillType, string? InvType, decimal? GrossAmount, decimal? DiscountAmount, decimal? Amount, decimal? VatAmount, decimal? NetAmount);
public record SalesReturnReportResponse(List<SalesReturnReportDtls> Reports, decimal? TotalGross, decimal? TotalDiscount, decimal? TotalAmount, decimal? TotalVat, decimal? TotalNet);

public record SalesDetailsReportDtls(int? SL, int? Id, string? InvoiceNo, string? Date, string? CustomerName, string? SalesmanName, string? ItemCode, string? ItemName, string? Unit, decimal Quantity, decimal Rate, decimal Amount, decimal? GrossAmount, decimal? DiscountAmount, decimal? TaxAmount, decimal? TotalAmount, string? User, string? InvType, decimal? ExRate);
public record SalesDetailsReportResponse(List<SalesDetailsReportDtls> Reports, decimal? TotalGross, decimal? TotalDiscount, decimal? TotalTax, decimal? TotalAmount);
public record SalesDetailsReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? CustomerId, int? SalesmanId, string InvoiceType, string BillType, string ItemName);
public record SalesReturnDetailsReportDtls(int? SL, int? Id, string? InvoiceNo, string? Date, string? CustomerName, string? SalesmanName, string? ItemCode, string? ItemName, string? Unit, decimal Quantity, decimal Rate, decimal Amount, decimal? GrossAmount, decimal? DiscountAmount, decimal? TaxAmount, decimal? TotalAmount, string? User, string? InvType, decimal? ExRate);
public record SalesReturnDetailsReportResponse(List<SalesReturnDetailsReportDtls> Reports, decimal? TotalGross, decimal? TotalDiscount, decimal? TotalTax, decimal? TotalAmount);
public record SalesReturnDetailsReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? CustomerId, int? SalesmanId, string InvoiceType, string BillType, string ItemName);
public record SalesReturnDetailsReportHdr(string DateRange, string PrintDate, string TotalGross, string TotalDiscount, string TotalAmount, string TotalVat, string TotalNet);
public record StockReportRequest
{
    public DateTime? Date { get; set; }
    public int ReportType { get; set; } = 0;
    public int BranchId { get; set; } = 0;
    public int StoreId { get; set; } = 0;
    public int CategoryId { get; set; } = 0;
    public int BrandId { get; set; } = 0;
}
public record PurchaseReportHdr(string DateRange, string PrintDate, string TotalGross, string TotalDiscount, string TotalVat, string TotalNet);
public record PurchaseReportDtls(int? SL, string? PurchaseNo, string? Date, string? BillType, string? VatNo, string? SupplierName, decimal? GrossAmount, decimal? DiscountAmount, decimal? VatAmount, decimal? NetAmount);
public record PurchaseReportResponse(List<PurchaseReportDtls> Reports, decimal? TotalGross, decimal? TotalDiscount, decimal? TotalVat, decimal? TotalNet);
public record PurchaseReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? SupplierId, string BillType);
public record PurchaseDetailsReportDtls(int? SL, string? Date, string? PurchaseNo, string? InvoiceNo, string? ItemCode, string? ItemName, decimal Quantity, decimal? Rate, decimal Amount);
public record PurchaseDetailsReportResponse(List<PurchaseDetailsReportDtls> Reports, decimal? TotalQuantity, decimal? TotalAmount);
public record PurchaseDetailsReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? SupplierId, string? ItemName);
public record PurchaseReturnDetailsReportDtls(int? SL, string? Date, string? PurchaseReturnNo, string? InvoiceNo, string? ItemCode, string? ItemName, decimal Quantity, decimal? Rate, decimal Amount);
public record PurchaseReturnDetailsReportResponse(List<PurchaseReturnDetailsReportDtls> Reports, decimal? TotalQuantity, decimal? TotalAmount);
public record PurchaseReturnDetailsReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? SupplierId, string? ItemName);
public record CategoryWiseSalesReportRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int CategoryId { get; set; }
    public string? InvoiceType { get; set; }
    public string? BillType { get; set; }
}
public record CategoryWiseSalesReportHdr(string Date, string Time, string GrossTotal, string DiscountTotal, string AmountTotal, string VatTotal, string NetTotal);
public record CategoryWiseSalesReportDtls(int? SL, string? CategoryName, decimal? GrossAmount, decimal? DiscountAmount, decimal? Taxable, decimal? VatAmount, decimal? NetAmount);
public record CategoryWiseSalesReportResponse(List<CategoryWiseSalesReportDtls> Items, decimal? TotalGross, decimal? TotalDiscount, decimal? TotalTaxable, decimal? TotalVat, decimal? TotalNet);
public record SalesQuotationRegisterRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record SalesQuotationRegisterHdr(string Date, string Time, string TotalVat, string TotalNet);
public record SalesQuotationRegisterDtls(int? SL, string? Date, string? QuotationNo, string? RefNo, string? CustomerName, string? Address, string? Telephone, decimal? VatAmount, decimal? NetAmount);
public record SalesQuotationRegisterResponse(List<SalesQuotationRegisterDtls> Items, decimal? TotalVat, decimal? TotalNet);
public record CustomerWiseSalesReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? CustomerId, int? SalesmanId, string InvoiceType, string BillType);
public record CustomerWiseSalesReportHdr(string Date, string Time);
public record CustomerWiseSalesReportDtls(int? SL, int? Id, string? InvoiceNo, string? Date, string? CustomerName, string? SalesmanName, string? BillType, string? InvType, decimal? GrossAmount, decimal? DiscountAmount, decimal? Amount, decimal? VatAmount, decimal? NetAmount);
public record CustomerWiseSalesReportResponse(List<CustomerWiseSalesReportDtls> Items, decimal? TotalVat, decimal? TotalNet);
public record CustomerWiseQuotationReportRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int CustomerId { get; set; }
}
public record CustomerWiseQuotationReportHdr(string Date, string Time, string TotalVat, string TotalNet);
public record CustomerWiseQuotationReportDtls(int? SL, string? Date, string? QuotationNo, string? RefNo, string? CustomerName, decimal? VatAmount, decimal? NetAmount);
public record CustomerWiseQuotationReportResponse(List<CustomerWiseQuotationReportDtls> Items, decimal? TotalVat, decimal? TotalNet);
public record DeliveryNoteRegisterRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record DeliveryNoteRegisterHdr(string Date, string Time, string TotalVat, string TotalNet);
public record DeliveryNoteRegisterDtls(int? SL, string? Date, string? DeliveryNo, string? RefNo, string? CustomerName, string? Address, string? Telephone, decimal? VatAmount, decimal? NetAmount);
public record DeliveryNoteRegisterResponse(List<DeliveryNoteRegisterDtls> Items, decimal? TotalVat, decimal? TotalNet);
public record DateWiseProfitReportRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record DateWiseProfitReportHdr(string Date, string Time, string TotalSalesAmount, string TotalSalesReturn, string TotalNetSales, string TotalCost, string TotalProfit, string TotalProfitPercentage);
public record DateWiseProfitReportDtls(int? SL, string? Date, decimal SalesAmount, decimal SalesReturn, decimal NetSales, decimal Cost, decimal ProfitAmount, decimal ProfitPercentage);
public record DateWiseProfitReportResponse(List<DateWiseProfitReportDtls> Items, decimal? TotalSalesAmount, decimal? TotalSalesReturn, decimal? TotalNetSales, decimal? TotalCost, decimal? TotalProfit, decimal? TotalProfitPercentage);
public record PurchaseOrderReportRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? SupplierId { get; set; }
}
public record PurchaseOrderReportHdr(string Date, string Time, string TotalVat, string TotalNet);
public record PurchaseOrderReportDtls(int? SL, string? Date, string? PurOrderNo, string? RefNo, string? SupplierName, decimal? VatAmount, decimal? TotalAmount);
public record PurchaseOrderReportResponse(List<PurchaseOrderReportDtls> Items, decimal? TotalVat, decimal? TotalNet);
public record ProformaInvoiceReportRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record ProformaInvoiceReportHdr(string Date, string Time, string TotalDiscount, string TotalVat, string TotalNet);
public record ProformaInvoiceReportDtls(int? SL, string? Date, string? InvNo, string? BillType, string? CustomerName, decimal? Discount, decimal? VatAmount, decimal? TotalAmount);
public record ProformaInvoiceReportResponse(List<ProformaInvoiceReportDtls> Items, decimal? TotalDiscount, decimal? TotalVat, decimal? TotalNet);
public record SalesOrderReportRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CustomerId { get; set; }
}
public record SalesOrderReportHdr(string Date, string Time, string TotalVat, string TotalNet);
public record SalesOrderReportDtls(int? SL, string? Date, string? SalesOrderNo, string? RefNo, string? CustomerName, decimal? VatAmount, decimal? TotalAmount);
public record SalesOrderReportResponse(List<SalesOrderReportDtls> Items, decimal? TotalVat, decimal? TotalNet);
public record JournalEntryReportRequest
{
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? AccountId { get; set; }
}
public record JournalEntryReportHdr(string Date, string Time, string TotalDebit, string TotalCredit);
public record JournalEntryReportDtls(int? SL, string? AccountCode, string? AccountName, string? InvNo, string? Date, string? Narration, decimal? Debit, decimal? DebitVat, decimal? Credit, decimal? CreditVat);
public record JournalEntryReportResponse(List<JournalEntryReportDtls> Items, decimal? TotalDebit, decimal? TotalCredit);
public record StockTransferReportRequest
{
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record StockTransferReportHdr(string Date, string Time, string TotalAmount);
public record StockTransferReportDtls(int? SL, string? Date, string? DocNo, string? ItemName, decimal? Quantity, decimal? Rate, decimal? Amount);
public record StockTransferReportResponse(List<StockTransferReportDtls> Items, decimal? TotalAmount);
public class TrialBalanceReportRequest
{
    public int BranchId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool ShowZeroBalance { get; set; }
}
public record TrialBalanceReportHdr(string Date, string Time, string TotalOpening, string TotalDebit, string TotalCredit, string TotalClosing);
public record TrialBalanceReportResponse
{
    public List<TrialBalanceReportDtls> Items { get; set; } = new();
    public decimal TotalOpening { get; set; }
    public decimal TotalTransactionDebit { get; set; }
    public decimal TotalTransactionCredit { get; set; }
    public decimal TotalClosing { get; set; }
}
public record TrialBalanceReportDtls(int? Id, string AccountCode, string AccountName, decimal Opening, decimal TransactionDebit, decimal TransactionCredit, decimal Closing);
public record ExpenseReportRequest
{
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record ExpenseReportHdr(string Date, string Time, string TotalAmount, string TotalVat, string TotalNet);
public record ExpenseReportDtls(int? SL, string? Date, string? DocNo, string? AccountCode, string? AccountName, decimal? Amount, decimal? VatAmount, decimal? TotalAmount);
public record ExpenseReportResponse(List<ExpenseReportDtls> Items, decimal? TotalAmount, decimal? TotalVat, decimal? TotalNet);
public record ProfitAndLossReportRequest(
    int? BranchId,
    DateTime? FromDate,
    DateTime? ToDate
);
public record ProfitAndLossPrintItem(
    string AccountCode,
    string AccountName,
    decimal Amount,
    string Category // "Income", "Expense", "COGS"
);
public record ProfitAndLossReportResponse(
    List<ProfitAndLossPrintItem> Items,
    decimal TotalIncome,
    decimal TotalCOGS,
    decimal TotalExpense,
    decimal GrossProfit,
    decimal NetProfit,
    decimal OpeningStock,
    decimal ClosingStock
);
public record ProfitAndLossReportHdr(
    string DateRange,
    string PrintDate,
    string TotalIncome,
    string TotalCOGS,
    string TotalExpense,
    string GrossProfit,
    string NetProfit
);
public class ProfitAndLossReportDtls
{
    public string DrAccount { get; set; }
    public string DrAmount { get; set; }
    public string CrAccount { get; set; }
    public string CrAmount { get; set; }
}
public record BalanceSheetReportRequest(
    int? BranchId,
    DateTime? FromDate,
    DateTime? ToDate
);
public record BalanceSheetPrintItem(
    string AccountCode,
    string AccountName,
    decimal Amount,
    string Category // "Asset", "Liability", "Capital"
);
public record BalanceSheetReportResponse(
    List<BalanceSheetPrintItem> Items,
    decimal TotalLiabilities,
    decimal TotalAssets,
    decimal NetProfitLoss, // From P&L
    decimal ClosingStock,
    decimal AccountBalanceDifference
);
public record BalanceSheetReportHdr(
    string DateRange,
    string PrintDate,
    string TotalLiabilities,
    string TotalAssets
);
public class BalanceSheetReportDtls
{
    public string LiabilityAccount { get; set; }
    public string LiabilityAmount { get; set; }
    public string AssetAccount { get; set; }
    public string AssetAmount { get; set; }
}
public record EnquiryRegistrationReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? CustomerId, int? SalesmanId);
public record EnquiryRegistrationReportHdr(string Date, string Time);
public record EnquiryRegistrationReportDtls(int? SL, string? Date, string? EnquiryNo, string? CustomerName, string? SalesmanName, string? Remarks, string? CustomerRefNo, string? User);
public record EnquiryRegistrationReportResponse(List<EnquiryRegistrationReportDtls> Items);
public record ChartOfAccountReportRequest(
    int? AccountGroupId
);
public record ChartOfAccountReportHdr(
    string Date,
    string Time
);
public record ChartOfAccountReportDtls(
    int SL,
    string AccountGroup,
    string AccountCode,
    string AccountName
);
public record ChartOfAccountReportResponse(
    List<ChartOfAccountReportDtls> Items
);
public class ItemTransactionHistoryReportRequest
{
    public int? BranchId { get; set; }
    public int? StoreId { get; set; }
    public int? ItemId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record ItemTransactionHistoryReportHdr(
    string DateRange,
    string PrintDate,
    string TotalInQty,
    string TotalOutQty,
    string ItemName
);
public record ItemTransactionHistoryReportDtls(
    int SL,
    string Date,
    string VoucherType,
    string VoucherNo,
    decimal InQty,
    decimal OutQty,
    string Unit,
    decimal Rate,
    string BranchName,
    string StoreName
);
public record ItemTransactionHistoryReportResponse(
    List<ItemTransactionHistoryReportDtls> Items,
    decimal? TotalInQty,
    decimal? TotalOutQty
);
public record StockAdjustmentReportRequest
{
    public int BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record StockAdjustmentReportHdr(string Date, string Time, string TotalQty, string TotalAmount);
public record StockAdjustmentReportDtls(int? SL, string? Date, string? VoucherNo, string? RefNo, string? ItemCode, string? ItemName, decimal? CurrentQty, decimal? Quantity, decimal? NewQty, string? Unit, decimal? Amount, string? Remarks);
public record StockAdjustmentReportResponse(List<StockAdjustmentReportDtls> Items, decimal? TotalQty, decimal? TotalAmount);
public record VatReportRequest
{
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public record VatReportHdr(string Date, string Time, string TotalTaxable, string TotalVatAmount);
public record VatReportDtls(int? SL, string? InvoiceType, decimal? TaxPercentage, decimal? Taxable, decimal? VatAmount);
public record VatReportResponse(List<VatReportDtls> Items, decimal? TotalTaxable, decimal? TotalVatAmount);
public record LedgerReportRequest
{
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? AccountId { get; set; }
}
public record LedgerReportHdr(string DateRange, string PrintDate, string TotalDebit, string TotalCredit, string TotalBalance);
public record LedgerReportDtls(int? SL, int? Id, string? Date, string? VoucherNo, string? VoucherType, string? Narration, decimal? Debit, decimal? Credit, string Balance);
public record LedgerReportResponse(List<LedgerReportDtls> Items, decimal? TotalDebit, decimal? TotalCredit, string TotalBalance, string OpeningBalance);
public record PurchaseReturnReportRequest(int BranchId, DateTime FromDate, DateTime ToDate, int? SupplierId, string BillType);
public record PurchaseReturnReportHdr(string DateRange, string PrintDate, string TotalGross, string TotalDiscount, string TotalVat, string TotalNet);
public record PurchaseReturnReportDtls(int? SL, string? ReturnNo, string? Date, string? BillType, string? VatNo, string? SupplierName, decimal? GrossAmount, decimal? DiscountAmount, decimal? VatAmount, decimal? NetAmount);
public record PurchaseReturnReportResponse(List<PurchaseReturnReportDtls> Reports, decimal? TotalGross, decimal? TotalDiscount, decimal? TotalVat, decimal? TotalNet);

// Item Stock Info DTOs
public record ItemStockInfoDTO(decimal OpeningStock, decimal CurrentStock, List<StoreStockDTO> StoreWiseStock);
public record StoreStockDTO(string StoreName, string BranchName, decimal Quantity);