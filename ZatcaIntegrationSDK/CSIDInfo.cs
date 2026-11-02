using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ZatcaIntegrationSDK
{
    [Guid("E49AB26B-B281-409B-898C-DF9B77335833")]
    [ClassInterface(ClassInterfaceType.AutoDual)]

    public class CSIDInfo
    {
        public string CertPem { get; set; }
        public string CSR { get; set; }
        public string PrivateKey { get; set; }
        public string Secret { get; set; }
    }
}
