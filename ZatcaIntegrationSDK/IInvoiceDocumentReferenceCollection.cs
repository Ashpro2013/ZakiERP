using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public interface IInvoiceDocumentReferenceCollection
    {
        int Count { get; }
        void Add(InvoiceDocumentReference obj);
    }
    public class InvoiceDocumentReferenceCollection : Collection<InvoiceDocumentReference>, IInvoiceDocumentReferenceCollection
    {
        int IInvoiceDocumentReferenceCollection.Count
        {
            get { return this.Count; }
        }

        void IInvoiceDocumentReferenceCollection.Add(InvoiceDocumentReference obj)
        {
            this.Add(obj);
        }
    }
}
