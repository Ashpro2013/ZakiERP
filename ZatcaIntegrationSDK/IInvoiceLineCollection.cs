using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("DEFE1FBE-3FC4-4E1E-AA07-B39B853CD81A")]
    public interface IInvoiceLineCollection
    {
        int Count { get; }
        void Add(InvoiceLine obj);
    }

    [ClassInterface(ClassInterfaceType.None)]
    [Guid("B21BDB8E-BD21-40C4-BC1C-B3F39B651800")]
    public class InvoiceLineCollection: Collection<InvoiceLine>, IInvoiceLineCollection
    {
        int IInvoiceLineCollection.Count
        {
            get { return this.Count; }
        }

        void IInvoiceLineCollection.Add(InvoiceLine obj)
        {
            this.Add(obj);
        }
    }
}
