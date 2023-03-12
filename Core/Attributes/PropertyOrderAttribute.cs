using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class PropertyOrderAttribute : System.Attribute
    {
        public int Order { get; }

        public PropertyOrderAttribute(int order) { Order = order; }
    }
}