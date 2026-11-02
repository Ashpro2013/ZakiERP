using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("3589718F-E887-4E06-9210-BBFA9380260A")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    public class AccountingCustomerParty
    {
        public PartyIdentification partyIdentification = new PartyIdentification();
        public PostalAddress postalAddress = new PostalAddress();
        public PartyLegalEntity partyLegalEntity = new PartyLegalEntity();
        public PartyTaxScheme partyTaxScheme = new PartyTaxScheme();
        public Contact contact = new Contact();
    }
}
