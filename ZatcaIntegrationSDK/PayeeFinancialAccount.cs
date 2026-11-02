using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("B8033279-34A0-49A4-B590-713D5F965AFB")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class PayeeFinancialAccount
    {
        public string ID { get; set; }
        public string paymentnote { get; set; }
    }
}
