using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class DocumentReference
    {
        public string ID { get; set; }
        public string UUID { get; set; }
        
        public string IssueDate { get; set; }
        public string IssueTime { get; set; }
        public int DocumentTypeCode { get; set; }
    }
}
