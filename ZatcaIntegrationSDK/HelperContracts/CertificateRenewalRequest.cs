using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ZatcaIntegrationSDK.APIHelper;
namespace ZatcaIntegrationSDK.HelperContracts
{
  
    public class CertificateRenewalRequest
    {
        public CertificateRenewalRequest()
        {
        }
        public string OldCSR { get; set; }
        public string OldPublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string OldSecret { get; set; }
        public string InvoiceType { get; set; }
        public string OTP { get; set; }
        public Mode Mode { get; set; }

    }
}
