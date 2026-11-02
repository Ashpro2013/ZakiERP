using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class Item
    {
        public string Name { set; get; }
        public ClassifiedTaxCategory classifiedTaxCategory = new ClassifiedTaxCategory();
        public string BuyersItemIdentificationID { set; get; }
        public string SellersItemIdentificationID { set; get; }
        public string StandardItemIdentificationID { set; get; }

    }
}
