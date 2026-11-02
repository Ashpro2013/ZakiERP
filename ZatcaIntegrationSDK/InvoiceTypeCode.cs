using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("3F2C46B0-9CFE-4B45-A7D6-45ED8D7103F2")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    public class InvoiceTypeCode
    {
        // فاتورة ضريبية او مبسطة 388
        //اشعار مدين 383
        //381 اشعار دائن
        public int id { get; set; }
        //inv.invoiceTypeCode.Name based on format NNPNESB
        //NN 01 للفاتورة الضريبية
        //NN 02 للفاتورة الضريبية المبسطة
        //P فى حالة فاتورة لطرف ثالث نكتب 1 وفى الحالة الاخرى نكتب 0
        //N فى حالة فاتورة اسمية نكتب 1 فى الحالة الاخرى نكتب 0
        //E فى حالة فاتورة للصادرات نكتب 1 وفى الحالة الاخرى نكتب 0
        //S فى حالة فاتورة ملخصة نكتب 1 وفى الحالة الاخرى نكتب 0
        //B  فى حالة فاتورة ذاتية نكتب 1
        //B فى حالة ان الفاتورة صادرات=1 لا يمكن ان تكون الفاتورة ذاتية =1

        public string Name { get; set; }
    }
}
