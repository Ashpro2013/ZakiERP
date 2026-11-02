using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK.GeneralLogic
{
    public class SchematronResult
    {
        public List<string> Errors { get; set; } = new List<string>();

        public List<string> Warnings { get; set; } = new List<string>();
    }
}
