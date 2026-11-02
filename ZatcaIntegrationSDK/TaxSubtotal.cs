using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class TaxSubtotal
    {
        public decimal TaxableAmount { set; get; }
        public decimal TaxAmount { set; get; }
        public TaxCategory taxCategory = new TaxCategory();
    }
}
