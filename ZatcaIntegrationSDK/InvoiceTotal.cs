using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class InvoiceTotal
    {
        public decimal LineExtensionAmount { set; get; } // sum of invoice line net amount
        public decimal TaxExclusiveAmount { set; get; } // total amount of invoice without vat
        public decimal TaxInclusiveAmount { set; get; }
        public decimal AllowanceTotalAmount { set; get; } // invoice total amount - net amount
        public decimal ChargeTotalAmount { set; get; }
        public InvoiceLineCollection InvoiceLines = new InvoiceLineCollection();


    }
}
