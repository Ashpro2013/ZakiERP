using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK.HelperContracts
{
    public static class ExtensionsMethods
    {
        public static string ToWarnings(this List<WarningModel> warning)
        {
            if (warning != null && warning.Count > 0)
                return string.Join("&", warning.Select(x => string.Concat(x.Code, " : ", x.Message)));
            else
                return "";
        }
        public static string ToWarning(this List<WarningModel> warning)
        {
            if (warning != null && warning.Count > 0)
                return string.Join(Environment.NewLine, warning.Select(x => string.Concat(x.Code, " : ", x.Message)));
            else
                return "";
            
        }
    }
}
