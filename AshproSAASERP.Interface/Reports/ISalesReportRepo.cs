namespace AshproSAASERP.Interface.Reports;

public interface ISalesReportRepo
{
    Task<SalesReportResponse> GetSalesReportAsync(SalesReportRequest req);
    Task<SalesReturnReportResponse> GetSalesReturnReportAsync(SalesReturnReportRequest req);
    Task<CustomerWiseSalesReportResponse> GetCustomerWiseSalesReportAsync(CustomerWiseSalesReportRequest req);
    Task<SalesDetailsReportResponse> GetSalesDetailsAsync(SalesDetailsReportRequest req);
    Task<SalesReturnDetailsReportResponse> GetSalesReturnDetailsAsync(SalesReturnDetailsReportRequest req);
    Task<CategoryWiseSalesReportResponse> GetCategoryWiseSalesReportAsync(CategoryWiseSalesReportRequest req);
    Task<SalesQuotationRegisterResponse> GetSalesQuotationRegisterAsync(SalesQuotationRegisterRequest req);
    Task<CustomerWiseQuotationReportResponse> GetCustomerWiseQuotationReportAsync(CustomerWiseQuotationReportRequest req);
    Task<DeliveryNoteRegisterResponse> GetDeliveryNoteRegisterAsync(DeliveryNoteRegisterRequest req);
    Task<DateWiseProfitReportResponse> GetDateWiseProfitReportAsync(DateWiseProfitReportRequest req);
    Task<ProformaInvoiceReportResponse> GetProformaInvoiceReportAsync(ProformaInvoiceReportRequest req);
    Task<SalesOrderReportResponse> GetSalesOrderReportAsync(SalesOrderReportRequest req);
    Task<EnquiryRegistrationReportResponse> GetEnquiryRegistrationReportAsync(EnquiryRegistrationReportRequest req);
    Task<VatReportResponse> GetVatReportAsync(VatReportRequest req);
    Task<SalesMonitorResponse> GetSalesMonitorReportAsync(SalesMonitorRequest req);
    Task<List<SalesMonitorReceiptItem>> GetReceiptsByInvoiceAsync(int invoiceId);
}
