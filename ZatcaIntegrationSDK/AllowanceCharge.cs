using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class AllowanceCharge
    {
        public string ID { get; set; }
        public bool ChargeIndicator { get; set; } = false; // 
        public decimal MultiplierFactorNumeric { get; set; } // من صفر لحد 100 ورقمين بعد العلامة
        public decimal Amount { get; set; } // 
        public string AllowanceChargeReason { get; set; }
        public string AllowanceChargeReasonCode { get; set; }
        public decimal BaseAmount { get; set; }

        public TaxCategory taxCategory = new TaxCategory();


    }
}
