using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZatcaIntegrationSDK
{
    [Guid("06B38D39-15BF-4D1A-B97F-242E0C295A42")]
    public interface IAllowanceChargeCollection
    {
        int Count { get; }
        void Add(AllowanceCharge obj);
    }

    [ClassInterface(ClassInterfaceType.None)]
    [Guid("336546CA-41DB-4B2D-98A7-492BC8963257")]
    public class AllowanceChargeCollection : Collection<AllowanceCharge>, IAllowanceChargeCollection
    {
        int IAllowanceChargeCollection.Count
        {
            get { return this.Count; }
        }

        void IAllowanceChargeCollection.Add(AllowanceCharge obj)
        {
            this.Add(obj);
        }
    }
}
