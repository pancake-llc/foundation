using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pancake.EventBus
{
    internal sealed class SubscriberWrapperComparer : IComparer<ISubscriberWrapper>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ISubscriberWrapper x, ISubscriberWrapper y)
        {
            return x.Order - y.Order;
        }
    }
}