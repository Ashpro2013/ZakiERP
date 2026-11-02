using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public class PaymentMeans
    {
        /// <summary>
        /// Code Description
        /// 10 In cash
        /// 30 Credit
        /// 42 Payment to bank account
        /// 48 Bank card
        /// 1 Instrument not defined(Free text)
        /// </summary>
        public string PaymentMeansCode { get; set; }
        /// <summary>
        /// فى حالة كان اشعار دائن او مدين لابد من ادخال الملاحظات
        /// </summary>
        public string InstructionNote { get; set; }

        public PayeeFinancialAccount payeefinancialaccount = new PayeeFinancialAccount(); //  فى حالة كان الدفع من خلال credit

    }
}
