using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    public interface IPaymentMeansCollection
    {
        int Count { get; }
        void Add(PaymentMeans obj);
    }
    public class PaymentMeansCollection : Collection<PaymentMeans>, IPaymentMeansCollection
    {
        int IPaymentMeansCollection.Count
        {
            get { return this.Count; }
        }

        void IPaymentMeansCollection.Add(PaymentMeans obj)
        {
            this.Add(obj);
        }
    }
}
