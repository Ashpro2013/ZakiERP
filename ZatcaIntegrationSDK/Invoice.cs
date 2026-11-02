using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("16AB957F-2B2F-4575-96C6-D7CE759B2638")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    public class Invoice
    {
        public Invoice()
        {
        }
        public string ProfileID { get; set; } // must be reporting:1.0
        public string ID { get; set; } //  IRN
        public string UUID { get; set; } 
        public string IssueDate { get; set; }
        public string IssueTime { get; set; }
        public InvoiceTypeCode invoiceTypeCode = new InvoiceTypeCode();
        public string Note { get; set; } // optional
        public string DocumentCurrencyCode { get; set; } //SAR او اى عملة اخرى
        public string TaxCurrencyCode { get; set; } //SAR لابد تكون
        public int LineCountNumeric { get; set; }
        public OrderReference orderReference = new OrderReference();
        public BillingReference billingReference = new BillingReference(); // فى حالة اشعار دائن او مدين فقط
        public ContractDocumentReference contractDocumentReference = new ContractDocumentReference(); //رقم العقد اختيارى
        
        public AdditionalDocumentReference AdditionalDocumentReferenceICV = new AdditionalDocumentReference(); // Invoice counter value
        public AdditionalDocumentReference AdditionalDocumentReferencePIH = new AdditionalDocumentReference(); //
        public AdditionalDocumentReference AdditionalDocumentReferenceQR = new AdditionalDocumentReference();
        
        public AccountingSupplierParty SupplierParty = new AccountingSupplierParty();
        public AccountingCustomerParty CustomerParty = new AccountingCustomerParty();

        public Delivery delivery = new Delivery(); //فى حالة فاتورة مبسطة وفاتورة ملخصة

        public PaymentMeansCollection paymentmeans = new PaymentMeansCollection();
        public TaxTotal TaxTotal = new TaxTotal();
        public AllowanceChargeCollection allowanceCharges = new AllowanceChargeCollection();
        public LegalMonetaryTotal legalMonetaryTotal = new LegalMonetaryTotal();

        public InvoiceLineCollection InvoiceLines = new InvoiceLineCollection();
        public CSIDInfo cSIDInfo = new CSIDInfo();
        public decimal CurrencyRate { get; set; }
        //public void AddInvoiceLine(InvoiceLine line)
        //{
        //    InvoiceLines.Add(line);
        //}
    }
}
