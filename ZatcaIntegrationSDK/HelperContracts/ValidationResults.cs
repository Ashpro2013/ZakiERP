using System.Collections.Generic;

namespace ZatcaIntegrationSDK.HelperContracts
{
    
    public class ValidationResults
    {
        public List<InfoModel> InfoMessages { get; set; }
        public List<WarningModel> WarningMessages { get; set; }
        public List<ErrorModel> ErrorMessages { get; set; }
        public string Status { get; set; }

    }
}
