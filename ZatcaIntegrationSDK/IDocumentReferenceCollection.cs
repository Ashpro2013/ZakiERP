using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public interface IDocumentReferenceCollection
    {
        int Count { get; }
        void Add(DocumentReference obj);
    }
    public class DocumentReferenceCollection : Collection<DocumentReference>, IDocumentReferenceCollection
    {
        int IDocumentReferenceCollection.Count
        {
            get { return this.Count; }
        }

        void IDocumentReferenceCollection.Add(DocumentReference obj)
        {
            this.Add(obj);
        }
    }
}
