using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class OrderAttribute : PancakeAttribute
    {
        public readonly int order;

        public OrderAttribute(int order) { this.order = order; }
    }
}