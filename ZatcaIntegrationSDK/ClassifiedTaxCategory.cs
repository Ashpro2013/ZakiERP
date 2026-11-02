using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class ClassifiedTaxCategory
    {
        public string ID {set; get;}
        public decimal Percent { set; get; }
        public TaxScheme taxScheme = new TaxScheme();
    }
}
