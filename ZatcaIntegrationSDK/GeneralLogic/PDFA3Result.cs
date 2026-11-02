using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK.GeneralLogic
{
    public class PDFA3Result
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string PDFA3FileNameFullPath { get; set; }
        public string PDFA3FileName { get; set; }
        public byte[] PDFA3ContentFile { get; set; }
        //public string PDFA3FileNameShortPath { get; set; }
    }
}
