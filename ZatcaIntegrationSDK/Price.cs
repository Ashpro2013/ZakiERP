using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class Price
    {
        public decimal PriceAmount { set; get; }
        //LineExtensionAmount=(PriceAmount/BaseQuantity)*InvoicedQuantity
        public decimal BaseQuantity { set; get; }
        
        public bool EncludingVat { set; get; }
        public AllowanceCharge allowanceCharge = new AllowanceCharge();

    }
}
