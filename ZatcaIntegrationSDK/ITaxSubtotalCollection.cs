using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("9AF10C21-5BD5-4F7F-B97A-7E486491604A")]
    public interface ITaxSubtotalCollection
    {
        int Count { get; }
        void Add(TaxSubtotal obj);
    }

    [ClassInterface(ClassInterfaceType.None)]
    [Guid("A4DF15E5-FB12-42FB-B44E-15E9425A7BFB")]
    public class TaxSubtotalCollection : Collection<TaxSubtotal>, ITaxSubtotalCollection
    {
        int ITaxSubtotalCollection.Count
        {
            get { return this.Count; }
        }

        void ITaxSubtotalCollection.Add(TaxSubtotal obj)
        {
            this.Add(obj);
        }
    }
}
