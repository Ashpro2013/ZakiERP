using System.Collections.Generic;

namespace ZatcaIntegrationSDK.HelperContracts
{
    
    public class InvoiceResultModel
    {
        public string InvoiceHash { get; set; }
        public string Status { get; set; } //Reported, Not Reported, Accepted with Warnings
        public List<WarningModel> Warnings { get; set; }
        public List<ErrorModel> Errors { get; set; }
    }
}
