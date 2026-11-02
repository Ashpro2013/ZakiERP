using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class TaxTotal
    {
        public decimal TaxAmount { set; get; }
        public decimal RoundingAmount { set; get; }
        
        public TaxSubtotal TaxSubtotal = new TaxSubtotal();
        //public TaxSubtotalCollection TaxSubtotal = new TaxSubtotalCollection();
    }
}
