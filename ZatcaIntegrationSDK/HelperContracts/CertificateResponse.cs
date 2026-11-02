using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ZatcaIntegrationSDK.HelperContracts
{
   
    public class CertificateResponse
    {
        public CertificateResponse() { }
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public string CSR { get; set; }
        public string PrivateKey { get; set; }
        public string CSID { get; set; }
        //public string BinarySecurityToken { get; set; }
        public string SecretKey { get; set; }
    }
}
