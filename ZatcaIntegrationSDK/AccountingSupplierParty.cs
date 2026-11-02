using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("699D816D-EB97-4CF0-8621-BF49FA427E9F")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    public class AccountingSupplierParty
    {
        public PartyIdentification partyIdentification = new PartyIdentification();
        public PostalAddress postalAddress = new PostalAddress();
        public PartyLegalEntity partyLegalEntity = new PartyLegalEntity();
        public PartyTaxScheme partyTaxScheme = new PartyTaxScheme();
    }
}
