using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("97D3BDD3-F053-4DAE-90A1-FE325D11C0BF")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    public class TaxScheme
    {
        public string ID { get; set; } = "VAT";
    }
}
