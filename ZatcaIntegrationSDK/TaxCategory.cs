using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class TaxCategory
    {
        /// <summary>
        /// S means standard rated
        /// Z means Zero rated
        /// E means Exempt from vat
        /// O means Not Subject to VAT
        /// </summary>
        public string ID { set; get; }
        public decimal Percent { set; get; }
        
        public string TaxExemptionReason { set; get; }
        public string TaxExemptionReasonCode { set; get; }
        
        public TaxScheme taxScheme = new TaxScheme();
    }
}
