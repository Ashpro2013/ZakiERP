using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("1B5D929C-F65F-4C56-9A97-C67DBCEFF983")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    public class InvoiceLine
    {
        public string ID { set; get; }
        public decimal InvoiceQuantity { set; get; }
        public decimal LineExtensionAmount { set; get; }
        
        //public AllowanceCharge allowanceCharge = new AllowanceCharge();
        public AllowanceChargeCollection allowanceCharges = new AllowanceChargeCollection();
        public TaxTotal taxTotal = new TaxTotal();
        public Item item = new Item();
        public Price price = new Price();
        public DocumentReferenceCollection documentReferences = new DocumentReferenceCollection();
    }
}
