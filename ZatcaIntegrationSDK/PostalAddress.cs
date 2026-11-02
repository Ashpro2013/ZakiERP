using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("862C833B-3A1E-4404-B18F-C662BB45CC82")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    public class PostalAddress
    {
        public string StreetName { get; set; }
        public string AdditionalStreetName { get; set; }
        public string BuildingNumber { get; set; }
        public string PlotIdentification { get; set; }
        public string CityName { get; set; }
        public string PostalZone { get; set; }
        public string CountrySubentity { get; set; }
        public string CitySubdivisionName { get; set; }

        public Country country = new Country();
       

    }
}
