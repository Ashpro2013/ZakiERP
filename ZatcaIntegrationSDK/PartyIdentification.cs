using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("FB892F0B-43FC-429D-A163-FB752E4898E9")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    public class PartyIdentification
    {
        /// <summary>
        /// رقم المعرف الخاص 
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// عند ادخال الرقم التعريفى لابد وان ندخل الرمز الخاص به  
        /// CRN  فى حالة رقم التسجيل التجارى
        /// MOM وزارة الشؤون البلدية و القروية والاسكان
        /// MLS وزارة العمل والتنمية الاجتماعية
        /// SAG التراخيص الاستثمارية
        /// OTH اى شىء آخر
        /// </summary>
        public string schemeID { get; set; }
    }
}
