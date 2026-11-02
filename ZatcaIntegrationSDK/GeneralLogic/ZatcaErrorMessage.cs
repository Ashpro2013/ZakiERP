using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZatcaIntegrationSDK
{
    public class ZatcaErrorMessage
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string BusinessRules { get; set; }
        public string MessageEN { get; set; }
        public string MessageAR { get; set; }
    }
}
