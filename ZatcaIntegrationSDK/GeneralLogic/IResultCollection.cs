using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("C17FC793-99BC-4BF6-9854-5E604141F396")]
    public interface IResultCollection
    {
        int Count { get; }
        void Add(Result obj);
    }

    [ClassInterface(ClassInterfaceType.None)]
    [Guid("35958A04-FFC8-4392-BC59-F8A457C86DE5")]
    public class ResultCollection: Collection<Result>, IResultCollection
    {
        int IResultCollection.Count
        {
            get { return this.Count; }
        }

        void IResultCollection.Add(Result obj)
        {
            this.Add(obj);
        }
    }
}
