using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class OrderAttribute : ApexAttribute
    {
        public readonly int order;

        public OrderAttribute(int order) { this.order = order; }
    }
}