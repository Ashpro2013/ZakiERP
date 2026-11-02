using System.Collections.Generic;

namespace ZatcaIntegrationSDK.HelperContracts
{
    
    public class ClearedInvoiceResultModel
    {
        public string InvoiceHash { get; set; }
        public string ClearedInvoice { get; set; }
        public string Status { get; set; } //Cleared, Not Cleared
        public List<WarningModel> Warnings { get; set; }
        public List<ErrorModel> Errors { get; set; }
    }
}
