namespace ZatcaIntegrationSDK.HelperContracts
{
    public class InvoiceReportingResponse
    {
        public ValidationResults validationResults { get; set; }
        public string ReportingStatus { get; set; }
        public string ClearanceStatus { get; set; }
        public object QrSellertStatus { get; set; }
        public object QrBuyertStatus { get; set; }
        public string ErrorMessage { get; set; }
        public string WarningMessage
        {
            get
            {
                if (this.validationResults != null)
                {
                    if (this.validationResults.WarningMessages == null)
                    { return string.Empty; }
                    else
                        return validationResults.WarningMessages.ToWarnings();
                }
                return string.Empty;
            }
        }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        
       
    }
}
